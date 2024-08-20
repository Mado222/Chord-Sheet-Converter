using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.WindowsForms;
using OxyPlot.Series;
using FeedbackDataLib.Modules;
using static FeedbackDataLib.Modules.CModuleBase;

namespace FeedbackDataLib_GUI
{
    public partial class uc_Spectrum_Impedance : UserControl
    {
        private PlotView plotView1;

        public int ChanNo { get; }
        public Color ChanColor { get; }

        public uc_Spectrum_Impedance(int ChanNo, Color ChanColor)
        {
            this.ChanNo = ChanNo;
            this.ChanColor = ChanColor;

            InitializeComponent();
            SuspendLayout();

            lblTitle.Text = $"FFT Channel {ChanNo}";

            // Create a new plot model
            var plotModel = new PlotModel();//{ Title = $"FFT Channel {ChanNo}" };

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

            //s1.Items.Add(new BarItem { Value = 1 });
            //s1.Items.Add(new BarItem { Value = 1 + 5 });
            //s1.LabelPlacement = LabelPlacement.Inside;
            //s1.LabelFormatString = "{0}";

            categoryAxis.Labels.Add("A");
            categoryAxis.Labels.Add("B");

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
            tableLayoutPanel1.Controls.Add (plotView1, 0, tableLayoutPanel1.RowCount-1);
            tableLayoutPanel1.SetColumnSpan(plotView1, tableLayoutPanel1.ColumnCount);
            ResumeLayout(false);

        }

        public void UpdateChartValues(double[] newData, ExtraData<CModuleExGADS1294.EnTypeExtradat_ADS>[] extraData)
        {
            var plotModel = plotView1.Model;
            if (plotModel.Series[0] is BarSeries series)
            {
                for (int i = 0; i < series.Items.Count; i++)
                {
                    series.Items[i].Value = newData[i];
                }
            }
            mtbxn.Text = (extraData[(int)CModuleExGADS1294.EnTypeExtradat_ADS.ExRn].Value / 1000).ToString();
            mtbxp.Text = (extraData[(int)CModuleExGADS1294.EnTypeExtradat_ADS.ExRp].Value / 1000).ToString();
            mtbUel.Text = (extraData[(int)CModuleExGADS1294.EnTypeExtradat_ADS.ExUp].Value / 1000).ToString();
            plotView1.Refresh();
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
    }
}
