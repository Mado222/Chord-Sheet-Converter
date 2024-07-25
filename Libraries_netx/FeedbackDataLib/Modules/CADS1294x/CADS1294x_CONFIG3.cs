namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** Config 3 ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1294x_CONFIG3
    {
        private byte registerValue;

        // Properties to access and modify specific bits in the register using enums
        public rldStatus RldStatus
        {
            get => (rldStatus)(registerValue & 0x01);
            set => registerValue = (byte)((registerValue & ~0x01) | ((byte)value & 0x01));
        }

        public rldSenseFunction RldSenseFunction
        {
            get => (rldSenseFunction)((registerValue >> 1) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 1)) | (((byte)value & 0x01) << 1));
        }

        public rldBufferPower RldBufferPower
        {
            get => (rldBufferPower)((registerValue >> 2) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 2)) | (((byte)value & 0x01) << 2));
        }

        public rldRefSignal RldRefSignal
        {
            get => (rldRefSignal)((registerValue >> 3) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 3)) | (((byte)value & 0x01) << 3));
        }

        public bool RLD_MEAS
        {
            get => (registerValue & (1 << 4)) != 0;
            set => registerValue = (byte)((registerValue & ~(0x01 << 4)) | ((value ? 1 : 0) << 4));
        }

        public vrefSetting VrefSetting
        {
            get => (vrefSetting)((registerValue >> 5) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 5)) | (((byte)value & 0x01) << 5));
        }

        public double Vref
        {
            get
            {
                if (VrefSetting == vrefSetting.VREFP_4V)
                    return 4.0;
                return 2.4;
            }
        }

        public pdRefbuf PdRefbuf
        {
            get => (pdRefbuf)((registerValue >> 7) & 0x01); // Use bit shifting and masking to get the enum value
            set => registerValue = (byte)((registerValue & ~(0x01 << 7)) | (((byte)value & 0x01) << 7)); // Set the enum value using bitwise operations
        }

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Constructor
        public CADS1294x_CONFIG3()
        {
            registerValue = 0x40;  // Default to Reserved bit set to 1
        }


        // Enums to define values for each bit field
        public enum rldStatus : byte
        {
            RLD_CONNECTED = 0,
            RLD_NOT_CONNECTED = 1
        }

        public enum rldSenseFunction : byte
        {
            RLD_SENSE_DISABLED = 0,
            RLD_SENSE_ENABLED = 1
        }

        public enum rldBufferPower : byte
        {
            RLD_BUFFER_POWER_DOWN = 0,
            RLD_BUFFER_ENABLED = 1
        }

        public enum rldRefSignal : byte
        {
            RLDREF_EXT = 0, // RLDREF signal fed externally
            RLDREF_INT_AVDD = 1 // RLDREF signal (AVDD - AVSS) / 2 generated internally
        }

        public enum vrefSetting : byte
        {
            VREFP_2_4V = 0, // VREFP is set to 2.4 V
            VREFP_4V = 1 // VREFP is set to 4 V (use only with a 5-V analog supply)
        }

        public enum pdRefbuf : byte
        {
            REFBUF_DOWN = 0,
            REFBUF_ON = 1
        }
    }
}