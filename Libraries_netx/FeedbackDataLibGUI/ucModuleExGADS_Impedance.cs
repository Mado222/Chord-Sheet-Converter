using FeedbackDataLib.Modules;
using FeedbackDataLib.Modules.CADS1294x;

namespace FeedbackDataLibGUI
{
    public partial class UcModuleExGADSImpedance : UserControl
    {
        public CModuleExGADS1294 ModuleExGADS1294 { get; set; }
        public UcModuleExGADSImpedance()
        {
            InitializeComponent();
            ModuleExGADS1294 = new CModuleExGADS1294
            {
                name = "ucModuleImpedance"
            };
        }

        public int GetSR_ms(int chNo)
        {
            int res = 200;
            if (chNo == 0)
            {
                if (int.TryParse(txtSR0.Text, out int _res))
                    res = _res;
            }
            else if (chNo == 1)
            {
                if (int.TryParse(txtSR1.Text, out int _res))
                    res = _res;
            }
            if (res < 100)
                res = 200;

            return res;
        }

        public void SetImpedanceBoxes(CADS1294x_ElectrodeImp mi)
        {
            //Electrode Information

            lblxnR1.Text = (mi.Get_ElectrodeInfo_n(0).Impedance_Ohm / 1000).ToString("F0");
            lblUel1.Text = (mi.Get_ElectrodeInfo_n(0).UElektrode_V * 1e6).ToString("F0");

            lblxnR2.Text = (mi.Get_ElectrodeInfo_p(0).Impedance_Ohm / 1000).ToString("F0");
            lblxnV2.Text = (mi.Get_ElectrodeInfo_p(0).UElektrode_V * 1e6).ToString("F0");

            lblxpR1.Text = (mi.Get_ElectrodeInfo_n(1).Impedance_Ohm / 1000).ToString("F0");
            lblxpV1.Text = (mi.Get_ElectrodeInfo_n(1).UElektrode_V * 1e6).ToString("F0");

            lblxpR2.Text = (mi.Get_ElectrodeInfo_p(1).Impedance_Ohm / 1000).ToString("F0");
            lblxpV2.Text = (mi.Get_ElectrodeInfo_p(1).UElektrode_V * 1e6).ToString("F0");
        }
    }
}

