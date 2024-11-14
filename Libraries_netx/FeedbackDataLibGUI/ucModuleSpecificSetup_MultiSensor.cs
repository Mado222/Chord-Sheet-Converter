using FeedbackDataLib.Modules;

namespace FeedbackDataLibGUI
{
    public partial class UcModuleSpecificSetupMultiSensor : UserControl
    {
        public UcModuleSpecificSetupMultiSensor()
        {
            InitializeComponent();
        }

        public void ReadModuleSpecificInfo(ref CModuleMultisensor ModuleInfo)
        {
            ModuleInfo.SCLTest = cbTestMultisensor.Checked;
            ModuleInfo.SCLTest_Resistor_High = cbResHiLow.Checked;
            ModuleInfo.PulseRun = cbClearPulse.Checked;
            ModuleInfo.Pulse_v_high = cbVPulseHiLo.Checked;
        }

        public void UpdateModuleInfo(CModuleMultisensor ModuleInfo)
        {
            gbPulse.Visible = true;
            gbSCL.Visible = false;

            if (ModuleInfo.ModuleType == EnModuleType.cModuleMultisensor)
            {
                gbSCL.Visible = true;
                cbTestMultisensor.Checked = ModuleInfo.SCLTest;
                cbResHiLow.Checked = ModuleInfo.SCLTest_Resistor_High;
            }
            cbClearPulse.Checked = ModuleInfo.PulseRun;
            cbVPulseHiLo.Checked = ModuleInfo.Pulse_v_high;
        }
    }
}

