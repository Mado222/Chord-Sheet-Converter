using WindControlLib;

namespace ComponentsLibGUI
{
    public partial class FrmSetLogging : Form
    {
        private readonly UCSetLogging _ucSetLogging;

        public FrmSetLogging(LoggingSettings logs)
        {
            InitializeComponent();

            _ucSetLogging = new UCSetLogging();
            _ucSetLogging.Dock = DockStyle.Fill;
            _ucSetLogging.Initialize(logs);

            _ucSetLogging.ButtonClicked += UcSetLogging_ButtonClicked!;

            Controls.Add(_ucSetLogging);
        }

        private void UcSetLogging_ButtonClicked(object sender, ButtonClickEventArgs e)
        {
            DialogResult = e.Result;
            Close();
        }

        public LoggingSettings GetLoggingSettings()
        {
            return _ucSetLogging.GetLoggingSettings();
        }
    }
}
