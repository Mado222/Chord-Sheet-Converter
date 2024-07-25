namespace FeedbackDataLib.Modules.CADS1292x
{
    /***************************************************************
    /************************** CHANxSET ***************************
    /***************************************************************/
    [Serializable]
    public class CADS1292x_CHANxSET
    {
        public CADS1292x_CHANxSET()
        {
            Gain_obj = new CADS1292x_Gain();
            Name = "ch_Name";
            Power = enChanxSet_bit7_Power.PowerOn;
            Gain_obj.enbit6_4_Gain = CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01;
            MUX = enChanxSet_bit3_0_MUX.Normal;
            Init_CADS1292x_CHANxSET();
        }

        public CADS1292x_CHANxSET(string ch_Name, enChanxSet_bit7_Power ch_Power,
            CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain ch_Gain, enChanxSet_bit3_0_MUX ch_MUX)
        {
            Gain_obj = new CADS1292x_Gain();
            Name = ch_Name;
            Power = ch_Power;
            Gain_obj.enbit6_4_Gain = ch_Gain;
            MUX = ch_MUX;
            Init_CADS1292x_CHANxSET();
        }

        protected void Init_CADS1292x_CHANxSET()
        {
            Gain_obj = new CADS1292x_Gain();
        }

        /*
    bit 7: Power-down 0 : Normal operation, 1 : Channel power-down.
    */
        public enum enChanxSet_bit7_Power
        {
            PowerOn = 0b00000000,
            PowerOff = 0b10000000
        }

        /*
        bit 6:4 GAINn[2:0] These bits determine the PGA gain setting.
            000 : 1
            001 : 2
            010 : 4
            011 : 6
            100 : 8
            101 : 12
            110 : 24
            111 : Do not use
         */

        /*
    Gain_x06 = 0b00000000,
    Gain_x01 = 0b00010000,
    Gain_x02 = 0b00100000,
    Gain_x03 = 0b00110000,
    Gain_x04 = 0b01000000,
    Gain_x08 = 0b01010000,
    Gain_x12 = 0b01100000
    */

        public enum enChanxSet_bit6_4_Gain
        {
            Gain_x01 = 0,
            Gain_x02 = 1,
            Gain_x03 = 2,
            Gain_x04 = 3,
            Gain_x06 = 4,
            Gain_x08 = 5,
            Gain_x12 = 6
        }

        public double GetGain()
        {
            return this.Gain_obj.Gain;
        }

        //Bits[3:0]   MUX1[3:0]: Channel 1 input selection
        //These bits determine the channel 1 input selection.
        /*
            0000 = Normal electrode input (default)
            0001 = Input shorted (for offset measurements)
            0010 = RLD_MEASURE
            0011 = MVDD (2) for supply measurement
            0100 = Temperature sensor
            0101 = Test signal
            0110 = RLD_DRP (positive input is connected to RLDIN)
            0111 = RLD_DRM (negative input is connected to RLDIN)
            1000 = RLD_DRPM (both positive and negative inputs are connected to RLDIN) 
            1001 = Route IN3P and IN3N to channel 1 inputs
            1010 = Reserved
         */

        public enum enChanxSet_bit3_0_MUX
        {
            Normal = 0b00000000, //Normal electrode input (default)
            Shorted = 0b00000001, //Input shorted (for offset measurements)
            RLD = 0b00000010, //RLD_MEASURE
            MVDD = 0b00000011, //MVDD (2) for supply measurement
            Temper = 0b00000100, //Temperature sensor
            Test_Signal = 0b00000101 //Test signal
        }

        public string Name { get; set; }
        public enChanxSet_bit7_Power Power { get; set; }
        public CADS1292x_Gain Gain_obj { get; set; }

        public enChanxSet_bit6_4_Gain Gain
        {
            get
            {
                if (this.Gain_obj != null)
                {
                    return this.Gain_obj.enbit6_4_Gain;
                }
                return CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01;
            }
            set
            {
                if (this.Gain_obj != null)
                {
                    Gain_obj.enbit6_4_Gain = value;
                }
            }
        }
        public enChanxSet_bit3_0_MUX MUX { get; set; }

        public byte GetChan_Config()
        {
            return (byte)((byte)Power | Gain_obj.bit6_4_Gain | (byte)MUX);
        }

        public void SetChan_Config(byte value)
        {
            Power = (enChanxSet_bit7_Power)(value & 0b10000000);
            Gain_obj = new CADS1292x_Gain();
            Gain_obj.bit6_4_Gain = (byte)(value & 0b01110000);
            MUX = (enChanxSet_bit3_0_MUX)(value & 0b00001111);
        }
    }
}
