using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System.Reflection;

namespace FeedbackDataLib_GUI
{
    public partial class frmSpectrum : Form
    {
        private PlotView[] chartViews;
        private Color[] cols = new Color[] { Color.MintCream, Color.MintCream, Color.MintCream };
        public frmSpectrum(int no_of_Charts = 2)
        {
            InitializeComponent();
            init_Charts(no_of_Charts);
        }

        private void init_Charts(int numberOfCharts)
        {
            SuspendLayout();
            // Initialize the array of PlotViews
            chartViews = new PlotView[numberOfCharts];

            // Create multiple charts
            for (int i = 0; i < numberOfCharts; i++)
            {
                // Create a new plot model
                var plotModel = new PlotModel { Title = $"FFT Channel {i}" };

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Key = "y1"
                };

                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    MinimumPadding = 0,
                    MaximumPadding = 0.06,
                    AbsoluteMinimum = 0,
                    Key = "x1"
                };

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(valueAxis);

                // Create a new bar series
                var s1 = new BarSeries
                {
                    Title = "Series 1",
                    StrokeColor = OxyColors.Black,
                    FillColor = OxyColors.Coral,
                    StrokeThickness = 1,
                    XAxisKey = "x1",
                    YAxisKey = "y1"
                };

                s1.Items.Add(new BarItem { Value = i });
                s1.Items.Add(new BarItem { Value = i + 5 });
                //s1.LabelPlacement = LabelPlacement.Inside;
                //s1.LabelFormatString = "{0}";

                categoryAxis.Labels.Add("A");
                categoryAxis.Labels.Add("B");

                plotModel.Series.Add(s1);

                // Create a PlotView, assign the model, and add it to the table
                chartViews[i] = new PlotView
                {
                    Model = plotModel,
                    Dock = DockStyle.Fill,
                    BackColor = cols[i],
                };
                tableLayoutPanel1.Controls.Add(chartViews[i], 0, i);
            }
            ResumeLayout(false);
        }

        public void UpdateChartValues(int chartIndex, double[] newData)
        {
            var plotModel = chartViews[chartIndex].Model;
            if (plotModel.Series[0] is BarSeries series)
            {
                for (int i = 0; i < series.Items.Count; i++)
                {
                    series.Items[i].Value = newData[i];
                }
            }
        }

        public void UpdateXAxisCategories(int chartIndex, string[] newCategories)
        {
            if (chartIndex < 0 || chartIndex >= chartViews.Length)
            {
                throw new ArgumentException("Invalid chart index");
            }

            var plotModel = chartViews[chartIndex].Model;
            var categoryAxis = plotModel.Axes[0] as CategoryAxis; // Assuming the first axis is the CategoryAxis

            if (categoryAxis == null)
            {
                throw new InvalidOperationException("The specified axis is not a CategoryAxis.");
            }

            // Clear the existing categories
            categoryAxis.Labels.Clear();

            // Add new categories
            foreach (var category in newCategories)
            {
                categoryAxis.Labels.Add(category);
            }

            BarSeries? series = plotModel.Series[0] as BarSeries;
            if (series != null)
            {
                series.Items.Clear();
                for (int i = 0; i < newCategories.Length; i++)
                {
                    series.Items.Add(new BarItem { Value = 0 });
                }
            }
        }
    }
}
