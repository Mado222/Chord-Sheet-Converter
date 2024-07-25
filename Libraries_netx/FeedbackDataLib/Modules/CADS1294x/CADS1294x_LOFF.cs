namespace FeedbackDataLib.Modules.CADS1294x
{
    /***************************************************************
    /************************** LOFF ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1294x_LOFF
    {
        private byte registerValue;

        // Properties to access and modify specific bits in the register using enums
        public LeadOffFreq FLEAD_OFF
        {
            get => (LeadOffFreq)(registerValue & 0x03);
            set => registerValue = (byte)((registerValue & ~0x03) | ((byte)value & 0x03));
        }

        public LeadOffCurrentMag ILEAD_OFF
        {
            get => (LeadOffCurrentMag)((registerValue >> 2) & 0x03);
            set => registerValue = (byte)((registerValue & ~(0x03 << 2)) | (((byte)value & 0x03) << 2));
        }

        public LeadOffDetectMode VLEAD_OFF_EN
        {
            get => (LeadOffDetectMode)((registerValue >> 4) & 0x01);
            set => registerValue = (byte)((registerValue & ~(0x01 << 4)) | (((byte)value & 0x01) << 4));
        }

        public CompTh COMP_TH
        {
            get => (CompTh)((registerValue >> 5) & 0x07);
            set => registerValue = (byte)((registerValue & ~(0x07 << 5)) | (((byte)value & 0x07) << 5));
        }

        public byte RegisterValue
        {
            get => registerValue;
            set => registerValue = value;
        }

        // Constructor
        public CADS1294x_LOFF()
        {
            registerValue = 0b01000000;  // Default value as per #define LEAD_OFF_Reg_default
        }
    

    // Enums to define values for each bit field
    public enum CompTh : byte
    {
        COMP_TH_95 = 0,
        COMP_TH_92_5,
        COMP_TH_90,
        COMP_TH_87_5,
        COMP_TH_85,
        COMP_TH_80,
        COMP_TH_75,
        COMP_TH_70
    }

    public enum LeadOffDetectMode : byte
    {
        LEAD_OFF_DETECT_CURRENT_SOURCE = 0,
        LEAD_OFF_DETECT_RESISTOR = 1
    }

    public enum LeadOffCurrentMag : byte
    {
        CURRENT_MAG_6NA = 0,
        CURRENT_MAG_12NA,
        CURRENT_MAG_18NA,
        CURRENT_MAG_24NA
    }

    public enum LeadOffFreq : byte
    {
        LEAD_OFF_FREQ_NORMAL = 0, // Follow datasheet instructions for FLEAD[1:0]
        LEAD_OFF_FREQ_AC = 1, // AC lead-off detection at fDR / 4
        LEAD_OFF_FREQ_UNUSED = 2, // Do not use
        LEAD_OFF_FREQ_DC = 3 // DC lead-off detection turned on
    }
}}