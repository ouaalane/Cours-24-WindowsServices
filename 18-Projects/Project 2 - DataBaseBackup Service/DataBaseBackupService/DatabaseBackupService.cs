using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace DataBaseBackupService
{
    /// <summary>
    /// Windows Service for automated SQL Server database backups.
    /// Handles configuration, scheduling, backup execution, and logging.
    /// </summary>
    public partial class DatabaseBackupService : ServiceBase
    {
        private string _ConnectionString;
        private string _LogFolder;
        private string _BackupFolder;
        private int _BackupIntervalMinutes;
        private static Timer _Timer;

        /// <summary>
        /// Initializes the service, loads configuration, and ensures required directories exist.
        /// </summary>
        public DatabaseBackupService()
        {
            InitializeComponent();
            InitializeConfiguration();

            if (string.IsNullOrEmpty(_ConnectionString))
            {
                Log("The connection string is empty");
                return;
            }

            // Set default folders if not configured
            if (string.IsNullOrEmpty(_LogFolder))
            {
                _LogFolder = @"C:\DatabaseBackups\Logs";
            }
            if (string.IsNullOrEmpty(_BackupFolder))
            {
                _BackupFolder = @"C:\DatabaseBackups";
            }

            // Create directories
            Directory.CreateDirectory(_LogFolder);
            Directory.CreateDirectory(_BackupFolder);
        }

        /// <summary>
        /// Writes a log message to the daily log file. Falls back to Windows Event Log on failure.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void Log(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                string logFilePath = Path.Combine(_LogFolder, $"backup_log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("DatabaseBackupService", $"Logging failed: {ex.Message}", EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Loads configuration values from App.config and sets default values if necessary.
        /// </summary>
        private void InitializeConfiguration()
        {
            _ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
            _LogFolder = ConfigurationManager.AppSettings["LogFolder"];
            _BackupFolder = ConfigurationManager.AppSettings["BackupFolder"];

            if (int.TryParse(ConfigurationManager.AppSettings["BackupIntervalMinutes"], out int result))
            {
                _BackupIntervalMinutes = result;
            }
            else
            {
                _BackupIntervalMinutes = 1; // Default to 1 minute
            }
        }

        /// <summary>
        /// Performs a full database backup and logs the result.
        /// </summary>
        /// <param name="state">Unused state object for Timer callback.</param>
        public void PerformDatabaseBackup(object state)
        {
            using (SqlConnection sqlConnection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // Construct the full backup file path
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"{sqlConnection.Database}_Backup_{timestamp}.bak";
                    string fullBackupPath = Path.Combine(_BackupFolder, backupFileName);

                    string backupQuery = $@"
                        BACKUP DATABASE [{sqlConnection.Database}] 
                        TO DISK = '{fullBackupPath}' 
                        WITH INIT, FORMAT, NAME = '{sqlConnection.Database}-FullDatabaseBackup-{timestamp}';";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, sqlConnection))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout for large databases
                        cmd.ExecuteNonQuery();
                        Log($"Database backup successful: {fullBackupPath}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error during backup: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Starts the service and schedules periodic database backups.
        /// </summary>
        /// <param name="args">Service start arguments.</param>
        protected override void OnStart(string[] args)
        {
            Log("Service Started.");

            _Timer = new Timer(
                callback: PerformDatabaseBackup,
                state: null,
                dueTime: 0, // Start immediately
                period: _BackupIntervalMinutes * 60 * 1000 // Convert minutes to milliseconds
            );
        }

        /// <summary>
        /// Stops the service and disposes the backup timer.
        /// </summary>
        protected override void OnStop()
        {
            Log("Service Stopped.");
            _Timer?.Dispose();
        }

        /// <summary>
        /// Simulates the service lifecycle for debugging purposes (DEBUG builds only).
        /// </summary>
        [Conditional("DEBUG")]
        public static void SimulateService()
        {
            Console.WriteLine("Service Load all Dependices and and required Configurations ...");
            DatabaseBackupService DataBaseBackUpService = new DatabaseBackupService();

            Console.WriteLine("Please Press Any Key to Start the service  ...");
            Console.ReadKey();

            DataBaseBackUpService.OnStart(null);
            Console.WriteLine("Service Is Started  :)");

            Console.WriteLine("If you want to Stop  the service Press Any key...");
            Console.ReadKey();

            DataBaseBackUpService.Stop();

            Console.WriteLine("Service Stoped :) Byyyyyyy");
        }
    }
}