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
    public partial class frmAmplitudeGain : Form
    {
        public frmAmplitudeGain()
        {
            InitializeComponent();
        }

        private void frmAmplitudeGain_Load(object sender, EventArgs e)
        {
            ClearChart();
        }

        public void ClearChart()
        {
            //if (chartAmplitudeGain != null)
            //chartAmplitudeGain.Series[0].Points.Clear();
        }


        public void UdpateAmplitudeGain(double f, double v_db)
        {
            //chartAmplitudeGain.Series[0].Points.AddXY(f, v_db);
        }
    }
}
