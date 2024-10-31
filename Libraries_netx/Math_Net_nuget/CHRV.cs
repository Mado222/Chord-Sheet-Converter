using WindControlLib;
using ComponentsLib_GUI;

namespace MathNetNuget
{
    /// <summary>
    /// Class, using Math_net functions (Math.net Library (http://www.mathdotnet.com/) to calculate HRV related parameters
    /// 
    /// 22.11.2012
    /// * Introduction of Calibration_factor_RSA_LF_VLF_ULF
    /// * New function: public void Set_IPI(double Hanninng_Percentage, double[] IPI, double[] IPI_time)
    /// * SetIPI uses Default_Hanning_Percentage_in_SetIPI
    /// 
    /// 21.4.2013
    /// * Introduction of Set_IPI_s(double[] IPI_s)
    /// * Introduction of Rebuild_IPI_time_s_from_IPI(double[] IPI_s)
    /// 
    /// 20.5.2013
    /// * In Set_IPI_s auf 0er in IPI_s prüfen, wegnehmen und damit _RRMean_ms und _HRMean_bpm berechnen
    /// * hinzufügen:
    /// public int Generic_IPI_Text_File_Importer(string pathToFile, int colNumIPI, enumIPIDataType IPIDataType, enumIPITimeType IPITimeType, ref double[] IPI_s, ref DateTime[] IPI_time, int numHeaderRows, ref string Header, int IPImin_ms, int IPImax_ms)
    /// 
    /// * Import_IPI_s_Data überarbeitet ... siehe OneNote "Fehlererkennung im Eingangsfile"
    /// 
    /// </summary>
    /// <remarks>
    /// see also: Kubios_HRV_Users_Guide.pdf
    /// </remarks>
    public class CHRV
    {
        private readonly CFFTMathNet FFT_MathNet;
        private double hRMean_Stdv;
        private double hRMean_bpm;
        private double rMSSD_ms;
        private int nN50;
        private double pNN50;
        private double rRMean_ms;
        private double rRMean_Stdv;

        /// <summary>
        /// 22.11.2012
        /// RSA, LF, VLF, ULF are multiplied with this factor
        /// to meet the refernece system BioSign
        /// see: "d:\Daten\Insight\HRV Test Data\30min testung\hrvscanner_hrv-auswertung.pdf" 
        /// 
        /// März 2013
        /// Entscheidung Dieter/Insight nehmen wir diesen Faktor wieder heraus
        /// </summary>
        //private const double Calibration_factor_RSA_LF_VLF_ULF = 0.4;
        private const double Calibration_factor_RSA_LF_VLF_ULF = 1;

        /// <summary>
        /// 22.11.2012
        /// Default_ hanning_ percentage_in_ SetIPI
        /// </summary>
        private const int Default_Hanning_Percentage_in_SetIPI = 2;

        /// <summary>
        /// Only IPIs above this value are valid
        /// </summary>
        private const double IPImin_ms = 300;  //200bpm
        private const double IPImin_s = IPImin_ms / 1000;
        /// <summary>
        /// Only IPIs below this value are valid
        /// </summary>
        private const double IPImax_ms = 2000;   //30bpm
        private const double IPImax_s = IPImax_ms / 1000;




        /// <summary>
        /// Initializes a new instance of the <see cref="CHRV"/> class.
        /// </summary>
        public CHRV()
        {
            FFT_MathNet = new CFFTMathNet();

            RSA = new CFrequencyRange(0.15, 0.4, Calibration_factor_RSA_LF_VLF_ULF);
            LF = new CFrequencyRange(0.04, 0.15, Calibration_factor_RSA_LF_VLF_ULF);
            VLF = new CFrequencyRange(0.003, 0.04, Calibration_factor_RSA_LF_VLF_ULF);
            ULF = new CFrequencyRange(0, 0.003, Calibration_factor_RSA_LF_VLF_ULF);
        }


        /// <summary>
        /// File that was loaded last
        /// </summary>
        public string LastTextfielPath { get; private set; } = "";

        /// <summary>
        /// Gets the RESAMPLED data
        /// </summary>
        /// <remarks>
        /// Set_RR_Inter_Beat_Interval must run first
        /// In practice, the NN and RR intervals appear to be the same and, thus, the term RR is preferred here.
        /// </remarks>
        public double[] IPIResampled => FFT_MathNet.Data_y;

        /// <summary>
        /// Gets related time to Resampled_RR_Inter_Beat_Interval
        /// </summary>
        /// <remarks>
        /// Set_RR_Inter_Beat_Interval must run first
        /// </remarks>
        public double[] IPITimeResampled => FFT_MathNet.Data_x;

        /// <summary>
        /// Gets the FFT amplitude of the RR_Inter_Beat_Interval/Data_x Dataset
        /// </summary>
        /// <remarks>
        /// ProcessIPI must run first
        /// </remarks>
        public double[] FftRRAmplitude => FFT_MathNet.FftAmplitudePeak;

        /// <summary>
        /// Gets the FFT power of the RR_Inter_Beat_Interval/Data_x Dataset
        /// </summary>
        public double[] FftRRPower => FFT_MathNet.FftAmplitudePower;

        /// <summary>
        /// Gets the FFT Frequency of the RR_Inter_Beat_Interval/Data_x Dataset
        /// </summary>
        /// <remarks>
        /// ProcessIPI must run first
        /// </remarks>
        public double[] FftRRFrequ => FFT_MathNet.FftFrequ;

        /// <summary>
        /// DC of the FFT
        /// </summary>
        /// <remarks>
        /// ProcessIPI must run first, independent of SupressDC parameter
        /// </remarks>
        public double FftDC { get; private set; }

        /// <summary>
        /// Power in the RR curve with DC
        /// </summary>
        /// <remarks>
        /// ProcessIPI must run first, independent of SupressDC parameter
        /// </remarks>
        public double FftRRPowerWithDC => FFT_MathNet.GetPower(FftRRFrequ[^1], FftRRFrequ[0]);

        /// <summary>
        /// Power in the RR curve without DC
        /// </summary>
        /// <remarks>
        /// ProcessIPI must run first, independent of SupressDC parameter
        /// </remarks>
        public double FftRRPowerNoDC => FFT_MathNet.GetPower(FftRRFrequ[^1], FftRRFrequ[1]);

        /// <summary>
        /// Gets the RSA (Respiratory Sinus Arrhythmia), Power [ms^2]
        /// = HF
        /// </summary>
        public CFrequencyRange RSA { get; }

        /// <summary>
        /// Gets the LF, Power [ms^2]
        /// </summary>
        public CFrequencyRange LF { get; }

        /// <summary>
        /// Gets the VLF, Power [ms^2]
        /// </summary>
        public CFrequencyRange VLF { get; }

        /// <summary>
        /// Gets the ULF, Power [ms^2]
        /// </summary>
        public CFrequencyRange ULF { get; }


        /// <summary>
        /// Mean RR intervall [ms]
        /// </summary>
        public double RRMeanMs { get => rRMean_ms; private set => rRMean_ms = value; }

        /// <summary>
        /// Standard deviation of RRMean_ms
        /// </summary>
        public double RRMeanStdv { get => rRMean_Stdv; private set => rRMean_Stdv = value; }

        /// <summary>
        /// Mean HR [bpm]
        /// </summary>
        public double HRMeanBpm { get => hRMean_bpm; private set => hRMean_bpm = value; }

        /// <summary>
        /// Standard deviation of HRMean_bpm
        /// </summary>
        public double HRMeanStdv { get => hRMean_Stdv; private set => hRMean_Stdv = value; }

        public double[] Breath
        {
            get { return FFT_MathNet.Data_y; }
            set { FFT_MathNet.Data_y = value; }
        }

        /// <summary>
        /// RMSSD [ms]
        /// </summary>
        public double RMSSDMs { get => rMSSD_ms; private set => rMSSD_ms = value; }

        /// <summary>
        /// NN50.
        /// </summary>
        public int NN50 { get => nN50; private set => nN50 = value; }

        /// <summary>
        /// pNN50.
        /// </summary>
        public double PNN50 { get => pNN50; private set => pNN50 = value; }

        /// <summary>
        /// Processes IPI and IPI_time with standard settings for HRV (SupressDC= true)
        /// </summary>
        /// <remarks>
        /// Calls ProcessIPI(true)
        /// </remarks>
        public void ProcessIPI()
        {
            ProcessIPI(true);
        }

        /// <summary>
        /// convert BPM [1/min] to IPI_s [s]
        /// <remarks>
        /// WARNING: BPM= 0 will NOT be set to infinit!! It stays 0 since this is a marker for "no value"
        /// </remarks>
        /// </summary>
        /// <param name="BPM">BPM</param>
        public static void BPMToIPIS(ref double[] BPM)
        {
            for (int i = 0; i < BPM.Length; i++)
            {
                if (BPM[i] != 0)
                {
                    BPM[i] = 60 / BPM[i];
                }
            }
        }

        /// <summary>
        /// Converts BPM to seconds and removes 0 values
        /// </summary>
        /// <param name="BPM">BPMs</param>
        /// <param name="RelatedTime">Realted time, can be null</param>
        public static void BPMToIPISRemove0s(ref List<double> BPM, ref List<DateTime> RelatedTime)
        {
            for (int i = BPM.Count - 1; i > -1; i--)
            {
                if (BPM[i] != 0)
                {
                    BPM[i] = 60 / BPM[i];
                }
                else
                {
                    //Remove value
                    BPM.RemoveAt(i);
                    RelatedTime?.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Converts BPM to seconds and removes 0 values
        /// </summary>
        /// <param name="BPMTime">BPM related time</param>
        /// <param name="BPM">BPMs</param>
        /// <param name="IPIS">IPI [s]</param>
        /// <param name="IPITimeS">IPI_time - relative</param>
        /// <param name="IPITimeRelated">Related Time from BPM</param>
        public static void BPM_to_IPI_s_remove_0s(DateTime[] BPMTime, double[] BPM, ref double[] IPIS, ref double[] IPITimeS, ref DateTime[] IPITimeRelated)
        {
            List<double> _IPI_s = [];
            List<double> _IPI_time_s = [];
            List<DateTime> _IPI_time_related = [];

            double _IPI_time_s_cnt = 0;
            bool firstpoint = true;

            for (int i = 0; i < BPM.Length; i++)
            {
                if (BPM[i] != 0)
                {
                    double ipi = 60 / BPM[i];
                    _IPI_s.Add(ipi);
                    _IPI_time_related.Add(BPMTime[i]);

                    if (firstpoint)
                    {
                        firstpoint = false;
                        _IPI_time_s.Add(0);
                    }
                    else
                    {
                        _IPI_time_s_cnt += ipi;
                        double dd = _IPI_time_s_cnt;
                        _IPI_time_s.Add(dd);
                    }
                }
            }

            IPIS = [.. _IPI_s];
            IPITimeS = [.. _IPI_time_s];
            IPITimeRelated = [.. _IPI_time_related];
        }


        /// <summary>
        /// Processes IPI and IPI_time
        /// Powerspectrum [s^2] is calculated
        /// </summary>
        /// <param name="SupressDC">if set to <c>true</c> DC is set to 0 except in DC Parameter</param>
        /// <remarks>
        /// Result in all fft_**** Params, RSA, LF, VLF, ULF, DC
        /// </remarks>
        public void ProcessIPI(bool SupressDC)
        {
            FFT_MathNet.FFT();
            //double[] fftPow = FFT_MathNet.fftAmplitudePowerDensity;   //Power Spektrum Density
            double[] fftPow = FFT_MathNet.FftAmplitudePower;          //Power Spektrum
            double[] fftFrequ = FFT_MathNet.FftFrequ;

            FftDC = fftPow[0];
            //Skip DC
            if (SupressDC)
            {
                fftPow[0] = 0;
                FFT_MathNet.SetDCZero();
            }

            RSA.Reset();
            LF.Reset();
            ULF.Reset();
            VLF.Reset();
            double f, y;

            for (int i = 0; i < fftPow.Length; i++)
            {
                f = fftFrequ[i];
                y = fftPow[i]; //Power [s^2/Hz]

                RSA.Add(f, y);
                LF.Add(f, y);
                ULF.Add(f, y);
                VLF.Add(f, y);
            }
            RSA.Value *= 1000000;  //[ms^2]
            LF.Value *= 1000000;
            VLF.Value *= 1000000;
            ULF.Value *= 1000000;
        }


        /// <summary>
        /// Set the date for further calculations, calculates RRMean_ms, RRMEan_Stdv, RMSSD, NN50, pNN50, HRMean, HRMean_StdDv
        /// </summary>
        /// <param name="HanninngPercentage">Percentage of datapoints that will be treated with Hanning window at the beginning AND the end</param>
        /// <param name="IPIMs">Interbeat interval [ms]</param>
        /// <param name="IPITime">Related Time to RR_Inter_Beat_Interval</param>
        /// <remarks>
        /// Values with IPI_s == 0 are removed
        /// RR_Inter_Beat_Interval data are resampled equidistantly
        /// Number of resampled values is next power of 2 (for FFT)
        /// Resampled data is stored in FFT_MathNet object
        /// </remarks>
        public void SetIPIMs(double HanninngPercentage, double[] IPIMs, double[] IPITime)
        {
            for (int i = 0; i < IPIMs.Length; i++)
                IPIMs[i] /= 1000;

            SetIPIS(HanninngPercentage, IPIMs, IPITime);
        }


        /// <summary>
        /// Set the date for further calculations, calculates RRMean_ms, RRMEan_Stdv, RMSSD, NN50, pNN50, HRMean, HRMean_StdDv
        /// </summary>
        /// <param name="HanninngPercentage">Percentage of datapoints that will be treated with Hanning window at the beginning AND the end</param>
        /// <param name="IPIS">Interbeat interval [s]</param>
        /// <param name="IPITime">Related Time to RR_Inter_Beat_Interval</param>
        /// <remarks>
        /// Values with IPI_s == 0 are removed
        /// RR_Inter_Beat_Interval data are resampled equidistantly
        /// Number of resampled values is next power of 2 (for FFT)
        /// Resampled data is stored in FFT_MathNet object
        /// </remarks>
        public void SetIPIS(double HanninngPercentage, double[] IPIS, double[] IPITime)
        {
            //Values are not equidistant ... make them and generate eqidistant time base; upsample to vals with next power of two
            int numVals = CMyTools.GetNearestPowerofTwoVal(IPIS.Length);

            //Calculate from original, uninterpolated values

            //Check for 0 in IPI_s
            List<double> IPI_s_Without0 = [];
            for (int i = 0; i < IPIS.Length; i++)
            {
                if (IPIS[i] > 0)
                    IPI_s_Without0.Add(IPIS[i]);
            }

            //FFT_MathNet.CalcAvgStdv(IPI_s, out _RRMean_ms, out _RRMean_Stdv);
            CFFTMathNet.CalcAvgStdv([.. IPI_s_Without0], out rRMean_ms, out rRMean_Stdv);
            RRMeanMs *= 1000; //[ms]
            RRMeanStdv *= 1000;//[ms]

            //HR
            double[] HR_Without0 = new double[IPI_s_Without0.Count];
            for (int i = 0; i < HR_Without0.Length; i++)
                HR_Without0[i] = 1 / IPI_s_Without0[i] * 60;

            CFFTMathNet.CalcAvgStdv(HR_Without0, out hRMean_bpm, out hRMean_Stdv);

            RMSSDMs = CalcRMSSD_ms([.. IPI_s_Without0], out nN50, out pNN50);

            FFT_MathNet.ResampleData(IPIS, IPITime, numVals); //Writes values to Data_x, Data_y

            if (HanninngPercentage != 0)
            {
                FFT_MathNet.HanningPercent(HanninngPercentage, RRMeanMs / 1000);
            }
        }


        /// <summary>
        /// Set the date for further calculations, calculates RRMean_ms, RRMEan_Stdv, RMSSD, NN50, pNN50, HRMean, HRMean_StdDv
        /// </summary>
        /// <param name="IPIMs">Interbeat interval [ms]</param>
        /// <param name="IPITime">Related Time to RR_Inter_Beat_Interval</param>
        /// <remarks>
        /// RR_Inter_Beat_Interval data are resampled equidistantly
        /// Number of resampled values is next power of 2 (for FFT)
        /// Resampled data is stored in FFT_MathNet object
        /// </remarks>
        public void SetIPIMs(double[] IPIMs, double[] IPITime)
        {
            SetIPIMs(Default_Hanning_Percentage_in_SetIPI, IPIMs, IPITime);
        }

        /// <summary>
        /// Set the date for further calculations, calculates RRMean_ms, RRMEan_Stdv, RMSSD, NN50, pNN50, HRMean, HRMean_StdDv
        /// </summary>
        /// <param name="IPIS">Interbeat interval [s]</param>
        /// <param name="IPITime">Related Time to RR_Inter_Beat_Interval</param>
        /// <remarks>
        /// Values with IPI_s == 0 are removed
        /// RR_Inter_Beat_Interval data are resampled equidistantly
        /// Number of resampled values is next power of 2 (for FFT)
        /// Resampled data is stored in FFT_MathNet object
        /// </remarks>
        public void SetIPIS(double[] IPIS, double[] IPITime)
        {
            SetIPIS(Default_Hanning_Percentage_in_SetIPI, IPIS, IPITime);
        }

        /// <summary>
        /// Set the date for further calculations, calculates RRMean_ms, RRMEan_Stdv, RMSSD, NN50, pNN50, HRMean, HRMean_StdDv
        /// Rebuilds time base from IPIs
        /// </summary>
        /// <param name="IPIS">Interbeat interval [s]</param>
        /// <remarks>
        /// Values with IPI_s == 0 are removed
        /// RR_Inter_Beat_Interval data are resampled equidistantly
        /// Number of resampled values is next power of 2 (for FFT)
        /// Resampled data is stored in FFT_MathNet object
        /// </remarks>
        public void SetIPIS(double[] IPIS)
        {
            SetIPIS(Default_Hanning_Percentage_in_SetIPI, IPIS, RebuildIPITimeSFromIPI(IPIS));
        }

        public enum EnumIPIDataType
        {
            IPIMs,
            IPIS,
            IPIBpm
        }

        public enum EnumIPITimeType
        {
            /// <summary>
            /// INT64 standard time: 634819178526493027
            /// </summary>
            IPITimeINT64StandardTime,

            /// <summary>
            /// Time String: 00:00:00.0000
            /// </summary>
            IPITimeTimeString
        }

        /// <summary>
        /// Only for backwards compatibility
        /// </summary>
        public int ImportIPIFromTextFile(string pathToFile, int colNumIPI, EnumIPIDataType IPIDataType, ref double[] IPI, ref double[] IPITime, ref DateTime[] IPITimeRelated, ref string Header)
        {
            return ImportIPIFromTextFile(pathToFile, colNumIPI, IPIDataType, EnumIPITimeType.IPITimeINT64StandardTime, ref IPI, ref IPITime, ref IPITimeRelated, 1, ref Header, (int)IPImin_ms, (int)IPImax_ms);
        }

        /// <summary>
        /// Only for backwards compatibility
        /// </summary>
        public int ImportIPIFromTextFile(string pathToFile, int colNumIPI, EnumIPIDataType IPIDataType, EnumIPITimeType IPITimeType, ref double[] IPIS, ref double[] IPITime, ref DateTime[] IPITimeRelated, int numHeaderRows, ref string Header)
        {
            return ImportIPIFromTextFile(pathToFile, colNumIPI, IPIDataType, IPITimeType, ref IPIS, ref IPITime, ref IPITimeRelated, numHeaderRows, ref Header, (int)IPImin_ms, (int)IPImax_ms);
        }


        /// <summary>
        /// Imports data from Text File , rebuilds time base, fills IPI and IPI_time
        /// </summary>
        /// <param name="pathToFile">Path to file; empty string "" opens file dialog</param>
        /// <param name="colNumIPI">Column number of the IPI data</param>
        /// <param name="IPIDataType">Type of the IPI data.</param>
        /// <param name="IPITimeType">Type of the IPI time.</param>
        /// <param name="IPIS">IPI</param>
        /// <param name="IPITime">IPI_time</param>
        /// <param name="IPITimeRelated">Related Time from the orignal import</param>
        /// <param name="numHeaderRows">Number of header rows</param>
        /// <param name="Header">Header line (first line) of the file</param>
        /// <param name="IPIminMs">Values lower IPImin_ms will be set to IPImin_ms; =0 no checking </param>
        /// <param name="IPImaxMs">Values lower IPImax_ms will be set to IPImax_ms; =0 no checking</param>
        /// <returns>
        /// -1 if function failes
        /// </returns>
        public int ImportIPIFromTextFile(string pathToFile, int colNumIPI, EnumIPIDataType IPIDataType, EnumIPITimeType IPITimeType, ref double[] IPIS, ref double[] IPITime, ref DateTime[] IPITimeRelated, int numHeaderRows, ref string Header, int IPIminMs, int IPImaxMs)
        {
            DateTime[] IPI_org_DateTime = new DateTime[1];
            double[] IPI_org_s = new double[1];

            int ret = GenericIPITextFileImporter(pathToFile, colNumIPI, IPIDataType, IPITimeType, ref IPI_org_s, ref IPI_org_DateTime, numHeaderRows, ref Header, IPIminMs, IPImaxMs);

            bool b = ImportIPISData(IPI_org_DateTime, IPI_org_s, ref IPIS, ref IPITime, ref IPITimeRelated);

            if (!b)
                ret = -1;

            return ret;
        }


        /// <summary>
        /// Imports data from Text file format, fills IPI and IPI_time, checks IPImin and IPImax according to default values
        /// </summary>
        /// <param name="pathToFile">Path to file; empty string "" opens file dialog</param>
        /// <param name="colNumIPI">Column number of the IPI data</param>
        /// <param name="IPIDataType">Type of the IPI data.</param>
        /// <param name="IPITimeType">Type of the IPI time.</param>
        /// <param name="IPIS">IPI</param>
        /// <param name="IPITime">IPI_time</param>
        /// <param name="numHeaderRows">Number of header rows</param>
        /// <param name="Header">Header line(s) of the file</param>
        /// <returns>
        /// -1 if function failes
        /// </returns>
        /// <remarks>
        /// Time is assumed to be in the first column
        /// Macht Basis PlausibilitätsCheck über IPImin_ms und IPImax_ms
        /// </remarks>
        public int GenericIPITextFileImporter(string pathToFile, int colNumIPI, EnumIPIDataType IPIDataType, EnumIPITimeType IPITimeType, ref double[] IPIS, ref DateTime[] IPITime, int numHeaderRows, ref string Header)
        {
            return GenericIPITextFileImporter(pathToFile, colNumIPI, IPIDataType, IPITimeType, ref IPIS, ref IPITime, numHeaderRows, ref Header, (int)IPImin_ms, (int)IPImax_ms);
        }

        /// <summary>
        /// Imports data from Text file format, fills IPI and IPI_time, checks IPImin and IPImax
        /// </summary>
        /// <param name="pathToFile">Path to file; empty string "" opens file dialog</param>
        /// <param name="colNumIPI">Column number of the IPI data</param>
        /// <param name="IPIDataType">Type of the IPI data.</param>
        /// <param name="IPITimeType">Type of the IPI time.</param>
        /// <param name="IPIS">IPI</param>
        /// <param name="IPITime">IPI_time</param>
        /// <param name="numHeaderRows">Number of header rows</param>
        /// <param name="Header">Header line(s) of the file</param>
        /// <param name="IPIminMs">Values lower IPImin_ms will be set to IPImin_ms; =0 no checking </param>
        /// <param name="IPImaxMs">Values lower IPImax_ms will be set to IPImax_ms; =0 no checking</param>
        /// <returns>
        /// -1 if function failes
        /// </returns>
        /// <remarks>
        /// Time is assumed to be in the first column
        /// Macht Basis PlausibilitätsCheck über IPImin_ms und IPImax_ms
        /// </remarks>
        public int GenericIPITextFileImporter(string pathToFile, int colNumIPI, EnumIPIDataType IPIDataType, EnumIPITimeType IPITimeType, ref double[] IPIS, ref DateTime[] IPITime, int numHeaderRows, ref string Header, int IPIminMs, int IPImaxMs)
        {
            double IPImin_s = IPIminMs; IPImin_s /= 1000;
            double IPImax_s = IPImaxMs; IPImax_s /= 1000;

            const int idxCol_Time = 0;
            int ret = -1;
            DateTime dt_old = DateTime.MinValue;
            string LastTextfielPath = pathToFile;

            List<string[]> importedData = CTextFileImporter.ImportTextFile(ref LastTextfielPath, ";", numHeaderRows, ref Header);
            this.LastTextfielPath = LastTextfielPath;
            
            if (importedData != null)
            {
                List<double> IPI_temp = [];
                List<DateTime> IPI_time_temp = [];

                if (importedData.Count > 2)
                {
                    ret = 0;
                    for (int i = 0; i < importedData.Count; i++)
                    {
                        if (importedData[i].Length >= colNumIPI + 1)
                        {
                            try
                            {
                                double d = Convert.ToDouble(importedData[i][colNumIPI]);
                                DateTime dt = DateTime.MinValue; //just to init dt

                                switch (IPIDataType)
                                {
                                    case EnumIPIDataType.IPIBpm:
                                        {
                                            d = 60 / d;
                                            break;
                                        }
                                    case EnumIPIDataType.IPIS:
                                        {
                                            break;
                                        }
                                    case EnumIPIDataType.IPIMs:
                                        {
                                            d /= 1000;  // d *= 1000; Ausgebessert 13.10.2015
                                            break;
                                        }
                                }

                                if ((d < IPImin_s) && (IPIminMs > 0))
                                    d = IPImin_s;

                                if ((d > IPImax_s) && (IPImaxMs > 0))
                                    d = IPImax_s;

                                switch (IPITimeType)
                                {
                                    case EnumIPITimeType.IPITimeINT64StandardTime:
                                        {
                                            dt = new DateTime(Convert.ToInt64(importedData[i][idxCol_Time]));
                                            break;
                                        }
                                    case EnumIPITimeType.IPITimeTimeString:
                                        {
                                            // Time String: 00:00:00.0000
                                            // String händisch zerlegen
                                            string[] part = importedData[i][idxCol_Time].Split('.');
                                            dt = DateTime.Parse(part[0]);
                                            dt = dt.AddMilliseconds(Convert.ToDouble(part[1]));
                                            break;
                                        }
                                }

                                if (dt > dt_old)
                                {
                                    IPI_temp.Add(d);
                                    IPI_time_temp.Add(dt);
                                    dt_old = dt;
                                }
                            }
                            catch
                            { }
                        }
                    }
                }
                IPIS = [.. IPI_temp];
                IPITime = [.. IPI_time_temp];
            }
            return ret;
        }

        /// <summary>
        /// ConvertIPI_time in DateTime format to s
        /// </summary>
        /// <returns>
        /// Converted Data
        /// </returns>
        public static double[] ConvertIPITimeDateTimeToS(DateTime[] IPITimeDateTime)
        {
            double[] IPI_time = new double[IPITimeDateTime.Length];
            DateTime begin = IPITimeDateTime[0];

            for (int i = 0; i < IPITimeDateTime.Length; i++)
            {
                IPI_time[i] = ((double)(IPITimeDateTime[i] - begin).TotalMilliseconds) / 1000;
            }
            return IPI_time;
        }


        /*
                /// <summary>
                /// Imports data as they come from Zyphir file format, rebuilds time base, fills IPI and IPI_time
                /// works from begin of file to end
                /// </summary>
                /// <param name="Zyphir_Time">Zyphir_ time (converted to DateTime from INT64)</param>
                /// <param name="Zyphir_IPI">Zyphir IPI [ms] - as it comes from file</param>
                /// <param name="IPI">IPI</param>
                /// <param name="IPI_time">IPI_time</param>
                /// <returns></returns>
                /// <remarks>
                /// </remarks>
                public bool Import_Zyphir_Data_Forward(DateTime[] Zyphir_Time, double[] Zyphir_IPI, ref double[] IPI, ref double[] IPI_time)
                {
                    const double percent_range = 0.3; //Nächstes IPI Intervall liegt in diesem Prozentbereich +-
            
                    bool ret = false;
                    List<double> IPI_temp = new List<double>();
                    List<double> IPI_time_temp = new List<double>();

                    List<DateTime> dt_time_temp = new List<DateTime>(); //debug

                    //Check for correct array sizes
                    if ((Zyphir_Time.Length == Zyphir_IPI.Length) && (Zyphir_IPI.Length > 2))
                    {
                        //IPI_temp.Add(Zyphir_IPI[0] / 1000); //[s]
                        //IPI_time_temp.Add(0);

                        int j = 1;
                        //Suche sinnvollen Anfang ... Datensatz wo IPI das 1. Mal springt
                        //634819178550173027; 528; 960; ; ;     IPI[j]
                        //634819178550213027; 528; 1008; ; ;    IPI[j+1]

                        bool FirstChange = false;
                        while (!FirstChange)
                        {
                            if (Convert.ToInt32(Zyphir_IPI[j]) != Convert.ToInt32(Zyphir_IPI[j + 1]))   //Double auf Gleichheit vergleichen -> keine gute Idee
                            {
                                if ((Zyphir_IPI[j + 1] > IPImin_ms) && (Zyphir_IPI[j + 1] < IPImax_ms) && (Zyphir_IPI[j] > IPImin_ms) && (Zyphir_IPI[j] < IPImax_ms))
                                {
                                    FirstChange = true;
                                }
                            }
                            j++;
                            if (j >= (Zyphir_IPI.Length - 1)) return false;
                        }
                
                        //Beginnwerte festlegen
                        double IPI_Start_s = (Zyphir_IPI[j] / 1000); //[s];
                        IPI_temp.Add(IPI_Start_s);
                        IPI_time_temp.Add(0);
                
                        DateTime dt_running = Zyphir_Time[j];
                        DateTime dt_next = dt_running + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(IPI_Start_s * 1000* (1 + percent_range)));  //in diesem Intervall muss nächster Herzschlag kommen

                        dt_time_temp.Add(Zyphir_Time[j]);

                        j++;    //Auf nächsten Wert zeigen
                        while (j < Zyphir_Time.Length - 1)
                        {
                            if (Convert.ToInt32(Zyphir_IPI[j]) == Convert.ToInt32(Zyphir_IPI[j + 1]))
                            {
                                if (Zyphir_Time[j] >= dt_next)
                                {
                                    //Kein Übergang gefunden, IPI ist so groß wie der davor
                                    dt_running = Zyphir_Time[j];
                                    double dd = IPI_temp[IPI_temp.Count - 1];
                                    IPI_temp.Add(dd); //Vorhergehender Wert[s]
                                    IPI_time_temp.Add(IPI_time_temp[IPI_time_temp.Count - 1] + dd);
                            
                                    dt_next = dt_running + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(dd*1000));  //Prozent ist schon vom vorhergehenden Wert dabei

                                    dt_time_temp.Add(dt_time_temp[dt_time_temp.Count-1] + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(dd * 1000)));
                                }
                            }
                            else
                            {
                                //Neuer Übergang gefunden
                                j++;
                                dt_running = Zyphir_Time[j];
                                int d = Convert.ToInt16(Zyphir_IPI[j]); //[ms]
                                double dd = d; dd /= 1000; //[s]
                                IPI_temp.Add(dd); //[s]
                                IPI_time_temp.Add(IPI_time_temp[IPI_time_temp.Count - 1] + dd);
                                dt_next = dt_running + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(d * (1 + percent_range)));

                                dt_time_temp.Add(dt_time_temp[dt_time_temp.Count - 1] + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(d)));
                            }
                            j++;

                        }

                        IPI = IPI_temp.ToArray();
                        IPI_time = IPI_time_temp.ToArray();
                        ret = true;
                    }

                    return ret;

                }

        */
        /// <summary>
        /// Imports data as they come from sampled file format, rebuilds time base, fills IPI and IPI_time
        /// works from end of file to begin, finally reverses data
        /// IPI [ms]
        /// </summary>
        /// <param name="IPITimeImport">Related time</param>
        /// <param name="IPIImportMs">IPI from device [ms]</param>
        /// <param name="IPIMs">IPI [ms]</param>
        /// <param name="IPITimeS">IPI_time [s]</param>
        /// <param name="IPITimeRelated">Related Time from the orignal import</param>
        /// <returns></returns>
        public static bool ImportIPIMsData(DateTime[] IPITimeImport, double[] IPIImportMs, ref double[] IPIMs, ref double[] IPITimeS, ref DateTime[] IPITimeRelated)
        {
            for (int i = 0; i < IPIImportMs.Length; i++)
            {
                IPIImportMs[i] /= 1000;
            }

            bool b = ImportIPISData(IPITimeImport, IPIImportMs, ref IPIMs, ref IPITimeS, ref IPITimeRelated);

            for (int i = 0; i < IPIMs.Length; i++)
            {
                IPIMs[i] *= 1000;
            }

            return b;
        }


        /// <summary>
        /// Imports data as they come from sampled file format, rebuilds time base, fills IPI and IPI_time
        /// works from end of file to begin, finally reverses data
        /// IPI [s]
        /// </summary>
        /// <param name="IPITimeImport">Related time</param>
        /// <param name="IPIImportS">IPI from device [s]</param>
        /// <param name="IPIS">IPI [s]</param>
        /// <param name="IPITimeS">IPI_time</param>
        /// <param name="IPITimeRelated">Related Time from the orignal import</param>
        /// <returns></returns>
        public static bool ImportIPISData(DateTime[] IPITimeImport, double[] IPIImportS, ref double[] IPIS, ref double[] IPITimeS, ref DateTime[] IPITimeRelated)
        {
            DateTime[] Time_with_File_inconsistencies = [];
            return ImportIPISData(IPITimeImport, IPIImportS, ref IPIS, ref IPITimeS, ref IPITimeRelated, ref Time_with_File_inconsistencies);
        }


        /// <summary>
        /// Imports data as they come from sampled file format, rebuilds time base, fills IPI and IPI_time
        /// works from end of file to begin, finally reverses data
        /// IPI [s]
        /// </summary>
        /// <param name="IPITimeImport">Related time</param>
        /// <param name="IPIImportS">IPI from device [s]</param>
        /// <param name="IPIS">IPI [s]</param>
        /// <param name="IPITimeS">IPI_time</param>
        /// <param name="IPITimeRelated">Related Time from the orignal import</param>
        /// <param name="TimeWithFileInconsistencies">Detected inconsistencies, Variable can be null</param>
        /// <returns></returns>
        public static bool ImportIPISData(DateTime[] IPITimeImport, double[] IPIImportS, ref double[] IPIS, ref double[] IPITimeS, ref DateTime[] IPITimeRelated, ref DateTime[] TimeWithFileInconsistencies)
        {

            bool ret = false;
            List<double> IPI_temp = []; //Time generated from IPIS
            List<DateTime> _IPI_time_related = [];    //Related Time from the orignal import
            List<DateTime> List_Time_with_File_inconsistencies = [];

            //Check for correct array sizes
            if ((IPITimeImport.Length == IPIImportS.Length) && (IPIImportS.Length > 2))
            {
                int j = IPIImportS.Length - 4;

                //Suche sinnvollen Anfang ... Datensatz wo IPI das 1. Mal springt
                bool FirstChange = false;
                while (!FirstChange)
                {
                    if (Convert.ToInt32(IPIImportS[j - 1] * 1000) != Convert.ToInt32(IPIImportS[j] * 1000))   //Double auf Gleichheit vergleichen -> keine gute Idee
                    {
                        if ((IPIImportS[j - 1] > IPImin_s) && (IPIImportS[j - 1] < IPImax_s) && (IPIImportS[j] > IPImin_s) && (IPIImportS[j] < IPImax_s))
                        {
                            FirstChange = true;
                        }
                    }
                    j--;
                    if (j == 0)
                        return false;
                }

                //Beginnwerte festlegen index [0]
                j++;    //Richtigen Wert aus obigem Vergleich
                double IPI_Start_s = IPIImportS[j]; //[s] Here we start the search
                IPI_temp.Add(IPI_Start_s);
                _IPI_time_related.Add(IPITimeImport[j]);

                DateTime dt_running = IPITimeImport[j];
                DateTime dt_next = dt_running - new TimeSpan(0, 0, 0, 0, Convert.ToInt32(IPI_Start_s * 1000));  //nach dieser Zeit muss der nächste Herzschlag kommen

                DateTime dt_next_prev = dt_next;   //prevoius search time
                j--;    //Auf nächsten Wert zeigen
                while (j > 3)
                {
                    if (IPITimeImport[j] <= dt_next) //search for index next to dt_next
                    {
                        //Genau hier müsste der nächste Wert stehen ....

                        //Sicherheitshalber im Umkreis von +-3 Werten schauen, ob hier im Umfeld ein Sprung ist, dann ggf darauf Rücksicht nehmen
                        for (int i = 3; i >= -2; i--)
                        {
                            if (IPIImportS[j + i] != IPIImportS[j + i - 1])
                            {
                                j += i; //Take Value closer to the end
                                break;
                            }
                        }

                        //Fehler im File? Ist der neue Zeitwert größer als der vorhergehende?
                        if (IPITimeImport[j] > dt_next_prev)
                        {
                            //Fehler im File; fürhrt zu Endlosschleife
                            //Rückwärts nächste Sprungstelle suchen .... resynchronisieren
                            List_Time_with_File_inconsistencies.Add(IPITimeImport[j]);
                            j--;
                            while (j > 3)
                            {
                                if (IPIImportS[j] != IPIImportS[j - 1])
                                {
                                    //Neue Sprungstelle gefunden
                                    break;
                                }
                                j--;
                            }
                        }

                        dt_next_prev = dt_next;

                        dt_running = IPITimeImport[j];
                        double dIPI_s = IPIImportS[j];
                        IPI_temp.Add(dIPI_s);
                        _IPI_time_related.Add(IPITimeImport[j]);

                        dt_next = dt_running - new TimeSpan(0, 0, 0, 0, Convert.ToInt32(dIPI_s * 1000));  //Prozent ist schon vom vorhergehenden Wert dabei
                    }
                    j--;
                }

                IPIS = new double[IPI_temp.Count];
                IPITimeRelated = new DateTime[IPI_temp.Count];

                j = IPI_temp.Count - 1;

                //Swap Arrays
                for (int i = 0; i < IPI_temp.Count; i++)
                {
                    IPIS[i] = IPI_temp[j];
                    IPITimeRelated[i] = _IPI_time_related[j];
                    j--;
                }

                //Rebuild time base
                IPITimeS = RebuildIPITimeSFromIPI(IPIS);
                TimeWithFileInconsistencies = [.. List_Time_with_File_inconsistencies];
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Rebuild_s the IPI time from the IPIs
        /// </summary>
        /// <param name="IPIS">IPI [s]</param>
        /// <returns></returns>
        public static double[] RebuildIPITimeSFromIPI(double[] IPIS)
        {

            double[] IPI_time_s = new double[IPIS.Length];
            IPI_time_s[0] = 0;
            for (int i = 1; i < IPIS.Length - 1; i++)
            {
                IPI_time_s[i] = IPI_time_s[i - 1] + IPIS[i + 1];
            }

            //Damit letzter Wert auch noch Sinn ergibt
            IPI_time_s[^1] = IPI_time_s[^2] + IPIS[IPI_time_s.Length - 1];

            return IPI_time_s;
        }


        /*
        /// <summary>
        /// Imports data as they come from Zyphir file format, rebuilds time base, fills IPI and IPI_time
        /// old forward algorithm
        /// </summary>
        /// <param name="Zyphir_Time">Zyphir_ time (converted to DateTime from INT64)</param>
        /// <param name="Zyphir_IPI">Zyphir IPI [ms] - as it comes from file</param>
        /// <param name="IPI">IPI</param>
        /// <param name="IPI_time">IPI_time</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public bool Import_Zyphir_Data_old(DateTime[] Zyphir_Time, double[] Zyphir_IPI, ref double[] IPI, ref double[] IPI_time)
        {
            bool ret = false;
            List<double> IPI_temp = new List<double>();
            List<double> IPI_time_temp = new List<double>();

            //Check for correct array sizes
            if ((Zyphir_Time.Length == Zyphir_IPI.Length) && (Zyphir_IPI.Length > 2))
            {
                IPI_temp.Add(Zyphir_IPI[0] / 1000); //[s]
                IPI_time_temp.Add(0);

                DateTime dt_running = Zyphir_Time[0];
                DateTime dt_next = dt_running + new TimeSpan(0, 0, 0, 0, Convert.ToInt16(Zyphir_IPI[0]));

                int j = 1;
                while (j < Zyphir_Time.Length)
                {
                    dt_running = Zyphir_Time[j];
                    if (dt_running >= dt_next)
                    {
                        int d = Convert.ToInt16(Zyphir_IPI[j]); //[ms]
                        double dd = d; dd /= 1000; //[s]
                        IPI_temp.Add(dd); //[s]
                        IPI_time_temp.Add(IPI_time_temp[IPI_time_temp.Count - 1] + dd);
                        dt_next = dt_running + new TimeSpan(0, 0, 0, 0, d);
                    }
                    j++;
                }

                IPI = IPI_temp.ToArray();
                IPI_time = IPI_time_temp.ToArray();
                ret = true;
            }

            return ret;
        }
         */


        /*
        public double [] CalcCrossCorrelation(double[] data, double[] time)
        {
            //RR Date is stored in FFTMathNet
            //Functions must have same number of samples, timely synchronised

            //Resample data
            if (data.Length != FFT_MathNet.Data_y.Length)
            {
                if (data.Length > FFT_MathNet.Data_y.Length)
                {
                    //Resample FFT_MathNet.Data_y
                    double [] y = FFT_MathNet.Data_y;
                    double [] x = FFT_MathNet.Data_x;

                    FFT_MathNet.ResampleData(ref y, ref x, data.Length);
                    return FFT_MathNet.CrossCorrelationNormalized(y, data);
                }
                else
                {
                    //Resample data
                    FFT_MathNet.ResampleData(ref data, ref time, FFT_MathNet.Data_y.Length);
                }
            }
            return FFT_MathNet.CrossCorrelationNormalized(FFT_MathNet.Data_y, data);
        }
         */



        /// <summary>
        /// Calculate RMSSD, NN50, pNN50
        /// </summary>
        /// <param name="values">Values</param>
        /// <param name="NN50">NN50</param>
        /// <param name="PNN50">pNN50</param>
        /// <returns>RMSSD</returns>
        private static double CalcRMSSD_ms(double[] values, out int NN50, out double PNN50)
        {
            double RMSSD = 0;
            NN50 = 0;
            for (int i = 0; i < values.Length - 1; i++)
            {
                double diff = values[i + 1] - values[i];
                RMSSD += diff * diff;
                if (Math.Abs(diff) > 0.05) NN50++;
            }
            double l = values.Length - 1;
            RMSSD /= l;
            RMSSD = Math.Sqrt(RMSSD) * 1000;  //ms
            PNN50 = NN50 / l * 100;
            return RMSSD;
        }

        #region IPIArtefactSupression


        /// <summary>
        /// Artefact supression in IPI [ms] (with Arrays)
        /// </summary>
        /// <param name="IPIMs">IPI [ms]</param>
        /// <param name="IPITime">Related time</param>
        /// <param name="numIntervallsBeforAfter">Number of values wich are observed before and after the processed value</param>
        /// <param name="AcceptedPercentageDeviation">Accepted percentage of deviation between the current value and the mean values of numIntervallsBeforAfter</param>
        /// <param name="sdevBefore">Standard deviation of intervalls before. Can be null</param>
        /// <param name="sdevAfter">TStandard deviation of intervalls after. Can be null</param>
        /// <param name="numValsCorrected">Number of values corrected</param>
        /// <param name="IPIisCorrected">true: This value was corrected</param>
        public static void IPIArtifactSuppression_ms(ref double[] IPIMs, ref double[] IPITime, int numIntervallsBeforAfter, double AcceptedPercentageDeviation, ref double[] sdevBefore, ref double[] sdevAfter, ref int numValsCorrected, ref bool[] IPIisCorrected)
        {
            List<double> Int = new(IPIMs);
            List<double> time = new(IPITime);
            List<bool> iPIisCorrected = new(IPIisCorrected);

            IPIArtifactSuppressionMs(ref Int, ref time, numIntervallsBeforAfter, AcceptedPercentageDeviation, ref sdevBefore, ref sdevAfter, ref numValsCorrected, ref iPIisCorrected);

            IPIMs = [.. Int];
            IPITime = [.. time];
            IPIisCorrected = [.. iPIisCorrected];
        }


        /// <summary>
        /// Artefact supression in IPI [s] (with Arrays)
        /// </summary>
        /// <param name="IPIS">IPI [s]</param>
        /// <param name="IPITimeS">Related time [s]</param>
        /// <param name="numIntervallsBeforAfter">Number of values wich are observed before and after the processed value</param>
        /// <param name="AcceptedPercentageDeviation">Accepted percentage of deviation between the current value and the mean values of numIntervallsBeforAfter</param>
        /// <param name="sdevBefore">Standard deviation of intervalls before. Can be null</param>
        /// <param name="sdevAfter">TStandard deviation of intervalls after. Can be null</param>
        /// <param name="numValsCorrected">Number of values corrected</param>
        /// <param name="IPIisCorrected">true: This value was corrected</param>
        public static void IPIArtifactSuppressionS(ref double[] IPIS, ref double[] IPITimeS, int numIntervallsBeforAfter, double AcceptedPercentageDeviation, ref double[] sdevBefore, ref double[] sdevAfter, ref int numValsCorrected, ref bool[] IPIisCorrected)
        {
            List<double> Int = new(IPIS);
            List<double> time = new(IPITimeS);
            List<bool> iPIisCorrected = new(IPIisCorrected);

            IPIArtifactSuppression_s(ref Int, ref time, numIntervallsBeforAfter, AcceptedPercentageDeviation, ref sdevBefore, ref sdevAfter, ref numValsCorrected, ref iPIisCorrected);

            IPIS = [.. Int];
            IPITimeS = [.. time];
            IPIisCorrected = [.. iPIisCorrected];
        }


        /// <summary>
        /// Artefact supression in IPI [ms] (with Lists)
        /// </summary>
        /// <param name="IPIMs">IPI [ms]</param>
        /// <param name="IPITimeS">Related time [s]</param>
        /// <param name="numIntervallsBeforAfter">Number of values wich are observed before and after the processed value</param>
        /// <param name="AcceptedPercentageDeviation">Accepted percentage of deviation between the current value and the mean values of numIntervallsBeforAfter</param>
        /// <param name="sdevBefore">Standard deviation of intervalls before. Can be null</param>
        /// <param name="sdevAfter">TStandard deviation of intervalls after. Can be null</param>
        /// <param name="numValsCorrected">Number of values corrected</param>
        /// <param name="IPIisCorrected">true: This value was corrected</param>
        public static void IPIArtifactSuppressionMs(ref List<double> IPIMs, ref List<double> IPITimeS, int numIntervallsBeforAfter, double AcceptedPercentageDeviation, ref double[] sdevBefore, ref double[] sdevAfter, ref int numValsCorrected, ref List<bool> IPIisCorrected)
        {
            for (int i = 0; i < IPIMs.Count; i++)
            {
                IPIMs[i] /= 1000;
            }

            IPIArtifactSuppression_s(
                ref IPIMs, ref IPITimeS, numIntervallsBeforAfter, AcceptedPercentageDeviation, ref sdevBefore, ref sdevAfter, ref numValsCorrected, ref IPIisCorrected);

            for (int i = 0; i < IPIMs.Count; i++)
            {
                IPIMs[i] *= 1000;
            }
        }

        /// <summary>
        /// Artefact supression in IPI [s] (with Arrays)
        /// </summary>
        /// <param name="IPIS">Values [s]</param>
        /// <param name="IPITimeS">Related time [s]</param>
        /// <param name="numIntervallsBeforAfter">Number of values wich are observed before and after the processed value</param>
        /// <param name="AcceptedPercentageDeviation">Accepted percentage of deviation between the current value and the mean values of numIntervallsBeforAfter</param>
        /// <param name="sdevBefore">Standard deviation of intervalls before. Can be null</param>
        /// <param name="sdevAfter">TStandard deviation of intervalls after. Can be null</param>
        /// <param name="numValsCorrected">Number of values corrected</param>
        /// <param name="IPIisCorrected">true: This value was corrected</param>
        /// <exception cref="System.Exception">Number of values must be &gt; 2* numIntervallsBeforAfter</exception>
        public static void IPIArtifactSuppression_s(ref List<double> IPIS, ref List<double> IPITimeS, int numIntervallsBeforAfter, double AcceptedPercentageDeviation, ref double[] sdevBefore, ref double[] sdevAfter, ref int numValsCorrected, ref List<bool> IPIisCorrected)
        {
            //Holds values before and after the investigated value
            List<double> valsBefore = [];
            List<double> valsAfter = [];

            IPIisCorrected = [];
            for (int i = 0; i < IPIS.Count; i++)
            {
                IPIisCorrected.Add(false);
            }

            bool CalcStdDev = false;

            //Standard deviation
            if ((sdevBefore != null) && (sdevAfter != null))
            {
                sdevBefore = new double[IPIS.Count - 2 * numIntervallsBeforAfter];
                sdevAfter = new double[IPIS.Count - 2 * numIntervallsBeforAfter];
                CalcStdDev = true;
            }

            if (IPIS.Count < 2 * numIntervallsBeforAfter + 1)
                throw new Exception("Number of values must be > 2* numIntervallsBeforAfter");

            /*
             * example numIntervallsBeforAfter = 5
             * Before    |val| After
             * 0 1 2 3 4 | 5 | 6 7 8 9 10
             */

            //Initialise at the beginning of the file
            for (int i = 0; i < numIntervallsBeforAfter; i++)
            {
                valsBefore.Add(IPIS[i]);
            }

            for (int i = numIntervallsBeforAfter + 1; i <= numIntervallsBeforAfter + numIntervallsBeforAfter; i++)
            {
                valsAfter.Add(IPIS[i]);
            }

            double factorMax = 1 + (AcceptedPercentageDeviation / 100);
            double factorMin = 1 - (AcceptedPercentageDeviation / 100);

            double[] RR_temp = new double[IPIS.Count];
            Buffer.BlockCopy(IPIS.ToArray(), 0, RR_temp, 0, IPIS.Count * sizeof(double));

            numValsCorrected = 0;

            for (int i = numIntervallsBeforAfter; i < IPIS.Count - numIntervallsBeforAfter; i++)
            {

                //avgBefore = valsBefore.Average();
                //avgAfter = valsAfter.Average();
                CFFTMathNet.CalcAvgStdv([.. valsAfter], out double avgAfter, out double _sdevAfter);
                CFFTMathNet.CalcAvgStdv([.. valsBefore], out double avgBefore, out double _sdevBefore);

                if ((sdevBefore != null) && (sdevAfter != null) && CalcStdDev)
                {
                    sdevAfter[i - numIntervallsBeforAfter] = _sdevAfter;
                    sdevBefore[i - numIntervallsBeforAfter] = _sdevBefore;
                }

                double avg = (avgAfter + avgBefore) / 2;
                double avgMax = avg * factorMax;
                double avgMin = avg * factorMin;

                if ((IPIS[i] < avgMax) && (IPIS[i] > avgMin))
                {
                    //Alles OK
                }
                else
                {
                    //Correct value
                    if (_sdevAfter > _sdevBefore)
                        IPIS[i] = avgBefore;
                    else
                        IPIS[i] = avgAfter;

                    IPIisCorrected[i] = true;
                    numValsCorrected++;
                }

                //Move one value
                valsBefore.Add(RR_temp[i]);
                valsBefore.RemoveAt(0);
                valsAfter.Add(RR_temp[i + numIntervallsBeforAfter - 1]);
                valsAfter.RemoveAt(0);
            }

            //Trim array 
            IPIS.RemoveRange(IPIS.Count - numIntervallsBeforAfter, numIntervallsBeforAfter);
            IPIS.RemoveRange(0, numIntervallsBeforAfter);
            IPITimeS.RemoveRange(IPITimeS.Count - numIntervallsBeforAfter, numIntervallsBeforAfter);
            IPITimeS.RemoveRange(0, numIntervallsBeforAfter);
            IPIisCorrected.RemoveRange(IPIisCorrected.Count - numIntervallsBeforAfter, numIntervallsBeforAfter);
            IPIisCorrected.RemoveRange(0, numIntervallsBeforAfter);

        }
        #endregion


        /// <summary>
        /// Class to hold reaults of function CalcIPIDeviation_inBreathingIntervalls
        /// </summary>
        /// <remarks>
        /// Details see function "CalcIPIDeviation_inBreathingIntervalls"
        /// </remarks>
        public class CCalcIPIDeviationInBreathingIntervallsResults
        {
            public List<double> maxAtem = [];
            public List<double> maxAtemtime = [];

            public List<double> minAtem = [];
            public List<double> minAtemtime = [];

            public List<double> deltaHRV = [];
            public List<double> deltaHRV_time = [];

            public double[] AtemNorm = [];
        }


        /// <summary>
        /// Calcs the realtive IPI deviation_in breathing intervalls.
        /// </summary>
        /// <remarks>
        /// (IPImax - IPImin)/(IPI mean) in the interval betweeen two Atem-peaks is calculated and stored in
        /// deltaHRV and deltaHRV_time. deltaHRV_time is the time between the two Atem-Peaks.
        /// maxAtem/maxAtemtime and minAtem/minAtemtime show, where the algoritm detected peaks.
        /// Atem_norm: normalises Atem, related to Atem_time
        /// </remarks>
        /// <param name="IPI">IPI, preprocessed (equidistant time intervalls, atrtivact supressed)</param>
        /// <param name="IPI_time">IPI Related time</param>
        /// <param name="Atem">Atem values</param>
        /// <param name="Atem_time">Atem related equidistant time</param>
        /// <returns>CCalcIPIDeviation_inBreathingIntervalls_Results</returns>

        /*
        public CCalcIPIDeviation_inBreathingIntervalls_Results CalcIPIDeviation_inBreathingIntervalls(List<double> IPI, List<double> IPI_time, List<double> Atem, List<double> Atem_time)
        {

            CCalcIPIDeviation_inBreathingIntervalls_Results res = new CCalcIPIDeviation_inBreathingIntervalls_Results();
            
            if (Atem.Count != Atem_time.Count)
            {
                throw new Exception("Atem and Atem_time must have same length");
            }
            
            //1.) Atem normieren
            res.AtemNorm = Atem.ToArray();
            CFFT_MathNet FFT_MathNet = new Math_Net.CFFT_MathNet();
            FFT_MathNet.Normalize(ref res.AtemNorm);

            //2) Maxima und Minima aus der Atemkurve suchen
            CConfigMinMaxSuche ConfigMinMaxSuche = new CConfigMinMaxSuche();

            ConfigMinMaxSuche.MaxNeg1 = new TimeSpan(0, 0, 0, 0, 6000);
            ConfigMinMaxSuche.MaxNeg2 = new TimeSpan(0, 0, 0, 0, 6000);
            ConfigMinMaxSuche.MaxPos1 = new TimeSpan(0, 0, 0, 0, 6000);

            CMinMaxSuche MinMaxSuche = new CMinMaxSuche(ConfigMinMaxSuche);

            for (int i = 0; i < Atem_time.Count; i++)
            {
                CDataIn cd = new CDataIn();
                cd.Value = (int)(res.AtemNorm[i] * 1000);
                cd.ts_Since_LastSync = new TimeSpan(0, 0, 0, 0, (int)(Atem_time[i] * 1000));
                if (MinMaxSuche.ProcessData(cd))
                {
                    //Gefunden
                    double ms = (double)MinMaxSuche.MaxLast.ts_Since_LastSync.TotalMilliseconds / 1000;
                    res.maxAtem.Add((double)MinMaxSuche.MaxLast.Value / 1000);
                    res.maxAtemtime.Add(ms);

                    ms = (double)MinMaxSuche.MinLast.ts_Since_LastSync.TotalMilliseconds / 1000;
                    res.minAtem.Add((double)MinMaxSuche.MinLast.Value / 1000);
                    res.minAtemtime.Add(ms);
                }
            }

            //3) Änderungen der HR im Intervall suchen
            int intervall_beginn = 0;
            int intervall_end = 0;

            for (int i = 1; i < res.maxAtem.Count - 1; i++) //Letztes Intervall auslassen
            {
                //IntervallBeginn finden:
                while (res.maxAtemtime[i - 1] > IPI_time[intervall_beginn])
                    intervall_beginn++;
                while (res.maxAtemtime[i] > IPI_time[intervall_end])
                    intervall_end++;

                //Jetzt HRV Bereich suchen
                List<double> yt = IPI.GetRange(intervall_beginn, intervall_end - intervall_beginn);
                double hrv_max = yt.Max();
                double hrv_min = yt.Min();
                double mean = yt.Sum() / ((double)yt.Count);

                res.deltaHRV.Add((hrv_max - hrv_min) / mean);
                res.deltaHRV_time.Add((IPI_time[intervall_end] + IPI_time[intervall_beginn]) / 2);

                //Nächstes Intervall
                intervall_beginn = intervall_end;
            }
            return res;
        }
        */
    }

}
