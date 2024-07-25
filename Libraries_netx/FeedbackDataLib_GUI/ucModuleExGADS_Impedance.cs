using FeedbackDataLib.Modules.CADS1292x;
using FeedbackDataLib;

namespace FeedbackDataLib_GUI
{
    public partial class ucModuleExGADS_Impedance : UserControl
    {
        public CModuleExGADS1292 cModuleExGADS1292 { get; set; }
        public ucModuleExGADS_Impedance()
        {
            InitializeComponent();
            cModuleExGADS1292 = new CModuleExGADS1292
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

        public void SetImpedanceBoxes(CADS1292x_ElectrodeImp mi)
        {
            //Electrode Information

            lblxnR1.Text = (mi.Get_ElectrodeInfo_n(0).Impedance_Ohm / 1000).ToString("F0");
            lblxnV1.Text = (mi.Get_ElectrodeInfo_n(0).UElektrode_V * 1e6).ToString("F0");

            lblxnR2.Text = (mi.Get_ElectrodeInfo_p(0).Impedance_Ohm / 1000).ToString("F0");
            lblxnV2.Text = (mi.Get_ElectrodeInfo_p(0).UElektrode_V * 1e6).ToString("F0");

            lblxpR1.Text = (mi.Get_ElectrodeInfo_n(1).Impedance_Ohm / 1000).ToString("F0");
            lblxpV1.Text = (mi.Get_ElectrodeInfo_n(1).UElektrode_V * 1e6).ToString("F0");

            lblxpR2.Text = (mi.Get_ElectrodeInfo_p(1).Impedance_Ohm / 1000).ToString("F0");
            lblxpV2.Text = (mi.Get_ElectrodeInfo_p(1).UElektrode_V * 1e6).ToString("F0");
        }
    }
}

