namespace FeedbackDataLib.Modules.CADS1292x
{
    /***************************************************************
    /************************** LOFF ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1292x_LOFF
    {
        public CADS1292x_LOFF()
        {
            CompThreshold = enLOFF_bit7_5_Comp_Threshold.thr_pos_80;
            LeadOff_current = enLOFF_bit3_2_LeadOff_Current.current_06nA;
            LeadOff_AC_DC = enLOFF_bit0_LeadOff_Frequency.AC;
        }
        public CADS1292x_LOFF(enLOFF_bit7_5_Comp_Threshold compThreshold, enLOFF_bit3_2_LeadOff_Current leadOff_current, enLOFF_bit0_LeadOff_Frequency leadOff_AC_DC)
        {
            CompThreshold = compThreshold;
            LeadOff_current = leadOff_current;
            LeadOff_AC_DC = leadOff_AC_DC;
        }


        //Bits[7:5] COMP_TH[2:0]: Lead-off comparator threshold
        /*
        These bits determine the lead-off comparator threshold. See the Lead-Off Detection subsection of the ECG-Specific
        Functions section for a detailed description. 
            Comparator positive side
            000 = 95% (default)
            001 = 92.5%
            010 = 90%
            011 = 87.5%
            100 = 85%
            101 = 80%
            110 = 75%
            111 = 70%
            Comparator negative side
            000 = 5% (default)
            001 = 7.5%
            010 = 10%
            011 = 12.5%
            100 = 15%
            101 = 20%
            110 = 25%
            111 = 30%
         */
        public enum enLOFF_bit7_5_Comp_Threshold
        {
            //Comparator positive side
            thr_pos_95 = 0b00000000, // 95% (default)
            thr_pos_92 = 0b00100000, // 92.5%
            thr_pos_90 = 0b01000000, // 90%
            thr_pos_87 = 0b01100000, // 87.5%
            thr_pos_85 = 0b10000000, // 85%
            thr_pos_80 = 0b10100000, // 80%
            thr_pos_75 = 0b11000000, // 75%
            thr_pos_70 = 0b11100000 // 70%
        }

        //Bit 4      Must be set to '1'

        //Bits[3:2]  ILEAD_OFF[1:0]: Lead-off current magnitude
        //These bits determine the magnitude of current for the current lead-off mode. 
        public enum enLOFF_bit3_2_LeadOff_Current
        {
            current_06nA = 0b0000, //6 nA (default)
            current_22nA = 0b0100, //22 nA
            current_06uA = 0b1000, //6 µA
            current_22uA = 0b1100 //22 µA
        }

        //Bit 1 Must be set to '0'

        //Bit 0 FLEAD_OFF: Lead-off frequency
        //This bit selects ac or dc lead-off.
        public enum enLOFF_bit0_LeadOff_Frequency
        {
            DC = 0b00000000, //0 = At dc lead-off detect (default)
            AC = 0b00000001 //1 = At ac lead-off detect at fDR / 4 (500 Hz for an 2-kHz output rate)
        }

        public enLOFF_bit7_5_Comp_Threshold CompThreshold { get; set; }
        public enLOFF_bit3_2_LeadOff_Current LeadOff_current { get; set; }
        public enLOFF_bit0_LeadOff_Frequency LeadOff_AC_DC { get; set; }

        public byte GetLOFF()
        {
            return (byte)(0b00010000 | (byte)CompThreshold | (byte)LeadOff_current | (byte)LeadOff_AC_DC);
        }

        public void SetLOFF(byte value)
        {
            CompThreshold = (enLOFF_bit7_5_Comp_Threshold)(value & 0b11100000);
            LeadOff_current = (enLOFF_bit3_2_LeadOff_Current)(value & 0b00001100);
            LeadOff_AC_DC = (enLOFF_bit0_LeadOff_Frequency)(value & 0b00000001);
        }
    }
}

