using WindControlLib;

namespace FeedbackDataLib.Modules.CADS1292x
{
    [Serializable]
    public partial class CADS1292x
    {
        //public CModuleExGADS1292 ModuleInfo_ADS1292x;
        //public CModuleExGADS1292 ModuleInfo_Configured;
        public CADS1292x()
        {
            _BasicConstructor();
        }

        public CADS1292x(CModuleExGADS1292 cm)
        {
            _BasicConstructor();
            //ModuleInfo_ADS1292x = cm;
        }


        private void _BasicConstructor()
        {
            CHANxSET =
            [
                new CADS1292x_CHANxSET("Ch1", CADS1292x_CHANxSET.enChanxSet_bit7_Power.PowerOn, CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01,
                CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX.Normal),
                new CADS1292x_CHANxSET("Ch2", CADS1292x_CHANxSET.enChanxSet_bit7_Power.PowerOff, CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01,
                CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX.Shorted)
            ];

            LEADOFF =
            [
                new CADS1292x_LOFF()
            ];

            LEADOFF_SENSE =
            [
                new CADS1292x_LOFF_SENSE()
            ];

            CONFIG1 =
            [
                new CADS1292x_CONFIG1()
            ];

            CONFIG2 =
            [
                new CADS1292x_CONFIG2()
            ];

            if (doubleMovingAverager == null)
            {
                doubleMovingAverager =
                [
                    new CDoubleMovingAverager(1024),
                    new CDoubleMovingAverager(1024)
                ];
            }
        }

        public List<CADS1292x_CONFIG1> CONFIG1 { get; set; }
        public List<CADS1292x_CONFIG2> CONFIG2 { get; set; }
        public List<CADS1292x_LOFF> LEADOFF { get; set; }
        public List<CADS1292x_LOFF_SENSE> LEADOFF_SENSE { get; set; }
        public List<CADS1292x_CHANxSET> CHANxSET { get; set; }


        public byte[] GetModuleSpecific()
        {
            byte[] ret = new byte[CModuleBase.ModuleSpecific_sizeof];
            int i = 0;

            ret[i] = CHANxSET[0].GetChan_Config(); i++;
            ret[i] = CHANxSET[1].GetChan_Config(); i++;
            ret[i] = CONFIG1[0].GetConfig1(); i++;
            ret[i] = CONFIG2[0].GetConfig2(); i++;
            ret[i] = LEADOFF[0].GetLOFF(); i++;
            ret[i] = LEADOFF_SENSE[0].GetLOFF_SENSE(); i++;
            return ret;
        }

        public void SetModuleSpecific(byte[] ModuleSpecific)
        {
            int i = 0;
            CHANxSET[0].SetChan_Config(ModuleSpecific[i]); i++;
            CHANxSET[1].SetChan_Config(ModuleSpecific[i]); i++;
            CONFIG1[0].SetConfig1(ModuleSpecific[i]); i++;
            CONFIG2[0].SetConfig2(ModuleSpecific[i]); i++;
            LEADOFF[0].SetLOFF(ModuleSpecific[i]); i++;
            LEADOFF_SENSE[0].SetLOFF_SENSE(ModuleSpecific[i]); i++;

            v1 = CHANxSET[0].GetGain();
            v2 = CHANxSET[1].GetGain();
        }

        //CSignalFilter_OnlineFilter ADSFilter = null;
        List<CDoubleMovingAverager> doubleMovingAverager;


        //Todo: Wie mach ma das??? Wer erzeugt die passenden fürdas jeweilige Fake-Modul?
        private CSWChannel[] arrSWChannels;
        public double v1 { get; set; }
        public double v2 { get; set; }

        public List<CSWChannel> SetupDataProcessing(List<CSWChannel> SWChannels)
        {
            this.arrSWChannels = new CSWChannel[SWChannels.Count];
            SWChannels.CopyTo(this.arrSWChannels);

            SWChannels[0].MidofRange = 0;
            SWChannels[0].Offset_d = 0;
            SWChannels[0].Offset_hex = 0;
            SWChannels[0].SkalValue_k = 2 * 2 / Math.Pow(2, 24) / v1; //Input Range Differential = (AINP-AINN) = 2VREF
            SWChannels[1].MidofRange = 0;
            SWChannels[1].Offset_d = 0;
            SWChannels[1].Offset_hex = 0;
            SWChannels[1].SkalValue_k = 2 * 2 / Math.Pow(2, 24) / v2;


            return new List<CSWChannel>(this.arrSWChannels);

        }
        //int LastADSHexValChan0 = 0;
        public List<CDataIn> AddExGSample(CDataIn dataIn)
        {
            List<CDataIn> ret = [];
            CDataIn chan = (CDataIn)dataIn.Clone();
            double d = dataIn.Value;// - 0x800000;      //‭8 388 608‬ ... -‭8 388 607
            doubleMovingAverager[dataIn.SW_cn].Push(d);
            d = doubleMovingAverager[dataIn.SW_cn].Average;
            chan.Value = dataIn.Value - 0x800000;
            //chan.Value = (int) ((dataIn.Value - d) * v);
            ret.Add(chan);
            return ret;
        }

    }
}