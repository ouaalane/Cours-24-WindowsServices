using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MyFirstWorkerService;


var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
