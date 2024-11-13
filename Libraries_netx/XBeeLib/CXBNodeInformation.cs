using System.Collections;

namespace XBeeLib
{

    /// <summary>
    /// Node Information according to ATND (Node discover) and other important information
    /// </summary>
    public class CXBNodeInformation : IComparer<CXBNodeInformation>, IComparer, IComparable, ICloneable
    {
        //defaults
        public const XBAPIMode default_APIMode = XBAPIMode.Disabled;
        public const byte default_A2_CoordinatorAssociation = 0x00;
        public const byte default_CE_CoordinatorEnable = 0x00;
        public const byte default_A1_EndDeviceAssociation = 0x00;
        public const ushort default_MY_MyAddress = 0x0000;
        public const string default_NI_NodeIdentifier = "";
        public const uint default_SH_SerialNumberHigh = 0x0;
        public const uint default_SL_SerialNumberLow = 0x0;
        public const XBSleepMode default_SM_SleepMode = 0;
        public const ushort default_SP_CyclicSleepPeriode = 0;
        public const ushort default_ST_TimeBeforeSleep = 0;
        public const byte default_SO_SleepOption = 0;
        public const byte default_RO_PacketizationTimeout = 3;


        /// <summary>
        /// constructor 
        /// </summary>
        /// <remarks>inits the members of the object</remarks>
        public CXBNodeInformation()
        {
            //set default values
            APIMode = default_APIMode;
            A2_CoordinatorAssociation = default_A2_CoordinatorAssociation;
            CE_CoordinatorEnable = default_CE_CoordinatorEnable;
            A1_EndDeviceAssociation = default_A1_EndDeviceAssociation;
            MY_MyAddress = default_MY_MyAddress;
            NI_NodeIdentifier = default_NI_NodeIdentifier;
            RecSignalStrength = 0x00;
            SH_SerialNumberHigh = default_SH_SerialNumberHigh;
            SL_SerialNumberLow = default_SL_SerialNumberLow;
            DestinationAddress = 0;
            SM_SleepMode = default_SM_SleepMode;
            SP_CyclicSleepPeriode = default_SP_CyclicSleepPeriode;
            ST_TimeBeforeSleep = default_ST_TimeBeforeSleep;
            SO_SleepOption = default_SO_SleepOption;
            R0_PacketizationTimeout = default_RO_PacketizationTimeout;
        }

        /// <summary>
        /// All not defaulut values in Updt are copied to properties
        /// </summary>
        public void UpdateIfNotDefault(CXBNodeInformation Updt)
        {
            if (Updt.APIMode != default_APIMode) { APIMode = Updt.APIMode; };
            if (Updt.A2_CoordinatorAssociation != default_A2_CoordinatorAssociation) { A2_CoordinatorAssociation = Updt.A2_CoordinatorAssociation; };
            if (Updt.CE_CoordinatorEnable != default_CE_CoordinatorEnable) { CE_CoordinatorEnable = Updt.CE_CoordinatorEnable; };
            if (Updt.A1_EndDeviceAssociation != default_A1_EndDeviceAssociation) { A1_EndDeviceAssociation = Updt.A1_EndDeviceAssociation; };
            if (Updt.MY_MyAddress != default_MY_MyAddress) { MY_MyAddress = Updt.MY_MyAddress; };
            if (Updt.NI_NodeIdentifier != default_NI_NodeIdentifier) { NI_NodeIdentifier = Updt.NI_NodeIdentifier; };
            if (Updt.SH_SerialNumberHigh != default_SH_SerialNumberHigh) { SH_SerialNumberHigh = Updt.SH_SerialNumberHigh; };
            if (Updt.SL_SerialNumberLow != default_SL_SerialNumberLow) { SL_SerialNumberLow = Updt.SL_SerialNumberLow; };
            if (Updt.SM_SleepMode != default_SM_SleepMode) { SM_SleepMode = Updt.SM_SleepMode; };
            if (Updt.SP_CyclicSleepPeriode != default_SP_CyclicSleepPeriode) { SP_CyclicSleepPeriode = Updt.SP_CyclicSleepPeriode; };
            if (Updt.ST_TimeBeforeSleep != default_ST_TimeBeforeSleep) { ST_TimeBeforeSleep = Updt.ST_TimeBeforeSleep; };
            if (Updt.SO_SleepOption != default_SO_SleepOption) { SO_SleepOption = Updt.SO_SleepOption; };
            if (Updt.R0_PacketizationTimeout != default_RO_PacketizationTimeout) { R0_PacketizationTimeout = Updt.R0_PacketizationTimeout; };
        }

        private ushort _MY_MyAddress;
        /// <summary>
        /// MY Address (16bit)
        /// </summary>
        public ushort MY_MyAddress
        {
            set { _MY_MyAddress = value; }
            get { return _MY_MyAddress; }
        }

        private uint _SH_SerialNumberHigh;
        /// <summary>
        /// SH (Serial Number High)
        /// </summary>
        public uint SH_SerialNumberHigh
        {
            set { _SH_SerialNumberHigh = value; }
            get { return _SH_SerialNumberHigh; }
        }

        private uint _SL_SerialNumberLow;
        /// <summary>
        /// SL (Serial Number Low)
        /// </summary>
        public uint SL_SerialNumberLow
        {
            set { _SL_SerialNumberLow = value; }
            get { return _SL_SerialNumberLow; }
        }


        private byte _SO_SleepOption;
        /// <summary>
        /// SO (Sleep Option)
        /// </summary>
        public byte SO_SleepOption
        {
            get { return _SO_SleepOption; }
            set { _SO_SleepOption = value; }
        }


        /// <summary>
        /// Serial number (SH + SL)
        /// </summary>
        public ulong SerialNumber
        {
            get
            {
                return SL_SerialNumberLow + ((ulong)SH_SerialNumberHigh << 32);
            }
            set
            {
                SL_SerialNumberLow = (uint)(value & 0x00000000FFFFFFFF);
                SH_SerialNumberHigh = (uint)((value >> 32) & 0x00000000FFFFFFFF);
            }
        }

        private byte _RecSignalStrength;
        /// <summary>
        /// received signal strength (during node discover)
        /// </summary>
        public byte RecSignalStrength
        {
            get { return _RecSignalStrength; }
            set { _RecSignalStrength = value; }
        }

        private string _NI_NodeIdentifier = "";
        /// <summary>
        /// NI (node identifier)
        /// </summary>
        public string NI_NodeIdentifier
        {
            get { return _NI_NodeIdentifier; }
            set { _NI_NodeIdentifier = value; }
        }

        private XBAPIMode _APIMode;
        /// <summary>
        /// API Mode
        /// </summary>
        public XBAPIMode APIMode
        {
            get { return _APIMode; }
            set { _APIMode = value; }
        }

        // is true, if CE = 1
        private byte _CE_CoordinatorEnable;
        /// <summary>
        /// CE (Coordinator Enable)
        /// </summary>
        /// <remarks>
        /// sets the CE byte to 0x01 (Coordinator) if new set-value is greater then 0x00,
        /// sets the CE byte to 0x00 (End Device) if the new set-value is 0x00
        /// </remarks>
        public byte CE_CoordinatorEnable
        {
            get { return _CE_CoordinatorEnable; }
            set
            {
                if (value > 0x00)
                    _CE_CoordinatorEnable = 0x01;
                else
                    _CE_CoordinatorEnable = 0x00;
            }
        }

        private byte _A2_CoordinatorAssociation;
        /// <summary>
        /// A2 (Coordinator Assocation)
        /// </summary>
        public byte A2_CoordinatorAssociation
        {
            get { return _A2_CoordinatorAssociation; }
            set { _A2_CoordinatorAssociation = value; }
        }

        private byte _A1_EndDeviceAssociation;
        /// <summary>
        /// A1 (End Device Association)
        /// </summary>
        public byte A1_EndDeviceAssociation
        {
            get { return _A1_EndDeviceAssociation; }
            set { _A1_EndDeviceAssociation = value; }
        }

        private uint _BaudRate;
        /// <summary>
        /// Baud Rate
        /// </summary>
        public uint BaudRate
        {
            get { return _BaudRate; }
            set { _BaudRate = value; }
        }

        private byte _R0_PacketizationTimeout;
        /// <summary>
        /// R0 Packetization Timeout
        /// </summary>
        public byte R0_PacketizationTimeout
        {
            get { return _R0_PacketizationTimeout; }
            set { _R0_PacketizationTimeout = value; }
        }

        private XBSleepMode _SM_SleepMode;
        /// <summary>
        /// SM (Sleep Mode)
        /// </summary>
        public XBSleepMode SM_SleepMode
        {
            get { return _SM_SleepMode; }
            set { _SM_SleepMode = value; }
        }

        private ushort _ST_TimeBeforeSleep;
        /// <summary>
        /// ST (Time before Sleep)
        /// </summary>
        public ushort ST_TimeBeforeSleep
        {
            get { return _ST_TimeBeforeSleep; }
            set { _ST_TimeBeforeSleep = value; }
        }

        private ushort _SP_CyclicSleepPeriode;
        /// <summary>
        /// SP (Cyclic Slepp Periode)
        /// </summary>
        public ushort SP_CyclicSleepPeriode
        {
            get { return _SP_CyclicSleepPeriode; }
            set { _SP_CyclicSleepPeriode = value; }
        }

        /// <summary>
        /// DH, DL (Destination Address, 64bit)
        /// </summary>
        public ulong DestinationAddress { get; set; }


        #region IComparer<CXBNodeInformation> Members
        public int Compare(CXBNodeInformation? x, CXBNodeInformation? y)
        {
            if (x == null || y == null) return 1;

            if (x.APIMode == y.APIMode)
                if (x.A2_CoordinatorAssociation == y.A2_CoordinatorAssociation)
                    if (x.CE_CoordinatorEnable == y.CE_CoordinatorEnable)
                        if (x.A1_EndDeviceAssociation == y.A1_EndDeviceAssociation)
                            if (x.MY_MyAddress == y.MY_MyAddress)
                                if (x.NI_NodeIdentifier == y.NI_NodeIdentifier)
                                    if (x.SerialNumber == y.SerialNumber)
                                        if (x.SP_CyclicSleepPeriode == y.SP_CyclicSleepPeriode)
                                            if (x.ST_TimeBeforeSleep == y.ST_TimeBeforeSleep)
                                                if (x.SO_SleepOption == y.SO_SleepOption)
                                                    if (x.SM_SleepMode == y.SM_SleepMode)
                                                        if (x.BaudRate == y.BaudRate)
                                                            if (x.DestinationAddress == y.DestinationAddress)
                                                                if (x.R0_PacketizationTimeout == y.R0_PacketizationTimeout)
                                                                    return 0;   //Equal


            //TODO
            //Would return equal (0) if Addressed are equal - not of use
            //return x.MyAddress.CompareTo(y.MyAddress);
            return -1;
        }
        #endregion

        #region IComparer Members
        public int Compare(object? x, object? y)
        {
            return Compare((CXBNodeInformation?)x, (CXBNodeInformation?)y);
        }
        #endregion

        #region IComparable Members
        public int CompareTo(object? obj)
        {
            return obj == null ? throw new ArgumentNullException(nameof(obj)) : Compare(this, (CXBNodeInformation)obj);
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }


}
