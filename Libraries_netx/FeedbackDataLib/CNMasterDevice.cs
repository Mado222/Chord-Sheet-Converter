using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    public partial class CNMaster
    {
        private List<CModuleBase> moduleInfos = [];

        public List<CModuleBase> ModuleInfos { get => moduleInfos; set => moduleInfos = value; }

        public List<CModuleBase> GetModuleInfo_Clone()
        {
            List<CModuleBase> ret = [];
            foreach (var module in moduleInfos)
            {
                ret.Add((CModuleBase)module.Clone());
            }
            return moduleInfos;
        }

        /// <summary>
        /// Basic Constructor
        /// </summary>
        public CNMaster()
        {
            DeviceClock = new CCDateTime();
            BatteryVoltage = new CBatteryVoltage();

            for (int i = 0; i < MaxNumHWChannels; i++)
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
                if (mi.ModuleType != EnModuleType.cModuleTypeEmpty)
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
                if (mi.ModuleType != EnModuleType.cModuleTypeEmpty)
                    mi.Calculate_SkalMax_SkalMin();
            }
        }


        private static readonly int ModuleTypeCount = Enum.GetNames(typeof(EnModuleType)).Length;
        /// <summary>
        /// Fill properties according to corresponding structure in Device
        /// </summary>
        /// <remarks>
        /// Dec 2013:
        /// Count Modules of same type and Generate/Set Virtual ID
        /// </remarks>
        //public void UbpdateModuleInfoFrom_ByteArray(byte[] InBuf, bool Update_from_xml_File)
        public bool UpdateModuleFromByteArray(byte[] inBuf)
        {
            uint[] cntTypes = new uint[ModuleTypeCount];
            int ptr = 0;
            ushort cntChannels = 0;

            while (ptr < inBuf.Length)
            {
                CModuleBase mi = new();
                mi.UpdateUIDModuleTypeFromByteArray(inBuf, ptr); // Only to get Module type and HWcn

                // Compatibility adjustment for older firmware versions
                if (mi.HWcn == 0xff)
                {
                    mi.SetHWcn(cntChannels);
                }
                cntChannels++;

                if (mi.HWcn < 0 || mi.HWcn >= ModuleInfos.Count) return false;

                // Use a factory method or dictionary to create module instances by type
                ModuleInfos[mi.HWcn] = CreateModuleInstance(mi.ModuleType);

                ptr = ModuleInfos[mi.HWcn].UpdateFromByteArray(inBuf, ptr); // Populate with full data

                // Update count and VirtualID for non-empty modules
                if (mi.ModuleType != EnModuleType.cModuleTypeEmpty && (uint)mi.ModuleType < cntTypes.Length)
                {
                    cntTypes[(uint)mi.ModuleType]++;

                    var module = ModuleInfos[mi.HWcn];
                    var count = cntTypes[(uint)mi.ModuleType];

                    for (int i = 0; i < module.NumSWChannels; i++)
                    {
                        module.SWChannels[i].SetVirtualID(count, module.ModuleType, (uint)i);
                    }
                }
            }
            return true;
        }

        // Factory method for module instance creation
        private static CModuleBase CreateModuleInstance(EnModuleType moduleType) => moduleType switch
        {
            EnModuleType.cModuleMultisensor => new CModuleMultisensor(),
            EnModuleType.cModuleMultiSCL => new CModule_MultiSCL(),
            EnModuleType.cModuleEMG => new CModuleEMG(),
            EnModuleType.cModuleECG => new CModuleECG(),
            EnModuleType.cModuleEEG => new CModuleEEG(),
            EnModuleType.cModuleAtemIRDig => new CModuleRespI(),
            EnModuleType.cModuleVasosensorDig => new CModuleVasoIR(),
            EnModuleType.cModuleAtem => new CModuleAtem(),
            EnModuleType.cModuleTypeEmpty => new CModuleEmpty(),
            EnModuleType.cModuleExGADS94 => new CModuleExGADS1294_EEG(),
            _ => new CModuleBase() // Default case
        };


        /// <summary>
        /// Updates the time the sample was acquired to a full DateTime value
        /// </summary>
        /// <param name="DataIn">Data</param>
        public void UpdateTime(CDataIn DataIn)
        {
            ModuleInfos?[DataIn.HWcn].SWChannels[DataIn.SWcn].UpdateTime(ref DataIn);
        }


        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        //public object Clone()
        //{
        //    C8Device d = (C8Device)MemberwiseClone();
        //    d.ModuleInfos = [];
        //    for (int i = 0; i < ModuleInfos.Count; i++)
        //    {
        //        CModuleBase c = (CModuleBase)ModuleInfos[i].Clone();
        //        d.ModuleInfos.Add(c);
        //    }
        //    return d;
        //}
        #endregion
    }
}
