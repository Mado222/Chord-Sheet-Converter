namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleAtem : CModuleBase
    {
        public CModuleAtem()
        {
            ModuleColor = Color.Blue;
            ModuleName = "Atem";
            ModuleType = EnModuleType.cModuleAtem;

            cSWChannelNames =
                [
                "Atem raw [1]",
                "Atem-Frequenz [1/min]",
                "Atem-Amplitude [1]",
                "Atem-Unused"
                ];
            cSWChannelTypes =
            [
                EnSWChannelType.cSWChannelTypeAtem0,
                EnSWChannelType.cSWChannelTypeAtem1,
                EnSWChannelType.cSWChannelTypeAtem2,
                EnSWChannelType.cSWChannelTypeAtem3
            ];
        }
    }
}
