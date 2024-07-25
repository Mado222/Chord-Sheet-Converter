using FeedbackDataLib.Modules.CADS1292x;
using FeedbackDataLib.Modules;
using System;
using System.Windows.Forms;


namespace FeedbackDataLib_GUI
{
    public partial class ucModuleEEG : UserControl
    {
        public CModuleEEG cModuleEEG { get; set; }
        public ucModuleEEG()
        {
            InitializeComponent();
            cModuleEEG = new CModuleEEG
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

