namespace FeedbackDataLib.Modules.CADS1294x
{
    public class CADS1294x_ElectrodeImp
    {
        private readonly List<CElectrode_InputImp[]> electrodeInfo;

        public List<CElectrode_InputImp[]> ElectrodeInfo { get => electrodeInfo; }

        public CADS1294x_ElectrodeImp(int num24bitChannels)
        {
            electrodeInfo = [];
            for (int i = 0; i < num24bitChannels; i++)
            {
                electrodeInfo.Add(new CElectrode_InputImp[2]);
            }
            foreach (var array in electrodeInfo)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new CElectrode_InputImp();
                }
            }
        }
        public void Update(byte[] bytein)
        {
            CElectrode_InputImp ei = new(bytein);
            int j = ei.IsN ? 1 : 0;
            electrodeInfo[ei.Channel_no][j] = ei;
        }

        public CElectrode_InputImp GetElectrodeInfo(int ChanNo, bool IsN)
        {
            int j = IsN ? 1 : 0;
            return electrodeInfo[ChanNo][j];
        }

        public CElectrode_InputImp Get_ElectrodeInfo_n(int ChanNo)
        {
            return GetElectrodeInfo(ChanNo, true);
        }
        public CElectrode_InputImp Get_ElectrodeInfo_p(int ChanNo)
        {
            return GetElectrodeInfo(ChanNo, false);
        }

    }


    /// <summary>
    /// Holds Electrode Information of one Input (+ or -)
    /// </summary>
    public class CElectrode_InputImp
    {
        private const int CONF_SWCH0_ADRESOLUTION = 24;
        private const double CONF_SWCH0_VREF = 4.033;
        private const double Imp_Iconst = 6e-6;
        private const double Rprotect_Ohm = 68000;              //Schutzwiderstand R4, R7, R12, R13
        private readonly int channel_no;
        private readonly double impedance_Ohm = -1;
        private readonly double uElektrode_V = -1;
        private readonly int succeeded = -1;
        private readonly int gain = -1;
        private readonly bool isN;

        public int Channel_no { get => channel_no; }
        public double Impedance_Ohm { get => impedance_Ohm; }
        public double UElektrode_V { get => uElektrode_V; }
        public int Succeeded { get => succeeded; }
        public int Gain { get => gain; }
        public bool IsN { get => isN; }

        private static double GetScaledValue(int val, int gain)
        {
            return val * CONF_SWCH0_VREF / ((1 << CONF_SWCH0_ADRESOLUTION) * gain);
        }

        public CElectrode_InputImp()
        { }

        public CElectrode_InputImp(byte[] bitin)
        {
            if (bitin != null && bitin.Length > 10)
            {
                channel_no = bitin[0] & 0x3F;                   //0b00111111;
                isN = false;
                if ((bitin[0] & 0x40) > 0) isN = true;        //0b01000000

                int i = 1;
                int UOFFxNP = BitConverter.ToInt32(bitin, i); i += 4;
                int UOFFxNPF = BitConverter.ToInt32(bitin, i); i += 4;
                gain = bitin[i]; i++;
                succeeded = bitin[i]; //i++;

                impedance_Ohm = GetScaledValue(Math.Abs(UOFFxNP - UOFFxNPF), Gain) / Imp_Iconst - Rprotect_Ohm;
                uElektrode_V = GetScaledValue(UOFFxNP + UOFFxNPF >> 1, Gain); //dUelxPN;
            }
        }
    }

}
