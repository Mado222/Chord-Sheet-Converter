using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8
{
    public partial class frmDatabase : Form
    {
        public frmDatabase()
        {
            InitializeComponent();
        }

        private void neurodevicesBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            Validate();
            neurodevicesBindingSource.EndEdit();
            //tableAdapterManager.Update(dsManufacturing.Neurodevices);
            neurodevicesTableAdapter.Update(dsManufacturing.Neurodevices);
        }

        private void frmDatabase_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dsManufacturing.Neurodevices' table. You can move, or remove it, as needed.
            neurodevicesTableAdapter.Fill(dsManufacturing.Neurodevices);

        }
    }
}
