using System;
using System.Drawing;
using System.Windows.Forms;

namespace Insight_Manufacturing5_net8
{
    public partial class frm_image_text : Form
    {
        public frm_image_text(string title, string image_name, string text)
        {
            InitializeComponent();
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            pictureBox1.Image =  Image.FromFile(System.IO.Path.GetDirectoryName(strExeFilePath) + @"\images\"+image_name);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            richTextBox1.Text= text;
            Text = title;
        }

        public frm_image_text()
        {
            InitializeComponent();
        }

        private void frm_image_text_Load(object sender, EventArgs e)
        {

        }
    }
}
