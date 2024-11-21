using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using WindControlLib;
using Serilog.Events;

namespace ComponentsLibGUI
{
    public partial class UCSetLogging : UserControl
    {
        private LoggingSettings _loggingSettings = new LoggingSettings();
        private BindingSource _bindingSource = new BindingSource();

        public event EventHandler<LoggingSettings>? LoggingSettingsChanged;
        public event EventHandler<ButtonClickEventArgs>? ButtonClicked;

        private CheckedListBox checkedListBox;

        public UCSetLogging()
        {
            InitializeComponent();

            BtOK.Click += (s, e) => OnButtonClicked(DialogResult.OK);
            BtCancel.Click += (s, e) => OnButtonClicked(DialogResult.Cancel);

            checkedListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            foreach (string enumName in Enum.GetNames(typeof(LogEventLevel)))
            {
                checkedListBox.Items.Add(enumName);
            }

            checkedListBox.Dock = DockStyle.Fill;
            PnDisplay.Controls.Add(checkedListBox);
        }

        public void Initialize(LoggingSettings logs)
        {
            CbLogLevel.DataSource = Enum.GetValues(typeof(LogEventLevel));

            // Initialize LoggingSettings instance
            _loggingSettings = logs;

            // Subscribe to PropertyChanged event
            _loggingSettings.PropertyChanged += LoggingSettings_PropertyChanged!;

            // Initialize BindingSource and set the data source
            _bindingSource = new BindingSource
            {
                DataSource = _loggingSettings
            };

            // Bind the form controls to the corresponding properties of LoggingSettings
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
            CbLogLevel.SelectedIndexChanged += (s, e) =>
            {
                _bindingSource.EndEdit();
                UpdateCheckedListBox((LogEventLevel)CbLogLevel.SelectedItem);
            };
        }

        private void UpdateCheckedListBox(LogEventLevel selectedLevel)
        {
            // Recreate the checkedListBox with only elements that are sent from the logger at the selected LogEventLevel
            PnDisplay.Controls.Remove(checkedListBox);

            checkedListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            foreach (string enumName in Enum.GetNames(typeof(LogEventLevel)))
            {
                var item = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), enumName);
                if ((int)item >= (int)selectedLevel)
                {
                    checkedListBox.Items.Add(enumName, true);
                }
            }

            PnDisplay.Controls.Add(checkedListBox);
        }

        private void LblLogPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new()
            {
                InitialDirectory = Path.GetDirectoryName(TxTLogPath.Text),
                FileName = Path.GetFileName(TxTLogPath.Text),
                Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*"
            };
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var logFilePath = saveFileDialog1.FileName;
                TxTLogPath.Text = logFilePath;
            }
        }

        public ColoredText? FilterMessage(ColoredText inputMessage)
        {
            foreach (string checkedItem in checkedListBox.CheckedItems)
            {
                if (inputMessage.Text.Contains($"[{checkedItem[..3].ToLower()}]", StringComparison.CurrentCultureIgnoreCase))
                {
                    return inputMessage;
                }
            }
            return null;
        }

        private void LoggingSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Fire the LoggingSettingsChanged event
            LoggingSettingsChanged?.Invoke(this, _loggingSettings);
        }

        protected virtual void OnButtonClicked(DialogResult result)
        {
            ButtonClicked?.Invoke(this, new ButtonClickEventArgs(result));
        }

        public LoggingSettings GetLoggingSettings()
        { return _loggingSettings; }

        public void SetButtonVisibility(bool visible)
        {
            BtOK.Visible = visible;
            BtCancel.Visible = visible;
        }
    }


    public class ButtonClickEventArgs(DialogResult result) : EventArgs
    {
        public DialogResult Result { get; } = result;
    }

}
