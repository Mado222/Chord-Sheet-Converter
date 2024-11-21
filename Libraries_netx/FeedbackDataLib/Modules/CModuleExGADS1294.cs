using BMTCommunicationLib;
using FeedbackDataLib.Modules.CADS1294x;
using WindControlLib;


namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleExGADS1294 : CModuleBase
    {
        public bool visible = true;

        //Change gain
        public event EventHandler<(CModuleExGADS1294 origin, CADS1294x_Gain gain)>? ChangeGainEvent;
        protected virtual void OnChangeGainEvent(CADS1294x_Gain gain)
        {
            var handler = ChangeGainEvent;
            handler?.Invoke(this, (this, gain));
        }

        public enum EnTypeExtradat_ADS
        {
            exUa0 = 0,
            exUa1 = 1,
            exUa2 = 2,
            exUa3 = 3,
            exgain = 4,
            empty
        }

        public ExtraData<EnTypeExtradat_ADS>[][] extraDatas;
        public CEEGElectrodeData[] ElectrodeDatas;



        //////////////////
        //Configurations
        //////////////////
        //private CHP_2nd_Butterworth_float[] HP;

        const int ADRESOLUTION = 24;
        //const double MaxADVal = 1 << (ADRESOLUTION - 1); //8388608.0;
        //const double MinADVal = -1* (ADRESOLUTION - 1) +1;

        //const int UpperLimitADVal = (int)(MaxADVal * 0.95);
        //int[] MaxVal;
        //int[] MinVal;
        //int[] cntMaxVal;
        //int[] cntMinVal;
        //DateTime LastCheck, NextCheck;
        //TimeSpan CheckIntervall = new TimeSpan(0, 0, 0, 2, 0);
        //TimeSpan CheckIntervall_long = new TimeSpan(0, 0, 0, 10, 0);
        //int[] sumMinVal;
        //int[] sumMaxVal;

        private readonly long[] sumVals;
        private readonly int[] numVals;


        public string name = "base";

        protected const double Iconst = 24e-9;
        //protected const double SKALVAL_K = 2 * 2.40386E-07;
        //protected const double SKALVAL_K_div_Iconst = SKALVAL_K / Iconst;
        protected const double Rprotect = 75000;

        public CModuleExGADS1294()
        {
            _num_raw_Channels = 4;

            sumVals = new long[NumRawChannels];
            numVals = new int[NumRawChannels];

            ModuleColor = Color.LightBlue;
            ModuleName = "ExGADS1294";

            cSWChannelNames =
                [
                "ExG Ch1",
                "ExG Ch2",
                "ExG Ch3",
                "ExG Ch4",
                ];

            cSWChannelTypes =
            [
                EnSWChannelType.cSWChannelTypeExGADS0,
                EnSWChannelType.cSWChannelTypeExGADS1,
                EnSWChannelType.cSWChannelTypeExGADS2,
                EnSWChannelType.cSWChannelTypeExGADS3
            ];

            extraDatas = new ExtraData<EnTypeExtradat_ADS>[NumRawChannels][];
            int innerSize = Enum.GetValues(typeof(EnTypeExtradat_ADS)).Length;
            ElectrodeDatas = new CEEGElectrodeData[NumRawChannels];

            for (int i = 0; i < NumRawChannels; i++)
            {
                ElectrodeDatas[i] = new CEEGElectrodeData();
                extraDatas[i] = new ExtraData<EnTypeExtradat_ADS>[innerSize];
                // Optionally initialize inner arrays' elements
                for (int j = 0; j < innerSize; j++)
                {
                    extraDatas[i][j] = new ExtraData<EnTypeExtradat_ADS>(EnTypeExtradat_ADS.empty);
                }
            }
        }

        public virtual void Init()
        {
            InitModuleSpecific();
        }

        protected double GetAmplification(int raw_chan_no)
        {
            return CHANxSET[raw_chan_no].GetAmplification();
        }

        protected double GetSkalValueK(int raw_chan_no)
        {
            return 2 * CONFIG3[0].Vref / Math.Pow(2, ADRESOLUTION) / GetAmplification(raw_chan_no);
        }

        protected void Update_SkalValue_k_s(int raw_chan_no)
        {
            //ToDo: New SkalVals for SW related channels
            double new_gain = GetSkalValueK(raw_chan_no);
            for (int i = 0; i < SWChannels.Count; i++)
            {
                SWChannels[i].SkalValue_k = new_gain;
            }
        }

        protected virtual void Setup_SWChannels(string prefix = "channel")
        {
            //Copy from Neuromodul coming Channels to SWChannelsModule
            SWChannelsModule = SWChannels.ConvertAll(channel => (CSWChannel)channel.Clone());

            SWChannels = [];
            int j = 0;
            while (j < num_SWChannels_sent_by_HW) // num_raw_Channels)
            {
                if (SWChannelsModule[j].Clone() is CSWChannel sws)
                {
                    sws.SWChannelName = prefix + j.ToString() + " raw [V]";
                    sws.SWChannelNumber = (byte)j;
                    sws.ADResolution = ADRESOLUTION;
                    sws.MidofRange = 0;
                    sws.Offset_d = 0;
                    sws.Offset_hex = 0;
                    //sws.SkalValue_k = 1;
                    //sws.SkalValue_k = getSkalValue_k(j);
                    SWChannels.Add(sws);
                }
                j++;
            }
        }


        public override List<CDataIn> Processdata(CDataIn dataIn)
        {
            var ret = new List<CDataIn>();
            var chan = (CDataIn)dataIn.Clone();
            int d = dataIn.Value;

            if (dataIn.SWcn < NumRawChannels)
            {
                //ExecAutorange(dataIn); //debug weg

                //im debug weg
                //d = (int)HP[dataIn.SWChannelNumber].ProcessSample(d);

                sumVals[dataIn.SWcn] += d;
                numVals[dataIn.SWcn]++;
            }

            if (dataIn.NumExtraDat > 0)
            {
                try
                {
                    var decodedValue = BitConverter.ToInt32(CInsightDataEnDecoder.DecodeFrom7Bit(dataIn.ExtraDat), 0);
                    var extraData = extraDatas[dataIn.SWcn][dataIn.TypeExtraDat];
                    extraData.Value = decodedValue;
                    extraData.DTLastUpdated = DateTime.Now;
                    extraData.TypeExtradat = (EnTypeExtradat_ADS)dataIn.TypeExtraDat;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(value: $"process.Data: {ee.Message}");
                }
            }

            chan.Value = d;
            ret.Add(chan);

            return ret;
        }



        //public void ExecAutorange (CDataIn dataIn)
        //{
        //    int val = dataIn.Value - (int)MaxADVal;
        //    int chan = dataIn.SWChannelNumber;

        //    if (val < MinVal [chan]) 
        //    { 
        //        cntMinVal [chan]++;
        //        sumMinVal [chan] += val;
        //        if (cntMinVal [chan] >= 5)
        //        {
        //            MinVal [chan] = sumMinVal [chan] / cntMinVal [chan];
        //            sumMinVal [chan] = 0;
        //            cntMinVal [chan] = 0;
        //        }
        //    }
        //    if (val > MaxVal [chan])
        //    {
        //        cntMaxVal [chan]++;
        //        sumMaxVal [chan] += val;
        //        if (cntMaxVal [chan] >= 5)
        //        {
        //            MaxVal [chan] = sumMaxVal [chan] / cntMaxVal [chan];
        //            sumMaxVal [chan] = 0;
        //            cntMaxVal [chan] = 0;
        //        }
        //    }
        //    if (DateTime.Now > NextCheck)
        //    {
        //        //Check
        //        LastCheck = DateTime.Now;
        //        NextCheck = LastCheck + CheckIntervall;

        //        //Calc opt Amplification
        //        MaxVal [chan] = Math.Abs(MaxVal [chan]);
        //        MinVal [chan] = Math.Abs(MinVal [chan]);
        //        if (MinVal [chan] > MaxVal [chan]) MaxVal [chan] = MinVal [chan];

        //        if (MaxVal [chan] > 0)
        //        {
        //            CADS1294x_Gain g = new CADS1294x_Gain();
        //            g.Gain = (int)Math.Truncate((decimal)(UpperLimitADVal / MaxVal [chan]));
        //            if (g.Gain != gains[0])
        //            {
        //                //OnChangeGainEvent(g);
        //                //Wait longer after Change
        //                NextCheck = LastCheck + CheckIntervall_long;
        //            }
        //        }

        //        MinVal [chan] = int.MaxValue;
        //        MaxVal [chan] = int.MinValue;
        //        Autorange_checked = true;
        //    }
        //}

        //public override byte[] Get_SWConfigChannelsByteArray()
        public byte[] Get_SWConfigChannelsByteArray2()
        {
            bool[] buSend = new bool[SWChannels.Count];
            for (int i = NumRawChannels; i < SWChannels.Count; i++)
            {
                buSend[i] = SWChannels[i].SendChannel;
            }
            byte[] ret = base.Get_SWConfigChannelsByteArray(SWChannels);

            for (int i = NumRawChannels; i < SWChannels.Count; i++)
            {
                SWChannels[i].SendChannel = buSend[i];
            }

            return ret;
        }

        #region ExGADS_Params
        public List<CADS1294x_CONFIG1> CONFIG1 { get; set; } = [];
        public List<CADS1294x_CONFIG2> CONFIG2 { get; set; } = [];
        public List<CADS1294x_CONFIG3> CONFIG3 { get; set; } = [];
        public List<CADS1294x_LOFF> LEADOFF { get; set; } = [];
        public List<CADS1294x_LOFFFLIP> LOFFFLIP { get; set; } = [];
        public List<CADS1294x_CHANxSET> CHANxSET { get; set; } = [];
        //public CADS1292x_ElectrodeImp ElectrodeImpedance { get; set; }
        //public CADS1292x_ElectrodeImp ElectrodeImpedance = new CADS1292x_ElectrodeImp(num24bitChannels);

        private void InitModuleSpecific()
        {
            CADS1294x_CONFIG1 c1 = new()
            {
                ClkEn = CADS1294x_CONFIG1.EnClkEn.OscillatorClockDisabled,
                DaisyEn = CADS1294x_CONFIG1.EnDaisyEn.MultipleReadbackMode,
                DataRate = CADS1294x_CONFIG1.EnDataRate.DataRate500,
                ResMode = CADS1294x_CONFIG1.EnResMode.LowPowerMode
            };

            CONFIG1 = [c1];

            CADS1294x_CONFIG2 c2 = new()
            {
                TestSource = CADS1294x_CONFIG2.EnTestSource.TEST_SIGNAL_INTERNAL,
                TestAmp = CADS1294x_CONFIG2.EnTestAmp.TEST_AMP_2X_VREF_DIV_2400,
                TestFrequ = CADS1294x_CONFIG2.EnTestFreq.TEST_FREQ_NOT_USED,
                WctChop = CADS1294x_CONFIG2.EnWctChop.WCT_CHOP_FREQ_CONSTANT
            };

            CONFIG2 = [c2];

            CADS1294x_CONFIG3 c3 = new()
            {
                PdRefbuf = CADS1294x_CONFIG3.EnPdRefbuf.REFBUF_ON,
                RldBufferPower = CADS1294x_CONFIG3.EnRldBufferPower.RLD_BUFFER_POWER_DOWN,
                RldRefSignal = CADS1294x_CONFIG3.EnRldRefSignal.RLDREF_INT_AVDD,
                RldSenseFunction = CADS1294x_CONFIG3.EnRldSenseFunction.RLD_SENSE_DISABLED,
                VrefSetting = CADS1294x_CONFIG3.EnVrefSetting.VREFP_4V,
                RldStatus = CADS1294x_CONFIG3.EnRldStatus.RLD_NoConnection
            };

            CONFIG3 = [c3];

            LOFFFLIP =
            [
                new() {
                    RegisterValue = 0
                }
            ];
        }

        public override byte[] GetModuleSpecific()
        {
            if (CHANxSET.Count > 0)
            {
                List<byte> ret =
                [
                    0,   //Command set_Module_specific
                CHANxSET[0].RegisterValue,
                CHANxSET[1].RegisterValue,
                CONFIG1[0].RegisterValue,
                CONFIG2[0].RegisterValue,
                LEADOFF[0].RegisterValue,
                LOFFFLIP[0].RegisterValue,
                (byte)ModuleType
                ];

                ret.AddRange(new byte[16 - ret.Count]);
                return [.. ret];
            }
            return [];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleSpecific"></param>
        public override void SetModuleSpecific(byte[] ModuleSpecific)
        {
            this.ModuleSpecific = ModuleSpecific;

            /*int i = 0;
            byte enumExGADS_command = ModuleSpecific[i]; i++;
            if (enumExGADS_command == 0)
            {
                CHANxSET = [];
                for (int j = 0; j < 4; j++)
                {
                    CADS1294x_CHANxSET _CHANxSET = new() { RegisterValue = ModuleSpecific[i] };
                    i++;
                    CHANxSET.Add(_CHANxSET);
                }

                // uint8_t ADS_CONF1;
                CONFIG1 =
                [
                    new ()
                    {
                        RegisterValue = ModuleSpecific[i++]
                    }
                ];
                // uint8_t ADS_CONF2;
                CONFIG2 =
                [
                    new ()
                    {
                        RegisterValue = ModuleSpecific[i++]
                    }
                ];
                CONFIG3 =
                [
                    new ()
                    {
                        RegisterValue = ModuleSpecific[i++]
                    }
                ];
                LEADOFF =
                [
                    new ()
                    {
                        RegisterValue = ModuleSpecific[i++]
                    }
                ];
                LOFFFLIP = new List<CADS1294x_LOFFFLIP>()
                {
                    new ()
                    {
                        RegisterValue = 0
                    }
                };

            }*/

        }
        #endregion
    }
}
