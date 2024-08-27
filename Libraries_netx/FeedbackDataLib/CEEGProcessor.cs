using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    public class CEEGProcessor
    {
        public CEEGCalcChannels EEGSWChannels;
        private CEEG_Spectrum _CEEG_Spectrum;


        /// <summary>
        /// Over this time the FFT will be calculated
        /// Out of this ... calculate RingPuffer size as Power of two
        /// </summary>

        private const int default_fft_window_width_ms = 2560;
        private int fftWindowWidth_ms= default_fft_window_width_ms;
        public int FFTWindowWidth_ms
        {
            get
            {
                return fftWindowWidth_ms;
            }
            set
            {
                fftWindowWidth_ms = value;
                Update();
            }
        }

        private int sampleInt_ms;
        public int SampleInt_ms
        {
            get => sampleInt_ms;
            set { 
                sampleInt_ms = value;
                Update();
            }

        }

        private int num_samples_to_calc;
        private DateTime SWCha_NextSampleTime = DateTime.Now;
        private readonly CSWChannel? rawChannel = null;
        public bool SpectrumChannelsactive = false;
        public TimeSpan SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(200);
        
        

        //private CRingpuffer RP;
        private CRingpuffer RP;

        public CEEGProcessor (CSWChannel rawChannel, CEEGCalcChannels eegChannels)
        {
            EEGSWChannels = eegChannels;
            _CEEG_Spectrum = new CEEG_Spectrum(eegChannels.EEG_FrequencyRanges);
            this.rawChannel = rawChannel;
            if (rawChannel != null)
            {
                SampleInt_ms = rawChannel.SampleInt;
            }
        }

        private void Update()
        {
            num_samples_to_calc = CMyTools.getNearestPowerofTwoVal(fftWindowWidth_ms / sampleInt_ms);
            fftWindowWidth_ms = num_samples_to_calc * sampleInt_ms;
            RP = new CRingpuffer(num_samples_to_calc)
            { IgnoreOverflowDuringPush = true };
        }

        /// <summary>
        /// Updates parameters zu calculate FFT
        /// </summary>
        /// <param name="RawSignal_ChannelNumber">The raw signal channel number.</param>
        /// <param name="cm">Module Information</param>
        /// <param name="FFT_WindowWidth_ms">The FFT window width [ms]</param>
        /// <returns></returns>
        //public int UpdateEEGProcessor(int RawSignal_ChannelNumber, CModuleBase cm, int FFT_WindowWidth_ms = default_fft_window_width_ms)
        //{
        //    EEGSWChannels = new CEEGSWChannels();
        //    _CEEG_Spectrum = new CEEG_Spectrum(EEGSWChannels._EEG_FFT_Channels);

        //    SWChannels.Clear();
        //    string prefix = RawSignal_ChannelNumber.ToString() + "_";
        //    for (int i = 0; i < cm.SWChannels.Count; i++)
        //    {
        //        string SWChannelName = cm.SWChannels[i].SWChannelName;
        //        if (!SWChannelName.Contains("raw"))
        //        {
        //            if (SWChannelName.Contains(prefix))
        //            {
        //                SWChannels.Add((CSWChannel)cm.SWChannels[i].Clone());
        //            }
        //        }
        //        else
        //        {
        //            if (SWChannelName.Contains(RawSignal_ChannelNumber.ToString()))
        //                rawChannel = (CSWChannel)cm.SWChannels[i].Clone();
        //        }
        //    }
        //    SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(SWChannels[0].SampleInt);
        //    SampleInt_ms = TimeSpan.FromMilliseconds(rawChannel.SampleInt);
        //    HWChannelNumber = cm.HW_cn;
        //    int ret = CMyTools.getNearestPowerofTwoVal(FFT_WindowWidth_ms / SampleInt_ms.Milliseconds);
        //    RP = new CRingpuffer(ret)
        //    { IgnoreOverflowDuringPush = true };

        //    fft_window_width_ms = ret * SampleInt_ms.Milliseconds;
        //    return fft_window_width_ms;
        //}

        public double[]? GetEEGSpectrum_1Hz_Steps()
        {
            double[] buf = [];
            RP.PopAll(ref buf);
            if (RP.StoredObjects == RP.Length)
                return _CEEG_Spectrum.GetEEGSpectrum_1Hz_Steps(buf, SampleInt_ms);
            return null;
        }


        public void AddEEGSample(double scaled_val)
        {
            List<CDataIn> res;
            RP.Push(scaled_val);    //Add to RingBuffer

            if (true)
            {
                //Prepare results
                res = [];
                //At least one EEG Channel active, Sample time of chan0 reached, enough values in buffer
                if (SpectrumChannelsactive && (DateTime.Now >= SWCha_NextSampleTime) && RP.StoredObjects >= num_samples_to_calc)
                {
                    //CDataIn cd;
                    double[] buf = [];
                    RP.PopAll(ref buf);

                    //==> es müssen genug Daten im Puffer sein!!
                    _CEEG_Spectrum.Process_Spectrum(buf, SampleInt_ms);

                    //foreach (CSWChannel swcn in SWChannels)
                    //{
                    //    if (swcn.SendChannel && !swcn.SWChannelName.Contains("raw"))
                    //    {
                    //        cd = (CDataIn)val.Clone();
                    //        double d = _CEEG_Spectrum.EEG_Bands[EEGSWChannels.get_idx_from_SWChannelName(swcn.SWChannelName)].value;
                    //        cd.Value = SWChannels[0].GetUnscaledValue(d);
                    //        cd.SW_cn = swcn.SWChannelNumber;
                    //        res.Add(cd);
                    //    }
                    //}
                    SWCha_NextSampleTime = DateTime.Now + SpectrumChannelSampleTime;
                }
            }
        }

        public double Get_EEG_Band_Value (int idx_Band)
        {
            return _CEEG_Spectrum.EEG_Bands[idx_Band].value;
        }
    }
}
