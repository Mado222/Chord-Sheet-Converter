using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindControlLib;

namespace ComponentsLibGUI
{
    public partial class FrmSetLogging : Form
    {
        public class ControlWriter(FrmSetLogging loggingWindow) : System.IO.TextWriter
        {
            private readonly FrmSetLogging _loggingWindow = loggingWindow;

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


        private readonly LoggingSettings _loggingSettings;
        private readonly BindingSource _bindingSource;
        public FrmSetLogging(LoggingSettings logs)
        {
            InitializeComponent();

            CbLogLevel.DataSource = Enum.GetValues(typeof(LogEventLevel));

            // Initialize LoggingSettings instance
            _loggingSettings = logs;

            // Initialize BindingSource and set the data source
            _bindingSource = new BindingSource
            {
                DataSource = _loggingSettings
            };

            // Bind the form controls to the corresponding properties of LoggingSettings
            richTextBoxLogs.ReadOnly = true; // Makes it read-only to prevent user edits
            BindControls();
        }

        private void BindControls()
        {
            // Bind the CheckBox to IsLoggingEnabled
            CbLogginOnOff.DataBindings.Add("Checked", _bindingSource, nameof(LoggingSettings.IsLoggingEnabled), false, DataSourceUpdateMode.OnPropertyChanged);

            // Bind the TextBox to LogFilePath
            TxTLogPath.DataBindings.Add("Text", _bindingSource, nameof(LoggingSettings.LogFilePath), false, DataSourceUpdateMode.OnPropertyChanged);

            // Bind the ComboBox to LogLevel
            CbLogLevel.DataBindings.Add("SelectedItem", _bindingSource, nameof(LoggingSettings.LogLevel), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public LoggingSettings GetLoggingSettings()
        {
            return _loggingSettings;
        }

        private void FrmSetLogging_Load(object sender, EventArgs e)
        {
        }

        private void TxTLogPath_MouseClick(object sender, MouseEventArgs e)
        {
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(TxTLogPath.Text);
            saveFileDialog1.FileName = Path.GetFileName(TxTLogPath.Text);
            saveFileDialog1.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {

                var logFilePath = saveFileDialog1.FileName;
                TxTLogPath.Text = logFilePath;
            }
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
    }
}
