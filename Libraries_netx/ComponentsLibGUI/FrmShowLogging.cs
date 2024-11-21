using Serilog.Core;
using Serilog.Events;
using System.Text;
using WindControlLib;
using Microsoft.Extensions.Logging;

namespace ComponentsLibGUI
{
    public partial class FrmShowLogging : Form
    {
        public event EventHandler<LoggingSettings>? LoggingSettingsChanged;
        private void OnLoggingSettingsChanged(LoggingSettings settings)
        {
            LoggingSettingsChanged?.Invoke(this, settings);
        }

        public FrmShowLogging(LoggingSettings? logs = null)
        {
            InitializeComponent();
            //_logger = logger;
            richTextBoxLogs.ReadOnly = true; // Makes it read-only to prevent user edits
            _ucSetLogging.SetButtonVisibility(false);

            _ucSetLogging.Initialize(logs ?? new LoggingSettings());

            _ucSetLogging.LoggingSettingsChanged += _ucSetLogging_LoggingSettingsChanged;
        }

        private void _ucSetLogging_LoggingSettingsChanged(object? sender, LoggingSettings e)
        {
            OnLoggingSettingsChanged(e);
        }

        // Method to add log messages to the RichTextBox
        public void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendLog(message)));
            }
            else
            {
                richTextBoxLogs.AppendText(message + Environment.NewLine);
                richTextBoxLogs.ScrollToCaret(); // Scroll to the latest log
            }
        }

        public void AppendColoredTextToRichTextBox(ColoredText coloredText)
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                    Invoke(new Action(() => AppendColoredTextToRichTextBox(coloredText)));
            }
            else
            {
                if (!IsDisposed && !richTextBoxLogs.IsDisposed && richTextBoxLogs.IsHandleCreated)
                {
                    if (_ucSetLogging != null)
                    {
                        ColoredText? s = _ucSetLogging.FilterMessage(coloredText);
                        if (s != null)
                        {
                            richTextBoxLogs.SelectionStart = richTextBoxLogs.TextLength;
                            richTextBoxLogs.SelectionLength = 0;
                            richTextBoxLogs.SelectionColor = coloredText.Color;
                            richTextBoxLogs.AppendText(coloredText.Text + Environment.NewLine);
                            richTextBoxLogs.SelectionColor = richTextBoxLogs.ForeColor;
                            richTextBoxLogs.ScrollToCaret();
                        }
                    }
                }
            }
        }

        private void BtClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxLogs.Clear();
        }
    }

    public class FormSink(FrmShowLogging loggingWindow) : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            // Determine the color based on log level
            Color color;
            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                    color = Color.Brown;
                    break;
                case LogEventLevel.Information:
                    color = Color.Blue;
                    break;
                case LogEventLevel.Warning:
                    color = Color.Yellow;
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    color = Color.Red;
                    break;
                default:
                    color = Color.Black;
                    break;
            }

            // Render the log event to a string
            var renderedMessage = logEvent.RenderMessage();

            // Shorten log level for display
            string logLevelShort = logEvent.Level.ToString()[..3];

            // Add the log level and timestamp if needed
            var outputMessage = $"[{logEvent.Timestamp:HH:mm:ss}] [{logLevelShort}] {renderedMessage}";

            // Create an instance of ColoredText
            var coloredText = new ColoredText(outputMessage, color);

            // Send the log message to the RichTextBox with appropriate color
            loggingWindow.AppendColoredTextToRichTextBox(coloredText);
        }
    }

    public class ControlWriter(FrmShowLogging loggingWindow) : System.IO.TextWriter
    {
        private readonly FrmShowLogging _loggingWindow = loggingWindow;

        public override void Write(char value)
        {
            _loggingWindow.AppendLog(value.ToString());
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                _loggingWindow.AppendLog(value);
            }
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
