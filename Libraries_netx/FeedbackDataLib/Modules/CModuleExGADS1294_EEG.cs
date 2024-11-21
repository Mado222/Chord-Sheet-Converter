﻿using BMTCommunicationLib;
using WindControlLib;

namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleExGADS1294_EEG : CModuleExGADS1294
    {

        /// <summary>
        /// Just a helper class to bundle two parameters
        /// </summary>
        private class CActiveFFTChannel
        {
            /// <summary>
            ///   <para>Channel no in SWChannels for the claculated (FFT) channel</para>
            /// </summary>
            public int calc_cn = -1;
            /// <summary>
            ///   <para>Index of the Band according to CEEGChannels</para>
            /// </summary>
            public int eegBandNo = -1;

            public CActiveFFTChannel()
            { }

            public CActiveFFTChannel(int swcn, int eegBandNo)
            {
                calc_cn = swcn;
                this.eegBandNo = eegBandNo;
            }
        }

        //////////////////
        //Configurations
        //////////////////

        /// <summary>Window width for FFT calculation [ms]<br />Will be automatically set to the next power of 2 of samples (Calculated with sample rate of chan 0)</summary>
        public int FFT_WindowWidth_ms { get; set; } = 2560;
        public int EEGBands_SampleInt_ms { get; set; }

        private readonly List<CEEGCalcChannels> EEGSWChannels = [];

        /// <summary>Default sample interval [ms] for the raw channel</summary>
        public const int Default_Sample_Int_ms = 10;
        /// <summary>Default sample interval [ms] for the FFT Channels</summary>
        public const int Default_EEGBands_Int_ms = 200;

        public TimeSpan default_SpectrumUpdateInterval_ms = new(0, 0, 0, 0, 2000);

        public void Set_EEGBands_SampleInt_ms(int chno, int value)
        {
            if (EEGProcessor is not null)
            {
                if (chno < EEGProcessor.Length)
                    EEGProcessor[chno].SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(value);
            }
        }

        //////////////////
        //Channel numbers
        //////////////////
        /// <summary>Caculates FFT</summary>
        [NonSerialized()] protected CEEGProcessor[]? EEGProcessor = null;
        /// <summary>
        ///   <para>
        /// Helper function: Holds the calculated channels to send</para>
        /// </summary>
        [NonSerialized()] private readonly List<CActiveFFTChannel>[] FFT_Channels_List;

        private DateTime[] dtNextSpectrumUpdate;
        private readonly TimeSpan[] tsSpectrumUpdateInterval_ms;

        public CModuleExGADS1294_EEG()
        {
            ModuleType_Unmodified = EnModuleType.cModuleExGADS94;
            ModuleType = EnModuleType.cModuleEEG;
            dtNextSpectrumUpdate = new DateTime[NumRawChannels];
            tsSpectrumUpdateInterval_ms = new TimeSpan[NumRawChannels];
            for (int i = 0; i < NumRawChannels; i++)
            {
                dtNextSpectrumUpdate[i] = DateTime.Now;
                tsSpectrumUpdateInterval_ms[i] = default_SpectrumUpdateInterval_ms;
            }
            FFT_Channels_List = new List<CActiveFFTChannel>[NumRawChannels];
            for (int i = 0; i < FFT_Channels_List.Length; i++)
            {
                FFT_Channels_List[i] = [];
            }
            Init();
        }

        public override void Init()
        {
            base.Init();
            ModuleName = "EEG";
            ModuleColor = Color.Yellow;
            dtNextSpectrumUpdate = new DateTime[NumRawChannels];

            //Setup new Software channel names
            cSWChannelNames.Clear();
            for (int i = 0; i < NumRawChannels; i++)
            {
                dtNextSpectrumUpdate[i] = DateTime.Now + tsSpectrumUpdateInterval_ms[i];
                cSWChannelNames.Add("EEG ch " + i.ToString());
            }
            for (int i = 0; i < NumRawChannels; i++)
            {
                string[] longnames = new CEEGCalcChannels().Get_longNamesArray();
                for (int j = 0; j < longnames.Length; j++)
                {
                    //Add ChanNo to LongNames: 0_Delta [Veff]
                    longnames[j] = i.ToString() + "_" + longnames[j];
                }
                cSWChannelNames.AddRange(longnames);
            }
        }

        /// <summary>Updates CSWConfigValues</summary>
        /// <param name="sWChannels">SWChannels to update from</param>
        public override void Update_SWChannels(List<CSWChannel> sWChannels)
        {
            base.Update_SWChannels(sWChannels);

            if (FFT_Channels_List != null)
            {
                for (int rawch = 0; rawch < NumRawChannels; rawch++)
                {
                    FFT_Channels_List[rawch].Clear();
                    for (int i = NumRawChannels; i < SWChannels.Count; i++)
                    {
                        if (SWChannels[i].SendChannel && char.GetNumericValue(SWChannels[i].SWChannelName[0]) == rawch)
                        {
                            FFT_Channels_List[rawch].Add(new CActiveFFTChannel(i, SWChannels[i].EEG_related_swcn));
                            SWChannels[rawch].SendChannel = true;   //Sicherheitshalber den zugehörigen raw channel auch setzen
                        }
                    }
                }
            }
        }

        public override int UpdateFromByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ret = base.UpdateFromByteArray(InBuf, Pointer_To_Array_Start);

            //SWChannelsModule.Clear();
            //for (int i = 0; i < SWChannels.Count; i++)
            //{
            //    SWChannelsModule.Add((CSWChannel)SWChannels[i].Clone());
            //}
            //Override Module Type back to EEG
            for (int i = 0; i < SWChannels.Count; i++)
                SWChannels[i].ModuleType = EnModuleType.cModuleEEG;

            Setup_SWChannels();
            return ret;
        }

        protected override void Setup_SWChannels(string prefix = "channel")
        {

            base.Setup_SWChannels("EEG");
            //Die GUI Kanäle erst umprogrammieren wenn sie nach SWChannels_Module kopiert wurden

            //In the beginning are the raw channels
            int i = NumRawChannels;
            while (i < cSWChannelNames.Count)
            {
                int related_raw_chanNo = int.Parse(cSWChannelNames[i][0].ToString());
                CSWChannel sws = (CSWChannel)SWChannels[related_raw_chanNo].Clone();
                sws.SWChannelName = cSWChannelNames[i];
                sws.SWChannelNumber = (byte)i;
                sws.SWChannelColor = Color.LightGray;
                sws.SampleInt = Default_EEGBands_Int_ms;
                sws.ModuleType = ModuleType;
                sws.EEG_related_swcn = new CEEGCalcChannels().Get_idx_from_SWChannelName(cSWChannelNames[i][2..]);
                SWChannels.Add(sws);
                i++;
            }
            EEGSWChannels.Clear();
            //Prepare sample Intervals for EEG Prozessor
            if (EEGProcessor == null || EEGProcessor.Length == 0)
            {
                EEGProcessor = new CEEGProcessor[NumRawChannels];
                for (i = 0; i < EEGProcessor.Length; i++)
                {
                    CEEGCalcChannels eegc = new();
                    EEGSWChannels.Add(eegc);
                    EEGProcessor[i] = new CEEGProcessor(SWChannels[i], eegc)
                    {
                        SpectrumChannelsactive = true
                    };
                }
            }
        }

        /// <summary>
        /// Gets the eeg spectrum in 1 hz steps.
        /// </summary>
        /// <param name="chanNo">The chan no.</param>
        /// <returns></returns>
        public double[]? GetEEGSpectrum_1Hz_Steps(int chanNo)
        {
            return EEGProcessor?[chanNo].GetEEGSpectrum_1Hz_Steps();
        }

        /// <summary>
        /// Processes incoming data
        /// Filtering
        /// Adds FFT Spectrum data
        /// </summary>
        /// <param name="original_Data">Original data from device</param>
        /// <returns></returns>
        public override List<CDataIn> Processdata(CDataIn originalData)
        {
            // Apply base processing (HP Filter)
            var processedData = base.Processdata(originalData);

            // Update original data with the processed result
            originalData = processedData[0];
            processedData.Clear();

            // Handle extra data if present
            if (originalData.NumExtraDat > 0)
            {
                // Decode and store the extra data
                var decodedValue = BitConverter.ToInt32(
                    CInsightDataEnDecoder.DecodeFrom7Bit(originalData.ExtraDat),
                    0);

                extraDatas[originalData.SWcn][originalData.TypeExtraDat].Value = decodedValue;
                extraDatas[originalData.SWcn][originalData.TypeExtraDat].DTLastUpdated = DateTime.Now;
                extraDatas[originalData.SWcn][originalData.TypeExtraDat].TypeExtradat = (EnTypeExtradat_ADS)originalData.TypeExtraDat;

                if (extraDatas[originalData.SWcn][originalData.TypeExtraDat].TypeExtradat == EnTypeExtradat_ADS.exgain)
                {
                    //All data of one Measure-sequence are in - calc values
                    var Uax2 = extraDatas[originalData.SWcn];
                    double gain = Uax2[(int)EnTypeExtradat_ADS.exgain].Value;

                    double SKALVAL_K = SWChannels[originalData.SWcn].SkalValue_k;

                    double Ua2 = Uax2[(int)EnTypeExtradat_ADS.exUa2].Value * SKALVAL_K / gain;
                    double Ua1 = Uax2[(int)EnTypeExtradat_ADS.exUa1].Value * SKALVAL_K / gain;
                    double Ua0 = Uax2[(int)EnTypeExtradat_ADS.exUa0].Value * SKALVAL_K / gain;

                    ElectrodeDatas[originalData.SWcn].Rp = ((Ua1 - Ua0) / Iconst) - Rprotect; //Keine Ahnung warum / 2
                    ElectrodeDatas[originalData.SWcn].Rn = ((Ua2 - Ua0) / Iconst / 4) - Rprotect;
                    ElectrodeDatas[originalData.SWcn].Uelectrode = Ua0;
                    ElectrodeDatas[originalData.SWcn].Ua2 = Ua2;
                    ElectrodeDatas[originalData.SWcn].Ua1 = Ua1;
                }
            }

            // Return the result after further processing
            //return ProcessDataEEG_Basic(originalData, processedData);
            //}

            //protected List<CDataIn> ProcessDataEEG_Basic(CDataIn originalData, List<CDataIn> processedData)
            //{
            if (originalData.SWcn < NumRawChannels)
            {
                int swcn = originalData.SWcn;
                if (SWChannelsModule[swcn].SendChannel)
                {
                    processedData.Add(originalData);

                    double sample = SWChannels[swcn].GetScaledValue(originalData.Value);
                    if (EEGProcessor is not null)
                    {
                        EEGProcessor[swcn].AddEEGSample(sample);

                        //Check for FFT results
                        List<CActiveFFTChannel> fftchan = FFT_Channels_List[swcn];
                        for (int i = 0; i < fftchan.Count; i++)
                        {
                            CDataIn cd = (CDataIn)originalData.Clone();
                            cd.Value = SWChannels[i].GetUnscaledValue(EEGProcessor[swcn].Get_EEG_Band_Value(fftchan[i].eegBandNo));
                            cd.SWcn = (byte)fftchan[i].calc_cn;
                            processedData.Add(cd);
                        }
                    }
                }
            }
            else
            {
                //Here we do the calculated channels
            }
            return processedData;
        }


        public override byte[] GetSWConfigChannelsByteArray()
        {
            if (IsModuleActive())
            {
                for (int i = 0; i < NumRawChannels; i++)
                {
                    SWChannelsModule[i].SWChannelConfig = SWChannels[i].SWChannelConfig; //Raw signals channels are in the beginning
                }
                //Module werden frisch gesetzt - EEG Prozessor aktualisieren
            }
            return base.Get_SWConfigChannelsByteArray(SWChannelsModule);
        }
    }
}
