using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight_Manufacturing5_net8
{
    public partial class frm_Multi_Temp : Form
    {
        public frm_Multi_Temp()
        {
            InitializeComponent();
        }

        //Event When 
        public event Multi_Temp_Closing_EventHandler Multi_Temp_Closing;

        public delegate void Multi_Temp_Closing_EventHandler(object sender, decimal Sollwert);
        protected virtual void OnMulti_Temp_Closing ()
        {
            Multi_Temp_Closing?.Invoke(this, nudTempSoll.Value);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            OnMulti_Temp_Closing();
            //Close();
            Hide();
        }

        public void SetIstTemperature(double Ist)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<double>(SetIstTemperature), Ist);
            }
            else
            {
                lblTempIst.Text = Ist.ToString("00.0");
            }
        }
    }
}
