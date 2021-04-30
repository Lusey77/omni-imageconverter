using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Omni.ImageConverter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(@"C:\Temp\Omni\ImageConverter\logs\image-converter.log", fileSizeLimitBytes: 1024 * 1024 * 1)
                .CreateLogger();

            try
            {
                Log.Information("Starting service: Omni.ImageConverter");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error starting service: Omni.ImageConverter");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConversionSettings>(hostContext.Configuration.GetSection(ConversionSettings.Key));
                    
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
    }
}
