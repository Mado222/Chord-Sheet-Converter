namespace FeedbackDataLib
{
#if VIRTUAL_EEG
    public class CEEGVirtual
    {
        //Channel numbers
        public const int chno_Delta = 1;
        public const int chno_Theta = 2;
        public const int chno_Alpha = 3;
        public const int chno_SMR = 4;
        public const int chno_LowBeta = 5;
        public const int chno_Beta = 6;
        public const int chno_HighBeta = 7;
        public const int chno_Artefacts = 8;
        public const int chno_Ratio_Theta_LowBeta = 9;
        public const int chno_Ratio_Theta_Beta = 10;
        public const int chno_Ratio_Theta_LowBeta_Beta = 11;

        public const int Default_Sample_Int_ms = 10;
        public const int Default_EEGBands_Int_ms = 200;

        public const int max_num_EEG_SWChannels = 12;
        public List<CEEGProcessor> EEGProcessor;
        public List<CModuleBase> RealEEGs;

        public CEEGVirtual()
        {
            Clear();
        }

        public void Clear()
        {
            EEGProcessor = new List<CEEGProcessor>();
            RealEEGs = new List<CModuleBase>();

            for (int i = 0; i < C8KanalReceiverV2_CommBase.max_num_HWChannels; i++)
            {
                EEGProcessor.Add(null);
                RealEEGs.Add(null);
            }
        }

        public void AddEEGModule (CModuleBase RealEEG)
        {

            RealEEG.SWChannels[0].SampleInt = Default_Sample_Int_ms;    //To make sure to have correct sampleitn
            RealEEGs[RealEEG.HW_cn & 0xff] = RealEEG;

            CModuleBase VirtualEEG = (CModuleBase) RealEEG.Clone();
            //Update SW Channels
            VirtualEEG.SWChannels.RemoveRange(1, VirtualEEG.SWChannels.Count - 1);  //Alle weg bis auf Rohkanal
            for (ushort i = 1; i <= max_num_EEG_SWChannels - 1; i++)
            {
                CSWChannel csw = (CSWChannel)VirtualEEG.SWChannels[0].Clone();
                csw.SWChannelType = new CDigitalSensors.CSWChannelType((ushort)(0x400 + i));
                csw.SampleInt = Default_EEGBands_Int_ms;
                VirtualEEG.SWChannels.Add(csw);
            }
            EEGProcessor[RealEEG.HW_cn & 0xff] = new CEEGProcessor(VirtualEEG);
        }

        public double[] GetEEGSpectrum_1Hz_Steps(int HW_cn)
        {
            return EEGProcessor[HW_cn].GetEEGSpectrum_1Hz_Steps();
        }

        public List<CDataIn> AddEEGSample(CDataIn val)
        {
            return EEGProcessor[val.HWChannelNumber].AddEEGSample(val);
        }

        public CModuleBase GetRealEEG (int HW_cn)
        {
            return RealEEGs[HW_cn];
        }

        public CModuleBase GetVirtualEEG(int HW_cn)
        {
            return EEGProcessor[HW_cn].ModuleInfo;
        }

        public void Update_ModuleInfo(int HW_cn, CModuleBase mi)
        {
            bool bModuleIsActive = false;
            RealEEGs[HW_cn].SWChannels[0].SaveChannel = mi.SWChannels[0].SaveChannel;

            for (int i = 0; i < EEGProcessor[HW_cn].ModuleInfo.SWChannels.Count; i++)
            {
                EEGProcessor[HW_cn].ModuleInfo.SWChannels[i].SaveChannel = mi.SWChannels[i].SaveChannel;
                if (mi.SWChannels[i].SaveChannel) bModuleIsActive = true;
                EEGProcessor[HW_cn].ModuleInfo.SWChannels[i].SendChannel= mi.SWChannels[i].SendChannel;
                if (mi.SWChannels[i].SendChannel) bModuleIsActive = true;
            }
            RealEEGs[HW_cn].SWChannels[0].SendChannel = bModuleIsActive;
        }
    }

#endif
}
