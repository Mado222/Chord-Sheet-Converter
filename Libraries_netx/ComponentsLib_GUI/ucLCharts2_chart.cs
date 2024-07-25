using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.Versioning;
using WindControlLib;



namespace ComponentsLib_GUI
{
    [ToolboxItem(true)]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    [SupportedOSPlatform("windows")]
    public partial class ucLCharts2_chart : UserControl
    {
        private List<CartesianChart> charts = [];
        private List<ObservableCollection<ObservablePoint>> chartData = [];
        private System.Timers.Timer displayRefreshTimer;
        private CFifoBuffer<CYvsTimeData> [] data_in;

        /********************** Properties ***********************/
        private int _numberOfCharts = 2; // Set this to desired number of charts

        [Category("Chart Settings")]
        [Description("Sets the number of tracks / charts")]
        [DefaultValue(typeof(int), "4")]
        public int numberOfCharts
        {
            get => _numberOfCharts;
            set
            {
                _numberOfCharts = value;
                InitializeCharts(_numberOfCharts);
            }
        }

        private TimeSpan tsxLength = new(0, 0, 20);

        [Category("Chart Settings")]
        [Description("Sets the length of the x-Axis [s]")]
        [DefaultValue(typeof(int), "20")]
        public int xLength_s
        {
            get { return (int)tsxLength.TotalSeconds; }
            set { tsxLength = new TimeSpan(0, 0, value);
                InitializeCharts(_numberOfCharts);
            }
        }

        private Color defaultBackColor = Color.Beige;
        [Category("Chart Settings")]
        [Description("Sets the display ChartColot")]
        [DefaultValue(typeof(Color), "Color.Beige")]
        [SupportedOSPlatform("windows")]
        public Color ChartBackColor
        {
            get => BackColor;
            set => BackColor = value;
        }

        [Category("Chart Settings")]
        [Description("Refresh rate of the Display - not really useful")]
        [DefaultValue(typeof(int), "200")]
        public int RefreshRate_ms { get; set; } = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="ucLCharts2_chart"/> class.
        /// </summary>
        public ucLCharts2_chart()
        {
            InitializeComponent();
            displayRefreshTimer = new System.Timers.Timer(RefreshRate_ms); // Refresh rate of 100ms
            displayRefreshTimer.Elapsed += DisplayRefreshTimer_Elapsed;
            data_in = new CFifoBuffer<CYvsTimeData>[numberOfCharts];

            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                FillWithRandomData();
            }
            else
            {
                InitializeCharts(_numberOfCharts);
            }
        }

        public void ScaleChartY(int chartIndex, double ymax, double ymin)
        {
            if (chartIndex < 0 || chartIndex >= charts.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(chartIndex), "Chart index is out of range.");
            }

            var chart = charts[chartIndex];

            // Ensure the chart has a Y-axis configured.
            if (chart.YAxes.Count() > 0)
            {
                // Set the minimum and maximum values for the Y-axis.
                chart.YAxes.ElementAt(0).MinLimit = ymin;
                chart.YAxes.ElementAt(0).MaxLimit = ymax;

                // Set the labeler function based on ymax or other conditions
                if (ymax > 1000000)
                    chart.YAxes.ElementAt(0).Labeler = value => (value / 1000000).ToString("0.##") + "M";
                else if (ymax > 1000)
                    chart.YAxes.ElementAt(0).Labeler = value => (value / 1000).ToString("0.##") + "k";
                else if (ymax > 1)
                    chart.YAxes.ElementAt(0).Labeler = value => value.ToString("0.##") + "";
                else if (ymax > 0.001)
                    chart.YAxes.ElementAt(0).Labeler = value => (value * 1000).ToString("0.##") + "m";
                else
                    chart.YAxes.ElementAt(0).Labeler = value => (value * 1000000).ToString("0.##") + "µ";
            }
            else
            {
                throw new InvalidOperationException("No Y-axis found for the specified chart.");
            }
        }


        private void DisplayRefreshTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var endTime = DateTime.Now;
            var startTime = endTime - tsxLength;

            try
            {
                Invoke(new Action(() =>
                {
                    for (int i = 0; i < charts.Count; i++)
                    {
                        UpdateChartDisplay(i, startTime.Ticks, endTime.Ticks);

                    }
                }));
            }
            catch { }
        }

        private void UpdateChartDisplay(int chartIndex, long startTime_ticks, long endTime_ticks)//, double interpolatedValue)
        {
            if (chartIndex < charts.Count && chartIndex < data_in.Length)
            {
                if (data_in[chartIndex].Count > 0)
                {
                    CYvsTimeData[] alldataobjects = data_in[chartIndex].PopAll();
                    if (alldataobjects.Length > 0)
                    {
                        double d = 0;
                        int numvals = 0;
                        for (int i = 0; i < alldataobjects.Length; i++)
                        {
                            if (alldataobjects[i] is not null)
                            {
                                d += alldataobjects[i].yData[0];
                                numvals++;
                            }
                        }
                        if (numvals != 0)
                            d /= alldataobjects.Length;
                        else
                            return;

                        var series = charts[chartIndex].Series.FirstOrDefault() as LineSeries<ObservablePoint>;

                        if (series is not null && series.Values is not null)
                        {
                            ObservableCollection<ObservablePoint>? observablePoints = series.Values as ObservableCollection<ObservablePoint>;
                            observablePoints?.Add(new ObservablePoint(endTime_ticks, d));
                        }
                    }

                    if (charts[chartIndex].Series != null)
                    {
                        var series = charts[chartIndex].Series.FirstOrDefault() as LineSeries<ObservablePoint>;

                        if (series is not null && series.Values is not null)
                        {
                            ObservableCollection<ObservablePoint>? observablePoints = series.Values as ObservableCollection<ObservablePoint>;

                            if (observablePoints is not null)
                            {
                                while (observablePoints.Count > 0 && observablePoints[0].X < startTime_ticks)
                                    observablePoints.RemoveAt(0);
                            }
                            var xAxis = charts[chartIndex].XAxes.First();
                            xAxis.MinLimit = startTime_ticks;
                            xAxis.MaxLimit = endTime_ticks;
                        }
                        charts[chartIndex].Update();
                    }
                }
            }
        }

        public void AddPoint(int chartIndex, DateTime x, double y)
        {
            data_in[chartIndex].Push(new CYvsTimeData(x, y));
            Start();
        }

        public void AddPoint(int chartIndex, CYvsTimeData cYvsTime)
        {
            AddPoint(chartIndex, cYvsTime.xData, cYvsTime.yData[0]);
        }

        public void AddPoint(int chartIndex, CDataIn cData)
        {
            AddPoint(chartIndex, cData.DT_absolute, cData.Value);
        }

        public void Start()
        {
            
            endisCharts(true);
            displayRefreshTimer.Start();
        }

        public void Stop()
        {
            displayRefreshTimer.Stop();
            endisCharts(false);
        }

        private void endisCharts(bool enable)
        {
            //charts.ForEach(chart => chart.AutoUpdateEnabled = enable);
            if (charts is not null && charts.Count > 0)
            {
                foreach (var chart in charts)
                {
                    chart.AutoUpdateEnabled = enable;
                }
            }
        }


        private void InitializeCharts(int numberOfCharts)
        {
            Stop();
            if (numberOfCharts == 0) { numberOfCharts = 1; }
            chartData = new List<ObservableCollection<ObservablePoint>>(numberOfCharts);

            SuspendLayout();
            // Clear existing charts and their associated data
            foreach (var chart in charts)
            {
                chart.Dispose(); // Properly dispose of each chart
            }
            charts.Clear(); // Clear the list of charts

            //Update tableLayoutPanel1
            tableLayoutPanel1.Controls.Clear(); // Clear existing controls
            tableLayoutPanel1.RowStyles.Clear(); // Clear existing row styles
            tableLayoutPanel1.RowCount = numberOfCharts; // Set the number of rows
            tableLayoutPanel1.ColumnCount = 1; // Ensure there is at least one column
            tableLayoutPanel1.Dock = DockStyle.Fill;

            for (int i = 0; i < numberOfCharts; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / numberOfCharts));
                var dataCollection = new ObservableCollection<ObservablePoint>();
                chartData.Add(dataCollection);
                var chart = CreateChart(dataCollection, SKColors.Blue, i == numberOfCharts - 1); //Gives true or false
                charts.Add(chart);
                chart.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(chart, 0, i);
            }
            ResumeLayout(false);

            // Calculate display timer resolution
            displayRefreshTimer.Interval =  (xLength_s*1000) / charts[0].Width;

            
            //Buffers for incoming data
            data_in = new CFifoBuffer<CYvsTimeData>[numberOfCharts];
            for (int i = 0;i < numberOfCharts;i++)
            {
                data_in[i] = new CFifoBuffer<CYvsTimeData>();   
                data_in[i].Push(new CYvsTimeData(DateTime.Now, 0));
            }
            Start();
        }

        private CartesianChart CreateChart(ObservableCollection<ObservablePoint> values, SKColor linecol, bool showXAxis = false)
        {
            showXAxis = true;
            var chart = new CartesianChart
            {
                AutoUpdateEnabled = false,
                BackColor = defaultBackColor,
                Series =
                [
            new LineSeries<ObservablePoint>
            {
                Values = values,
                LineSmoothness = 0,
                GeometrySize = 0,
                Fill = null,
                Stroke = new SolidColorPaint(linecol, 2),
                AnimationsSpeed = TimeSpan.Zero
            }
                ],
                XAxes =
        [
            new Axis
            {
                IsVisible = showXAxis,
                ShowSeparatorLines = showXAxis, // Assuming you always want to hide separator lines, adjust if needed
                LabelsRotation = showXAxis ? 15 : 0, // Conditionally set rotation
                Labeler = showXAxis ? value => new DateTime((long)value).ToString("mm:ss") : value => "",
                UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                MinStep = TimeSpan.FromSeconds(5).Ticks,
                MinZoomDelta = 10000000, // This sets the minimum zoom delta, adjust this value based on trial and error
        }
        ],
                YAxes =
        [
            new Axis
            {
                // Do not set MinLimit or MaxLimit to enable automatic scaling
                //MinLimit = -1,
                //MaxLimit = 1
            }
        ]
            };
            return chart;
        }

        private void FillWithRandomData()
        {
            var random = new Random();
            foreach (var collection in chartData)
            {
                for (int i = 0; i < 10; i++) // Fill each chart with 10 random data points
                {
                    long now = DateTime.Now.Ticks - TimeSpan.FromSeconds(10 - i).Ticks; // Simulate past 10 seconds
                    double value = random.NextDouble() * 2 - 1; // Generate random values between -1 and 1
                    collection.Add(new ObservablePoint(now, value));
                }
            }

            // Update axes limits to current time span
            var minX = DateTime.Now.Ticks - TimeSpan.FromSeconds(10).Ticks;
            var maxX = DateTime.Now.Ticks;
            UpdateAxesLimits(minX, maxX);
        }

        private void UpdateAxesLimits(long minX, long maxX)
        {
            foreach (var chart in charts)
            {
                var xAxis = chart.XAxes.First();
                xAxis.MinLimit = minX;
                xAxis.MaxLimit = maxX;
            }
        }

    }
}
