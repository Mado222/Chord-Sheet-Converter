using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace WindControlLib
{
    public class LoggingSettings
    {
        public bool IsLoggingEnabled { get; set; } = true;
        public string LogFilePath { get; set; } = "";
        public string LogFileName { get; set; } = "";
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Error;

        public LoggingSettings()
        {
            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "noName";

            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName, "logs");
            Directory.CreateDirectory(logPath); // Ensure the directory exists

            LogFileName = $"{applicationName}.log";
            LogFilePath = Path.Combine(logPath, LogFileName);
        }
    }
    public static class AppLogger
    {
        private static ILoggerFactory? _loggerFactory;
        private static LoggingSettings _loggingSettings = new ();
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
                .MinimumLevel.Is(_loggingSettings.LogLevel)  // Set minimum logging level

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

