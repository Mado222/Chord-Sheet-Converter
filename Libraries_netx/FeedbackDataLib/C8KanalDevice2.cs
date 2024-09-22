using FeedbackDataLib.Modules;
using System.Collections.Generic;
using WindControlLib;

namespace FeedbackDataLib
{
    public class C8KanalDevice2 : ICloneable
    {
        private List<CModuleBase> moduleInfos = [];

        public List<CModuleBase> ModuleInfos { get => moduleInfos; set => moduleInfos = value; }

        public List<CModuleBase> GetModuleInfo_Clone() {
            List<CModuleBase> ret = [];
            foreach (var module in moduleInfos) {
                ret.Add((CModuleBase) module.Clone());
            }
            return moduleInfos; }


        public event CModuleExGADS1294.ChangeGainEventHandler? ChangeGainEvent;

        public C8KanalDevice2()
        {
            for (int i = 0; i < C8KanalReceiverV2_CommBase.max_num_HWChannels; i++)
            {
                moduleInfos.Add(new CModuleBase());
            }
        }

        /// <summary>
        /// Resyncs all channels
        /// </summary>
        public void Resync()
        {
            foreach (CModuleBase mi in ModuleInfos)
            {
                if (mi.ModuleType != enumModuleType.cModuleTypeEmpty)
                    mi.Resync();
            }
        }


        /// <summary>
        /// Fills in every Software Channel the properties SkalMax and SkalMin with the
        /// related scaled values (max, min)
        /// </summary>
        public void Calculate_SkalMax_SkalMin()
        {
            foreach (CModuleBase mi in ModuleInfos)
            {
                if (mi.ModuleType != enumModuleType.cModuleTypeEmpty)
                    mi.Calculate_SkalMax_SkalMin();
            }
        }


        /// <summary>
        /// Fill properties according to corresponding structure in Device
        /// </summary>
        /// <remarks>
        /// Dec 2013:
        /// Count Modules of same type and Generate/Set Virtual ID
        /// </remarks>
        //public void UbpdateModuleInfoFrom_ByteArray(byte[] InBuf, bool Update_from_xml_File)
        public bool UbpdateModuleInfoFrom_ByteArray(byte[] InBuf)
        {
            uint[] cntTypes = new uint[Enum.GetNames(typeof(enumModuleType)).Length];

            int ptr = 0;
            ushort cnt_Channels = 0;

            while (ptr < InBuf.Length)
            {
                CModuleBase mi = new();
                mi.Update_UID_ModuleType_From_ByteArray(InBuf, ptr);    //Only to get Module type
                uint idx = (uint)mi.ModuleType;

                //3.11.2021 - um Kompatibilität mit NW FW Versionen < 5.0 herzustellen
                //"Leere Module werden nicht mehr dem Kanal 255 zugeordnet"
                if (mi.HW_cn == 0xff)
                {
                    mi.SetHW_cn(cnt_Channels);
                }
                cnt_Channels++;

                if (mi.HW_cn < 0 || mi.HW_cn >= ModuleInfos.Count)
                    return false;

                switch (mi.ModuleType)
                {
                   case enumModuleType.cModuleMultisensor:
                        ModuleInfos[mi.HW_cn] = new CModuleMultisensor();
                        break;
                    case enumModuleType.cModuleMultiSCL:
                        ModuleInfos[mi.HW_cn] = new CModule_MultiSCL();
                        break;
                    case enumModuleType.cModuleEMG:
                        ModuleInfos[mi.HW_cn] = new CModuleEMG();
                        break;
                    case enumModuleType.cModuleECG:
                        ModuleInfos[mi.HW_cn] = new CModuleECG();
                        break;
                    case enumModuleType.cModuleEEG:
                        ModuleInfos[mi.HW_cn] = new CModuleEEG();
                        break;
                    case enumModuleType.cModuleAtemIRDig:
                        ModuleInfos[mi.HW_cn] = new CModuleRespI();
                        break;
                    case enumModuleType.cModuleVasosensorDig:
                        ModuleInfos[mi.HW_cn] = new CModuleVasoIR();
                        break;
                    case enumModuleType.cModuleAtem:
                        ModuleInfos[mi.HW_cn] = new CModuleAtem();
                        break;
                    case enumModuleType.cModuleTypeEmpty:
                        ModuleInfos[mi.HW_cn] = new CModuleEmpty();
                        break;
                    case enumModuleType.cModuleSCLADS:
                        //ModuleInfos[mi.HW_cn] = new CModuleSCLADS1292();
                        break;
                    case enumModuleType.cModuleExGADS94:
                        //ModuleInfos[mi.HW_cn]= new CModuleExGADS1294_EEG_GUI();
                        ModuleInfos[mi.HW_cn] = new CModuleExGADS1294_EEG();
                        //ModuleInfos[mi.HW_cn] = new CModuleExGADS1294();
                        break;
                    default:
                        ModuleInfos[mi.HW_cn] = new CModuleBase();
                        break;
                }
                ptr = ModuleInfos[mi.HW_cn].UpdateFrom_ByteArray(InBuf, ptr);   //Now get the full stuff


                if ((idx < cntTypes.Length) && ModuleInfos[mi.HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty)      //damit Array Grenzen nicht überschritten werden 
                {
                    cntTypes[idx]++;    //Count modules of same type
                    //Now update SWChannels with VirtualID
                    for (int i = 0; i < ModuleInfos[mi.HW_cn].NumSWChannels; i++)
                    {
                        ModuleInfos[mi.HW_cn].SWChannels[i].SetVirtualID(cntTypes[idx], ModuleInfos[mi.HW_cn].ModuleType, (uint)i);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the time the sample was acquired to a full DateTime value
        /// </summary>
        /// <param name="DataIn">Data</param>
        public void UpdateTime(CDataIn DataIn)
        {
            if (ModuleInfos is not null)
            {
                ModuleInfos[DataIn.HW_cn].SWChannels[DataIn.SW_cn].UpdateTime(ref DataIn);
            }
        }


        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            C8KanalDevice2 d = (C8KanalDevice2)MemberwiseClone();
            d.ModuleInfos = [];
            for (int i = 0; i < ModuleInfos.Count; i++)
            {
                CModuleBase c = (CModuleBase)ModuleInfos[i].Clone();
                d.ModuleInfos.Add(c);
            }
            return d;
        }
        #endregion
    }
}
