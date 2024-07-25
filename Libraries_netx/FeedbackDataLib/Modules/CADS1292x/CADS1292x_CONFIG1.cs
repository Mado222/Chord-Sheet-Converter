namespace FeedbackDataLib.Modules.CADS1292x
{
    /***************************************************************
    /************************** Config 1 ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1292x_CONFIG1
    {
        public CADS1292x_CONFIG1()
        {
            Sample_Mode = enConfig1_bit7.continuous;
            Sample_Rate = enConfig1_bit2_0.sps_250;
        }
        public CADS1292x_CONFIG1(enConfig1_bit7 sample_Mode, enConfig1_bit2_0 sample_Rate)
        {
            Sample_Mode = sample_Mode;
            Sample_Rate = sample_Rate;
        }

        //Bit 7 SINGLE_SHOT: Single-shot conversion This bit sets the conversion mode
        public enum enConfig1_bit7
        {
            continuous = 0b00000000,//0 = Continuous conversion mode (default) 
            single_shot = 0b10000000 //1 = Single-shot mode
        }

        public enum enConfig1_bit2_0
        {
            sps_125 = 0b000, //fMOD / 1024 125 SPS 
            sps_250 = 0b001, //fMOD / 512 250 SPS 
            sps_500 = 0b010, //fMOD / 256 500 SPS (default)
            sps_01k = 0b011, //fMOD / 128 1 kSPS 
            sps_02k = 0b100, //fMOD / 64 2 kSPS 
            sps_04k = 0b101, //fMOD / 32 4 kSPS 
            sps_08k = 0b110 //fMOD / 16 8 kSPS
        }

        public enConfig1_bit7 Sample_Mode { get; set; }
        public enConfig1_bit2_0 Sample_Rate { get; set; }

        public byte GetConfig1()
        {
            return (byte)((byte)Sample_Mode | (byte)Sample_Rate);
        }

        public void SetConfig1(byte value)
        {
            Sample_Rate = (enConfig1_bit2_0)(value & 0b00000111);
            Sample_Mode = (enConfig1_bit7)(value & 0b10000000);
        }
    }
}

