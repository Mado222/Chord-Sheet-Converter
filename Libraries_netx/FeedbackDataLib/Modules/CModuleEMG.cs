namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleEMG : CModuleBase
    {
        public CModuleEMG()
        {
            ModuleColor = Color.Gray;   //Color.FromArgb(Convert.ToInt32("FFACAAAE", 16)); //Grey
            ModuleName = "EMG";
            ModuleType = EnModuleType.cModuleEMG;

            cSWChannelNames =
                [
                "EMG raw [V]",
                "EMG moving average",
                "EMG-unused",
                "EMG-unused"
                ];

            cSWChannelTypes =
            [
                EnSWChannelType.cSWChannelTypeEMG0,
                EnSWChannelType.cSWChannelTypeEMG1,
                EnSWChannelType.cSWChannelTypeEMG2,
                EnSWChannelType.cSWChannelTypeEMG3
            ];
        }
    }
}
