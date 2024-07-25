using ComponentsLib_GUI;
using FeedbackDataLib;
using static Insight_Manufacturing5_net8.tests_measurements.CBase_tests_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CAgainValues
    {
        public List<CAgainValue> AgainValues = new List<CAgainValue>();
        private List<double> _f = new List<double>();
        private List<double> _Vppin = new List<double>();
        private List<double> _MeasureTime_s = new List<double>();
        private List<int> _noSamplesIgnored_in_the_Beginning = new List<int>();
        private List<double> _Ueff_soll = new List<double>();
        private List<enMeasureType> _Measure_Type = new List<enMeasureType>();

        public double[] f { get => _f.ToArray(); }
        public double[] Vppin { get => _Vppin.ToArray(); }
        public double[] MeasureTime_s { get => _MeasureTime_s.ToArray(); }
        public int[] noSamplesIgnored_in_the_Beginning { get => _noSamplesIgnored_in_the_Beginning.ToArray(); }

        public int Count { get => AgainValues.Count; }

        public double[] Ueff_EEG_soll { get => _Ueff_soll.ToArray(); }
        public enMeasureType[] Measure_Type { get => _Measure_Type.ToArray(); }
        //public int num_Periodes_to_measure { get; private set; }

        private enumModuleType _ModuleType;

        private void add_noSamplesIgnored_in_the_Beginning (double MeasureTime_s, double SampleInt_ms, double Ignore_Time_percent= -1, int noSamplesIgnored_in_the_Beginning = -1)
        {
            if (noSamplesIgnored_in_the_Beginning >= 0)
                _noSamplesIgnored_in_the_Beginning.Add(noSamplesIgnored_in_the_Beginning);

            if (Ignore_Time_percent >= 0)
            {
                double d = Ignore_Time_percent / 100 * MeasureTime_s; //time to ignore
                d = d / SampleInt_ms * 1000;
                _noSamplesIgnored_in_the_Beginning.Add((int)d);
            }
        }

        public CAgainValues(enumModuleType ModuleType)
        {
            _ModuleType = ModuleType;
        }

        private double Eval_MeasureTime_s(double f_Again, double Measure_duration_s_or_periodes_neg)
        {
            double ret;
            if (Measure_duration_s_or_periodes_neg < 0)
            {
                ret = 1 / f_Again * Math.Abs(Measure_duration_s_or_periodes_neg);
            }
            else
                ret = Measure_duration_s_or_periodes_neg;

            if (ret < 1) ret = 1;       //shortest measure time = 1s
            
            _MeasureTime_s.Add(ret);
            return ret;
        }

        public void Add_Again(double f_Again, double vppin_Again, double again_soll_db, double againAgain_Tolerance_db,
            double Measure_duration_s_or_periodes_neg, double SampleInt_ms, double Ignore_Time_percent = -1, int noSamplesIgnored_in_the_Beginning = -1)
        {
            double MeasureTime_s = Eval_MeasureTime_s (f_Again, Measure_duration_s_or_periodes_neg);
            CAgainValue agv = new CAgainValue(f_Again, vppin_Again, _ModuleType, MeasureTime_s);
            agv.InitAgain(again_soll_db,againAgain_Tolerance_db);

            Add_vals(SampleInt_ms, Ignore_Time_percent, noSamplesIgnored_in_the_Beginning, MeasureTime_s, agv);
        }

        public void Add_Sinus(double f_Again, double vppin, double Ueff_soll, double sinus_tolerance_percent,
            double Measure_duration_s_or_periodes_neg, double SampleInt_ms, double Ignore_Time_percent = -1, int noSamplesIgnored_in_the_Beginning = -1)
        {
            double MeasureTime_s = Eval_MeasureTime_s(f_Again, Measure_duration_s_or_periodes_neg);
            CAgainValue agv = new CAgainValue(f_Again, vppin, _ModuleType, MeasureTime_s);
            agv.InitSinus(Ueff_soll, sinus_tolerance_percent);

            Add_vals(SampleInt_ms, Ignore_Time_percent, noSamplesIgnored_in_the_Beginning, MeasureTime_s, agv);
        }


        public void Add_Notch(double f_Again, double vppin_Again, double must_be_smaller_than,
            double Measure_duration_s_or_periodes_neg, double SampleInt_ms, double Ignore_Time_percent = -1, int noSamplesIgnored_in_the_Beginning = -1)
        {
            double MeasureTime_s = Eval_MeasureTime_s(f_Again, Measure_duration_s_or_periodes_neg);
            CAgainValue agv = new CAgainValue(f_Again, vppin_Again, _ModuleType, MeasureTime_s);
            agv.InitNotch(must_be_smaller_than);
            
            Add_vals(SampleInt_ms, Ignore_Time_percent, noSamplesIgnored_in_the_Beginning, MeasureTime_s, agv);
        }

        public void Add_Arbitrary(string ArbitraryFile, double f_Again, double vppin, double hr_bpm_soll, double Tolerance_percent,
            double Measure_duration_s_or_periodes_neg, double SampleInt_ms, double Ignore_Time_percent = -1, int noSamplesIgnored_in_the_Beginning = -1)
        {
            double MeasureTime_s = Eval_MeasureTime_s(f_Again, Measure_duration_s_or_periodes_neg);
            CAgainValue agv = new CAgainValue(f_Again, vppin, _ModuleType, MeasureTime_s);
            agv.InitArbitratry(ArbitraryFile, hr_bpm_soll, Tolerance_percent);

            Add_vals(SampleInt_ms, Ignore_Time_percent, noSamplesIgnored_in_the_Beginning, MeasureTime_s, agv);
        }

        private void Add_vals(double SampleInt_ms, double Ignore_Time_percent, 
            int noSamplesIgnored_in_the_Beginning, double MeasureTime_s, CAgainValue agv)
        {
            AgainValues.Add(agv);
            _f.Add(agv.f_Hz);
            _Vppin.Add(agv.Vppin);
            _Ueff_soll.Add(agv.Ueff_soll);
            _Measure_Type.Add(agv.MeasureType);
            add_noSamplesIgnored_in_the_Beginning(MeasureTime_s, SampleInt_ms, Ignore_Time_percent, noSamplesIgnored_in_the_Beginning);
        }
    }

    public class CAgainValue
    {
        public double f_Hz;
        public double Vppin;
        public double Again_soll_db;
        public double Again_Tolerance_db;
        public double Tolerance_percent;
        public double MeasureTime_s;
        public double Ueff_soll;
        public double Offset_V;
        public double hr_bpm_soll;
        public enMeasureType MeasureType;
        public enumModuleType ModuleType;
        public string ArbitraryFile { get; set; } = "";

        public CAgainValue(double f_Again,
            double vppin,
            enumModuleType ModuleType,
            double MeasureTime_s)
        {
            f_Hz = f_Again;
            Vppin = vppin;
            this.ModuleType = ModuleType;
            this.MeasureTime_s = MeasureTime_s;
        }
        public void InitAgain(double again_soll_db,double againAgain_Tolerance_db)
        {
            Again_soll_db = again_soll_db;
            Again_Tolerance_db = againAgain_Tolerance_db;
            double k = Math.Pow(10, Again_soll_db / 20);
            Ueff_soll = Get_Ueff_soll(Vppin, ModuleType)*k;

            double kplustol = Math.Pow(10, (Again_soll_db + againAgain_Tolerance_db ) / 20);
            double kminustol = Math.Pow(10, (Again_soll_db - againAgain_Tolerance_db) / 20);

            double Ueff_soll_plustol = Get_Ueff_soll(Vppin, ModuleType) * kplustol;
            double Ueff_soll_minustol = Get_Ueff_soll(Vppin, ModuleType) * kminustol;

            double plusProzent = Ueff_soll_plustol / (Ueff_soll / 100) -100;
            double minusProzent = 100 - Ueff_soll_minustol / (Ueff_soll / 100);
            Tolerance_percent = (plusProzent + minusProzent)/2;

            MeasureType = enMeasureType.Sinus;
        }

        public bool isValueOK_dB (double Usoll_db)
        {
            bool ret = true;
            if (MeasureType == enMeasureType.Sinus)
            {
                if ((Usoll_db > Again_soll_db + Again_Tolerance_db) ||
                    (Usoll_db < Again_soll_db - Again_Tolerance_db))
                {
                    //Todo agv.Again_Tolerance_db = 0
                    ret = false;
                }
            }
            else if (MeasureType == enMeasureType.Notch)
            {
                if (Usoll_db > Again_soll_db)
                {
                    ret = false;
                }
            }
            else
            {
                throw new InvalidOperationException("Not a valid measurement type configured");
            }
            return ret;
        }

        public bool isValueOK_Ueff_Percent(double Ueff_ist)
        {
            bool ret = false;
            if (MeasureType == enMeasureType.Sinus)
            {
                double max = Ueff_soll * (1 + Tolerance_percent / 100);
                double min = Ueff_soll * (1 - Tolerance_percent / 100);
                if ((Ueff_ist < max) && (Ueff_ist > min))
                    ret = true;
            }
            else if (MeasureType == enMeasureType.Notch)
            {
                //Notch frequency
                if (Ueff_ist < Math.Abs(Ueff_soll))
                    ret = true;
            }
            else
            {
                throw new InvalidOperationException("Not a valid measurement type configured");
            }
            return ret;
        }

            public void InitSinus(
            double Ueff_soll,
            double Tolerance_percent)
        {
            this.Ueff_soll = Ueff_soll;
            this.Tolerance_percent = Tolerance_percent;
            MeasureType = enMeasureType.Sinus;
        }

        public void InitNotch(
            double Ueff_soll)
        {
            this.Ueff_soll = Ueff_soll;
            Tolerance_percent = 0;
            MeasureType = enMeasureType.Notch;
        }
        public void InitArbitratry(
                string ArbitraryFile,
                double HR_bpm_soll,
                double Tolerance_percent)
        {
            this.ArbitraryFile = ArbitraryFile;
            Ueff_soll = Get_Ueff_soll(Vppin, ModuleType);
            this.Tolerance_percent = Tolerance_percent;
            hr_bpm_soll = HR_bpm_soll;
            MeasureType = enMeasureType.Arbitrary;
        }


/*        public CAgainValue(double f_Again,
            double vppin_Again,
            double measureTime_s,
            enumModuleType ModuleType,
            enMeasureType MeasureType) :
            this(f_Again, vppin_Again, ModuleType, measureTime_s)
        {
            MeasureTime_s = measureTime_s;
            this.MeasureType = MeasureType;
        }*/

        public bool SetGenerator(CFY6900 fY6900)
        {
            bool ret = false;
            if (fY6900.isOpen)
            {
                if (fY6900.SetOutput_Off(true))
                {
                    if (ArbitraryFile == "")
                    {
                        if (!fY6900.SetSinus(true))
                            return ret;
                    }
                    else
                    {
                        if (!fY6900.SetArbitrary(Convert.ToInt32(ArbitraryFile.Substring(0, 1)), true))
                            return ret;
                    }

                    if (fY6900.SetFrequency(f_Hz, true))
                        if (fY6900.SetVss(Vppin, true))
                            if (fY6900.SetOutput_On(true))
                                ret = true;
                }
            }
            return ret;
        }

    }
}
