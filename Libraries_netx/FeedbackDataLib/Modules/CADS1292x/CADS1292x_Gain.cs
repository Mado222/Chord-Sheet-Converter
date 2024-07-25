namespace FeedbackDataLib.Modules.CADS1292x
{
    public class CADS1292x_Gain
    {
        private static int[] _gains_available = { 1, 2, 3, 4, 6, 8, 12 };

        private static byte[] _bit6_4_Gain = { 0b00010000, 0b00100000, 0b00110000,
                0b01000000, 0b00000000, 0b01010000, 0b01100000};

        private int idxgain = 0;

        public int chan_ADS { get; set; } = 0;

        public CADS1292x_Gain()
        { }

        public CADS1292x_Gain(int chan_ADS)
        {
            this.chan_ADS = chan_ADS;
        }

        public int Gain
        {
            get => _gains_available[idxgain];
            set
            {
                if (value <= _gains_available[0])
                {
                    idxgain = 0;
                    return;
                }
                if (value >= _gains_available[_gains_available.Length - 1])
                {
                    idxgain = _gains_available.Length - 1;
                    return;
                }

                //find nearest value
                for (int i = 0; i < _gains_available.Length - 1; i++)
                {
                    if (value >= _gains_available[i] && value < _gains_available[i + 1])
                    {
                        idxgain = i;
                        return;
                    }
                }
            }
        }

        public CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain enbit6_4_Gain
        {
            get => (CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain)idxgain;
            set
            {
                idxgain = Get_idx(value);
            }
        }

        public byte bit6_4_Gain
        {
            get => _bit6_4_Gain[Get_idx(Gain)];
            set
            {
                Gain = _gains_available[Get_idx(value)];
            }
        }

        private int Get_idx(int gain)
        {
            return Array.FindIndex(_gains_available, x => x == gain);
        }

        private int Get_idx(byte bit6_4_Gain)
        {
            return Array.FindIndex(_bit6_4_Gain, x => x == bit6_4_Gain);
        }

        private int Get_idx(CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain engain)
        {
            return (int)engain;
        }

        public void increaseGain()
        {
            if (idxgain < _gains_available.Length - 1)
                idxgain++;
        }
        public void decreaseGain()
        {
            if (idxgain > 0)
                idxgain--;
        }


    }
}
