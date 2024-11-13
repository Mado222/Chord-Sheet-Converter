using FeedbackDataLib.Modules;

namespace FeedbackDataLibGUI
{
    internal class CModuleExGADS1294_EEG_GUI : CModuleExGADS1294_EEG
    {
        private readonly frmSpectrum frmSpectrum;
        public CModuleExGADS1294_EEG_GUI() : base()
        {
            frmSpectrum = new frmSpectrum();
            frmSpectrum.Show();
        }
    }
}
