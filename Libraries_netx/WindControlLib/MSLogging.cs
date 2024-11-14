using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace WindControlLib
{
    public class LoggingSettings
    {
        public bool IsLoggingEnabled { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/app.log";
    }
    public static class AppLogger
    {
        private static ILoggerFactory? _loggerFactory;
        private static LoggingSettings _loggingSettings = new LoggingSettings();
        private static Logger? _serilogLogger;

        public static void Initialize(ILoggerFactory loggerFactory, LoggingSettings settings)
        {
            _loggerFactory = loggerFactory;
            _loggingSettings = settings;
            ConfigureSerilog();
        }

        public static void UpdateLoggingSettings(LoggingSettings settings)
        {
            _loggingSettings = settings;
            ConfigureSerilog();
        }

        private static void ConfigureSerilog()
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()  // Set minimum logging level

                // Write to both file and debug output
                .WriteTo.File(_loggingSettings.LogFilePath, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug();  // This logs to Debug.WriteLine

            Log.CloseAndFlush();  // Close any previous loggers
            _serilogLogger = config.CreateLogger();
            Log.Logger = _serilogLogger;  // Assign the new configuration to the global logger
        }


        public static Microsoft.Extensions.Logging.ILogger<T> CreateLogger<T>() => _loggerFactory!.CreateLogger<T>();
    }
}

