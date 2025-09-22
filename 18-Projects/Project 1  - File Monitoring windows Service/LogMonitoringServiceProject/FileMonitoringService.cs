using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitoringServiceProject
{
    public partial class FileMonitoringService : ServiceBase
    {

        private FileSystemWatcher _FileSystemWatcher;
        private string _DestinationFolder = "";
        private string _SourceFolder = "";
        private string _LogFolder = "";




        private void  EnsureFolderPathsSet()
        {
            if (string.IsNullOrEmpty(_DestinationFolder))
            {
                _DestinationFolder = "C:\\FileMonitoring\\Destination";

            }
            if (string.IsNullOrEmpty(_SourceFolder))
            {
                _SourceFolder = @"C:\FileMonitoring\Source";
            }

            if (string.IsNullOrEmpty(_LogFolder))
            {
                _LogFolder = @"C:\\FileMonitoring\\Logs\";
            }
        }
        private void  EnsureFoldersExist()
        {


          


            if (!Directory.Exists(_DestinationFolder))
            {
                Directory.CreateDirectory(_DestinationFolder);
            }

            if (!Directory.Exists(_SourceFolder))
            {
                Directory.CreateDirectory(_SourceFolder);

            }

            // Ensure the directory for the log file exists
            string logDirectory = Path.GetDirectoryName(_LogFolder);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }


   
        /// <summary>
        /// Moves the specified file from the source folder to the destination folder after a short delay.
        /// Generates a new unique file name for the destination file and logs the operation.
        /// </summary>
        /// <param name="SourceFilePath">The full path of the file to move.</param>
        private void MoveFile(string SourceFilePath)
        {

            try
            {
                string Extension = Path.GetExtension(SourceFilePath);


                string FileName = $"{Guid.NewGuid()}{Extension}";
                string DestinationPath = Path.Combine(_DestinationFolder, FileName);

                // move the file after 500 ms 
                Task.Delay(500).ContinueWith(_ => { File.Move(SourceFilePath, DestinationPath); });
                
                LogServiceEvent($"Moved {SourceFilePath} -> {DestinationPath}");
            }
            catch (Exception ex)
            {
                LogServiceEvent($"Error Moved {SourceFilePath}.  Exeception :{ex.Message} ");
            }

           
        }




        public FileMonitoringService()
        {
            InitializeComponent();
            LoadConfigurationSettings();

            EnsureFolderPathsSet();
            EnsureFoldersExist();
            







        }


        /// <summary>
        ///  Load configuration settings of the service like folder paths
        /// </summary>
        private void LoadConfigurationSettings()
        {
            _DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
            _SourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            _LogFolder = ConfigurationManager.AppSettings["LogFolder"];
        }


        /// <summary>
        /// Sets up the FileSystemWatcher to monitor the source folder for new .txt files
        /// and attaches the event handler for file creation events.
        /// </summary>
        private void _SetupFileWatcher()
        {

            _FileSystemWatcher = new FileSystemWatcher
            {

                Path  =  _SourceFolder,
                Filter = "*.",
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _FileSystemWatcher.Created += FileSystemWatcher_Created;
        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            LogServiceEvent($"Detected : {e.FullPath}");

            MoveFile(e.FullPath);
           




         
        }


        /// <summary>
        /// Logs service events to the log file.
        /// </summary>
        /// <param name="message">Message describing the event or action.</param>
        private void LogServiceEvent(string Messaage)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {Messaage}\n";
            File.AppendAllText(_LogFolder, logMessage);
        }

        protected override void OnStart(string[] args)
        {
            _SetupFileWatcher();
            LogServiceEvent("Service Started");
            _FileSystemWatcher.EnableRaisingEvents = true;
            

        }

        protected override void OnStop()
        {
            _FileSystemWatcher.EnableRaisingEvents = false;
            _FileSystemWatcher.Dispose();
            LogServiceEvent("Service Stopped");
            
           
        }


        protected override void OnPause()
        {
            LogServiceEvent("Service Paused");
            _FileSystemWatcher.EnableRaisingEvents = false;

        }


        protected override void OnContinue()
        {
            _FileSystemWatcher.EnableRaisingEvents = true;
            LogServiceEvent("Service Resumed");

        }



        /// <summary>
        /// Simulates the lifecycle of the FileMonitoringService for debugging purposes.
        /// Allows manual control of service start, pause, resume, and stop actions via console input.
        /// Only runs in DEBUG builds.
        /// </summary>
        [Conditional("DEBUG")]
        public static void  SimulateService()
       {

            Console.WriteLine("Service Load all Dependices and and required Configurations ...");
            FileMonitoringService fileMonitoringService = new FileMonitoringService();

            Console.WriteLine("Please Press Any Key to Start the service  ...");
            Console.ReadKey();

            fileMonitoringService.OnStart(null);
            Console.WriteLine("Service Is Started  :)");

            Console.WriteLine("If you want to Pause it Press Any key...");
            Console.ReadKey();

            fileMonitoringService.OnPause();
            Console.WriteLine("Service Paused :)");

            Console.WriteLine("If you want to Resume  Press Any key...");
            Console.ReadKey();

            fileMonitoringService.OnContinue();
            Console.WriteLine("Service Resumed  :)");



            Console.WriteLine("If you want to Stop  the service Press Any key...");
            Console.ReadKey();

            fileMonitoringService.Stop();

            Console.WriteLine("Service Stoped :) Byyyyyyy");







        }

    }

  
}
