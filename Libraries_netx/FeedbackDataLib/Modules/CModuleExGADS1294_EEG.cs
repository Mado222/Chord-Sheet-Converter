using BMTCommunicationLib;
using System.DirectoryServices.ActiveDirectory;
using WindControlLib;

namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleExGADS1294_EEG : CModuleExGADS1294
    {

        //////////////////
        //Configurations
        //////////////////

        /// <summary>Window width for FFT calculation [ms]<br />Will be automatically set to the next power of 2 of samples (Calculated with sample rate of chan 0)</summary>
        public int FFT_WindowWidth_ms { get; set; } = 2560;
        public int EEGBands_SampleInt_ms { get; set; }

        public void Set_EEGBands_SampleInt_ms(int chno, int value)
        {
            if (EEGProcessor != null)
            {
                if (chno < EEGProcessor.Length)
                    EEGProcessor[chno].SpectrumChannelSampleTime = TimeSpan.FromMilliseconds(value);
            }
        }

        //////////////////
        //Channel numbers
        //////////////////

        public const int Default_Sample_Int_ms = 10;
        public const int Default_EEGBands_Int_ms = 200;

        [NonSerialized()] protected CEEGProcessor[]? EEGProcessor = null;

        //private frmSpectrum frmSpectrum = null;
        private DateTime[] dtNextSpectrumUpdate;
        private readonly TimeSpan[] tsSpectrumUpdateInterval_ms;

        public CModuleExGADS1294_EEG()
        {
            _ModuleType_Unmodified = enumModuleType.cModuleExGADS94;
            _ModuleType = enumModuleType.cModuleEEG;
            dtNextSpectrumUpdate = new DateTime[num_raw_Channels];
            tsSpectrumUpdateInterval_ms = new TimeSpan[num_raw_Channels];
            for (int i = 0; i < num_raw_Channels; i++)
            {
                dtNextSpectrumUpdate[i] = DateTime.Now;
                tsSpectrumUpdateInterval_ms[i] = new TimeSpan(0, 0, 0, 0, 2000);
            }

            Init();
        }

        public override void Init ()
        {
            base.Init ();
            ModuleName = "EEG";
            ModuleColor = Color.Yellow;
            dtNextSpectrumUpdate = new DateTime[num_raw_Channels];

            cSWChannelNames.Clear();
            for (int i = 0; i < num_raw_Channels; i++)
            {
                dtNextSpectrumUpdate[i] = DateTime.Now + tsSpectrumUpdateInterval_ms[i];
                cSWChannelNames.Add("EEG ch " + i.ToString());
            }
            for (int i = 0; i < num_raw_Channels; i++)
            {
                string[] longnames = new CEEGSWChannels().get_longNamesArray();
                for (int j = 0; j < longnames.Length; j++)
                {
                    //Add ChanNo to LongNames: 0_Delta [Veff]
                    longnames[j] = i.ToString() + "_" + longnames[j];
                }
                cSWChannelNames.AddRange(longnames);
            }
        }

        public override int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ret = base.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start);

            //Override Module Type back to EEG
            for (int i = 0; i < SWChannels.Count; i++)
                SWChannels[i].ModuleType = _ModuleType;

            ModuleType = _ModuleType;
            ModuleType_Unmodified = _ModuleType_Unmodified;
            Setup_SWChannels();
            return ret;
        }

        protected override void Setup_SWChannels(string prefix = "channel")
        {

            base.Setup_SWChannels("EEG");
            //Die GUI Kanäle erst umprogrammieren wenn sie nach SWChannels_Module kopiert wurden

            //In the beginning are the raw channels
            int i = num_raw_Channels;
            while (i < cSWChannelNames.Count)
            {
                int related_raw_chanNo = int.Parse(cSWChannelNames[i][0].ToString());
                CSWChannel sws = (CSWChannel)sWChannels[related_raw_chanNo].Clone();
                sws.SWChannelName = cSWChannelNames[i];
                sws.SWChannelNumber = (byte)i;
                sws.SWChannelColor = Color.LightGray;
                sws.SampleInt = Default_EEGBands_Int_ms;
                sws.ModuleType = ModuleType;
                sWChannels.Add(sws);
                i++;
            }
            //Prepare sample Intervals for EEG Prozessor
            if (EEGProcessor == null || EEGProcessor.Length == 0)
            {
                EEGProcessor = new CEEGProcessor[num_raw_Channels];
                for (i = 0; i < EEGProcessor.Length; i++)
                {
                    EEGProcessor[i] = new CEEGProcessor(i, this) 
                    { 
                        SpectrumChannelsactive = true 
                    };
                    FFT_WindowWidth_ms = EEGProcessor[i].UpdateEEGProcessor(i, this, FFT_WindowWidth_ms);
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
            return EEGProcessor == null ? null : EEGProcessor[chanNo].GetEEGSpectrum_1Hz_Steps();
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

                var extraData = extraDatas[originalData.HW_cn][originalData.TypeExtraDat];
                extraData.Value = decodedValue;
                extraData.DTLastUpdated = DateTime.Now;
                extraData.TypeExtradat = (EnTypeExtradat_ADS)originalData.TypeExtraDat;
            }

            // Return the result after further processing
            return ProcessDataEEG_Basic(originalData, processedData);
        }

        protected List<CDataIn> ProcessDataEEG_Basic(CDataIn originialData, List<CDataIn> processedData)
        {
            if (originialData.SW_cn < num_raw_Channels)
            {
                int swcn = originialData.SW_cn;
                if (sWChannels_Module[swcn].SendChannel)
                {
                    processedData.Add(originialData);

                    if (EEGProcessor is not null)
                    {
                        List<CDataIn> _DataEEG = EEGProcessor[swcn].AddEEGSample(originialData);
                        if (_DataEEG != null)
                            processedData.AddRange(_DataEEG);
                    }
                }
            }
            return processedData;
        }


        public override byte[] Get_SWConfigChannelsByteArray()
        {
            if (IsModuleActive)
            {
                for (int i=0; i< num_raw_Channels; i++)
                {
                    SWChannels_Module[i].SWConfigChannel= SWChannels[i].SWConfigChannel; //Raw signals channels are in the beginning
                }
                //Module werden frisch gesetzt - EEG Prozessor aktualisieren
            }
            return base.Get_SWConfigChannelsByteArray(SWChannels_Module);
        }
    }
}
