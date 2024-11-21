using ComponentsLibGUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neuromaster_V5;
using Serilog;
using Serilog.Core;
using Serilog.Events;
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
            // Set Windows Forms settings before creating any forms
            ApplicationConfiguration.Initialize();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create logging settings with the initial log level and path
            var loggingSettings = new LoggingSettings();  // Create instance of LoggingSettings

            // Create the logging form
            FrmShowLogging loggingWindow = new();

            // Create a single LoggingLevelSwitch that will be used throughout the app
            var loggingLevelSwitch = new LoggingLevelSwitch(loggingSettings.LogLevel);

            // Configure Serilog as the logging provider with the dynamic level switch
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch) // Use the shared LoggingLevelSwitch for dynamic control
                .WriteTo.Console()  // Write to console for debugging
                .WriteTo.Sink(new FormSink(loggingWindow));  // Write to the logging form

            if (loggingSettings.IsLoggingEnabled)
            {
                // Only add file logging if logging is enabled
                loggerConfig.WriteTo.File(loggingSettings.LogFilePath, rollingInterval: RollingInterval.Day);
            }

            Log.Logger = loggerConfig.CreateLogger(); // Set Log.Logger once at the beginning

            // Start the logging window - run it in its own thread to ensure it's active
            var loggingWindowThread = new Thread(() =>
            {
                Application.Run(loggingWindow);
            })
            {
                IsBackground = true
            };
            loggingWindowThread.Start();

            // Build the host with Dependency Injection and Serilog
            var host = Host.CreateDefaultBuilder()
                .UseSerilog(Log.Logger, dispose: true) // Use the already configured Serilog instance
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders(); // Remove other logging providers
                        loggingBuilder.AddSerilog(); // Add Serilog
                    });
                    // Register other services here if needed
                })
                .Build();

            // Initialize AppLogger with the logger factory and pass the shared LoggingLevelSwitch
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            AppLogger.Initialize(loggerFactory, loggingSettings, loggingLevelSwitch);

            // Use a generic logger context
            var logger = AppLogger.CreateLogger<object>();

            try
            {
                logger.LogInformation("Application starting");

                // Start the main application form
                Application.Run(new NeuromasterV5(loggingWindow));
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