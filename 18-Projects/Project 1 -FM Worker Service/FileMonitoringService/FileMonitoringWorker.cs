namespace FileMonitoringService
{
    public class FileMonitoringWorker : BackgroundService
    {
        private readonly ILogger<FileMonitoringWorker> _logger;
        private readonly IConfiguration _configuration;


        private FileSystemWatcher _fileSystemWatcher;


        private string sourcePath;
        private string destinationfolder;
        private string logfolder;

        private void ValidateFoldersPath()
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                   sourcePath = @"C:\FileMonitoring\Source";
                _logger.Log(LogLevel.Warning, $"Source folder is missing in appsettings.json Using Default {sourcePath} ");
            }
            else
            {
                
                sourcePath = _configuration["SourceFolder"].ToString();
            }


            if (string.IsNullOrEmpty(destinationfolder))
            {
                destinationfolder = @"C:\FileMonitoring\Source";
                _logger.Log(LogLevel.Warning, $"Source folder is missing in appsettings.json Using Default {destinationfolder} ");
            }
            else
            {
                destinationfolder = _configuration["DestinationFolder"].ToString();
            }

             logfolder = _configuration["LogFolder"].ToString();




        }

        private  void EnsureFoldersExist()
        {
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(destinationfolder);
            Directory.CreateDirectory(logfolder);
        }

        public FileMonitoringWorker (ILogger<FileMonitoringWorker> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            ValidateFoldersPath();
            EnsureFoldersExist();
            EnsureFoldersExist();

        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = sourcePath,
                Filter = "*.*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _logger.Log(LogLevel.Information, $"File Monitoring Started On Folder : {sourcePath}");

        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {

            try
            {
                _logger.Log(LogLevel.Information,$"File Detected : {e.FullPath}");


                string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(e.Name)}";
                string destinationFile = Path.Combine(destinationfolder, newFileName);

                File.Move(e.FullPath,destinationfolder);

                _logger.Log(LogLevel.Information,$"File Moved : {e.FullPath} -> {destinationfolder}"); 
                



            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Information,$"Error processing file : {e.FullPath}. Exception : {ex.Message}");
            }

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await  ExecuteAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Dispose();
            _logger.Log(LogLevel.Information,"Service Stopped.");

        }




    }
}
