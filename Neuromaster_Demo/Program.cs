using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using WindControlLib;

namespace Neuromaster_Demo_Library_Reduced__netx
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Define log file path and logging level - these can be configured dynamically
            var logFilePath = "logs/app.log";  // Default path; change to a variable or configuration value if needed
            var isLoggingEnabled = true;       // Set to false to disable logging entirely

            // Configure Serilog as the logging provider
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()         // Set minimum logging level here
                .WriteTo.Console();

            if (isLoggingEnabled)
            {
                // Only add file logging if logging is enabled
                loggerConfig.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day);
            }

            Log.Logger = loggerConfig.CreateLogger();

            // Build the host with Dependency Injection and Serilog
            var host = Host.CreateDefaultBuilder()
                .UseSerilog() // Use Serilog for logging
                .ConfigureServices((context, services) =>
                {
                    // Register other services here if needed
                })
                .Build();

            // Initialize AppLogger with the logger factory
            var loggingSettings = new LoggingSettings
            {
                IsLoggingEnabled = true,
                LogFilePath = "logs/app.log"
            };

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            AppLogger.Initialize(loggerFactory, loggingSettings);

            // Use a generic logger context
            var logger = AppLogger.CreateLogger<object>();

            try
            {
                logger.LogInformation("Application starting");

                // Initialize application configuration and run the main form
                ApplicationConfiguration.Initialize();
                Application.Run(new Neuromaster_Demo.Neuromaster_Demo());
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush(); // Ensure logs are properly flushed on application exit
            }
        }
    }
}
