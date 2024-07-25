using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    public class CEEGProcessor
    {
        public CEEGSWChannels EEGSWChannels;
        private CEEG_Spectrum _CEEG_Spectrum;

        private const int default_fft_window_width_ms = 2560;

        private int _fft_window_width_ms = default_fft_window_width_ms;
        /// <summary>
        /// Over this time the FFT will be calculated
        /// Out of this ... calculate RingPuffer size as Power of two
        /// </summary>
        public int fft_window_width_ms { get => _fft_window_width_ms; set => _fft_window_width_ms = value; }

        public int HWChannelNumber { get; private set; }
        private List<CSWChannel> SWChannels = [];
        private DateTime SWCha_NextSampleTime = DateTime.Now;
        private CSWChannel rawChannel = null;
        private bool SpectrumChannelsactive = false;
        public TimeSpan SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(200);
        private TimeSpan SampleInt_ms;

        private CRingpuffer RP;

        public CEEGProcessor(int RawSignal_ChannelNumber, CModuleBase cm, int FFT_WindowWidth_ms = default_fft_window_width_ms)
        {
            UpdateEEGProcessor(RawSignal_ChannelNumber, cm, FFT_WindowWidth_ms);
        }

        /// <summary>
        /// Updates parameters zu calculate FFT
        /// </summary>
        /// <param name="RawSignal_ChannelNumber">The raw signal channel number.</param>
        /// <param name="cm">Module Information</param>
        /// <param name="FFT_WindowWidth_ms">The FFT window width [ms]</param>
        /// <returns></returns>
        public int UpdateEEGProcessor(int RawSignal_ChannelNumber, CModuleBase cm, int FFT_WindowWidth_ms = default_fft_window_width_ms)
        {
            EEGSWChannels = new CEEGSWChannels();
            _CEEG_Spectrum = new CEEG_Spectrum(EEGSWChannels._EEG_FFT_Channels);
            SpectrumChannelsactive = false;

            SWChannels.Clear();
            string prefix = RawSignal_ChannelNumber.ToString() + "_";
            for (int i = 0; i < cm.SWChannels.Count; i++)
            {
                string SWChannelName = cm.SWChannels[i].SWChannelName;
                if (!SWChannelName.Contains("raw"))
                {
                    if (SWChannelName.Contains(prefix))
                    {
                        SWChannels.Add((CSWChannel)cm.SWChannels[i].Clone());
                        if (cm.SWChannels[i].SendChannel)
                            SpectrumChannelsactive = true;
                    }
                }
                else
                {
                    if (SWChannelName.Contains(RawSignal_ChannelNumber.ToString()))
                        rawChannel = (CSWChannel)cm.SWChannels[i].Clone();
                }
            }
            SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(SWChannels[0].SampleInt);
            SampleInt_ms = TimeSpan.FromMilliseconds(rawChannel.SampleInt);
            HWChannelNumber = cm.HW_cn;
            int ret = CMyTools.getNearestPowerofTwoVal(FFT_WindowWidth_ms / SampleInt_ms.Milliseconds);
            RP = new CRingpuffer(ret)
            { IgnoreOverflowDuringPush = true };

            fft_window_width_ms = ret * SampleInt_ms.Milliseconds;
            return fft_window_width_ms;
        }

        public double[] GetEEGSpectrum_1Hz_Steps()
        {
            double[] buf = null;
            RP.PopAll(ref buf);
            if (RP.StoredObjects == RP.Length)
                return _CEEG_Spectrum.GetEEGSpectrum_1Hz_Steps(buf, SampleInt_ms.TotalMilliseconds);
            return null;
        }


        public List<CDataIn> AddEEGSample(CDataIn val)
        {
            List<CDataIn> res;

            double scaled_val = SWChannels[0].GetScaledValue(val.Value);
            RP.Push(scaled_val);    //Add to RingBuffer

            if (true)
            {
                //Prepare results
                res = [];
                //At least one EEG Channel active, Sample time of chan0 reached, enough values in buffer
                if (SpectrumChannelsactive && (DateTime.Now >= SWCha_NextSampleTime) && RP.StoredObjects >= RP.Length)
                {
                    CDataIn cd;
                    double[] buf = null;
                    RP.PopAll(ref buf);
                    _CEEG_Spectrum.Process_Spectrum(buf, SampleInt_ms.TotalMilliseconds);

                    foreach (CSWChannel swcn in SWChannels)
                    {
                        if (swcn.SendChannel && !swcn.SWChannelName.Contains("raw"))
                        {
                            cd = (CDataIn)val.Clone();
                            double d = _CEEG_Spectrum.EEG_Bands[EEGSWChannels.get_idx_from_SWChannelName(swcn.SWChannelName)].value;
                            cd.Value = SWChannels[0].GetUnscaledValue(d);
                            cd.SW_cn = swcn.SWChannelNumber;
                            res.Add(cd);
                        }
                    }
                    SWCha_NextSampleTime = DateTime.Now + SpectrumChannelSampleTime;
                }
            }

            return res;
        }
    }
}
