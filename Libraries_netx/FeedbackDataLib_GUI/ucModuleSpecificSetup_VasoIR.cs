using System;
using System.Drawing;
using System.Windows.Forms;
using FeedbackDataLib.Modules;

namespace FeedbackDataLib_GUI
{
    public partial class ucModuleSpecificSetup_VasoIR : UserControl
    {
        public ucModuleSpecificSetup_VasoIR()
        {
            InitializeComponent();
            this.toolTip1.SetToolTip(this.txtt_calc_new_scaling_ms, "The interval in [ms] after which the amplification is checked");
            this.toolTip1.SetToolTip(this.txtt_max_overload_time_ms, "The time [ms] for which the Sensor has to be in sturation before a new calibration proceure is initiated");
            //this.toolTip1.SetToolTip(this.txtt_inOverload_time_ms, "The time [ms] for which the Sensor recalibrates in overload mode");
            //this.toolTip1.SetToolTip(this.txtpost_shift_value, "Amplification as exponent of 2 ... -2 -> 1/4, -1 -> 1/2, 0 -> 1, 1 -> 2, 2-> 2, ...");
            this.toolTip1.SetToolTip(this.txtMovingAVG_Buffersize_asPowerof2, "Moving average buffer size as exponent of 2 ... 10 -> 1024");
            this.toolTip1.SetToolTip(this.txtMovingAVG_Buffersize_overload_asPowerof2, "Moving average buffer size as exponent of 2 in overload mode 6 -> 64");

            this.toolTip1.SetToolTip(this.txtAmpl_max, "Amplification maximal as exponent of 2");
            this.toolTip1.SetToolTip(this.txtAmpl_min, "Amplification minimal as exponent of 2");
            this.toolTip1.SetToolTip(this.txtAmpl_curr, "Amplification current as exponent of 2");
        }


        public void ReadModuleSpecificInfo(ref CModuleVasoIR ModuleInfo)
        {
           
            ModuleInfo.VasoIR_MovingAVG_Buffersize_overload_asPowerof2 = Convert.ToByte(ProcessTextBox(txtMovingAVG_Buffersize_overload_asPowerof2));
            ModuleInfo.VasoIR_MovingAVG_Max_Buffersize_asPowerof2= Convert.ToByte(ProcessTextBox(txtMovingAVG_Buffersize_asPowerof2));
            ModuleInfo.VasoIR_t_calc_new_scaling_ms= Convert.ToUInt16(ProcessTextBox(txtt_calc_new_scaling_ms));
            ModuleInfo.VasoIR_t_max_overload_time_ms= Convert.ToUInt16(ProcessTextBox(txtt_max_overload_time_ms));

            ModuleInfo.VasoIR_Max_ScalingFactor_asPowerof2 = Convert.ToByte(ProcessTextBox(txtAmpl_max));
            ModuleInfo.VasoIR_Min_ScalingFactor_asPowerof2 = Convert.ToByte(ProcessTextBox(txtAmpl_min));
            ModuleInfo.VasoIR_Current_scalingfactor_asPowerof2= Convert.ToByte(ProcessTextBox(txtAmpl_curr));

            ModuleInfo.VasoIR_led_current_for_proximity= Convert.ToByte(ProcessTextBox(txtLEDCurrent));
        }

        public void UpdateModuleInfo(CModuleVasoIR ModuleInfo)
        {
            txtMovingAVG_Buffersize_asPowerof2.Text = ModuleInfo.VasoIR_MovingAVG_Max_Buffersize_asPowerof2.ToString();
            txtMovingAVG_Buffersize_overload_asPowerof2.Text = ModuleInfo.VasoIR_MovingAVG_Buffersize_overload_asPowerof2.ToString();
            txtt_calc_new_scaling_ms.Text = ModuleInfo.VasoIR_t_calc_new_scaling_ms.ToString();
            txtt_max_overload_time_ms.Text = ModuleInfo.VasoIR_t_max_overload_time_ms.ToString();

            txtAmpl_max.Text = ModuleInfo.VasoIR_Max_ScalingFactor_asPowerof2.ToString();
            txtAmpl_min.Text = ModuleInfo.VasoIR_Min_ScalingFactor_asPowerof2.ToString();
            txtAmpl_curr.Text = ModuleInfo.VasoIR_Current_scalingfactor_asPowerof2.ToString();

            txtLEDCurrent.Text = ModuleInfo.VasoIR_led_current_for_proximity.ToString();
        }

        private int ProcessTextBox(TextBox box)
        {
            box.BackColor = Color.White;
            int ret;
            try
            {
                ret = Convert.ToInt32(box.Text);
            }
            catch
            {
                txtMovingAVG_Buffersize_asPowerof2.BackColor = Color.Red;
                ret = int.MinValue;
            }
            return ret;
        }

    }
}
