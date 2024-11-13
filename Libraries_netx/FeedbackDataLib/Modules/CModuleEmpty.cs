namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleEmpty : CModuleBase
    {
        public CModuleEmpty()
        {
            ModuleColor = Color.LightGray;
            ModuleName = "Empty";

            cSWChannelNames =
    [
                "unused 0",
                "unused 0",
                "unused 0",
                "unused 0"
    ];
        }
    }
}
