namespace FeedbackDataLib.Modules.CADS1292x
{
    /***************************************************************
    /************************** Config 2 ***************************
    /***************************************************************/

    [Serializable]
    public class CADS1292x_CONFIG2
    {
        public CADS1292x_CONFIG2()
        {
            Lead_off = enConfig2_bit6_Lead_off.disabled;
            RefBuf = enConfig2_bit5_REFBUF.enabled;
            enVref = enConfig2_bit4_VREF.VREF_2V;
            Oscout = enConfig2_bit3_OscOut.disabled;
            Testsignal = enConfig2_bit1_Testsignal.disabled;
            Testfrequency = enConfig2_bit0_TestFrequency.f_1Hz;
        }
        public CADS1292x_CONFIG2(enConfig2_bit6_Lead_off lead_off, enConfig2_bit5_REFBUF refBuf, enConfig2_bit4_VREF envref, enConfig2_bit3_OscOut oscout, enConfig2_bit1_Testsignal testsignal, enConfig2_bit0_TestFrequency testfrequency)
        {
            Lead_off = lead_off;
            RefBuf = refBuf;
            enVref = envref;
            Oscout = oscout;
            Testsignal = testsignal;
            Testfrequency = testfrequency;
        }

        public double Vref
        {
            get
            {
                if (enVref == enConfig2_bit4_VREF.VREF_4V)
                {
                    return 4.033;
                }
                return 2.42;
            }
        }


        //Bit 7       Must be set to '1'

        //Bit 6       PDB_LOFF_COMP: Lead-off comparator power-down
        /*
        This bit powers down the lead-off comparators. 
            0 = Lead-off comparators disabled (default)
            1 = Lead-off comparators enabled
        */

        public enum enConfig2_bit6_Lead_off
        {
            disabled = 0b00000000,
            enabled = 0b01000000
        }

        //Bit 5      PDB_REFBUF: Reference buffer power-down
        /*
        This bit powers down the internal reference buffer so that the external reference can be used.
                0 = Reference buffer is powered down (default) 
                1 = Reference buffer is enabled
        */
        public enum enConfig2_bit5_REFBUF
        {
            disabled = 0b00000000,
            enabled = 0b00100000
        }


        //Bit 4         VREF_4V: Enables 4-V reference
        /*
        This bit chooses between 2.42-V and 4.033-V reference.
            0 = 2.42-V reference (default) 
            1 = 4.033-V reference
        */
        public enum enConfig2_bit4_VREF
        {
            VREF_2V = 0b00000000,
            VREF_4V = 0b00010000
        }

        //Bit 3     CLK_EN: CLK connection
        /* 
        This bit determines if the internal oscillator signal is connected to the CLK pin when an internal oscillator is used.
            0 = Oscillator clock output disabled (default) 
            1 = Oscillator clock output enabled
        */
        public enum enConfig2_bit3_OscOut
        {
            disabled = 0b00000000,
            enabled = 0b00001000
        }
        //Bit 2         Must be set to '0'

        //Bit 1         INT_TEST: Test signal selection
        /*
        This bit determines whether the test signal is turned on or off. 
            0 = Off (default)
            1 = On; amplitude = ±(VREFP ? VREFN) / 2400
         */

        public enum enConfig2_bit1_Testsignal
        {
            disabled = 0b00000000,
            enabled = 0b00000010
        }


        //Bit 0     TEST_FREQ: Test signal frequency
        /*
        This bit determines the test signal frequency.
            0 = At dc (default)
            1 = Square wave at 1 Hz
        */

        public enum enConfig2_bit0_TestFrequency
        {
            f_DC = 0b00000000,
            f_1Hz = 0b00000001
        }

        public enConfig2_bit6_Lead_off Lead_off { get; set; }
        public enConfig2_bit5_REFBUF RefBuf { get; set; }
        public enConfig2_bit4_VREF enVref { get; set; }
        public enConfig2_bit3_OscOut Oscout { get; set; }
        public enConfig2_bit1_Testsignal Testsignal { get; set; }
        public enConfig2_bit0_TestFrequency Testfrequency { get; set; }

        public byte GetConfig2()
        {
            return (byte)(0b10000000 | (byte)Lead_off | (byte)RefBuf | (byte)enVref | (byte)Oscout | (byte)Testsignal | (byte)Testfrequency);
        }
        public void SetConfig2(byte value)
        {
            Lead_off = (enConfig2_bit6_Lead_off)(value & 0b01000000);
            RefBuf = (enConfig2_bit5_REFBUF)(value & 0b00100000);
            enVref = (enConfig2_bit4_VREF)(value & 0b00010000);
            Oscout = (enConfig2_bit3_OscOut)(value & 0b00001000);
            Testsignal = (enConfig2_bit1_Testsignal)(value & 0b00000010);
            Testfrequency = (enConfig2_bit0_TestFrequency)(value & 0b00000001);
        }
    }
}

