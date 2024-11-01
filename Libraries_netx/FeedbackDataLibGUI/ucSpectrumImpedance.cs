using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using FeedbackDataLib.Modules;

namespace FeedbackDataLib_GUI
{
    public partial class ucSpectrumImpedance : UserControl
    {
        private PlotView plotView1;

        public int ChanNo { get; }
        public Color ChanColor { get; }

        public const int default_YMAX = 1000000;

        public ucSpectrumImpedance(int ChanNo, Color ChanColor)
        {
            this.ChanNo = ChanNo;
            this.ChanColor = ChanColor;

            InitializeComponent();
            SuspendLayout();

            lblTitle.Text = $"FFT Channel {ChanNo}";

            // Create a new plot model
            var plotModel = new PlotModel();//{ Title = $"FFT Channel {ChanNo}" };

            //Typically, in OxyPlot, BarSeries are plotted horizontally by default, meaning the bars
            //extend along the X - axis(bottom axis).Therefore, the CategoryAxis should be aligned
            //with the Y-axis(left axis) rather than the X-axis.

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
                Maximum = double.NaN, //= Autoscaling on
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

            //categoryAxis.Labels.Add("A");
            //categoryAxis.Labels.Add("B");

            plotModel.Series.Add(s1);

            // Create a PlotView, assign the model, and add it to the table
            plotView1 = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill,
                BackColor = ChanColor,
                Name = "plotView1",
                PanCursor = Cursors.Hand,
                //Size = new Size(367, 232),
                //TabIndex = 6,
                //Text = "plotView1",
                ZoomHorizontalCursor = Cursors.SizeWE,
                ZoomRectangleCursor = Cursors.SizeNWSE,
                ZoomVerticalCursor = Cursors.SizeNS,
                Visible = true
            };
            tableLayoutPanel1.Controls.Add(plotView1, 0, tableLayoutPanel1.RowCount - 1);
            tableLayoutPanel1.SetColumnSpan(plotView1, tableLayoutPanel1.ColumnCount);

            nudYmax.Maximum = 2 * default_YMAX;
            nudYmax.Value = default_YMAX;

            ResumeLayout(false);

        }

        public void UpdateChartValues(double[] newData, CEEGElectrodeData electrodeData)
        {
            var plotModel = plotView1.Model;
            if (plotModel.Series[0] is BarSeries series)
            {
                // Update existing items
                for (int i = 0; i < series.Items.Count; i++)
                {
                    series.Items[i].Value = newData[i] * 1000000;        //mV
                }
            }
            mtbxn.Text = (electrodeData.Rn / 1000).ToString("F0");
            mtbxp.Text = (electrodeData.Rp / 1000).ToString("F0");
            mtbUel.Text = (electrodeData.Uelectrode * 1e6).ToString("F0");
            tbUa1.Text = (electrodeData.Ua1 * 1e6).ToString("F1");
            tbUa2.Text = (electrodeData.Ua2 * 1e6).ToString("F1");

            plotModel.InvalidatePlot(true);
            if (cbAutoscale.Checked)
                nudYmax.Value = GetYMAX();

            //plotView1.Refresh();
        }

        public void UpdateXAxisCategories(string[] newCategories)
        {
            var plotModel = plotView1.Model;

            var categoryAxis = plotModel.Axes[0] as CategoryAxis ?? throw new InvalidOperationException("The specified axis is not a CategoryAxis."); // Assuming the first axis is the CategoryAxis

            // Clear the existing categories
            categoryAxis.Labels.Clear();

            // Add new categories
            foreach (string category in newCategories)
            {
                categoryAxis.Labels.Add(category);
            }

            if (plotModel.Series[0] is BarSeries series)
            {
                series.Items.Clear();
                for (int i = 0; i < newCategories.Length; i++)
                {
                    series.Items.Add(new BarItem { Value = 0 });
                }
            }
        }

        private void SetYMAX(double max)
        {
            var plotModel = plotView1.Model;
            var valueAxis = plotModel.Axes.FirstOrDefault(axis => axis.Key == "x1");
            if (valueAxis != null)
            {
                valueAxis.Maximum = max; // Set to your desired maximum value
            }
        }

        private int GetYMAX()
        {
            var plotModel = plotView1.Model;
            var valueAxis = plotModel.Axes.FirstOrDefault(axis => axis.Key == "x1");
            if (valueAxis != null)
            {
                return (int)valueAxis.ActualMaximum;
            }
            return 0;
        }

        private void nudYmax_ValueChanged(object sender, EventArgs e)
        {
            if (!cbAutoscale.Checked)
            {
                SetYMAX((int)nudYmax.Value);
                return;
            }
            SetYMAX(double.NaN);
        }

        private void cbAutoscale_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbAutoscale.Checked)
            {
                nudYmax.Value = GetYMAX();
                return;
            }
            SetYMAX(double.NaN);
        }
    }
}
