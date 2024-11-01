using FeedbackDataLib.Modules;

namespace FeedbackDataLib_GUI
{
    internal class CModuleExGADS1294_EEG_GUI : CModuleExGADS1294_EEG
    {
        private frmSpectrum frmSpectrum;
        public CModuleExGADS1294_EEG_GUI() : base()
        {
            frmSpectrum = new frmSpectrum();
            frmSpectrum.Show();
        }
    }
}
