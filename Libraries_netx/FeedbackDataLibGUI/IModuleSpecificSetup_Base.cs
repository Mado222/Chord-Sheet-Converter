using FeedbackDataLib.Modules;
namespace FeedbackDataLibGUI
{
    public interface IModuleSpecificSetup_Base
    {
        void ReadModuleSpecificInfo(ref CModuleBase ModuleInfo);
        void UpdateModuleInfo(CModuleBase ModuleInfo);
    }
}