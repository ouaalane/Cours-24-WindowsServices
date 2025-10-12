using Serilog;

namespace FileMonitoringService
{
    public class FileMonitoringWorker : BackgroundService
    {
        private readonly ILogger<FileMonitoringWorker> _logger;
        private readonly IConfiguration _configuration;
        private FileSystemWatcher? _fileSystemWatcher;
        private readonly string _sourcePath;
        private readonly string _destinationFolder;
        private readonly string _logPath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileMonitoringWorker(ILogger<FileMonitoringWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Initialize paths during construction
            _sourcePath = ValidateAndGetPath("SourceFolder", @"C:\FileMonitoring\Source");
            _destinationFolder = ValidateAndGetPath("DestinationFolder", @"C:\FileMonitoring\Destination");
            _logPath = ValidateAndGetPath("LogPath", "logs");

            EnsureFoldersExist();
        }

        private string ValidateAndGetPath(string configKey, string defaultPath)
        {
            var path = _configuration[configKey];

            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogWarning("{ConfigKey} is missing in appsettings.json. Using default: {DefaultPath}",
                    configKey, defaultPath);
                return defaultPath;
            }

            return path;
        }

        private void EnsureFoldersExist()
        {
            try
            {
                Directory.CreateDirectory(_sourcePath);
                Directory.CreateDirectory(_destinationFolder);

                // Create the file if it doesn't exist
                if (!File.Exists(_logPath))
                {
                    File.Create(_logPath).Dispose(); // Dispose to release the file handle
                }


                _logger.LogInformation("Folders created/verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create required folders");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _fileSystemWatcher = new FileSystemWatcher
                {
                    Path = _sourcePath,
                    Filter = "*.*",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                _fileSystemWatcher.Created += OnFileCreated;
                _fileSystemWatcher.Error += OnError;

                _logger.LogInformation("File monitoring started on folder: {SourcePath}", _sourcePath);

                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("File monitoring service is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ExecuteAsync");
                throw;
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Ensure only one file is processed at a time to avoid conflicts
            await _semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("File detected: {FilePath}", e.FullPath);

                // Wait for file to be fully written
                await WaitForFileToBeReady(e.FullPath);

                string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(e.Name)}";
                string destinationFile = Path.Combine(_destinationFolder, newFileName);

                File.Move(e.FullPath, destinationFile, overwrite: false);

                _logger.LogInformation("File moved: {SourcePath} -> {DestinationPath}",
                    e.FullPath, destinationFile);
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "IO error processing file: {FilePath}", e.FullPath);
                // Optionally retry or move to error folder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", e.FullPath);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task WaitForFileToBeReady(string filePath, int maxRetries = 10)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Try to open the file exclusively
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return; // File is ready
                    }
                }
                catch (IOException)
                {
                    // File is still being written, wait and retry
                    await Task.Delay(500);
                }
            }

            throw new IOException($"File {filePath} is still locked after {maxRetries} retries");
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();
            _logger.LogError(exception, "FileSystemWatcher encountered an error");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping file monitoring service");

            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Created -= OnFileCreated;
                _fileSystemWatcher.Error -= OnError;
                _fileSystemWatcher.Dispose();
            }

            _semaphore?.Dispose();

            await base.StopAsync(cancellationToken);

            _logger.LogInformation("Service stopped successfully");
        }
    }
}