using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neuromaster_V5;
using Serilog;
using WindControlLib;

namespace Neuromaster5Net8
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
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
            var loggingSettings = new LoggingSettings();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            AppLogger.Initialize(loggerFactory, loggingSettings);

            // Use a generic logger context
            var logger = AppLogger.CreateLogger<object>();

            try
            {
                logger.LogInformation("Application starting");

                // To customize application configuration such as set high DPI settings or default font,
                ApplicationConfiguration.Initialize();
                Application.Run(new NeuromasterV5());

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