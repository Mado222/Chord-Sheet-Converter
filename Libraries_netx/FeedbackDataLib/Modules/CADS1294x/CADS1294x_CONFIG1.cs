namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** Config 1 ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1294x_CONFIG1
    {
        private byte registerValue;

        // Properties to access and modify specific bits in the register using enums
        public dataRate DataRate
        {
            get => (dataRate)(registerValue & 0x07);
            set => registerValue = (byte)((registerValue & ~0x07) | ((byte)value & 0x07));
        }

        public clkEn ClkEn
        {
            get => (clkEn)((registerValue >> 5) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 5)) | (((byte)value & 0x01) << 5));
        }

        public daisyEn DaisyEn
        {
            get => (daisyEn)((registerValue >> 6) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 6)) | (((byte)value & 0x01) << 6));
        }

        public resMode ResMode
        {
            get => (resMode)((registerValue >> 7) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 7)) | (((byte)value & 0x01) << 7));
        }

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Constructor
        public CADS1294x_CONFIG1()
        {
            registerValue = 0;  // Default to all bits cleared
        }


        // Enums to define values for each bit field
        public enum dataRate : byte
        {
            DataRate32K = 0,
            DataRate16K,
            DataRate8K,
            DataRate4K,
            DataRate2K,
            DataRate1K,
            DataRate500,
            DataRateReserved
        }

        public enum clkEn : byte
        {
            OscillatorClockDisabled = 0,
            OscillatorClockEnabled = 1
        }

        public enum daisyEn : byte
        {
            DaisyChainMode = 0,
            MultipleReadbackMode = 1
        }

        public enum resMode : byte
        {
            LowPowerMode = 0,
            HighResolutionMode = 1
        }
    }
}
