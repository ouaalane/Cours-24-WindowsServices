using DataBaseBackupWorkerServicenamespace;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);


var logFilePath = builder.Configuration["LogPath"];

var Logfolder = Path.GetDirectoryName(logFilePath);

if (string.IsNullOrEmpty(logFilePath))
{
    logFilePath = "logs\\log.txt";
}

if (!File.Exists(logFilePath))
{
    File.Create(logFilePath);
}


// config serilog

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}]  {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}]  {Message:lj}{NewLine}{Exception}")
    .CreateLogger();


builder.Services.AddHostedService<DataBaseBackupWorkerService>();


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
var host = builder.Build();
host.Run();
