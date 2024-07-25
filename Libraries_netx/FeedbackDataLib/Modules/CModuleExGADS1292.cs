using WindControlLib;
using FilteringLibrary;
using FeedbackDataLib.Modules.CADS1292x;
using FeedbackDataLib.Modules;

namespace FeedbackDataLib
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleExGADS1292 : CModuleBase
    {
        public bool visible = true;

        //Change gain
        public delegate void ChangeGainEventHandler(object sender, CModuleExGADS1292 origin, CADS1292x_Gain gain);
        public event ChangeGainEventHandler ChangeGainEvent;
        protected virtual void OnChangeGainEvent(CADS1292x_Gain gain)
        {
            ChangeGainEvent?.Invoke(this, this, gain);
        }

        //////////////////
        //Configurations
        //////////////////

        public double[] gains;
        public double vref;
        private CHP_2nd_Butterworth_float[] HP;

        const int ADRESOLUTION = 24;
        const double MaxADVal = 1 << (ADRESOLUTION - 1); //8388608.0;
        const double MinADVal = -1 * (ADRESOLUTION - 1) + 1;

        const int UpperLimitADVal = (int)(MaxADVal * 0.95);
        int[] MaxVal;
        int[] MinVal;
        int[] cntMaxVal;
        int[] cntMinVal;
        DateTime LastCheck, NextCheck;
        TimeSpan CheckIntervall = new TimeSpan(0, 0, 0, 2, 0);
        TimeSpan CheckIntervall_long = new TimeSpan(0, 0, 0, 10, 0);
        int[] sumMinVal;
        int[] sumMaxVal;
        long[] sumVals;
        int[] numVals;
        bool Autorange_checked = false;

        public string name = "base";

        public CModuleExGADS1292()
        {
            _num_raw_Channels = 2;
            Init();
        }

        public virtual void Init()
        {
            MaxVal = new int[num_raw_Channels];
            MinVal = new int[num_raw_Channels];
            cntMaxVal = new int[num_raw_Channels];
            cntMinVal = new int[num_raw_Channels];
            sumMinVal = new int[num_raw_Channels];
            sumMaxVal = new int[num_raw_Channels];
            sumVals = new long[num_raw_Channels];
            numVals = new int[num_raw_Channels];

            ModuleColor = Color.LightBlue;
            ModuleName = "ExGADS1292";

            cSWChannelNames =
                [
                "ExG Ch1",
                "ExG Ch2",
                "DC Ch1",
                "DC Ch2"
                ];

            cSWChannelTypes = new enumSWChannelType[]
            {
                enumSWChannelType.cSWChannelTypeExGADS0,
                enumSWChannelType.cSWChannelTypeExGADS1,
                enumSWChannelType.cSWChannelTypeExGADS2,
                enumSWChannelType.cSWChannelTypeExGADS3
            };

            InitModuleSpecific();

            HP = new CHP_2nd_Butterworth_float[num_raw_Channels];
            for (int i = 0; i < HP.Length; i++)
            {
                HP[i] = new CHP_2nd_Butterworth_float(
                    CHP_2nd_Butterworth_float.enFilter_Params.HP_fg_0o5_100Hz,
                    CHP_2nd_Butterworth_float.enFilter_Form.Form1);
            }

            if (gains == null)
                gains = new double[num_raw_Channels];
        }

        protected double getGain(int raw_chan_no)
        {
            return 2 * vref / Math.Pow(2, ADRESOLUTION) / gains[raw_chan_no];
        }

        protected void UpdateGain(int raw_chan_no, CADS1292x_Gain gain)
        {
            if (SWChannels != null)
            {
                gains[raw_chan_no] = gain.Gain;
                double new_gain = getGain(raw_chan_no);
                for (int i = 0; i < SWChannels.Count; i++)
                {
                    SWChannels[i].SkalValue_k = new_gain;
                }
            }
        }


        protected virtual void Setup_SWChannels(string prefix = "channel")
        {
            if (SWChannels != null && sWChannels != null)
            {
                //Copy from Neuromodul coming Channels to sWChannels_Module
                sWChannels_Module.Clear();
                for (int i = 0; i < SWChannels.Count; i++)
                {
                    CSWChannel swc = (CSWChannel)SWChannels[i].Clone();
                    sWChannels_Module.Add(swc);
                }

                SWChannels = [];
                int j = 0;
                while (j < num_raw_Channels)
                {
                    CSWChannel sws = (CSWChannel)SWChannels_Module[j].Clone();
                    sws.SWChannelName = prefix + j.ToString() + " raw [V]";
                    sws.SWChannelNumber = (byte)j;
                    sws.ADResolution = ADRESOLUTION;
                    sws.MidofRange = 0;
                    sws.Offset_d = 0;
                    sws.Offset_hex = 0;
                    sws.SkalValue_k = 1;
                    sws.SkalValue_k = getGain(j);
                    sWChannels.Add(sws);
                    j++;
                }
            }
        }


        public override List<CDataIn> Processdata(CDataIn dataIn)
        {
            List<CDataIn> ret = [];
            CDataIn chan = (CDataIn)dataIn.Clone();
            int d = dataIn.Value;// - 0x800000;      //‭8 388 608‬ ... -‭8 388 607

            if (dataIn.SW_cn < num_raw_Channels)
            {
                //ExecAutorange(dataIn); //debug weg
                
                //im debug weg
                //d = (int)HP[dataIn.SWChannelNumber].ProcessSample(d);
                sumVals[dataIn.SW_cn] += d;
                numVals[dataIn.SW_cn]++;
            }

            chan.Value = d;
            ret.Add(chan);
            return ret;
        }


        public void ExecAutorange (CDataIn dataIn)
        {
            int val = dataIn.Value - (int)MaxADVal;
            int chan = dataIn.SW_cn;

            if (val < MinVal [chan]) 
            { 
                cntMinVal [chan]++;
                sumMinVal [chan] += val;
                if (cntMinVal [chan] >= 5)
                {
                    MinVal [chan] = sumMinVal [chan] / cntMinVal [chan];
                    sumMinVal [chan] = 0;
                    cntMinVal [chan] = 0;
                }
            }
            if (val > MaxVal [chan])
            {
                cntMaxVal [chan]++;
                sumMaxVal [chan] += val;
                if (cntMaxVal [chan] >= 5)
                {
                    MaxVal [chan] = sumMaxVal [chan] / cntMaxVal [chan];
                    sumMaxVal [chan] = 0;
                    cntMaxVal [chan] = 0;
                }
            }
            if (DateTime.Now > NextCheck)
            {
                //Check
                LastCheck = DateTime.Now;
                NextCheck = LastCheck + CheckIntervall;

                //Calc opt Amplification
                MaxVal [chan] = Math.Abs(MaxVal [chan]);
                MinVal [chan] = Math.Abs(MinVal [chan]);
                if (MinVal [chan] > MaxVal [chan]) MaxVal [chan] = MinVal [chan];

                if (MaxVal [chan] > 0)
                {
                    CADS1292x_Gain g = new CADS1292x_Gain();
                    g.Gain = (int)Math.Truncate((decimal)(UpperLimitADVal / MaxVal [chan]));
                    if (g.Gain != gains[0])
                    {
                        OnChangeGainEvent(g);
                        //Wait longer after Change
                        NextCheck = LastCheck + CheckIntervall_long;
                    }
                }

                MinVal [chan] = int.MaxValue;
                MaxVal [chan] = int.MinValue;
                Autorange_checked = true;
            }
        }

        public override byte[] Get_SWConfigChannelsByteArray()
        {
            bool[] buSend = new bool[SWChannels.Count];
            for (int i = num_raw_Channels; i < SWChannels.Count; i++)
            {
                buSend[i] = SWChannels[i].SendChannel;
            }
            byte [] ret = base.Get_SWConfigChannelsByteArray(SWChannels);

            for (int i = num_raw_Channels; i < SWChannels.Count; i++)
            {
                SWChannels[i].SendChannel = buSend[i];
            }

            return ret;
        }

        #region ExGADS_Params
        public List<CADS1292x_CONFIG1> CONFIG1 { get; set; }
        public List<CADS1292x_CONFIG2> CONFIG2 { get; set; }
        public List<CADS1292x_LOFF> LEADOFF { get; set; }
        public List<CADS1292x_LOFF_SENSE> LEADOFF_SENSE { get; set; }
        public List<CADS1292x_CHANxSET> CHANxSET { get; set; }
        public CADS1292x_ElectrodeImp ElectrodeImpedance { get; set; }
        //public CADS1292x_ElectrodeImp ElectrodeImpedance = new CADS1292x_ElectrodeImp(num24bitChannels);

        private void InitModuleSpecific()
        {
            CHANxSET =
            [
                new CADS1292x_CHANxSET("Ch1", CADS1292x_CHANxSET.enChanxSet_bit7_Power.PowerOn,
                CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01,
                CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX.Normal),

                new CADS1292x_CHANxSET("Ch2", CADS1292x_CHANxSET.enChanxSet_bit7_Power.PowerOff,
                CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain.Gain_x01,
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

            ElectrodeImpedance = new CADS1292x_ElectrodeImp(num_raw_Channels);
        }

        public override byte[] GetModuleSpecific()
        {
            List<byte> ret =
            [
                0,   //Command set_Module_specific
                CHANxSET[0].GetChan_Config(),
                CHANxSET[1].GetChan_Config(),
                CONFIG1[0].GetConfig1(),
                CONFIG2[0].GetConfig2(),
                LEADOFF[0].GetLOFF(),
                LEADOFF_SENSE[0].GetLOFF_SENSE(),
                (byte)ModuleType, // uint8_t Configured_Module_Type;
                //0, // uint8_t ADS_LEAD_OFF_DETECTED;
                //Vals_for_chan_no //Impedance_Info;
            ];
            ret.AddRange(new byte[16 - ret.Count]);
            return ret.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleSpecific"></param>
        public override void SetModuleSpecific(byte[] ModuleSpecific)
        {
            this.ModuleSpecific = ModuleSpecific;
            int i = 0;
            byte enumExGADS_command = ModuleSpecific[i]; i++;
            if (enumExGADS_command == 0)
            {
                CHANxSET[0].SetChan_Config(ModuleSpecific[i]); i++; //uint8_t ADS_CH1_SET;
                CHANxSET[1].SetChan_Config(ModuleSpecific[i]); i++; // uint8_t ADS_CH2_SET;
                CONFIG1[0].SetConfig1(ModuleSpecific[i]); i++;  // uint8_t ADS_CONF1;
                CONFIG2[0].SetConfig2(ModuleSpecific[i]); i++; // uint8_t ADS_CONF2;
                LEADOFF[0].SetLOFF(ModuleSpecific[i]); i++; // uint8_t ADS_LEAD_OFF_CONF;
                LEADOFF_SENSE[0].SetLOFF_SENSE(ModuleSpecific[i]); i++; // uint8_t ADS_LEAD_OFF_SENSE;
                //i++; // uint8_t ADS_LEAD_OFF_DETECTED;
                //i++; // uint8_t Configured_Module_Type;
            }
            vref = CONFIG2[0].Vref; //Muss vorher stehen!!
            UpdateGain(0, CHANxSET[0].Gain_obj);
            UpdateGain(1, CHANxSET[1].Gain_obj);
            
        }
        #endregion
    }
}
