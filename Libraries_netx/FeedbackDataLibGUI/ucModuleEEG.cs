using FeedbackDataLib.Modules;


namespace FeedbackDataLibGUI
{
    public partial class ucModuleEEG : UserControl
    {
        public CModuleEEG ModuleEEG { get; set; }
        public ucModuleEEG()
        {
            InitializeComponent();
            ModuleEEG = new CModuleEEG
            {
                name = "ucModuleEEG"
            };
        }

        public int GetSR_ms()
        {
            int res = 200;
            if (int.TryParse(txtSR0.Text, out int _res))
                res = _res;

            if (res < 100)
                res = 200;

            return res;
        }
    }
}

