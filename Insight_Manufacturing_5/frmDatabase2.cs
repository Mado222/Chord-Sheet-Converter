using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters;

namespace Insight_Manufacturing5_net8
{
    public partial class frmDatabase2 : Form
    {
        private NeurodevicesTableAdapter neurodevicesTableAdapter;
        private dsManufacturing dsManufacturing;

        public frmDatabase2()
        {
            InitializeComponent();

            // Initialize the DataSet
            dsManufacturing = new dsManufacturing();

            // Initialize the TableAdapter
            neurodevicesTableAdapter = new NeurodevicesTableAdapter();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void frmDatabase2_Load(object sender, EventArgs e)
        {
            // Fill the DataTable within dsManufacturing using the TableAdapter
            neurodevicesTableAdapter.Fill(dsManufacturing.Neurodevices);

            // Bind the DataGridView to the DataTable
            dataGridView1.DataSource = dsManufacturing.Neurodevices;
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the index of the currently selected row in the DataGridView
                int index = dataGridView1.SelectedRows[0].Index;

                // Assuming the primary key column is named "ID" and it's the first column
                //int id = (int)dataGridView1.SelectedRows[0].Cells[0].Value;

                // Remove the row from the DataTable
                dsManufacturing.Neurodevices.Rows[index].Delete();
                //dsManufacturing.Neurodevices.AcceptChanges();

                // Update the database
                neurodevicesTableAdapter.Update(dsManufacturing.Neurodevices);

                // Optionally, refresh the DataGridView
                neurodevicesTableAdapter.Fill(dsManufacturing.Neurodevices);
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }
    }
}
