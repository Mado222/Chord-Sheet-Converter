namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleEMG : CModuleBase
    {
        public CModuleEMG()
        {
            ModuleColor = Color.Gray;   //Color.FromArgb(Convert.ToInt32("FFACAAAE", 16)); //Grey
            ModuleName = "EMG";
            ModuleType = enumModuleType.cModuleEMG;

            cSWChannelNames =
                [
                "EMG raw [V]",
                "EMG moving average",
                "EMG-unused",
                "EMG-unused"
                ];

            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeEMG0,
                enumSWChannelType.cSWChannelTypeEMG1,
                enumSWChannelType.cSWChannelTypeEMG2,
                enumSWChannelType.cSWChannelTypeEMG3
            ];
        }
    }
}
