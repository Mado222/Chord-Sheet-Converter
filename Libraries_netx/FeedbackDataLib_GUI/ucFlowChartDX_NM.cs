using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using WindControlLib;


namespace FeedbackDataLib_GUI
{
    public class ucFlowChartDX_NM : ucLCharts2_chart
    {
        public CLink_Track_ModuleConfig Link_Track_ModuleConfig = new CLink_Track_ModuleConfig();

        public void AddValue_dt_absolut_notscaled(CDataIn ci)
        {
            CYvsTimeData c = new CYvsTimeData(1)
            {
                xData = ci.DT_absolute
            };
            c.yData[0] = ci.Value;
            AddValue(c, ci.HW_cn, ci.SW_cn);
        }

        public void AddValue(CYvsTimeData cYvsTimeData, CDataIn ci)
        {
            AddValue(cYvsTimeData, ci.HW_cn, ci.SW_cn);
        }

        public void AddValue(CYvsTimeData cYvsTimeData, int HWChannelNumber, int SWChannelNumber)
        {
            int idx_Track = Link_Track_ModuleConfig.Get_idx_Track(HWChannelNumber, SWChannelNumber);
            if (idx_Track >= 0)
            {
                this.AddPoint(idx_Track, cYvsTimeData);
            }
        }

        public void SetupFlowChart_for_Module(CModuleBase ModuleInfo)
        {
            Stop();
            Link_Track_ModuleConfig.LinkedValues.Clear();

            //Set Color
            BackColor = ModuleInfo.ModuleColor;

            if (ModuleInfo.SWChannels is not null)
            {
                //Count active channel with LINQ
                numberOfCharts = ModuleInfo.SWChannels.Count(channel => channel.SendChannel);

                int cnt_active_swchannels = -1;
                for (int i = 0; i < ModuleInfo.SWChannels.Count; i++)
                {
                    if (ModuleInfo.SWChannels[i].SendChannel)
                    {
                        cnt_active_swchannels++;
                        double max = ModuleInfo.SWChannels[i].SkalMax;
                        double min = ModuleInfo.SWChannels[i].SkalMin;
                        if (double.IsInfinity(max) || double.IsInfinity(min)) {
                            max = 10000;
                            min = -10000;
                        }

                        ScaleChartY(cnt_active_swchannels, max, min);
                        Link_Track_ModuleConfig.LinkedValues.Add(new CLink_Track_ModuleConfig.CLinkedValues(
                            ModuleInfo.HW_cn,
                            i,
                            cnt_active_swchannels,
                            i,
                            ModuleInfo.SWChannels[i].SWChannelName));
                    }
                }
            }
        }
          
        
        /*
        public void SetupFlowChart_for_RawData(List<CModuleBase> ModuleInfos)
        {
            this.Stop();
            this.Tracks.Clear();
            Link_Track_ModuleConfig.LinkedValues.Clear();

            for (int i = 0; i < ModuleInfos.Count; i++)
            {
                if (ModuleInfos[i].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    CTRackScaledDX TRackScaledDX = this.GetFlowChartDX1_Track(ModuleInfos[i]);
                    this.ScaleOneTrack(ref TRackScaledDX, ModuleInfos[i], 0);
                    this.Tracks.Add(TRackScaledDX);

                    Link_Track_ModuleConfig.LinkedValues.Add(new CLink_Track_ModuleConfig.CLinkedValues(
                        ModuleInfos[i].HW_cn,
                        0,
                        this.Tracks.Count - 1,
                        i, ""));
                }
            }
        }*/

        public void SetupFlowChart_for_Module_Default(List<CModuleBase> ModuleInfos)
        {
            this.Stop();
            Link_Track_ModuleConfig.LinkedValues.Clear();

            CDefaultChannels Def = new();
            for (int i = 0; i < ModuleInfos.Count; i++)
            {
                if (ModuleInfos[i] is not null)
                {
                    if (ModuleInfos[i].ModuleType != enumModuleType.cModuleTypeEmpty)
                    {
                        int cnt_active_swchannels = -1;
                        numberOfCharts = Def.DefChannel.Count;

                        for (int sw_cn = 0; sw_cn < Def.DefChannel.Count; sw_cn++)
                        {
                            var cdc = Def.DefChannel[sw_cn];
                            if (cdc.ModuleType == ModuleInfos[i].ModuleType)
                            {

                                if (ModuleInfos[i].SWChannels is not null)
                                    if (ModuleInfos[i].SWChannels[sw_cn] is not null)
                                    {
                                        ScaleChartY(sw_cn,
                                                    ymax: ModuleInfos[i].SWChannels[sw_cn].SkalMax,
                                                    ymin: ModuleInfos[i].SWChannels[sw_cn].SkalMin);

                                        Link_Track_ModuleConfig.LinkedValues.Add(item: new CLink_Track_ModuleConfig.CLinkedValues(
                                            ModuleInfos[i].HW_cn,
                                            sw_cn,
                                            cnt_active_swchannels,
                                            i,
                                            ""));
                                    }
                                break;
                            }

                        }
                    }
                }
            }
        }

        public class CDefaultChannels
        {
            /// <summary>
            /// 
            /// </summary>
            public class CDefChannel
            {
                /// <summary>
                /// The module type
                /// </summary>
                public enumModuleType ModuleType = enumModuleType.cModuleTypeEmpty;
                /// <summary>
                /// The S W_CN
                /// </summary>
                public int SW_cn = 0;

                public CDefChannel(enumModuleType ModuleType, int SW_cn)
                {
                    this.ModuleType = ModuleType;
                    this.SW_cn = SW_cn;
                }
            }

            /// <summary>
            /// The def channel
            /// </summary>
            public List<CDefChannel> DefChannel = new List<CDefChannel>();

            /// <summary>
            /// Initializes a new instance of the <see cref="CDefaultChannels"/> class.
            /// </summary>
            public CDefaultChannels()
            {
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleAtem, 1));       //
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleECG, 1));       //
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleEEG, 3));       //
                //DefChannel.Add(new CDefChannel(enumModuleType.cModuleEEGADS, 1));       //
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleExGADS94, 1));       //
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleEMG, 1));       //
                DefChannel.Add(new CDefChannel(enumModuleType.cModuleMultisensor, 2));       //
            }
        }


        /// <summary>
        /// Link between HW_cn + SW_cn, idx of Tracks and idx of Module iNfo
        /// </summary>
        public class CLink_Track_ModuleConfig
        {
            public class CLinkedValues
            {
                public int HW_cn;
                public int SW_cn;
                public int idx_Track;
                public int idx_ModuleInfos;
                public string SWChannelName;

                public CLinkedValues(int HW_cn, int SW_cn, int idx_Track, int idx_ModuleInfos, string SWChannelName)
                {
                    this.HW_cn = HW_cn;
                    this.SW_cn = SW_cn;
                    this.idx_Track = idx_Track;
                    this.idx_ModuleInfos = idx_ModuleInfos;
                    this.SWChannelName = SWChannelName;
                }
            }

            public List<CLinkedValues> LinkedValues;

            public CLink_Track_ModuleConfig()
            {
                LinkedValues = new List<CLinkedValues>();
            }

            public int Get_idx_Track(int Hw_cn, int Sw_cn)
            {
                int ret = -1;
                foreach (CLinkedValues clv in LinkedValues)
                {
                    if ((clv.HW_cn == Hw_cn) && (clv.SW_cn == Sw_cn))
                    {
                        ret = clv.idx_Track;
                        break;
                    }
                }
                return ret;
            }

            public int Get_idx_ModuleInfos(int Hw_cn, int Sw_cn)
            {
                int ret = -1;
                foreach (CLinkedValues clv in LinkedValues)
                {
                    if ((clv.HW_cn == Hw_cn) && (clv.SW_cn == Sw_cn))
                    {
                        ret = clv.idx_ModuleInfos;
                        break;
                    }
                }
                return ret;
            }

        }
    }
}
