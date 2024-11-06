using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using WindControlLib;


namespace FeedbackDataLib_GUI
{
    public class UcFlowChartDX_NM : ucLCharts2_chart
    {
        public CLink_Track_ModuleConfig Link_Track_ModuleConfig = new();

        public void AddValue_dt_absolut_notscaled(CDataIn ci)
        {
            CYvsTimeData c = new(1)
            {
                xData = ci.DTAbsolute
            };
            c.yData[0] = ci.Value;
            AddValue(c, ci.HWcn, ci.SWcn);
        }

        public void AddValue(CYvsTimeData cYvsTimeData, CDataIn ci)
        {
            AddValue(cYvsTimeData, ci.HWcn, ci.SWcn);
        }

        public void AddValue(CYvsTimeData cYvsTimeData, int HWChannelNumber, int SWChannelNumber)
        {
            int idx_Track = Link_Track_ModuleConfig.Get_idx_Track(HWChannelNumber, SWChannelNumber);
            if (idx_Track >= 0)
            {
                AddPoint(idx_Track, cYvsTimeData);
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
                        if (double.IsInfinity(max) || double.IsInfinity(min))
                        {
                            max = 10000;
                            min = -10000;
                        }

                        ScaleChartY(cnt_active_swchannels, max, min);
                        Link_Track_ModuleConfig.LinkedValues.Add(new CLink_Track_ModuleConfig.CLinkedValues(
                            ModuleInfo.HWcn,
                            i,
                            cnt_active_swchannels,
                            i,
                            ModuleInfo.SWChannels[i].SWChannelName));
                    }
                }
            }
        }


        public void SetupFlowChart_for_Module_Default(List<CModuleBase> ModuleInfos)
        {
            Stop();
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
                                            ModuleInfos[i].HWcn,
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
            public class CDefChannel(enumModuleType ModuleType, int SW_cn)
            {
                /// <summary>
                /// The module type
                /// </summary>
                public enumModuleType ModuleType = ModuleType;
                /// <summary>
                /// The S W_CN
                /// </summary>
                public int SWcn = SW_cn;
            }

            /// <summary>
            /// The def channel
            /// </summary>
            public List<CDefChannel> DefChannel = [];

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
            public class CLinkedValues(int HW_cn, int SW_cn, int idx_Track, int idx_ModuleInfos, string SWChannelName)
            {
                public int HW_cn = HW_cn;
                public int SW_cn = SW_cn;
                public int idx_Track = idx_Track;
                public int idx_ModuleInfos = idx_ModuleInfos;
                public string SWChannelName = SWChannelName;
            }

            public List<CLinkedValues> LinkedValues;

            public CLink_Track_ModuleConfig()
            {
                LinkedValues = [];
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
