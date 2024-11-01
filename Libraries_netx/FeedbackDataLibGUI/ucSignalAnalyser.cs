using WindControlLib;

namespace FeedbackDataLib_GUI
{
    public partial class UcSignalAnalyser : UserControl
    {
        private CRingpuffer cache = new (1000);
        private readonly System.Windows.Forms.Timer updater = new ();
        private string unit = "_";

        private string _HeaderText = "";
        public string HeaderText
        {
            get { return _HeaderText; }
            set
            {
                _HeaderText = value;
                Setup_tlpMeasure();
                //Extract Unit
                unit = "_";
                if (value.Contains('[') && value.Contains(']'))
                {
                    string u = value.Split('[', ']')[1];
                    if (u != "")
                    {
                        unit = u;
                    }
                }
            }
        }

        public UcSignalAnalyser()
        {
            Init(0);
        }
        public UcSignalAnalyser(int cacheSizeSamples)
        {
            if (cacheSizeSamples != 1000)
                Init(cacheSizeSamples);
            else
                Init(0);
        }

        private void Init(int cache_size_samples = 0)
        {
            InitializeComponent();
            lblHeader.Text = _HeaderText;
            Setup_tlpMeasure();
            if (cache_size_samples > 0)
                cache = new CRingpuffer(cache_size_samples);
            updater.Tick += new EventHandler(Updater_Tick);
        }

        void Setup_tlpMeasure()
        {
            while (tlpMeasure.RowStyles.Count <= tlpMeasure.RowCount)
            {
                tlpMeasure.RowStyles.Add(new RowStyle(SizeType.Percent));
            }

            lblHeader.Text = _HeaderText;
            if (_HeaderText == "")
            {
                tlpMeasure.RowStyles[0].Height = 0;
                int d = Height / (tlpMeasure.RowCount - 1);

                for (int i = 1; i < tlpMeasure.RowStyles.Count; i++)
                {
                    tlpMeasure.RowStyles[i].Height = d;
                }
            }
            else
            {
                int d = Height / tlpMeasure.RowCount;

                for (int i = 0; i < tlpMeasure.RowStyles.Count; i++)
                {
                    tlpMeasure.RowStyles[i].Height = d;
                }
            }
        }

        private void Updater_Tick(object? sender, EventArgs e)
        {
            UpdateVals();
        }

        public void Add(double ci)
        {
            cache.Push(ci);
        }

        public void UpdateVals()
        {
            double[] l = new double[1];
            cache.PopAll(ref l);

            double ueff = 0;
            double uplus = 0;
            double uminus = 0;
            double umean = 0;

            for (int i = 0; i < l.Length; i++)
            {
                ueff += Math.Pow(l[i], 2);
                if (l[i] > uplus) uplus = l[i];
                if (l[i] < uminus) uminus = l[i];
                umean += l[i];
            }
            ueff = Math.Sqrt(ueff / l.Length);
            if (l.Length != 0)
            {
                umean /= l.Length;
                lblValeff.Text = CMyTools.Format_with_SI_prefixes(ueff, unit + "eff");
                lblValmean.Text = CMyTools.Format_with_SI_prefixes(umean, unit + "mean");
                lblpeakplusValue.Text = CMyTools.Format_with_SI_prefixes(uplus, unit + "+p");
                lblpeakminusValue.Text = CMyTools.Format_with_SI_prefixes(uminus, unit + "-p");
            }
            cache.Clear();
        }

        public void Autoupdate(int timeMs)
        {
            updater.Stop();
            updater.Interval = timeMs;
            updater.Start();
        }


    }
}


