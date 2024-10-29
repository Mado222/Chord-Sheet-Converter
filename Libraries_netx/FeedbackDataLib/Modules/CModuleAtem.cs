namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleAtem : CModuleBase
    {
        public CModuleAtem()
        {
            ModuleColor = Color.Blue;
            ModuleName = "Atem";
            ModuleType = enumModuleType.cModuleAtem;

            cSWChannelNames =
                [
                "Atem raw [1]",
                "Atem-Frequenz [1/min]",
                "Atem-Amplitude [1]",
                "Atem-Unused"
                ];
            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeAtem0,
                enumSWChannelType.cSWChannelTypeAtem1,
                enumSWChannelType.cSWChannelTypeAtem2,
                enumSWChannelType.cSWChannelTypeAtem3
            ];
        }
    }
}
