namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleSCLADS1292 : CModuleBase
    {
        public CModuleSCLADS1292()
        {
            ModuleColor = Color.LightBlue;
            ModuleName = "SCLADS1292";

            cSWChannelNames =
                [
                "SCL1",
                "SCL2",
                "SCL3",
                "SCL4"
                ];

            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeSCLADS0,
                enumSWChannelType.cSWChannelTypeSCLADS1,
                enumSWChannelType.cSWChannelTypeSCLADS2,
                enumSWChannelType.cSWChannelTypeSCLADS3
            ];
        }
    }
}
