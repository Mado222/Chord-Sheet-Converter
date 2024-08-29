using WindControlLib;
using System.Linq; 

namespace FeedbackDataLib.Modules
{

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleBase : ICloneable
    {
        protected int _num_raw_Channels = 4;
        public int num_raw_Channels { get => _num_raw_Channels; }

        public const int ModuleSpecific_sizeof = 16;
        protected byte[] ModuleSpecific = new byte[ModuleSpecific_sizeof];

        protected byte num_SWChannels_sent_by_HW = C8KanalReceiverV2_CommBase.num_SWChannels_sent_by_HW;

        public CModuleBase()
        {
            SWChannels = [];
            ModuleName = "Base Module";
        }

        public virtual void SetModuleSpecific(byte[] value)
        {
            if (value != null)
                ModuleSpecific = value;
        }

        public virtual byte[] GetModuleSpecific()
        {
            return ModuleSpecific;
        }


        public byte[] UUID_bytearray { get; } = new byte[16];
        public string UUID => System.Text.Encoding.ASCII.GetString(UUID_bytearray);

        protected List<string> cSWChannelNames =
        [
            "SWChan0",
            "SWChan1",
            "SWChan2",
            "SWChan3",
        ];

        protected enumSWChannelType[] cSWChannelTypes =
{
            enumSWChannelType.cSWChannelTypeNotDefined,
            enumSWChannelType.cSWChannelTypeNotDefined,
            enumSWChannelType.cSWChannelTypeNotDefined,
            enumSWChannelType.cSWChannelTypeNotDefined,
        };


        protected virtual void Setup_SWChannels()
        {
            if (SWChannels != null)
            {
                foreach (var (channel, i) in SWChannels.Select((channel, index) => (channel, index)))
                {
                    channel.SWChannelNumber = (byte)i;
                    channel.SWChannelColor = ModuleColor;
                    channel.SWChannelName = cSWChannelNames[i];
                    channel.SWChannelType_enum = cSWChannelTypes[i];
                    channel.ModuleType = ModuleType;
                }
            }
        }

        public class ExtraData <T>
        {
            public double Value { get; set; } = -1;
            public DateTime DTLastUpdated = DateTime.MinValue;

            public ExtraData(T typeExtradat)
            {
                TypeExtradat = typeExtradat;
            }

            public T TypeExtradat { get; set; }
        }


        /// <summary>
        /// Hardware Channel number
        /// </summary>
        public ushort HW_cn { get; private set; }

        public void SetHW_cn(ushort HW_cn) => this.HW_cn = HW_cn;

        /// <summary>
        /// Gets or sets the SW channels for the GUI
        /// </summary>
        public List<CSWChannel> SWChannels { get; set; } = [];

        /// <summary>
        /// SWChannels die vom Modul kommen
        /// </summary>
        protected List<CSWChannel> sWChannels_Module = [];
        /// <summary>
        /// Gets or sets the SW channels for the Module side
        /// </summary>
        public List<CSWChannel> SWChannels_Module
        {
            get => GetSWChannelsModule();
            set => SetSWChannelsModule(value);
        }

        /// <summary>SWChannels to Communicate with Module (eg ADS)</summary>
        /// <returns>The sw channels module.</returns>
        public virtual List<CSWChannel> GetSWChannelsModule() => sWChannels_Module;

        /// <summary>SWChannels to Communicate with Module (eg ADS)</summary>
        /// <param name="value">The sw channels module.</param>
        public virtual void SetSWChannelsModule(List<CSWChannel> value) => sWChannels_Module = value;

        /// <summary>
        /// Gets the number of SW channels.
        /// </summary>
        public byte NumSWChannels
        {
            get
            {
                if (SWChannels != null)
                {
                    return (byte)SWChannels.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// Indicates if the Module has Power Supply
        /// </summary>
        /// <value>
        ///   <c>true</c> Mudule is active / supplied; otherwise, <c>false</c>.
        /// </value>
        public bool IsModuleActive()
        {
            for (int i = 0; i < SWChannels.Count; i++)
            if (SWChannels[i].SendChannel)
                    return true;
            return false;
        }


        /// <summary>
        /// Firmware Version
        /// </summary>
        /// <remarks>upper 4 bit: major version number, lower 12 bit minor version number</remarks>
        public ushort SWRevision { get; set; }

        /// <summary>
        /// Software Version as string
        /// </summary>
        /// <remarks>upper 4 bit: major version number, lower 12 bit minor version number</remarks>
        public string SWRevision_string
        {
            get
            {
                int maj = SWRevision;
                maj = (maj >> 12) & 0x000F;
                int min = SWRevision;
                min &= 0x0FFF;
                return maj.ToString() + "." + min.ToString();
            }
        }

        /// <summary>
        /// Gets the related module color
        /// </summary>
        public Color ModuleColor { get; protected set; } = Color.White;

        /// <summary>
        /// Gets the module type as string
        /// </summary>
        //public string ModuleType_string => _ModuleType.ModuleName;
        public string ModuleType_string { get { return ModuleName; } }

        /// <summary>
        /// Gets a value indicating whether Module was nor properly flashed => bootloader error
        /// </summary>
        /// <value>
        /// <c>true</c> if [module bootloader error]; otherwise, <c>false</c>.
        /// </value>
        public bool ModuleBootloaderError { get; private set; }

        /// <summary>
        /// For internal storage
        /// </summary>
        protected enumModuleType _ModuleType = enumModuleType.cModuleTypeEmpty;
        protected enumModuleType _ModuleType_Unmodified = enumModuleType.cModuleTypeEmpty;

        /// <summary>
        /// Gets the type of the module.
        /// </summary>
        public enumModuleType ModuleType { get; protected set; }
        public enumModuleType ModuleType_Unmodified { get; protected set; }

        /// <summary>
        /// Gets the module type number
        /// </summary>
        public byte ModuleTypeNumber { get; protected set; }
        public byte ModuleTypeNumber_Unmodified { get; private set; }

        public byte ModuleTechnologyNo { get; private set; }

        /// <summary>
        /// Gets the module revision.
        /// </summary>
        public byte ModuleRevision { get; private set; }

        public byte HWRevision { get; private set; }

        public string ModuleName { get; protected set; }

        public virtual byte[] Get_SWConfigChannelsByteArray()
        {
            if (SWChannels is not null)
                return Get_SWConfigChannelsByteArray(SWChannels);
            return [];
        }

        public virtual byte[] Get_SWConfigChannelsByteArray(List<CSWChannel> swc)
        {
            List<byte> buf =
            [
                (byte)HW_cn   //First byte
            ];
            for (int SW_cn = 0; SW_cn < swc.Count; SW_cn++)
            {
                buf.AddRange(Get_SWConfigChannelByteArray(swc, SW_cn));
            }
            buf.AddRange(UUID_bytearray);
            //return buf.ToArray();
            return [.. buf];
        }

        protected virtual byte[] Get_SWConfigChannelByteArray(List<CSWChannel> swc, int SW_cn)
        {
            byte[] buf1 = [];
            swc[SW_cn].GetByteArray(ref buf1, 0);
            return buf1;
        }

        /// <summary>
        /// Resyncs all SWChannels
        /// </summary>
        public void Resync()
        {
            if (SWChannels is not null)
            {
                foreach (CSWChannel cs in SWChannels)
                {
                    cs.Resync();
                }
            }
        }

        /// <summary>
        /// Fills in every Software Channel the properties SkalMax and SkalMin with the
        /// related scaled values (max, min)
        /// </summary>
        public void Calculate_SkalMax_SkalMin()
        {
            if (SWChannels is not null)
            {
                foreach (CSWChannel swc in SWChannels)
                {
                    swc.SkalMax = swc.GetMaxScaledValue();
                    swc.SkalMin = swc.GetMinScaledValue();
                    if (swc.SkalMax < swc.SkalMin)
                    {
                        //Vertauschen with Tuple
                        (swc.SkalMin, swc.SkalMax) = (swc.SkalMax, swc.SkalMin);
                    }
                }
            }
        }


        public virtual int Update_UID_ModuleType_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            Array.Copy(InBuf, ptr, UUID_bytearray, 0, UUID_bytearray.Length); ptr += UUID_bytearray.Length;
            ushort moduleTypeFromDevice = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(moduleTypeFromDevice);
            Update_ModuleTypeFromDevice(moduleTypeFromDevice);
            SWRevision = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(SWRevision);
            num_SWChannels_sent_by_HW = InBuf[ptr]; ptr += System.Runtime.InteropServices.Marshal.SizeOf(num_SWChannels_sent_by_HW);
            HW_cn = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(HW_cn);
            return ptr;
        }

        /// <summary>
        /// Fill properties according to corresponding structure in Device
        /// </summary>
        public virtual int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Update_UID_ModuleType_From_ByteArray(InBuf, Pointer_To_Array_Start);
            sWChannels_Module = [];

            //for (uint i = 0; i < C8KanalReceiverV2_CommBase.num_SWChannels_sent_by_HW; i++)
            for (uint i = 0; i < num_SWChannels_sent_by_HW; i++)
            {
                CSWChannel swc = new();
                ptr = swc.UpdateFrom_ByteArray(InBuf, ptr);
                SWChannels.Add(swc);
            }

            //Get Module specific struct
            Buffer.BlockCopy(InBuf, ptr, ModuleSpecific, 0, ModuleSpecific.Length);
             ptr += ModuleSpecific.Length;

            Setup_SWChannels();
            SetModuleSpecific(ModuleSpecific);
            return ptr;
         }

        /// <summary>
        /// Processes the data, coming drom distributor thread
        /// </summary>
        /// <param name="di">The di.</param>
        /// <returns></returns>
        public virtual List<CDataIn> Processdata(CDataIn di)
        {
            List<CDataIn> ret = [di];
            return ret;
        }

        public virtual void Update_ModuleTypeFromDevice(ushort ModuleTypeFromDevice)
        {
            ModuleBootloaderError = false;
            //if ((ModuleTypeFromDevice & 0x8000) > 0)
            if (ModuleTypeFromDevice > 0x8000)
            {
                //Module Error
                ModuleBootloaderError = true;
                ModuleTypeFromDevice = (ushort)(ModuleTypeFromDevice & 0x7FFF);
            }

            ModuleTypeNumber = (byte)(CMyConvert.HighByte(ModuleTypeFromDevice) & 0xf);
            ModuleType = (enumModuleType)ModuleTypeNumber;
            ModuleType_Unmodified = ModuleType;
            HWRevision = CMyConvert.LowByte(ModuleTypeFromDevice);

            ModuleTechnologyNo = (byte)(ModuleTypeNumber & 0xF0);
            ModuleTechnologyNo = CMyConvert.SwapNibbles(ModuleTechnologyNo);

            ModuleTypeNumber_Unmodified = ModuleTypeNumber;

            ModuleRevision = CMyConvert.LowByte(ModuleTypeFromDevice);

            ModuleName = ModuleName + " " +
                ModuleRevision.ToString() + "." +
                ModuleTechnologyNo.ToString();
        }

        /// <summary>
        /// Updates all Channels
        /// </summary>
        /// <param name="sWConfigValues">Array with configuratin info.</param>
        /// <returns></returns>
        public virtual void Update_SWChannels(CSWConfigValues[] sWConfigValues)
        {
            for (int i = 0; i < sWConfigValues.Length; i++)
            {
                Update_SWChannel(i, sWConfigValues[i]);
            }
        }

        public virtual void Update_SWChannels(List<CSWChannel> sWChannels)
        {
            for (int i =0; i<sWChannels.Count; i++)
            {
                Update_SWChannel(i, sWChannels[i].SWChannelConfig);
            }
        }

        /// <summary>
        /// Updates the indexed SWCHannel with CSWConfigValues
        /// 
        /// </summary>
        /// <param name="swcn">The SWCN.</param>
        /// <param name="sWConfigValues">The configuration values.</param>
        /// <returns></returns>
        public virtual void Update_SWChannel(int swcn, CSWConfigValues sWConfigValues)
        {
            SWChannels[swcn].SWChannelConfig.Update(sWConfigValues);
        }

        public object Clone()
        {
            CModuleBase mi = (CModuleBase)MemberwiseClone();
            mi.SWChannels = [];
            for (int i = 0; i < SWChannels.Count; i++)
            {
                CSWChannel c = (CSWChannel)SWChannels[i].Clone();
                mi.SWChannels.Add(c);
            }
            return mi;
        }

    }
}
