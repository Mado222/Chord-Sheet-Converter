using FeedbackDataLib_GUI;
namespace Test_BarChart
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        frmSpectrum frmSpectrum = new frmSpectrum();

        private void button1_Click(object sender, EventArgs e)
        {
            frmSpectrum.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //category
            frmSpectrum.UpdateXAxisCategories(1, new string[] { "A", "B", "C" });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmSpectrum.UpdateChartValues(1, new double[] { 1, 2, 3 });
        }
    }
}
