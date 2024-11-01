using FeedbackDataLib.Modules;
namespace FeedbackDataLib_GUI
{
    public interface IModuleSpecificSetup_Base
    {
        void ReadModuleSpecificInfo(ref CModuleBase ModuleInfo);
        void UpdateModuleInfo(CModuleBase ModuleInfo);
    }
}