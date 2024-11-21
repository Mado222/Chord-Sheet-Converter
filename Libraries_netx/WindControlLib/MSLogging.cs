using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace WindControlLib
{
    public class LoggingSettings : INotifyPropertyChanged
    {
        private bool _isLoggingEnabled= true;
        private string _logFilePath ="";
        private string _logFileName = "";
        private LogEventLevel _logLevel = LogEventLevel.Information;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsLoggingEnabled
        {
            get => _isLoggingEnabled;
            set
            {
                if (_isLoggingEnabled != value)
                {
                    _isLoggingEnabled = value;
                    OnPropertyChanged(nameof(IsLoggingEnabled));
                }
            }
        }

        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                if (_logFilePath != value)
                {
                    _logFilePath = value;
                    OnPropertyChanged(nameof(LogFilePath));
                }
            }
        }

        public string LogFileName
        {
            get => _logFileName;
            set
            {
                if (_logFileName != value)
                {
                    _logFileName = value;
                    OnPropertyChanged(nameof(LogFileName));
                }
            }
        }

        public LogEventLevel LogLevel
        {
            get => _logLevel;
            set
            {
                if (_logLevel != value)
                {
                    _logLevel = value;
                    OnPropertyChanged(nameof(LogLevel));
                }
            }
        }

        public LoggingSettings()
        {
            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "noName";

            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName, "logs");
            Directory.CreateDirectory(logPath); // Ensure the directory exists

            LogFileName = $"{applicationName}.log";
            LogFilePath = Path.Combine(logPath, LogFileName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class AppLogger
    {
        private static readonly object _lock = new();
        private static ILoggerFactory? _loggerFactory;
        private static LoggingSettings _loggingSettings = new();
        private static Logger? _serilogLogger;
        private static LoggingLevelSwitch? _loggingLevelSwitch;

        public static void Initialize(ILoggerFactory loggerFactory, LoggingSettings settings, LoggingLevelSwitch loggingLevelSwitch)
        {
            if (_loggerFactory != null)
            {
                throw new InvalidOperationException("AppLogger has already been initialized.");
            }

            lock (_lock)
            {
                _loggerFactory = loggerFactory;
                _loggingSettings = settings;
                _loggingLevelSwitch = loggingLevelSwitch; // Use the shared LoggingLevelSwitch

                ConfigureSerilog(); // Configure the logger initially
            }
        }

        public static void UpdateLoggingSettings(LoggingSettings settings)
        {
            lock (_lock)
            {
                // Update the logging settings
                _loggingSettings = settings;

                // Update log level dynamically using the shared LoggingLevelSwitch
                if (_loggingLevelSwitch  != null)
                    _loggingLevelSwitch.MinimumLevel = settings.LogLevel;

                // Reconfigure the logger if the log file path changes
                if (!string.Equals(_loggingSettings.LogFilePath, settings.LogFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    ReconfigureFilePath(settings);
                }
            }
        }

        public static Microsoft.Extensions.Logging.ILogger<T> CreateLogger<T>()
        {
            EnsureInitialized();
            return _loggerFactory!.CreateLogger<T>();
        }

        private static void EnsureInitialized()
        {
            if (_loggerFactory == null)
            {
                throw new InvalidOperationException("AppLogger has not been initialized.");
            }
        }

        private static void ConfigureSerilog()
        {
            if (_loggingLevelSwitch == null) return;

            var config = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_loggingLevelSwitch) // Use the shared LoggingLevelSwitch
                .WriteTo.File(_loggingSettings.LogFilePath, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug(); // Output to debug for testing

            Log.CloseAndFlush(); // Flush any previous logs
            _serilogLogger = config.CreateLogger();
            Log.Logger = _serilogLogger; // Set this only once during initialization.
        }

        private static void ReconfigureFilePath(LoggingSettings settings)
        {
            if (_loggingLevelSwitch == null) return;
            
            Log.CloseAndFlush(); // Flush the logs before reconfiguring

            var newConfig = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_loggingLevelSwitch) // Use the shared LoggingLevelSwitch for dynamic control
                .WriteTo.File(settings.LogFilePath, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug(); // Output to debug for testing

            _serilogLogger = newConfig.CreateLogger();
            Log.Logger = _serilogLogger; // Set Log.Logger only if absolutely needed
        }
    }



}
