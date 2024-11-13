using FeedbackDataLib.Modules;

namespace FeedbackDataLibGUI
{
    public partial class frmSpectrum : Form
    {
        private readonly ucSpectrumImpedance[] spectrumViews;
        private readonly Color[] cols = [Color.MintCream, Color.MintCream, Color.MintCream, Color.MintCream];
        public frmSpectrum(int no_of_Charts = 4)
        {
            InitializeComponent();

            SuspendLayout();
            // Initialize the array of PlotViews
            spectrumViews = new ucSpectrumImpedance[no_of_Charts];

            // Create multiple charts
            for (int i = 0; i < no_of_Charts; i++)
            {
                spectrumViews[i] = new ucSpectrumImpedance(i, cols[i])
                {
                    Dock = DockStyle.Fill
                };
            }

            // Setting up the number of rows and columns in the TableLayoutPanel
            int totalControls = spectrumViews.Length;
            int columns = tableLayoutPanel1.ColumnCount;
            int rows = (int)Math.Ceiling((double)totalControls / columns);

            // Ensure the TableLayoutPanel has the correct number of rows
            tableLayoutPanel1.RowCount = rows;

            // Loop to add the controls to the TableLayoutPanel
            for (int i = 0; i < totalControls; i++)
            {
                // Calculate the row and column indices for the current control
                int row = i / columns;
                int column = i % columns;

                // Add the control to the TableLayoutPanel
                tableLayoutPanel1.Controls.Add(spectrumViews[i], column, row);
            }

            ResumeLayout(false);
        }

        public void UpdateChartValues(int chartIndex, double[] newData, CEEGElectrodeData electrodeData)
        {
            if (chartIndex < 0 || chartIndex >= spectrumViews.Length)
            {
                throw new ArgumentException("Invalid chart index");
            }
            spectrumViews[chartIndex].UpdateChartValues(newData, electrodeData);
        }

        public void UpdateXAxisCategories(int chartIndex, string[] newCategories)
        {
            if (chartIndex < 0 || chartIndex >= spectrumViews.Length)
            {
                throw new ArgumentException("Invalid chart index");
            }
            spectrumViews[chartIndex].UpdateXAxisCategories(newCategories);
        }
    }
}
