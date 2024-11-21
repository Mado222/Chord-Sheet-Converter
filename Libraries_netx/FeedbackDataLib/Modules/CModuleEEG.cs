namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleEEG : CModuleExGADS1294_EEG
    {
        public CModuleEEG()
        {
            _num_raw_Channels = 1;
            ModuleType_Unmodified = EnModuleType.cModuleEEG;
            ModuleType = EnModuleType.cModuleEEG;
            Init();
        }

        public override byte[] GetSWConfigChannelsByteArray()
        {
            if (SWChannels != null)
            {
                SWChannelsModule[0].SampleInt = SWChannels[0].SampleInt;
                return base.GetSWConfigChannelsByteArray();
            }
            return [];
        }
    }
}
