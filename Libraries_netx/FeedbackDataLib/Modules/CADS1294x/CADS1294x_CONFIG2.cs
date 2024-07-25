namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** Config 2 ***************************
    /***************************************************************/

    [Serializable]
    public class CADS1294x_CONFIG2
    {
        private byte registerValue;

        // Properties to access and modify specific bits in the register using enums
        public testFreq TestFrequ
        {
            get => (testFreq)(registerValue & 0x03);
            set => registerValue = (byte)((registerValue & ~0x03) | ((byte)value & 0x03));
        }

        public testAmp TestAmp
        {
            get => (testAmp)((registerValue >> 2) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 2)) | (((byte)value & 0x01) << 2));
        }

        // Reserved bits should not be settable so no property for them

        public testSource TestSource
        {
            get => (testSource)((registerValue >> 4) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 4)) | (((byte)value & 0x01) << 4));
        }

        public wctChop WctChop
        {
            get => (wctChop)((registerValue >> 5) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 5)) | (((byte)value & 0x01) << 5));
        }

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Constructor
        public CADS1294x_CONFIG2()
        {
            registerValue = 0;  // Default to all bits cleared
        }

        public byte GetConfig2()
        { return registerValue; }

        // Enums to define values for each bit field
        public enum testFreq : byte
        {
            TEST_FREQ_FCLK_DIV_2_21 = 0, // Pulsed at fCLK / 2^21
            TEST_FREQ_FCLK_DIV_2_20,     // Pulsed at fCLK / 2^20
            TEST_FREQ_NOT_USED,          // Not used
            TEST_FREQ_DC                 // At dc
        }

        public enum testAmp : byte
        {
            TEST_AMP_1X_VREF_DIV_2400 = 0, // 1 x -(VREFP - VREFN) / 2400 V
            TEST_AMP_2X_VREF_DIV_2400      // 2 x -(VREFP - VREFN) / 2400 V
        }

        public enum testSource : byte
        {
            TEST_SIGNAL_EXTERNAL = 0, // Test signals are driven externally
            TEST_SIGNAL_INTERNAL      // Test signals are generated internally
        }

        public enum wctChop : byte
        {
            WCT_CHOP_FREQ_VARIES = 0,  // Chopping frequency varies
            WCT_CHOP_FREQ_CONSTANT     // Chopping frequency constant at fMOD / 16
        }
    }
}

