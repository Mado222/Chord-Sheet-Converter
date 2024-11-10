using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    public class C8Device : ICloneable
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

#pragma warning disable CS0067
        public event CModuleExGADS1294.ChangeGainEventHandler? ChangeGainEvent;
#pragma warning restore CS0067

        public C8Device()
        {
            for (int i = 0; i < C8CommBase.max_num_HWChannels; i++)
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
        public bool UpdateModuleInfoFromByteArray(byte[] inBuf)
        {
            uint[] cntTypes = new uint[Enum.GetNames(typeof(enumModuleType)).Length];
            int ptr = 0;
            ushort cntChannels = 0;

            while (ptr < inBuf.Length)
            {
                CModuleBase mi = new();
                mi.Update_UID_ModuleType_From_ByteArray(inBuf, ptr); // Only to get Module type

                // Compatibility adjustment for older firmware versions
                if (mi.HWcn == 0xff)
                {
                    mi.SetHW_cn(cntChannels);
                }
                cntChannels++;

                if (mi.HWcn < 0 || mi.HWcn >= ModuleInfos.Count)
                    return false;

                // Use a factory method or dictionary to create module instances by type
                ModuleInfos[mi.HWcn] = CreateModuleInstance(mi.ModuleType);

                ptr = ModuleInfos[mi.HWcn].UpdateFrom_ByteArray(inBuf, ptr); // Populate with full data

                // Update count and VirtualID for non-empty modules
                if (mi.ModuleType != enumModuleType.cModuleTypeEmpty && (uint)mi.ModuleType < cntTypes.Length)
                {
                    cntTypes[(uint)mi.ModuleType]++;
                    UpdateSWChannelsWithVirtualID(ModuleInfos[mi.HWcn], cntTypes[(uint)mi.ModuleType]);
                }
            }
            return true;
        }

        // Factory method for module instance creation
        private static CModuleBase CreateModuleInstance(enumModuleType moduleType) => moduleType switch
        {
            enumModuleType.cModuleMultisensor => new CModuleMultisensor(),
            enumModuleType.cModuleMultiSCL => new CModule_MultiSCL(),
            enumModuleType.cModuleEMG => new CModuleEMG(),
            enumModuleType.cModuleECG => new CModuleECG(),
            enumModuleType.cModuleEEG => new CModuleEEG(),
            enumModuleType.cModuleAtemIRDig => new CModuleRespI(),
            enumModuleType.cModuleVasosensorDig => new CModuleVasoIR(),
            enumModuleType.cModuleAtem => new CModuleAtem(),
            enumModuleType.cModuleTypeEmpty => new CModuleEmpty(),
            enumModuleType.cModuleExGADS94 => new CModuleExGADS1294_EEG(),
            _ => new CModuleBase() // Default case
        };

        // Helper method to update SWChannels with VirtualID
        private static void UpdateSWChannelsWithVirtualID(CModuleBase module, uint count)
        {
            for (int i = 0; i < module.NumSWChannels; i++)
            {
                module.SWChannels[i].SetVirtualID(count, module.ModuleType, (uint)i);
            }
        }


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
        public object Clone()
        {
            C8Device d = (C8Device)MemberwiseClone();
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
