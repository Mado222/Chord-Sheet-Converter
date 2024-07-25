using BMTCommunicationLib;
using System.Collections;
using WindControlLib;

namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleExGADS1292_EEG : CModuleExGADS1292
    {

        public enum EnTypeExtradat_ADS
        {
            ExRp = 0,
            ExRn = 1,
            ExUp = 2,
            empty
        }

        public class ExtraData
        {
            public double Value { get; set; } = -1;
            public DateTime DTLastUpdated = DateTime.MinValue;

            public ExtraData(EnTypeExtradat_ADS typeExtradat)
            {
                TypeExtradat = typeExtradat;
            }

            public EnTypeExtradat_ADS TypeExtradat { get; set; } = EnTypeExtradat_ADS.empty;
        }

        public ExtraData[] extraDatas = [];


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

        [NonSerialized()] protected CEEGProcessor[] EEGProcessor;

        //private frmSpectrum frmSpectrum = null;
        private DateTime [] dtNextSpectrumUpdate;
        private readonly TimeSpan[] tsSpectrumUpdateInterval_ms = [
            new (0,0,0,0,2000),
            new(0,0,0,0,2000) ];

        public CModuleExGADS1292_EEG()
        {
            _num_raw_Channels = 2;
            _ModuleType_Unmodified = enumModuleType.cModuleExGADS;
            _ModuleType = enumModuleType.cModuleEEG;
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

            extraDatas = new ExtraData [Enum.GetValues(typeof(EnTypeExtradat_ADS)).Length];
            for (int i = 0;i< Enum.GetValues(typeof(EnTypeExtradat_ADS)).Length; i++)
            {
                extraDatas[i] = new((EnTypeExtradat_ADS)i);
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
                    EEGProcessor[i] = new CEEGProcessor(i, this);
                    FFT_WindowWidth_ms = EEGProcessor[i].UpdateEEGProcessor(i, this, FFT_WindowWidth_ms);
                }
            }
        }

        /// <summary>
        /// Gets the eeg spectrum in 1 hz steps.
        /// </summary>
        /// <param name="chanNo">The chan no.</param>
        /// <returns></returns>
        public double[] GetEEGSpectrum_1Hz_Steps(int chanNo)
        {
            return EEGProcessor[chanNo].GetEEGSpectrum_1Hz_Steps();
        }

        /// <summary>
        /// Processes incoming data
        /// Filtering
        /// Adds FFT Spectrum data
        /// </summary>
        /// <param name="original_Data">Original data from device</param>
        /// <returns></returns>
        public override List<CDataIn> Processdata(CDataIn original_Data)
        {
           
            List<CDataIn> processedData = base.Processdata(original_Data); //HP Filter
            original_Data = processedData[0];
            processedData.Clear();
            if (original_Data.NumExtraDat > 0)
            {
                //Extradata, it is a Int32 value
                extraDatas[original_Data.TypeExtraDat].Value = 
                    BitConverter.ToInt32(
                        CInsightDataEnDecoder.DecodeFrom7Bit(original_Data.ExtraDat), 
                        0);
                extraDatas[original_Data.TypeExtraDat].DTLastUpdated = DateTime.Now;
                extraDatas[original_Data.TypeExtraDat].TypeExtradat = (EnTypeExtradat_ADS) original_Data.TypeExtraDat;
            }
            return ProcessDataEEG_Basic(original_Data, processedData);
        }

        protected List<CDataIn> ProcessDataEEG_Basic(CDataIn originialData, List<CDataIn> processedData)
        {
            if (originialData.SW_cn < num_raw_Channels)
            {
                int swcn = originialData.SW_cn;
                if (sWChannels_Module[swcn].SendChannel)
                {
                    processedData.Add(originialData);
                    
                    List<CDataIn> _DataEEG = EEGProcessor[swcn].AddEEGSample(originialData);
                    if (_DataEEG != null)
                        processedData.AddRange(_DataEEG);
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
