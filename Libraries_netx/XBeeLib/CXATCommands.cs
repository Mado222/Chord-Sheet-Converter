namespace XBeeLib
{
    /// <summary>
    /// Provides or genertes AT Commands
    /// </summary>
    public static class CXBATCommands
    {

        /// <summary>
        /// important Commands - for entering the command mode / response
        /// </summary>
        public const string XBAT_EnterCommandMode = "+++";
        public const string XBAT_OKResponse = "OK\r";
        public const string XBAT_ERRORResponse = "ERROR\r";

        /// <summary>
        /// AT Commands - Special
        /// </summary>

        //Write
        public const string XBAT_Write_to_nonvolatilememory = "ATWR\r";

        //Apply Changes
        public const string XBAT_Apply_Changes = "ATAC\r";

        // Restore module parameters to factory defaults.
        public const string XBAT_RestoreDefaults = "ATRE\r";

        // Software Reset. Responds immediately with an OK then performs a hard reset ~100ms later.
        public const string XBAT_SotfwareReset = "ATFR\r";


        /// <summary>
        /// AT Commands for Networking and Security
        /// </summary>

        //Channel
        public const string XBAT_GetChannel = "ATCH\r";
        public static string XBAT_SetChannel(byte channel)
        {
            return "ATCH " + channel.ToString("X2") + "\r";
        }

        //PAN ID
        public const string XBAT_GetPanId = "ATID\r";
        public static string XBAT_SetPanId(ushort panId)
        {
            return "ATID " + panId.ToString("X4") + "\r";
        }

        //Destination Address Low
        public const string XBAT_GetDestinationAddressLow = "ATDL\r";
        public static string XBAT_SetDestinationAddressLow(uint Address)
        {
            return "ATDL " + Address.ToString("X8") + "\r";
        }
        public static string XBAT_SetDestinationAddressLow(ulong Address)
        {
            uint L = (uint)(0x00000000FFFFFFFF & Address);
            return "ATDL " + L.ToString("X8") + "\r";
        }

        //Destination Address High
        public const string XBAT_GetDestinationAddressHigh = "ATDH\r";
        public static string XBAT_SetDestinationAddressHigh(uint Address)
        {
            return "ATDH " + Address.ToString("X8") + "\r";
        }
        public static string XBAT_SetDestinationAddressHigh(ulong Address)
        {
            uint H = (uint)(0x00000000FFFFFFFF & (Address >> 32));
            return "ATDH " + H.ToString("X8") + "\r";
        }


        /// <summary>
        /// Sets the Destination Address 
        /// </summary>
        /// <remarks>sets the DL and DH in one command, therefore two OK are returned in response</remarks>
        /// <param name="Address">64bit address</param>
        /// <returns>AT command in AT format</returns>
        public static string XBAT_SetDestinationAddress(ulong Address)
        {
            uint H = (uint)(0x00000000FFFFFFFF & (Address >> 32));
            uint L = (uint)(0x00000000FFFFFFFF & Address);

            return "ATDH" + H.ToString("X8") + "," + "DL" + L.ToString("X8") + "\r";
        }

        /// <summary>
        /// broadcast address (64bit) for the PAN 
        /// </summary>
        public const ulong XBAT_BroadcastAddress64bit = 0x000000000000FFFF;

        //16-bit Source Address (MyAddress)
        public const string XBAT_GetMyAddress = "ATMY\r";
        public static string XBAT_SetMyAddress(ushort Address)
        {
            return "ATMY " + Address.ToString("X4") + "\r";
        }

        /// <summary>
        /// constant value (0xFFFF) to disable reception of packets with 16bit addresses 
        /// </summary>
        public const ushort XBAT_DisableReception16bitAddress = 0xFFFF;

        //Serial Number High (only readable)
        public const string XBAT_GetSerialNumberH = "ATSH\r";
        //Serial Number Low (only readable)
        public const string XBAT_GetSerialNumberL = "ATSL\r";

        //XBee Retries
        public const string XBAT_GetXbeeRetries = "ATRR\r";
        public static string XBAT_SetXbeeRetries(byte number)
        {
            return "ATRR " + number.ToString("X2") + "\r";
        }

        //Random Delay Slots
        public const string XBAT_GetRandomDelaySlots = "ATRN\r";
        public static string XBAT_SetRandomDelaySlots(byte number)
        {
            return "ATRN " + number.ToString("X2") + "\r";
        }

        //MAC Mode
        public const string XBAT_GetMacMode = "ATMM\r";
        public static string XBAT_SetMacMode(byte number)
        {
            return "ATMM " + number.ToString("X2") + "\r";
        }

        //Node Identifier
        public const string XBAT_GetNodeIdentifier = "ATNI\r";
        /// <summary>
        /// sets the NI (please have a look at the documentation, and read the rules for string!!!
        /// </summary>
        /// <param name="nodeId">new NI</param>
        /// <returns></returns>
        public static string XBAT_SetNodeIdentifier(string nodeId)
        {
            if (nodeId == string.Empty)
            {
                return "ATNI \r";
            }
            else
            {
                if (nodeId.Length > 20)
                    return "ATNI " + nodeId.Substring(0, 20) + "\r";
                else
                    return "ATNI " + nodeId + "\r";
            }
        }

        //Node Discover
        /// <summary>
        /// Node Discover
        /// </summary>
        /// <remarks>
        /// Node Discover. Discovers and reports all RF modules found. The following information 
        ///         is reported for each module discovered (the example cites use of Transparent operation 
        ///         (AT command format) - refer to the long ND command description regarding differences 
        ///         between Transparent and API operation). 
        ///         Node Discover Response (AT command mode format - Transparent operation): 
        ///         MY (Source Address) value(CR) 
        ///         SH (Serial Number High) value(CR) 
        ///         SL (Serial Number Low) value(CR) 
        ///         DB (Received Signal Strength) value(CR) 
        ///         NI (Node Identifier) value(CR) 
        ///         (CR)  (This is part of the response and not the end of command indicator.)
        ///          
        ///         .... Next Node
        /// </remarks>
        public const string XBAT_NodeDiscover = "ATND\r";

        //Node Discover Time
        public const string XBAT_GetNodeDiscoverTime = "ATNT\r";
        public static string XBAT_SetNodeDiscoverTime(byte time)
        {
            return "ATNT " + time.ToString("X2") + "\r";
        }

        //Node Discover Options
        public const string XBAT_NodeDiscoverOptionsEnable = "ATNO 1\r";
        public const string XBAT_NodeDiscoverOptionsDisable = "ATNO 0\r";
        public const string XBAT_GetXBAT_NodeDiscoverOptions = "ATNO\r";

        //Destination Node
        public static string XBAT_GetDestinationNode(string nodeId)
        {
            if (nodeId.Length > 20)
                return "ATDN " + nodeId.Substring(0, 20) + "\r";
            else
                return "ATDN " + nodeId + "\r";
        }

        //CoordinatorEnable
        public const string XBAT_GetCoordinatorEnable = "ATCE\r";
        public const string XBAT_CoordinatorEnable = "ATCE 01\r";
        public const string XBAT_CoordinatorDisable = "ATCE 00\r";
        public static string XBAT_SetCoordinatorEnable(byte bitfield)
        {
            if (bitfield > 0)
                return XBAT_CoordinatorEnable;
            else
                return XBAT_CoordinatorDisable;
        }

        //Scan Channels
        public const string XBAT_GetScanChannels = "ATSC\r";
        public static string XBAT_SetScanChannels(ushort bitfield)
        {
            return "ATSC " + bitfield.ToString("X4") + "\r";
        }

        //Scan Duration
        public const string XBAT_GetScanDuration = "ATSD\r";
        public static string XBAT_SetScanDuration(byte bitfield)
        {
            return "ATSD " + bitfield.ToString("X2") + "\r";
        }

        //End Device Association
        public const string XBAT_GetEndDeviceAssociation = "ATA1\r";
        public static string XBAT_SetEndDeviceAssociation(byte bitfield)
        {
            return "ATA1 " + bitfield.ToString("X2") + "\r";
        }

        //Coordinator Association
        public const string XBAT_GetCoordinatorAssociation = "ATA2\r";
        public static string XBAT_SetCoordinatorAssociation(byte bitfield)
        {
            return "ATA2 " + bitfield.ToString("X2") + "\r";
        }
        public const string XBAT_CoordinatorAssocAllow = "ATA2 04\r";   //special case

        //Association Indication (readonly)
        public const string XBAT_GetAssociationIndication = "ATAI\r";

        //Force Disassociation
        public const string XBAT_GetForceDisassociation = "ATDA\r";

        //Force Poll
        public const string XBAT_GetForcePoll = "ATFP\r";

        /// <summary>
        /// Active Scan
        /// </summary>
        /// <remarks>is not implemenent to handle the response of this command</remarks>
        /// <param name="bitfield">bitfield parameter</param>
        /// <returns>string in AT format</returns>
        public static string XBAT_GetActiveScan(byte bitfield)
        {
            return "ATAS " + bitfield.ToString("X2") + "\r";
        }

        /// <summary>
        /// Energy Scan 
        /// </summary>
        /// <remarks>is not implemenent to handle the response of this command</remarks>
        /// <param name="bitfield">bitfield parameter</param>
        /// <returns>string in AT format</returns>
        public static string XBAT_GetEnergyScan(byte bitfield)
        {
            return "ATED " + bitfield.ToString("X2") + "\r";
        }

        //AES Encryption Enable
        public const string XBAT_GetAESEncryption = "ATEE\r";
        public const string XBAT_AESEncryptionEnable = "ATEE 01\r";
        public const string XBAT_AESEncryptionDisable = "ATEE 00\r";

        //AES Encryption Key
        public static string XBAT_GetAESEncryptionKey = "ATKY\r";
        public static string XBAT_SetAESEncryptionKey(ulong bitfieldH, ulong bitfieldL)
        {
            return "ATKY " + bitfieldH.ToString("X8") + bitfieldL.ToString("X8") + "\r";
        }

        /// <summary>
        /// AT Commands - RF Interfacing
        /// </summary>

        //Power Level
        public const string XBAT_GetPowerLevel = "ATPL\r";
        public static string XBAT_SetPowerLevel(byte number)
        {
            return "ATPL " + number.ToString("X2") + "\r";
        }

        //CCA Treshold
        public const string XBAT_GetCCAThreshold = "ATCA\r";
        public static string XBAT_SetCCAThreshold(byte number)
        {
            return "ATCA " + number.ToString("X2") + "\r";
        }

        /// <summary>
        /// AT Commands - Sleep (Low Power)
        /// </summary>

        //Sleep Mode
        public const string XBAT_GetSleepMode = "ATSM\r";
        public static string XBAT_SetSleepMode(XBSleepMode mode)
        {
            byte i = (byte)mode;
            /*
            switch (mode)
            {
                case XBSleepMode.NoSleep:
                    i = 0; break;
                case XBSleepMode.PinHibernate:
                    i = 1; break;
                case XBSleepMode.PinDoze:
                    i = 2; break;
                case XBSleepMode.Reserved:
                    i = 3; break;
                case XBSleepMode.Cycle_sleep_remote:
                    i = 4; break;
                case XBSleepMode.Cycle_sleep_remote_pin_wakeUp:
                    i = 5; break;
                case XBSleepMode.Sleep_coordinator:
                    i = 6; break;
                default:
                    i = 0; break;
            }*/
            return "ATSM " + i.ToString("X2") + "\r";
        }

        //Sleep Option
        public const string XBAT_GetSleepOption = "ATSO\r";
        public static string XBAT_SetSleepOption(byte number)
        {
            return "ATSO " + number.ToString("X2") + "\r";
        }

        //Time before Sleep
        public const string XBAT_GetTimeBeforeSleep = "ATST\r";
        public static string XBAT_SetTimeBeforeSleep(ushort number)
        {
            return "ATST " + number.ToString("X4") + "\r";
        }

        //Cyclic Sleep Period
        public const string XBAT_GetCyclicSleepPeriod = "ATSP\r";
        public static string XBAT_SetCyclicSleepPeriod(ushort number)
        {
            return "ATSP " + number.ToString("X4") + "\r";
        }

        //Disassociated Cyclid Sleep Period
        public const string XBAT_GetDisassociatedCyclicSleepPeriod = "ATDP\r";
        public static string XBAT_SetDisassociatedCyclicSleepPeriod(ushort number)
        {
            return "ATDP " + number.ToString("X4") + "\r";
        }

        /// <summary>
        /// Calculate Param for XBEE Series1, according to default Baud Rates
        /// </summary>
        public static class XBBaudRates
        {
            private static readonly uint[] _DefaultBaud = [1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200];
            public static uint GetBaudRate(uint XBReturned_BD_Value)
            {
                if (XBReturned_BD_Value < _DefaultBaud.Length)
                    return _DefaultBaud[XBReturned_BD_Value];
                return XBReturned_BD_Value;
            }

            public static uint Get_BD_Value(uint BaudRate)
            {
                for (uint i = 0; i < _DefaultBaud.Length; i++)
                {
                    if (BaudRate == _DefaultBaud[i])
                        return i;
                }
                //No Standard Baud Rate
                return BaudRate;
            }
        }


        /// <summary>
        /// Creates string to set Baud rate
        /// </summary>
        public static string XBAT_SetBaudRate(uint Baud)
        {
            uint br = XBBaudRates.Get_BD_Value(Baud);
            return "ATBD " + br.ToString("X") + "\r";
        }

        public static string XBAT_GetBaudRate = "ATBD\r";

        //Packetizing Timeout
        public const string XBAT_GetPacketizingTimeout = "ATRO\r";
        public static string XBAT_SetPacketizingTimeout(byte number)
        {
            return "ATRO " + number.ToString("X2") + "\r";
        }

        //API Enable
        public const string XBAT_GetAPIMode = "ATAP\r";
        public static string XBAT_SetAPIMode(XBAPIMode mode)
        {
            byte i;
            switch (mode)
            {
                case XBAPIMode.Disabled:
                    i = 0; break;
                case XBAPIMode.Enabled:
                    i = 1; break;
                case XBAPIMode.Enabled_w_Escape_Control_Chars:
                    i = 2; break;
                default:
                    i = 0; break;
            }
            return "ATAP " + i.ToString("X2") + "\r";
        }

        //Parity
        public const string XBAT_GetParity = "ATNB\r";
        public static string XBAT_SetParity(System.IO.Ports.Parity parity)
        {
            byte i = 0;
            switch (parity)
            {
                case System.IO.Ports.Parity.None:
                    i = 0; break;
                case System.IO.Ports.Parity.Even:
                    i = 1; break;
                case System.IO.Ports.Parity.Odd:
                    i = 2; break;
                case System.IO.Ports.Parity.Mark:
                    i = 3; break;
                case System.IO.Ports.Parity.Space:
                    i = 4; break;
            }
            return "ATNB " + i.ToString("X2") + "\r";
        }

        //RO Packetization Timeout
        public const string XBAT_GetROPacketizationTimeout = "ATRO\r";
        public static string XBAT_SetROPacketizationTimeout(byte number)
        {
            return "ATRO " + number.ToString("X2") + "\r";
        }

        //Pull-up Resistor Enable
        public const string XBAT_GetPullUpResistorEnable = "ATPR\r";
        public static string XBAT_SetPullUpResistorEnable(byte number)
        {
            return "ATPR " + number.ToString("X2") + "\r";
        }

        /// <summary>
        /// AT Commands - I/O-Setttings
        /// </summary>

        //DIO Configuration (for D8-D0)
        public static string XBAT_GetDIOConfiguration(uint linenumber)
        {
            return "ATD" + linenumber.ToString() + "\r";
        }

        public static string XBAT_SetDI0Configuration(uint linenumber, byte parameter)
        {
            return "ATD" + linenumber.ToString() + " " + parameter.ToString("X2") + "\r";
        }

        //IO Output Enable
        public const string XBAT_IOOutputEnable = "ATIU 01\r";
        public const string XBAT_IOOutputDisable = "ATIU 00\r";
        public const string XBAT_GetIOOutput = "ATIU\r";

        //Samples before TX
        public const string XBAT_GetSamplesBeforeTX = "ATIT\r";
        public static string XBAT_SetSamplesBeforeTX(byte number)
        {
            return "ATIT " + number.ToString("X2") + "\r";
        }

        //Force Sample (only readable)
        public static string XBAT_ForceSample()
        {
            return "ATIS\r";
        }
        public static string XBAT_ForceSample(byte bitfield)
        {
            return "ATIS " + bitfield.ToString("X2") + "\r";
        }

        //Digital Output Level
        public const string XBAT_GetDigitalOutputLevel = "ATIO\r";
        public static string XBAT_SetDigitalOutputLevel(byte bitfield)
        {
            return "ATIO " + bitfield.ToString("X2") + "\r";
        }

        //Digital Change Detect
        public const string XBAT_GetDigitalChangeDetect = "ATIC\r";
        public static string XBAT_SetDigitalChangeDetect(byte number)
        {
            return "ATIC " + number.ToString("X2") + "\r";
        }

        //Sample Rate
        public const string XBAT_GetSampleRate = "ATIR\r";
        public static string XBAT_SetSampleRate(ushort number)
        {
            return "ATIR " + number.ToString("X4") + "\r";
        }

        //I/O Input Address
        public const string XBAT_GetIOInputAddress = "ATIA\r";
        public static string XBAT_SetIOInputAddress(ulong address)
        {
            return "ATIA " + address.ToString("X16") + "\r";
        }

        //Output Timeout (for T7-T0)
        public static string XBAT_GetOutputTimeout(uint linenumber)
        {
            return "ATT" + linenumber.ToString() + "\r";
        }
        public static string XBAT_SetOutputTimeout(uint linenumber, byte parameter)
        {
            return "ATT" + linenumber.ToString() + " " + parameter.ToString("X2") + "\r";
        }

        //PWM Configuration
        public static string XBAT_GetPWMConfiguration(uint number)
        {
            return "ATP" + number.ToString() + "\r";
        }

        public static string XBAT_SetPWMConfiguration(uint number, XBPWMConfig mode)
        {
            byte i = 0;
            switch (mode)
            {
                case XBPWMConfig.Disabled:
                    i = 0; break;
                case XBPWMConfig.RSSI:
                    i = 1; break;
                case XBPWMConfig.PWM_Output:
                    i = 2; break;
                default:
                    break;
            }
            return "ATP" + number.ToString() + " " + i.ToString("X2") + "\r";
        }

        //PWM Output Level
        public static string XBAT_GetPWMOutputLevel(uint number)
        {
            return "ATM" + number.ToString() + "\r";
        }
        public static string XBAT_SetPWMOutputLevel(uint number, ushort time)
        {
            return "ATM" + number.ToString() + " " + time.ToString("X4") + "\r";
        }

        //PWM Output Timeout
        public const string XBAT_GetPWMOutputTimeout = "ATPT\r";
        public static string XBAT_SetPWMOutputTimeout(byte number)
        {
            return "ATPT " + number.ToString("X2") + "\r";
        }

        //RSSI PWM Timer
        public const string XBAT_GetRSSIPWMTimer = "ATRP\r";
        public static string XBAT_SetRSSIPWMTimer(byte number)
        {
            return "ATRP " + number.ToString("X2") + "\r";
        }

        /// <summary>
        /// AT Commands - Diagnostics
        /// </summary>

        //Firmware Version
        public const string XBAT_GetFirmwareVersion = "ATVR\r";

        //Firmware Version - Verbose
        public const string XBAT_GetFirmwareVersionVerbose = "ATVL\r";

        //Hardware Version
        public const string XBAT_GetHardwareVersion = "ATHV\r";

        //Received Signal Strength
        public const string XBAT_GetReceivedSignalStrength = "ATDB\r";

        //CCA Failure
        public const string XBAT_GetCCAFailure = "ATEC\r";
        public const string XBAT_ResetCCAFailure = "ATEC 00\r";

        //ACK Failure
        public const string XBAT_GetACKFailure = "ATEA\r";
        public const string XBAT_ResetACKFailure = "ATEA 00\r";

        /// <summary>
        /// AT Commands - AT Commands Options
        /// </summary>

        // Command Mode Timeout
        public const string XBAT_GetCommandModeTimeout = "ATCT\r";
        public static string XBAT_SetCommandModeTimeout(ushort number)
        {
            return "ATCT " + number.ToString("X4") + "\r";
        }

        //Exit Command Mode
        public const string XBAT_ExitCommandMode = "ATCN\r";

        //Apply Changes
        public const string XBAT_ApplyChanges = "ATAC\r";

        //Guard Times
        public const string XBAT_GetGuardTimes = "ATGT\r";
        public static string XBAT_SetGuardTimes(ushort number)
        {
            return "ATGT " + number.ToString("X4") + "\r";
        }

        //Guard Times
        public const string XBAT_GetCommandSequenceCharacter = "ATCC\r";
        public static string XBAT_SetCommandSequenceCharacter(byte number)
        {
            return "ATCC " + number.ToString("X2") + "\r";
        }

    }

    /// <summary>
    /// Sleep mode options
    /// </summary>
    public enum XBSleepMode
    {
        //Careful with order - must match datasheet
        NoSleep,
        PinHibernate,
        PinDoze,
        Reserved,
        Cycle_sleep_remote,
        Cycle_sleep_remote_pin_wakeUp,
        Sleep_coordinator
    }

    /// <summary>
    /// API Modes
    /// </summary>
    /// <remarks>
    /// 0 =Disabled 
    /// 1 = API enabled 
    /// 2 = API enabled  (w/escaped control characters)
    /// </remarks>
    public enum XBAPIMode
    {
        Disabled,
        Enabled,
        Enabled_w_Escape_Control_Chars
    }

    /// <summary>
    /// PWM Configuration Options
    /// </summary>
    public enum XBPWMConfig
    {
        Disabled,
        RSSI,
        PWM_Output
    }

}