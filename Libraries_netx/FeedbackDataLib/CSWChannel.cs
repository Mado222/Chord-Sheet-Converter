using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    /// <summary>
    /// To wrap CSWConfigValues and CSWChannelInfo in one class
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CSWChannel : ICloneable
    {

        /// <summary>Last SyncSignal received on this channel</summary>
        public DateTime SWChan_LastSync = DateTime.MinValue;

        /// <summary>Time of first Sync Signal</summary>
        public DateTime SWChan_Started = DateTime.MinValue;

        /// <summary>  Time since last sync signal</summary>
        public TimeSpan SWChan_ts_Since_LastSync;

        /// <summary>Time since first sync signal</summary>
        public TimeSpan SWChan_ts_Since_ChannelStarted;

        /// <summary>  Sample interval of this channel</summary>
        public TimeSpan SWChan_ts_SampleInt;

        public int SynPackagesreceived = 0;

        public enumModuleType ModuleType { get; set; }

        public enumSWChannelType SWChannelType_enum { get; set; }

        public int EEG_related_swcn { get; set; }

        public CSWConfigValues SWChannelConfig = new();

        public CSWConfigValues GetUserChangableValues()
        {
            return (CSWConfigValues)SWChannelConfig.Clone();
        }

        /// <summary>
        /// Type of SW channel
        /// </summary>
        /// <remarks>
        /// SWChannelType = Sample Command; Higher Byte = Module Type, Lower Byte = Channel number
        /// </remarks>
        public ushort SWChannelType
        {
            get
            {
                return (ushort)(((ushort)ModuleType << 8) + SWChannelNumber);
            }
        }

        private ushort SWChannelTypeFromDevice { get; set; } = 0;

        /// <summary>
        /// Virtual (unique) Identifier
        /// </summary>
        public ushort VirtualID { get; set; }

        /// <summary>
        /// Calculate and set Virtual Identifier
        /// </summary>
        /// <param name="ModuleTypeCount">1.. number of Modules of the same type</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <param name="sw_cn">SOftware channel number</param>
        public void SetVirtualID(uint ModuleTypeCount, enumModuleType ModuleType, uint sw_cn)
        {
            uint u = ModuleTypeCount << 8;
            u += ((uint)ModuleType) << 4;
            u += sw_cn;
            VirtualID = (ushort)u;
        }

        /// <summary>
        /// Gets the name of the SW channel.
        /// </summary>
        //public string SWChannelName => SWChannelType.SWChannelName;
        public string SWChannelName { get; set; } = "";


        /// <summary>
        /// Gets the SW channel number.
        /// </summary>
        //public byte SWChannelNumber => (byte)SWChannelType.SWChannelTypeNo;
        public byte SWChannelNumber { get; set; }


        /// <summary>
        /// To remember Max. Scale of Graph
        /// </summary>
        /// <remarks>
        /// Calculate_SkalMax_SkalMin fills this value with max possible value
        /// </remarks>
        public double SkalMax { get => SWChannelConfig.SkalMax; set => SWChannelConfig.SkalMax = value; }

        /// <summary>
        /// To remember Min. Scale of Graph
        /// </summary>
        /// <remarks>
        /// Calculate_SkalMax_SkalMin fills this value with min possible value
        /// </remarks>
        public double SkalMin { get => SWChannelConfig.SkalMin; set => SWChannelConfig.SkalMin = value; }

        public Color SWChannelColor { get; set; } = Color.White;

        /// <summary>
        /// Checks if Send Flag = false
        /// </summary>
        private void CheckSWConfig()
        {
            SWChan_ts_SampleInt = new TimeSpan(0, 0, 0, 0, SWChannelConfig.SampleInt);
            if (SWChannelConfig.SendChannel == false)
            {
                //Channel Start Time zurücksetzen
                Resync();
            }
        }

        /// <summary>
        /// Iformation about the Software Channel
        /// </summary>
        public CSWChannelInfo SWChannelInfo = new();

        /// <summary>
        ///  Properties that help to access data in CSWConfigChannel, CSWChannelInfo 
        ///  So we can use Database components to access values
        /// </summary>
        #region Access_Helper_Properties
        public ushort SampleInt
        {
            get { return SWChannelConfig.SampleInt; }
            set
            {
                SWChannelConfig.SampleInt = value;
                CheckSWConfig();
            }
        }


        /// <summary>
        /// Flag: Channel data is sent
        /// </summary>
        public bool SendChannel
        {
            get => SWChannelConfig.SendChannel;
            set
            {
                SWChannelConfig.SendChannel = value;
                CheckSWConfig();
            }
        }

        /// <summary>
        /// Flag: Channel data is saved to MMC
        /// </summary>
        public bool SaveChannel
        {
            get => SWChannelConfig.SaveChannel;
            set => SWChannelConfig.SaveChannel = value;
        }

        /// <summary>
        /// Gets the AD resolution.
        /// </summary>
        public ushort ADResolution
        {
            set => SWChannelInfo.ADResolution = value;
            get => SWChannelInfo.ADResolution;
        }

        /// <summary>
        /// Gets the mid of the DAC range
        /// </summary>
        public ushort MidofRange
        {
            set => SWChannelInfo.MidofRange = value;
            get => SWChannelInfo.MidofRange;
        }

        /// <summary>
        /// Gets the AD-uref
        /// </summary>
        public double Uref
        {
            set => SWChannelInfo.uref = value;
            get => SWChannelInfo.uref;
        }

        /// <summary>
        /// Gets the offset_hex AD offest to Zero in hex 
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public short Offset_hex
        {
            set => SWChannelInfo.Offset_hex = value;
            get => SWChannelInfo.Offset_hex;
        }

        /// <summary>
        /// Gets the skal value_k.
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public double SkalValue_k
        {
            get => SWChannelInfo.SkalValue_k;
            set
            {
                SWChannelInfo.SkalValue_k = value;
                if (value == 0)
                    SWChannelInfo.SkalValue_k = -1;
            }
        }

        /// <summary>
        /// Gets offset_d
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public double Offset_d
        {
            set => SWChannelInfo.Offset_d = value;
            get => SWChannelInfo.Offset_d;
        }
        #endregion

        public const double Max_BPM = 210;      //Max Heart Rate

        public CSWChannel()
        {
            SWChannelInfo = new CSWChannelInfo();
        }

        /// <summary>
        /// Gets the max scaled value corresponding to 2^ADResolution
        /// </summary>
        /// <returns></returns>
        public double GetMaxScaledValue() => GetScaledValue(1 << ADResolution);

        /// <summary>
        /// Gets the max scaled value corresponding to 0
        /// </summary>
        /// <returns></returns>
        public double GetMinScaledValue()
        {

            if ((SWChannelType_enum == enumSWChannelType.cSWChannelTypeAtem1)
            || (SWChannelType_enum == enumSWChannelType.cSWChannelTypePulse1)
            || (SWChannelType_enum == enumSWChannelType.cSWChannelTypeECG1)
            || (SWChannelType_enum == enumSWChannelType.cSWChannelTypeVaso1)
            || (SWChannelType_enum == enumSWChannelType.cSWChannelTypeVasoIRDig1))
            {
                return Max_BPM;
            }

            return GetScaledValue(0);
        }

        /// <summary>
        /// Gets the scaled value corresponding to HexVal
        /// </summary>
        /// <param name="HexVal">The hex val.</param>
        /// <returns>
        /// Scaled Value
        /// </returns>
        public double GetScaledValue(int HexVal)
        {
            double d = HexVal;
            // ;ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d
            d -= Offset_hex;
            d = (d * SkalValue_k) + Offset_d;

            if (SWChannelType_enum == enumSWChannelType.cSWChannelTypeSCL ||
                ((ushort)SWChannelType_enum >> 8) == (ushort)enumModuleType.cModuleSCLADS ||
                ((ushort)SWChannelType_enum >> 8) == (ushort)enumModuleType.cModuleMultiSCL)

            {
                if (d < C8CommBase.minSCLhexValue) d = C8CommBase.minSCLhexValue;

                //Calculate reciprocal
                if (d < 0) d = 0;       //There are no negative values in SCL
                if (d != 0)
                    d = 1 / d;
                else
                    d = double.MaxValue;
            }
            else if (SWChannelType_enum == enumSWChannelType.cSWChannelTypeAtem1)
            {
                //Atemfrequenz kommt als Zeit rein (Atem-Zeit [ms]) - umrechnen in Atemzüge / min
                d = IPI_ms_to_BPM(d);
            }
            else if (SWChannelType_enum == enumSWChannelType.cSWChannelTypePulse1)
            {
                //Pulse kommt als Zeit rein (IPI [ms]) - umrechnen in BPM
                d = IPI_ms_to_BPM(d);
            }
            else if (SWChannelType_enum == enumSWChannelType.cSWChannelTypeECG1)
            {
                //Pulse kommt als Zeit rein (IPI [ms]) - umrechnen in BPM
                d = IPI_ms_to_BPM(d);
            }
            else if ((SWChannelType_enum == enumSWChannelType.cSWChannelTypeVaso1) ||
                (SWChannelType_enum == enumSWChannelType.cSWChannelTypeVasoIRDig1))
            {
                //Pulse kommt als Zeit rein (IPI [ms]) - umrechnen in BPM
                d = IPI_ms_to_BPM(d);
            }
            return d;
        }

        public int GetUnscaledValue(double ScaledVal)
        {
            double d = ScaledVal;
            d = (d - Offset_d) / SkalValue_k;
            d += Offset_hex;
            return (int)d;
        }

        /// <summary>
        /// convert IPI [ms] to BPM [1/min]
        /// <remarks>
        /// WARNING: IPI= 0 will NOT be set to infinit!! It stays 0 since this is a marker for "no value"
        /// </remarks>
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        private static double IPI_ms_to_BPM(double d)
        {
            if (d < 0) d = 0;       //There are no negative values in time
            if (d != 0)
                d = 60000 / d;

            if (d > Max_BPM) d = Max_BPM;
            return d;
        }

        /// <summary>
        /// Fill properties according to corresponding structure in Device
        /// </summary>
        public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer

            SWChannelTypeFromDevice = BitConverter.ToUInt16(InBuf, ptr);
            ptr += System.Runtime.InteropServices.Marshal.SizeOf(SWChannelTypeFromDevice);
            SWChannelInfo = new CSWChannelInfo();
            ptr = SWChannelInfo.UpdateFrom_ByteArray(InBuf, ptr);
            ptr = Update_configVals_From_ByteArray(InBuf, ptr);
            CheckSWConfig();
            return ptr;
        }

        /// <summary>
        /// CXhannel is synchronized (restarted time base) with the next Sync packet
        /// </summary>
        public void Resync()
        {
            SWChan_Started = DateTime.MinValue;
            SynPackagesreceived = 0;
        }


        /// <summary>
        /// Updates the time, the sample was acquired, to a full DateTime value
        /// </summary>
        /// <remarks>
        /// As long as first sync value is not received or channel is waiting for a resync
        /// ChannelStarted == DateTime.MinValue!!!!!!
        /// </remarks>
        /// <param name="DataIn">Data</param>
        public void UpdateTime(ref CDataIn DataIn)
        {
            if (SWChan_Started != DateTime.MinValue)
            {
                //Channel is Initialised
                DataIn.Resync = false;
                if (DataIn.SyncFlag == 1)
                {
                    //It is a Sync packet, Resync
                    //For abolute time
                    SWChan_ts_Since_LastSync = new TimeSpan(0, 0, 0, 0, DataIn.SyncVal);
                    SWChan_LastSync = DataIn.LastSync;

                    //For relative time
                    //Number of received sync-Packages (1s)+DataIn.SyncVal
                    int ms_Since_ChannelStarted = C8CommBase.SyncInterval_ms * SynPackagesreceived + DataIn.SyncVal;
                    SWChan_ts_Since_ChannelStarted = new TimeSpan(0, 0, 0, 0, ms_Since_ChannelStarted);

                    DataIn.Resync = true;
                }
                else
                {
                    //Time span from LastReceivedValue
                    //For abolute time
                    SWChan_ts_Since_LastSync += SWChan_ts_SampleInt;

                    //For relative time
                    SWChan_ts_Since_ChannelStarted += SWChan_ts_SampleInt;
                }
                DataIn.LastSync = SWChan_LastSync;
                DataIn.TSSinceLastSync = SWChan_ts_Since_LastSync;
                DataIn.TSSinceChannelStarted = SWChan_ts_Since_ChannelStarted;
                DataIn.ChannelStarted = SWChan_Started;
            }
            else
            {
                //Go here until the first SYNC Value is received
                if (DataIn.SyncFlag == 1)
                {
                    //This is the first sync packet
                    SWChan_LastSync = DataIn.LastSync;
                    SWChan_Started = SWChan_LastSync;

                    SynPackagesreceived = 0;
                    SWChan_ts_Since_LastSync = new TimeSpan(0, 0, 0, 0, DataIn.SyncVal);
                    SWChan_ts_Since_ChannelStarted = SWChan_ts_Since_LastSync;
                    DataIn.Resync = true;
                }
            }
        }

        /// <summary>
        /// Fills properties from received byte array
        /// </summary>
        public int Update_configVals_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            SWChannelConfig.SampleInt = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(SWChannelConfig.SampleInt);
            SWChannelConfig.ConfigByte = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(SWChannelConfig.ConfigByte);
            return ptr;
        }

        /// <summary>
        /// Array as it can be sent to Device
        /// </summary>
        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = [];
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            byte[] b = BitConverter.GetBytes(SWChannelConfig.SampleInt);
            buf.AddRange(b);
            b = BitConverter.GetBytes(SWChannelConfig.ConfigByte);
            buf.AddRange(b);
            buffer = [.. buf];
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
            CSWChannel c = (CSWChannel)MemberwiseClone();
            c.SWChannelInfo = (CSWChannelInfo)SWChannelInfo.Clone();
            c.SWChannelConfig = (CSWConfigValues)SWChannelConfig.Clone();
            return c;
        }

        #endregion
    }
}
