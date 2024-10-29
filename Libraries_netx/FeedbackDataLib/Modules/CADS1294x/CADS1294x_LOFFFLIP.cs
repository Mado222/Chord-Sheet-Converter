namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** LOFF ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1294x_LOFFFLIP
    {
        private byte registerValue;

        // Properties for each channel's LOFF flip setting using the LoffFlip enum
        public LoffFlip LOFF_FLIP1 { get => GetFlip(0); set => SetFlip(0, value); }
        public LoffFlip LOFF_FLIP2 { get => GetFlip(1); set => SetFlip(1, value); }
        public LoffFlip LOFF_FLIP3 { get => GetFlip(2); set => SetFlip(2, value); }
        public LoffFlip LOFF_FLIP4 { get => GetFlip(3); set => SetFlip(3, value); }
        public LoffFlip LOFF_FLIP5 { get => GetFlip(4); set => SetFlip(4, value); }
        public LoffFlip LOFF_FLIP6 { get => GetFlip(5); set => SetFlip(5, value); }
        public LoffFlip LOFF_FLIP7 { get => GetFlip(6); set => SetFlip(6, value); }
        public LoffFlip LOFF_FLIP8 { get => GetFlip(7); set => SetFlip(7, value); }

        // Helper methods for getting and setting bits
        private LoffFlip GetFlip(int bitPosition) => (LoffFlip)((registerValue >> bitPosition) & 0x01);
        private void SetFlip(int bitPosition, LoffFlip value) => registerValue = (byte)((registerValue & ~(1 << bitPosition)) | (((int)value & 0x01) << bitPosition));

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Constructor
        public CADS1294x_LOFFFLIP()
        {
            registerValue = 0;  // Default to all bits cleared
        }

        // Enums for LOFF_FLIP register
        public enum LoffFlip : byte
        {
            LOFF_NO_FLIP = 0, // No flip: INP is pulled to AVDD and INN pulled to AVSS
            LOFF_FLIP = 1 // Flipped: INP is pulled to AVSS and INN pulled to AVDD
        }
    }
}