using Microsoft.Data.SqlClient;
using System.Data;
namespace DataBaseBackupWorkerServicenamespace
{
    public class DataBaseBackupWorkerService : BackgroundService
    {
        private readonly ILogger<DataBaseBackupWorkerService> _logger;
        private readonly IConfiguration _configuration;


        private string _ConnectionString;
        private string _LogPath;
        private string _BackupFolder;
        private int _BackupIntervalMinutes =1;





        public DataBaseBackupWorkerService(ILogger<DataBaseBackupWorkerService> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            try
            {
                _ConnectionString = _configuration["ConnectionString"];
                
                _LogPath= _configuration["LogPath"];
                _BackupFolder = _configuration["BackupFolder"];
                _BackupIntervalMinutes = int.Parse(_configuration["BackupIntervalMinutes"]);

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error,$"{ex.Message}");
            }

          
             if (string.IsNullOrEmpty(_ConnectionString) )
             {
                _logger.Log(LogLevel.Error, "The connection String is empy");
                throw new Exception("connection string empty");
             }
          
              
             if (string.IsNullOrEmpty(_BackupFolder))
             {
                _BackupFolder = @"C:\DatabaseBackups";
             }

            
        }

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
                        _logger.Log (LogLevel.Information,$"Database backup successful: {fullBackupPath}");
                    }
                }
                catch (Exception ex)
                {
                   _logger.Log(LogLevel.Information ,$"Error during backup: {ex.Message}");
                }
            }
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }


        
    }
}
