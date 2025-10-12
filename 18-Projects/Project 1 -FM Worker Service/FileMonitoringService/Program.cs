using FileMonitoringService;
using Serilog;;


var builder = Host.CreateApplicationBuilder(args);

var logFilePath = builder.Configuration["LogPath"];

// Ensure log folder exists
var logFolder = Path.GetDirectoryName(logFilePath);
if (!Directory.Exists(logFolder))
{
    Directory.CreateDirectory(logFolder);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddSystemd();


builder.Services.AddHostedService<FileMonitoringWorker>();

var host = builder.Build();
host.Run();
