using System.Drawing;
using System.Windows.Forms;
using FeedbackDataLib.Modules;

namespace FeedbackDataLib_GUI
{
    public partial class ucModuleSpecificSetup_AtemIR : UserControl
    {
        public ucModuleSpecificSetup_AtemIR()
        {
            InitializeComponent();
            this.toolTip1.SetToolTip(this.txtt_calc_new_scaling_ms, "The interval in [ms] after which the amplification is checked");
            this.toolTip1.SetToolTip(this.txtt_max_overload_time_ms, "The time [ms] for which the Sensor has to be in sturation before a new calibration proceure is initiated");
            this.toolTip1.SetToolTip(this.txtt_inOverload_time_ms, "The time [ms] for which the Sensor recalibrates in overload mode");
            this.toolTip1.SetToolTip(this.txtpost_shift_value, "Amplification as exponent of 2 ... -2 -> 1/4, -1 -> 1/2, 0 -> 1, 1 -> 2, 2-> 2, ...");
            this.toolTip1.SetToolTip(this.txtMovingAVG_Buffersize_asPowerof2, "Moving average buffer size as exponen of 2 ... 10 -> 1024");
            this.toolTip1.SetToolTip(this.txtMovingAVG_Buffersize_overload_asPowerof2, "Moving average buffer size as exponen of 2 in overload mode 6 -> 64");
            this.toolTip1.SetToolTip(this.txtILED, "Current through IR Leds, x10 [mA]");
        }

        /*
        protected override 
        {
            base.Update_ModuleInfo();
            if (txtt_calc_new_scaling_ms.DataBindings.Count == 0)
            {
                txtt_calc_new_scaling_ms.DataBindings.Add("Text", ModuleInfo, "AtemIR_t_calc_new_scaling_ms", true);
                txtt_max_overload_time_ms.DataBindings.Add("Text", ModuleInfo, "AtemIR_t_max_overload_time_ms", true);
                txtt_inOverload_time_ms.DataBindings.Add("Text", ModuleInfo, "AtemIR_t_inOverload_time_ms", true);
                txtpost_shift_value.DataBindings.Add("Text", ModuleInfo, "AtemIR_post_shift_value", true);
                txtMovingAVG_Buffersize_asPowerof2.DataBindings.Add("Text", ModuleInfo, "AtemIR_MovingAVG_Max_Buffersize_asPowerof2", true);
                txtMovingAVG_Buffersize_overload_asPowerof2.DataBindings.Add("Text", ModuleInfo, "AtemIR_MovingAVG_Buffersize_overload_asPowerof2", true);
            }
            
            txtt_calc_new_scaling_ms.Update();
        }*/

        public  void ReadModuleSpecificInfo(ref CModuleRespI ModuleInfo)
        {
          
            ModuleInfo.AtemIR_MovingAVG_Buffersize_overload_asPowerof2 = Convert.ToByte(ProcessTextBox(txtMovingAVG_Buffersize_overload_asPowerof2));
            //ModuleInfo.AtemIR_MovingAVG_Current_Buffersize_asPowerof2= Convert.ToByte(ProcessTextBox(txt_MovingAVG_Current_Buffersize_asPowerof2));
            ModuleInfo.AtemIR_MovingAVG_Max_Buffersize_asPowerof2= Convert.ToByte(ProcessTextBox(txtMovingAVG_Buffersize_asPowerof2));
            ModuleInfo.AtemIR_post_shift_value= Convert.ToInt16(ProcessTextBox(txtpost_shift_value));
            ModuleInfo.AtemIR_t_calc_new_scaling_ms= Convert.ToUInt16(ProcessTextBox(txtt_calc_new_scaling_ms));
            ModuleInfo.AtemIR_t_inOverload_time_ms= Convert.ToUInt16(ProcessTextBox(txtt_inOverload_time_ms));
            ModuleInfo.AtemIR_t_max_overload_time_ms= Convert.ToUInt16(ProcessTextBox(txtt_max_overload_time_ms));
            ModuleInfo.AtemIR_ILED_10 = Convert.ToByte(ProcessTextBox(txtILED));

        }

        public void UpdateModuleInfo(CModuleRespI ModuleInfo)
        {
            txtMovingAVG_Buffersize_asPowerof2.Text = ModuleInfo.AtemIR_MovingAVG_Max_Buffersize_asPowerof2.ToString();
            //ModuleInfo.AtemIR_MovingAVG_Current_Buffersize_asPowerof2= Convert.ToByte(txt_MovingAVG_Current_Buffersize_asPowerof2;
            txtMovingAVG_Buffersize_overload_asPowerof2.Text = ModuleInfo.AtemIR_MovingAVG_Buffersize_overload_asPowerof2.ToString();
            txtpost_shift_value.Text = ModuleInfo.AtemIR_post_shift_value.ToString();
            txtt_calc_new_scaling_ms.Text = ModuleInfo.AtemIR_t_calc_new_scaling_ms.ToString();
            txtt_inOverload_time_ms.Text = ModuleInfo.AtemIR_t_inOverload_time_ms.ToString();
            txtt_max_overload_time_ms.Text = ModuleInfo.AtemIR_t_max_overload_time_ms.ToString();
            txtILED.Text = ModuleInfo.AtemIR_ILED_10.ToString();
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
                ret = Int32.MinValue;
            }
            return ret;
        }

    }
}
