namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** CHANxSET ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1294x_CHANxSET
    {
        private byte registerValue;
        private CADS1294x_Gain gainControl = new CADS1294x_Gain();

        // Properties for each field in the register using corresponding enums and the GainControl class
        public MUX Mux
        {
            get => (MUX)(registerValue & 0x07);
            set => registerValue = (byte)((registerValue & ~0x07) | ((byte)value & 0x07));
        }

        public int GetAmplification()
        {
            return gainControl.GetAmplification((CADS1294x_Gain.Gain)((registerValue >> 4) & 0x07));
        }

        public void SetAmplification(int value)
        {
            var gainEnum = gainControl.GetBitMaskFromAmplification(value);
            registerValue = (byte)((registerValue & ~(0x07 << 4)) | (((byte)gainEnum & 0x07) << 4));
        }

        public void SetAmplification(CADS1294x_Gain.Gain gain)
        {
            int ampl=gainControl.GetAmplification(gain);
            var gainEnum = gainControl.GetBitMaskFromAmplification(ampl);
            registerValue = (byte)((registerValue & ~(0x07 << 4)) | (((byte)gainEnum & 0x07) << 4));
        }


        public PDn Pdn
        {
            get => (PDn)((registerValue >> 7) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 7)) | (((byte)value & 0x01) << 7));
        }

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Methods to increase and decrease gain using the GainControl class
        public void IncreaseGain()
        {
            var currentGainEnum = (CADS1294x_Gain.Gain)((registerValue >> 4) & 0x07);
            var newGainEnum = gainControl.IncreaseGain(currentGainEnum);
            SetAmplification(gainControl.GetAmplification(newGainEnum));
        }

        public void DecreaseGain()
        {
            var currentGainEnum = (CADS1294x_Gain.Gain)((registerValue >> 4) & 0x07);
            var newGainEnum = gainControl.DecreaseGain(currentGainEnum);
            SetAmplification(gainControl.GetAmplification(newGainEnum));
        }

        // Constructor
        public CADS1294x_CHANxSET()
        {
            registerValue = 0;  // Default to all bits cleared
        }


        // Enums for CHANxSET register
        public enum MUX : byte
        {
            MUX_NORMAL = 0,
            MUX_INPUT_SHORTED = 1,
            MUX_RLD_MEASUREMENTS = 2,
            MUX_SUPPLY_MEASUREMENT = 3,
            MUX_TEMPERATURE_SENSOR = 4,
            MUX_TEST_SIGNAL = 5,
            MUX_RLD_DRP = 6,
            MUX_RLD_DRN = 7
        }

        public enum PDn : byte
        {
            POWER_NORMAL = 0,
            POWER_DOWN = 1
        }
    }
}
