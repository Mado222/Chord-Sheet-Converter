namespace FeedbackDataLib.Modules.CADS1292x
{
    /***************************************************************
   /************************** LOFF_SENSE ***************************
   /***************************************************************/
    [Serializable]
    public class CADS1292x_LOFF_SENSE
    {
        public CADS1292x_LOFF_SENSE()
        {
            LOFF1P = enLOFFSENSE_bit0_LOFF1P.disabled;
            LOFF1N = enLOFFSENSE_bit1_LOFF1N.disabled;
            LOFF2P = enLOFFSENSE_bit2_LOFF2P.disabled;
            LOFF2N = enLOFFSENSE_bit3_LOFF2N.disabled;
            FLIP1 = enLOFFSENSE_bit4_FLIP1.disabled;
            FLIP2 = enLOFFSENSE_bit5_FLIP2.disabled;
        }
        /*
        Bit 5 FLIP2: Current direction selection
            This bit controls the direction of the current used for lead-off derivation for channel 2.
            0 = Disabled(default)
            1 = Enabled
        Bit 4 FLIP1: Current direction selection
            This bit controls the direction of the current used for lead-off derivation for channel 1.
            0 = Disabled(default)
            1 = Enabled
        Bit 3 LOFF2N: Channel 2 lead-off detection negative inputs
            This bit controls the selection of negative input from channel 2 for lead-off detection.
            0 = Disabled (default)
            1 = Enabled
        Bit 2 LOFF2P: Channel 2 lead-off detection positive inputs
            This bit controls the selection of positive input from channel 2 for lead-off detection.
            0 = Disabled (default)
            1 = Enabled
        Bit 1 LOFF1N: Channel 1 lead-off detection negative inputs
            This bit controls the selection of negative input from channel 1 for lead-off detection.
            0 = Disabled (default)
            1 = Enabled
        Bit 0 LOFF1P: Channel 1 lead-off detection positive inputs
            This bit controls the selection of positive input from channel 1 for lead-off detection.
            0 = Disabled (default)
            1 = Enabled
        */
        public enum enLOFFSENSE_bit0_LOFF1P
        {
            disabled = 0b00000000,
            enabled =  0b00000001
        }

        public enum enLOFFSENSE_bit1_LOFF1N
        {
            disabled = 0b00000000,
            enabled =  0b00000010
        }

        public enum enLOFFSENSE_bit2_LOFF2P
        {
            disabled = 0b00000000,
            enabled =  0b00000100
        }
        public enum enLOFFSENSE_bit3_LOFF2N
        {
            disabled = 0b00000000,
            enabled =  0b00001000
        }
        public enum enLOFFSENSE_bit4_FLIP1
        {
            disabled = 0b00000000,
            enabled =  0b00010000
        }

        public enum enLOFFSENSE_bit5_FLIP2
        {
            disabled = 0b00000000,
            enabled =  0b00100000
        }


        public enLOFFSENSE_bit0_LOFF1P LOFF1P { get; set; }
        public enLOFFSENSE_bit1_LOFF1N LOFF1N { get; set; }
        public enLOFFSENSE_bit2_LOFF2P LOFF2P { get; set; }
        public enLOFFSENSE_bit3_LOFF2N LOFF2N { get; set; }
        public enLOFFSENSE_bit4_FLIP1 FLIP1 { get; set; }
        public enLOFFSENSE_bit5_FLIP2 FLIP2 { get; set; }



        public byte GetLOFF_SENSE()
        {
            return (byte)(((byte)LOFF1P) |
            ((byte)LOFF1N) |
            ((byte)LOFF2P) |
            ((byte)LOFF2N) |
            ((byte)FLIP1) |
            ((byte)FLIP2));
        }

        public void SetLOFF_SENSE(byte value)
        {

            LOFF1P = (enLOFFSENSE_bit0_LOFF1P)(value & 0b1);
            LOFF1N = (enLOFFSENSE_bit1_LOFF1N)(value & 0b10);
            LOFF2P = (enLOFFSENSE_bit2_LOFF2P)(value & 0b100);
            LOFF2N = (enLOFFSENSE_bit3_LOFF2N)(value & 0b1000);
            FLIP1 = (enLOFFSENSE_bit4_FLIP1)(value & 0b10000);
            FLIP2 = (enLOFFSENSE_bit5_FLIP2)(value & 0b100000);
        }
    }
}

