using ComponentsLib_GUI;

namespace Neuromaster_V5
{
    public partial class frmSaveData : Form
    {
        public frmSaveData()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            saveFileDialog_data.FileName = System.IO.Path.GetFileName(txtPath.Text);
            saveFileDialog_data.InitialDirectory = System.IO.Path.GetDirectoryName(txtPath.Text);
            if (saveFileDialog_data.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = saveFileDialog_data.FileName;
            }
        }

        public delegate void SavingHandler();
        public event SavingHandler StartSaving;
        protected virtual void OnStartSaving()
        {
            StartSaving?.Invoke();
        }

        public event SavingHandler StopSaving;
        protected virtual void OnStopSaving()
        {
            StopSaving?.Invoke();
        }

        private void ctbSaving_ToState1(object sender, EventArgs e)
        {
            OnStopSaving();
        }

        private void ctbSaving_ToState2(object sender, EventArgs e)
        {
            if (File.Exists(txtPath.Text))
            {
                MessageBox.Show("FILE EXISTS", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ((CToggleButton)sender).AcceptChange = false;
            }
            else
            {
                OnStartSaving();
            }
        }

        private void frmSaveData_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
