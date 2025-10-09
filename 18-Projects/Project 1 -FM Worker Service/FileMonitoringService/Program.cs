using FileMonitoringService;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FileMonitoringService";
});

// Read file path from appsettings.json
 string filePath = builder.Configuration.GetValue<string>("LogFolder") ?? "C:\\Programming Advice\\Cours-24-Windows Services\\18-Projects\\Project 1 -FM Worker Service\\FileMonitoringService\\logs";



Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs\\log.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();



builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddHostedService<FileMonitoringWorker>();

var host = builder.Build();
host.Run();