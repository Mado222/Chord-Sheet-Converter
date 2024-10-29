using System.Text;
using WindControlLib;
using XBeeLib;
using BMTCommunicationLib;


namespace BMTCommunication
{
    /************************************************************************************
     * ************************************************************************************
     * CLASS: CXBeeSeries1
     * ************************************************************************************
    ************************************************************************************/

    /// <summary>
    /// Configurator for XBee Series1 Modules in AT Mode
    /// </summary>
    /// <remarks>
    /// The Module can be in two Modes:
    /// Transparent / AT Mode
    /// API Mode
    /// </remarks>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <remarks>
    /// inits the private members of this class
    /// sets the CC, GT, NT to default values
    /// </remarks>
    /// <param name="SerialPort">serial port</param>
    public class CXBeeSeries1(ISerialPort SerialPort) : IDisposable
    {
        /// <summary>
        /// log4net 
        /// </summary>
        //private static readonly ILog logger = LogManager.GetLogger(typeof(CXBeeSeries1));

        /// <summary>
        /// Experimental evaluated Timeout for XBGetResponse
        /// </summary>
        private static readonly int Timeout_ms = 500;

        /// <summary>
        /// Open Serial Port Connection
        /// </summary>
        public ISerialPort Seriell32 = SerialPort;

        /// <summary>
        /// Time at which Command Mode was entered
        /// </summary>
        private DateTime TimeSentCommandString = DateTime.MinValue;

        /// <summary>
        /// Time after XB module leaves automatically command mode
        /// </summary>
        /// <remarks>Read in XBEnterCommanMode see "ATCT" Command</remarks>
        private TimeSpan TimeoutCommandMode;

        public CXBNodeInformation? LocalDevice { get; set; } = new CXBNodeInformation();
        public CXBNodeInformation? CurrentEndDevice { get; set; } = new CXBNodeInformation();
        public bool DisplayMessages { get; set; } = false;

        //private const string MessageBoxCaption = "XBee Error";

        /// <summary>
        /// commom sequence character
        /// </summary>
        /// <remarks>
        /// please be careful if you change this register
        /// </remarks>
        private char _CommonSequenceCharacter = '+';

        /// <summary>
        /// guard times
        /// </summary>
        /// <remarks>
        /// please be careful if you change this register
        /// </remarks>
        private ushort _GuardTimes = 0x3E8;

        /// <summary>
        /// node discover time
        /// </summary>
        /// <remarks>
        /// please be careful if you change this register
        /// </remarks>
        private byte _NodeDiscoverTime = 0x19;

        /// <summary>
        /// returns the enter command mode string (default: +++)
        /// </summary>
        /// <returns>enter command mode string</returns>
        private string XBGetEnterCommandModeString()
        {
            return _CommonSequenceCharacter.ToString() +
                _CommonSequenceCharacter.ToString() +
                _CommonSequenceCharacter.ToString();
        }

        public List<CXBNodeInformation> DiscoveredEndDevices { get; private set; } = [];

        /// <summary>
        /// error class
        /// </summary>
        private readonly CXBError _Error = new ();

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }
        #endregion


        /// <summary>
        /// returns the last errors which occurs
        /// </summary>
        /// <remarks>use this method to getting some information about an occured error</remarks>
        /// <returns>error-object</returns>
        public int XBGetLastErrorNo()
        {
            return _Error.Last_ErrorNo;
        }

        public string XBGetLastErrorTxt()
        {
            return _Error.LastError_String;
        }

        /// <summary>
        /// Checks if the specific baudrate is set on the local device 
        /// </summary>
        /// <returns>
        /// 1: baudrate is correct
        /// -1: baudrate is not correct
        /// </returns>
        public int XBCheckBaudrate()
        {
            Seriell32.DtrEnable = true;
            Thread.Sleep(_GuardTimes);         //last version: Thread.Sleep(1000);

            //Switch to Command Mode
            XBSendString(XBGetEnterCommandModeString());      //last version: XBSendString(CXBATCommands.XBAT_EnterCommandMode);

            //if (logger.IsInfoEnabled) logger.Info("checking the baudrate...");
            //check for proper response
            int ret = XBCheckResponse(CXBATCommands.XBAT_OKResponse);
            Thread.Sleep(_GuardTimes);

            //No characters sent for one second [GT (Guard Times) parameter = 0x3E8]
            if (ret > 0)
            {
                //XBLeaveCommandMode();
                XBSendString_CheckOKResponse(CXBATCommands.XBAT_ExitCommandMode);
                //if (logger.IsInfoEnabled) logger.Info("checking the baudrate successfully finished");
                return ret;
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("checking the baudrate not successfully finished or wrong baudrate");
                return ret;
            }
        }

        /// <summary>
        /// Checks if local device is still in Command mode
        /// </summary>
        /// <param name="SetCommandMode">true: If device is not in Command Mode - activate it</param>
        /// <returns>
        /// 1: Local Device is in Command Mode
        /// 0: Local Device is not in Command Mode
        /// -1: An Error occurs
        /// </returns>
        public int XBCheckCommandMode(bool SetCommandMode)         // old version: bool return-value
        {
            //if (logger.IsInfoEnabled) logger.Info("Checking if local device is in command mode...");
            if (DateTime.Now < (TimeSentCommandString + TimeoutCommandMode))
            {
                //if (logger.IsInfoEnabled) logger.Info("Local device is in command mode");
                return 1;
            }
            else
            {
                if (SetCommandMode)
                {

                    //Wait at least one second (no chars may be transmitted)
                    Thread.Sleep(_GuardTimes);

                    //Switch to Command Mode
                    XBSendString(XBGetEnterCommandModeString());      //last version: XBSendString(CXBATCommands.XBAT_EnterCommandMode);

                    //check for proper response
                    int ret = XBCheckResponse(CXBATCommands.XBAT_OKResponse);

                    //No characters sent for one second [GT (Guard Times) parameter = 0x3E8]
                    Thread.Sleep(_GuardTimes);

                    if (ret > 0)
                    {
                        //get CommandModeTimeout
                        XBSendString(CXBATCommands.XBAT_GetCommandModeTimeout);
                        string s = XBGetString(1000);
                        if (s != null)
                        {
                            int ms = CMyConvert.HexStringToInt(s) * 100; //[ms]
                            TimeoutCommandMode = new TimeSpan(0, 0, 0, 0, ms);
                            TimeSentCommandString = DateTime.Now;
                        }
                        else
                        {
                            TimeoutCommandMode = new TimeSpan(0, 0, 0, 10, 0); // def.value is assumed 
                            TimeSentCommandString = DateTime.Now;
                            //logger.Warn("CommandModeTimeout could not read successfully, thus can lead to timeout's in next 10 sec.");
                        }
                        //if (logger.IsInfoEnabled) logger.Info("Local device is in command mode");
                        return 1;
                    }
                    //XBSetLastError(5);
                    _Error.SetError_CommandModeFailed();
                    //if (logger.IsInfoEnabled) logger.Info("During checking if local device is in command mode an failure occured");
                    return ret;
                }
                //if (logger.IsInfoEnabled) logger.Info("Local device is not in command mode");
                return 0;
            }
        }

        /// <summary>
        /// Leaves the command mode on the local device if it is still active
        /// </summary>
        /// <returns>
        /// 1: command mode has leaved
        /// -1: command mode not successfully leaved
        /// </returns>
        public int XBLeaveCommandMode()                            // last version: private bool XBLeaveCommandMode()
        {
            //if (logger.IsInfoEnabled) logger.Info("Leaving the command mode...");
            if (XBCheckCommandMode(false) > 0)
            {
                int res = XBSendString_CheckOKResponse(CXBATCommands.XBAT_ExitCommandMode); //vorher bool res
                if (res > 0)
                {
                    TimeoutCommandMode = new TimeSpan(0, 0, 0, 0, 0); // reset
                    //if (logger.IsInfoEnabled) logger.Info("Command mode successfully left");
                    return 1;
                }
                //if (logger.IsInfoEnabled) logger.Info("Command mode not successfully left");
                return -1;
            }
            //if (logger.IsInfoEnabled) logger.Info("Command mode successfully left");
            return 1;
        }

        /// <summary>
        /// Sends string on serial port and checks response
        /// </summary>
        /// <remarks>Uses XSendString and waits for an OK response</remarks>
        /// <returns>
        /// 1: OK response received
        /// -1: Error occors or no OK response received
        /// </returns>
        private int XBSendString_CheckOKResponse(string StringToSend)           //last version: private bool XBSendString_CheckOKResponse(string StringToSend)
        {
            //if (logger.IsInfoEnabled) logger.Info("Sending string and checking OK-response...");
            XBSendString(StringToSend);
            return XBCheckResponse(CXBATCommands.XBAT_OKResponse);
        }

        /// <summary>
        /// Switches Device in API mode, sends string on serial port and checks response, clears API mode
        /// </summary>
        /// <remarks>Uses XSendString and waits for an OK response</remarks>
        /// <returns>
        /// 1: OK response received
        /// -1: Error occors or no OK response received
        /// </returns>
        public int XBSendAPIString_CheckOKResponse(string StringToSend, bool FinallyLeaveCommanMode)           //last version: private bool XBSendString_CheckOKResponse(string StringToSend)
        {
            if (XBCheckCommandMode(true) > 0)
            {
                if (XBSendString_CheckOKResponse(StringToSend) > 0)
                {
                    if (FinallyLeaveCommanMode)
                        return XBLeaveCommandMode();
                    return 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Clears Serial RX/TX Buffers and sends string on serial port
        /// </summary>
        /// <param name="StringToSend">String to send</param>
        private void XBSendString(string StringToSend)
        {
            //if (logger.IsInfoEnabled) logger.Info("Sending string through serial port...");
            //if (logger.IsInfoEnabled) logger.Info(StringToSend);
            Seriell32.DiscardInBuffer();
            Seriell32.DiscardOutBuffer();
            byte[] b = WindControlLib.CMyConvert.StringToByteArray(StringToSend);
            Seriell32.Write(b, 0, b.Length);
            //if (logger.IsInfoEnabled) logger.Info("String sent");
        }

        /// <summary>
        /// Clears Serial RX/TX buffers and sends byte array on serial port
        /// </summary>
        /// <param name="ArrayToSend">Byte array to send</param>
        private void XBSendByteArray(byte[] ArrayToSend)
        {
            ////if (logger.IsInfoEnabled) logger.Info("Send byte array through serial port...");
            Seriell32.DiscardInBuffer();
            Seriell32.DiscardOutBuffer();
            Seriell32.Write(ArrayToSend, 0, ArrayToSend.Length);
            ////if (logger.IsInfoEnabled) logger.Info("Byte array sent");
        }

        /// <summary>
        /// Reads incoming bytes on serial port
        /// </summary>
        /// <remarks>
        /// <para>Reads incoming bytes on serial port until 0x0d is received</para>
        /// <para>Default Timeout = 300ms</para>
        /// </remarks>
        /// <returns>
        /// String that was received
        /// null: if an error occurs
        /// </returns>
        private string? XBGetString()
        {
            return XBGetString(300);
        }

        /// <summary>
        /// Reads incoming bytes on serial port
        /// </summary>
        /// <remarks>Reads incoming bytes on serial port until 0x0d is received</remarks>
        /// <param name="Timeout_ms">Read timeout in msec</param>
        /// <returns>
        /// String that was received
        /// null: if an error occurs
        /// </returns>
        private string? XBGetString(int Timeout_ms)
        {
            //if (logger.IsInfoEnabled) logger.Info("Getting the string through serial port...");
            string s = string.Empty;

            List<byte> bt = [];
            byte[] inBuf = new byte[1];
            bool finished = false;
            int numberByteRead;
            do              // new version of do-while (for waiting of the 0x0D) with error handling
            {
                numberByteRead = Seriell32.Read(ref inBuf, 0, 1, Timeout_ms);
                if (inBuf[0] != 0xd)
                    bt.Add(inBuf[0]);
                else
                    finished = true;
            }
            while (!finished && numberByteRead > 0);
            if (numberByteRead <= 0)
            {
                //XBSetLastError(1);
                _Error.SetError_SPReadTimeout();
                //if (logger.IsInfoEnabled) logger.Info("Getting the string through serial port not successfully finished");
                return null;
            }

            if (bt.Count > 0)
            {
                s = WindControlLib.CMyConvert.ByteArraytoString([.. bt]);
            }

            //if (logger.IsDebugEnabled) logger.Debug("'" + s + "'" + " received");
            //if (logger.IsInfoEnabled) logger.Info("Getting the string through serial port successfully finished");
            return s;
        }


        /// <summary>
        /// Reads an API-Frame on serial port
        /// </summary>
        /// <returns>
        /// response (instance of a subclass of CBasicAPIResponse)
        /// null: if an error occurs
        /// </returns>
        public CBasicAPIResponse? XBGetResponse()
        {
            //if (logger.IsInfoEnabled) logger.Info("Getting the response message...");
            CBasicAPIResponse response = new CATCommandResponse();          //init with some sub-classes

            byte[] frameData;
            //List<byte> debugList = new List<byte>();

            byte[] inBuf = new byte[2];
            if (LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
            {
                throw new Exception("XBAPIMode.Enabled_w_Escape_Control_Chars: not implemented in XBGetResponse");
            }

            int numberReadByte;
            //read until start delimiter
            do
            {
                numberReadByte = Seriell32.Read(ref inBuf, 0, 1, Timeout_ms);
            }
            while (inBuf[0] != CXBAPICommands.StartDelimiter && numberReadByte > 0);

            if (numberReadByte <= 0)
            {
                _Error.SetError_SPReadTimeout();
                return null;
            }

            //read length
            numberReadByte = Seriell32.Read(ref inBuf, 0, 2, Timeout_ms);
            if (numberReadByte == 0)
            {
                _Error.SetError_SPReadTimeout();
                return null;
            }
            uint ulength = (ushort)CMyConvert.FromUIntBytestoUInt(inBuf[1], inBuf[0]);

            //read APID
            numberReadByte = Seriell32.Read(ref inBuf, 0, 1, Timeout_ms);
            if (numberReadByte == 0)
            {
                _Error.SetError_SPReadTimeout();
                return null;
            }
            byte APIdentifier = inBuf[0];
            ulength--;

            //read frame data (without APID)
            frameData = new byte[ulength];
            numberReadByte = Seriell32.Read(ref frameData, 0, (int)ulength, Timeout_ms);
            if (numberReadByte != ulength)
            {
                _Error.SetError_SPReadTimeout();
                return null;    //!!
            }

            switch (APIdentifier)
            {
                case (CXBAPICommands.ATCommandResponse):
                    response = new CATCommandResponse();
                    break;
                case (CXBAPICommands.RemoteCommandResponse):
                    response = new CRemoteCommandResponse();
                    break;
                case (CXBAPICommands.TXStatus):
                    response = new CTXStatusResponse();
                    break;
                case (CXBAPICommands.RXPacket64bit):
                    response = new CRXPacket64();
                    break;
                case (CXBAPICommands.RXPacket16bit):
                    response = new CRXPacket16();
                    break;
                case (CXBAPICommands.ModemStatus):
                    response = new CModemStatus();
                    break;
                default:
                    break;
            }

            List<byte> byteList = new(frameData);

            response.initResponse(byteList);

            //read and check checksum
            numberReadByte = Seriell32.Read(ref inBuf, 0, 1, Timeout_ms);
            if (numberReadByte == 0)
            {
                _Error.SetError_SPReadTimeout();
                return null;
            }
            byte checksum_fd = inBuf[0];
            checksum_fd += APIdentifier;
            for (int i = 0; i < frameData.Length; i++)
            {
                checksum_fd += frameData[i];
            }

            if (checksum_fd == 0xff)
            {
                return response;
            }
            else
            {
                _Error.SetError_Checksum();
                return null;
            }
        }

        /// <summary>
        /// checks the checksum of the receiving API frame
        /// </summary>
        /// <param name="listToCheck">list of bytes to check, including the checksum, but not delimiter and length</param>
        /// <returns>
        /// true: if checksum correct
        /// false: if checksum is not correct
        /// </returns>
        /*
        private bool XBCheckRecCheckSum(List<byte> listToCheck)
        {
            //if (logger.IsInfoEnabled) logger.Info("Checking the received checksum...");
            byte Checksum = 0;
            for (int i = 0; i < listToCheck.Count; i++)
            {
                Checksum += listToCheck[i];
            }

            if (Checksum == 0xFF)
            {
                //if (logger.IsInfoEnabled) logger.Info("Checksum is correct");
                return true;
            }
            else
            {
                //XBSetLastError(2);
                _Error.SetError_Checksum();
                return false;
            }
        }*/


        /// <summary>
        /// Searches for a specific nodes with a given Node-Identifier and returns its Serial Number
        /// </summary>
        /// <param name="NodeIdentifier">Node-Identifier of the wanted node</param>
        /// <returns>
        /// SerialNumber (64bit) of Node: if MY-Address of wanted node is set to 0xFFFF
        /// MY-Address (in 64bit-format) of Node: if MY-Address of wanted node is set lower than 0xFFFF
        /// UInt64.MaxValue, if an error occurs or node with given Node-Identifier not found
        /// </returns>
        public ulong XBGetSerialNumberOfNode(string NodeIdentifier)
        {
            string s;
            if (XBCheckCommandMode(true) > 0)
            {
                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_GetDestinationNode(NodeIdentifier)) > 0)
                {
                    TimeoutCommandMode = new TimeSpan(0, 0, 0, 0, 0); // because local device leaves automatically the command mode after DN-command
                    if (XBCheckCommandMode(true) > 0)
                    {
                        XBSendString(CXBATCommands.XBAT_GetDestinationAddressHigh);
                        s = XBGetString(1000);
                        if (s != null)
                        {
                            ulong DH = Convert.ToUInt32(s, 16);
                            XBSendString(CXBATCommands.XBAT_GetDestinationAddressLow);
                            s = XBGetString(1000);
                            if (s != null)
                            {
                                //if (logger.IsInfoEnabled) logger.Info("Getting serial number of specific node with knowing node-identifier...");

                                ulong DL = Convert.ToUInt32(s, 16);
                                XBLeaveCommandMode();
                                //if (logger.IsInfoEnabled) logger.Info("Getting serial number of node with knowing node-identifier successfully finished");
                                return (DL + (DH << 32));
                            }
                        }
                    }
                }
            }
            //if (logger.IsInfoEnabled) logger.Info("Getting serial number of node with knowing node-identifier not successfully finished");
            return ulong.MaxValue;
        }

        /// <summary>
        /// restores the local members of this class to default values
        /// </summary>
        private void XBRestoreMembers()
        {
            _CommonSequenceCharacter = '+';
            _GuardTimes = 0x3E8;
            _NodeDiscoverTime = 0x19;
        }

        /// <summary>
        /// restores the local device to default values
        /// </summary>
        /// <returns>
        /// 1: restoring successfully
        /// -1: restoring was not successfully
        /// </returns>
        public int XBRestoreLocalDevice()
        {
            //if (logger.IsInfoEnabled) logger.Info("Restoring Local Device...");
            if (XBCheckCommandMode(true) > 0)
            {
                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_RestoreDefaults) > 0)
                {
                    //Save
                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory) > 0)
                    {
                        if (XBGetConfigurationOfLocalDevice() != null)
                        {
                            XBLeaveCommandMode();
                            XBRestoreMembers();
                            //if (logger.IsInfoEnabled) logger.Info("Restoring local device successfully finished");
                            return 1;
                        }
                    }
                }
            }
            //if (logger.IsInfoEnabled) logger.Info("Restoring local device not successfully finished");
            return -1;
        }

        /// <summary>
        /// Enters Command mode and reads available nodes
        /// </summary>
        public List<CXBNodeInformation> XBNodeDiscoverAPI2()
        {
            List<CXBNodeInformation> NodeInfos = [];
            if (XBCheckCommandMode(true) == 1)
            {
                //Send ATND Command
                Thread.Sleep(100);
                XBSendString(CXBATCommands.XBAT_NodeDiscover);
                //My Addresse kommt
                string s = XBGetString(3000);

                while (s != "")
                {
                    CXBNodeInformation NodeInfo = new()
                    {
                        MY_MyAddress = Convert.ToUInt16(s, 16),
                        SH_SerialNumberHigh = Convert.ToUInt32(XBGetString(1000), 16),
                        SL_SerialNumberLow = Convert.ToUInt32(XBGetString(1000), 16),
                        RecSignalStrength = Convert.ToByte(XBGetString(1000), 16),
                        NI_NodeIdentifier = XBGetString(1000)
                    };
                    NodeInfos.Add(NodeInfo);
                    //<CR>  (This is part of the response and not the end of command indicator.)
                    XBGetString(500);
                    //If there is another <CR> the Command is finshed, otherwise there is another Node
                    s = XBGetString(500);
                }
            }
            return NodeInfos;
        }

        /// <summary>
        /// Local Device commits a node discover
        /// </summary>
        /// <remarks>local device must be in API-Mode</remarks>
        /// <returns>
        /// list of found nodes
        /// null: if an error occurs
        /// </returns>
        public List<CXBNodeInformation>? XBNodeDiscoverAPI()
        {
            //if (logger.IsInfoEnabled) logger.Info("Discover available nodes...");

            int NTmsec = _NodeDiscoverTime * 100 + 300;                //+300ms tolerance

            List<CXBNodeInformation> NodeInfos = [];

            if (LocalDevice.APIMode == XBAPIMode.Disabled)
            {
                //XBSetLastError(9);
                _Error.SetError_LocalDevNotInApi();
                //if (logger.IsInfoEnabled) logger.Info("Node discovery failed");
                return null;
            }

            CATCommand request = new()
            {
                ATCommand = CXBATCommands.XBAT_NodeDiscover,
                ATCommandResponse = true
            };
            CATCommandResponse? response = (CATCommandResponse)XBSendRequest(request, NTmsec);   // last version: CATCommandResponse response = (CATCommandResponse)XBSendRequest(request, 3000);
            if (response != null)
            {
                List<byte> listOfByte = new(response.valueOfCommand);
                List<byte>.Enumerator listEnum = listOfByte.GetEnumerator();

                while (listEnum.MoveNext())
                {
                    CXBNodeInformation NodeInfo = new();

                    //Read My Address
                    byte[] be = new byte[2];
                    for (int i = 1; i >= 0; i--)
                    {
                        be[i] = listEnum.Current;
                        listEnum.MoveNext();
                    }
                    NodeInfo.MY_MyAddress = BitConverter.ToUInt16(be, 0);

                    //Read Serial Number High
                    be = new byte[4];
                    for (int i = 3; i >= 0; i--)
                    {
                        be[i] = listEnum.Current;
                        listEnum.MoveNext();
                    }
                    NodeInfo.SH_SerialNumberHigh = BitConverter.ToUInt32(be, 0);

                    //Read Serial Number Low
                    be = new byte[4];
                    for (int i = 3; i >= 0; i--)
                    {
                        be[i] = listEnum.Current;
                        listEnum.MoveNext();
                    }
                    NodeInfo.SL_SerialNumberLow = BitConverter.ToUInt32(be, 0);

                    //Read Rec Signal Strength
                    NodeInfo.RecSignalStrength = listEnum.Current;

                    List<byte> helpList = [];
                    while (listEnum.MoveNext() && listEnum.Current != 0x00)
                    {
                        helpList.Add(listEnum.Current);
                    }
                    NodeInfo.NI_NodeIdentifier = ASCIIEncoding.ASCII.GetString(helpList.ToArray());

                    NodeInfos.Add(NodeInfo);
                }
                //if (logger.IsInfoEnabled) logger.Info(NodeInfos.Count.ToString() + " available nodes found");
                DiscoveredEndDevices = NodeInfos;
                return NodeInfos;
            }
            //if (logger.IsInfoEnabled) logger.Info("Nodes discovering not successfully finished");
            return null;
        }

        /// <summary>
        /// Sends an API frame (request) and reads the response
        /// </summary>
        /// <remarks>
        /// <para>waits for the response which was requested, other responses are discarded</para>
        /// <para>local device must be in API-Mode</para>
        /// </remarks>
        /// <param name="request">request</param>
        /// <param name="Timeout">read timeout in msec</param>
        /// <returns>
        /// response (instance of a subclass of CBasicAPIResponse)
        /// null: if an error occurs
        /// </returns>
        public CBasicAPIResponse XBSendRequest(CBasicAPIRequest request, int Timeout)
        {
            //if (logger.IsInfoEnabled) logger.Info("Sending request...");
            //if (logger.IsInfoEnabled) logger.Info("request AP-ID: " + request.APID);

            if (LocalDevice.APIMode == XBAPIMode.Disabled)
            {
                _Error.SetError_LocalDevNotInApi();
                //if (logger.IsInfoEnabled) logger.Info("API Mode must be enabled");
                return null;
            }
            XBSendByteArray(request.Get_CommandRequest_DataFrame(LocalDevice.APIMode)); //Discards Buffers
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, Timeout);
            CBasicAPIResponse response;
            while (DateTime.Now < dt)
            {
                response = XBGetResponse();
                if (response != null)
                {
                    if (request.checkResponse(response))
                    {
                        //if (logger.IsInfoEnabled) logger.Info("response successfully received");
                        return response;
                    }
                }
                Thread.Sleep(50);
            }
            //if (logger.IsInfoEnabled) logger.Info("Sending request timed out: " + Timeout.ToString() + "ms");
            return null;
        }


        /// <summary>
        /// Checks if the given string is received on the serial port
        /// </summary>
        /// <param name="result">string that must be received</param>
        /// <returns>
        /// 1: if the string was received
        /// -1: if an error occurs
        /// </returns>
        private int XBCheckResponse(string result)
        {
            //if (logger.IsInfoEnabled) logger.Info("Checking the received response message...");
            byte[] b = WindControlLib.CMyConvert.StringToByteArray(result);
            byte[] bin = new byte[b.Length];
            int res = Seriell32.Read(ref bin, 0, b.Length, 3000);
            if (res > 0)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    if (b[i] != bin[i])
                    {
                        //XBSetLastError(3);
                        _Error.SetError_noOKAT();
                        //if (logger.IsInfoEnabled) logger.Info("Response message is not correct. Expected: " + result + " Received: " + bin.ToString());
                        return -1;
                    }
                }
                //if (logger.IsInfoEnabled) logger.Info("Response message is correct");
                return 1;
            }
            else
            {
                //XBSetLastError(1);
                _Error.SetError_SPReadTimeout();
                //if (logger.IsInfoEnabled) logger.Info("Response message is not correct");
                return -1;
            }
        }

        /// <summary>
        /// write changes on local device to non-volatile memory
        /// </summary>
        /// <remarks>is not used in this class</remarks>
        /// <returns>
        /// 1: successfully written
        /// -1: an error occurs
        /// </returns>
        private int XBWrite_to_nonvolatilememory()
        {
            //if (logger.IsInfoEnabled) logger.Info("Sending WRITE-message for writing changes to nonvolatile memory...");
            if (XBCheckCommandMode(true) > 0)
            {
                return XBSendString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory);
            }
            //if (logger.IsInfoEnabled) logger.Info("Sending WRITE-message not successfully finished");
            return -1;
        }



        /// <summary>
        /// Sets Serial Port parameter on local device and an this class
        /// </summary>
        /// <remarks>it changes also PC Serial Port parameters</remarks>
        /// <param name="BaudRate">baudrate</param>
        /// <param name="parity">parity</param>
        /// <returns>
        /// 1: changes successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetSerialPort2LocalDevice(uint BaudRate, System.IO.Ports.Parity parity)
        {
            //if (logger.IsInfoEnabled) logger.Info("Setting the serial port configuration of the local device...");
            if (XBCheckCommandMode(true) > 0)
            {
                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetBaudRate(BaudRate)) > 0)
                {
                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetParity(parity)) > 0)
                    {
                        if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory) > 0)
                        {
                            //if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_ApplyChanges) > 0) // not neccassary if WR+CN was sent
                            //{
                            XBLeaveCommandMode();
                            Seriell32.BaudRate = (int)BaudRate;
                            Seriell32.Parity = parity;
                            //if (logger.IsInfoEnabled) logger.Info("Setting the serial port configuration of the local device successfully finished");
                            return 1;
                            //}
                        }
                    }
                }
            }
            //if (logger.IsInfoEnabled) logger.Info("Setting the serial port configuration of the local device not successfully finished");
            return -1;
        }

        /// <summary>
        /// Sends an command in AT-format for quering an register
        /// </summary>
        /// <param name="ATCommand">AT-command</param>
        /// <remarks>use this method if is in AT-Mode, not in API-Mode</remarks>
        /// <returns>
        /// value of the requested register
        /// null: if an error occurs
        /// </returns>
        public string XBSendGetCommand2LocalDevice(string ATCommand)
        {
            string s;
            //if (logger.IsInfoEnabled) logger.Info("Getting a register on local device in command-mode...");
            if (XBCheckCommandMode(true) > 0)
            {
                XBSendString(ATCommand);
                s = XBGetString(1000);
                if (s != null)
                {
                    XBLeaveCommandMode();
                    //if (logger.IsInfoEnabled) logger.Info("Getting register successfully finished");
                    return s;
                }
            }
            XBLeaveCommandMode();
            //if (logger.IsInfoEnabled) logger.Info("Getting register not successfully finished");
            return null;
        }

        /// <summary>
        /// Gets the configuration of the local device
        /// </summary>
        /// <remarks>the following register are queried:
        /// MY, SH, SL, NI, AP, CE, A2, A1, SM, SP, ST, DH, DL</remarks>
        /// <returns>
        /// configuration of the local device
        /// null: if an error occurs
        /// </returns>
        public CXBNodeInformation XBGetConfigurationOfLocalDevice()     //old name: public CXBNodeInformation XBConnectToLocalDevice()
        {
            //if (logger.IsInfoEnabled) logger.Info("Connect to local device and read configuration...");

            if (XBCheckCommandMode(true) > 0)
            {
                XBSendString(CXBATCommands.XBAT_GetMyAddress);
                string? s = XBGetString(1000);
                if (s != null)
                {
                    LocalDevice.MY_MyAddress = Convert.ToUInt16(s, 16); //s= "A074" bei XB3 Modul
                    //SH
                    XBSendString(CXBATCommands.XBAT_GetSerialNumberH);
                    s = XBGetString(1000);
                    if (s != null)
                    {
                        LocalDevice.SH_SerialNumberHigh = Convert.ToUInt32(s, 16);
                        //SL
                        XBSendString(CXBATCommands.XBAT_GetSerialNumberL);
                        s = XBGetString(1000);
                        if (s != null)
                        {
                            LocalDevice.SL_SerialNumberLow = Convert.ToUInt32(s, 16);

                            XBSendString(CXBATCommands.XBAT_GetNodeIdentifier);
                            s = XBGetString(1000);
                            if (s != null)
                            {
                                if (s == " ") s = "";
                                LocalDevice.NI_NodeIdentifier = s;

                                XBSendString(CXBATCommands.XBAT_GetAPIMode);
                                s = XBGetString(1000);
                                if (s != null)
                                {
                                    LocalDevice.APIMode = (XBAPIMode)(Convert.ToInt16(s));

                                    XBSendString(CXBATCommands.XBAT_GetCoordinatorEnable);
                                    s = XBGetString(1000);
                                    if (s != null)
                                    {
                                        LocalDevice.CE_CoordinatorEnable = Convert.ToByte(s, 16);
                                        //A2
                                        XBSendString(CXBATCommands.XBAT_GetCoordinatorAssociation);
                                        s = XBGetString(1000); //=> Error bei XB3
                                        if (s != null)
                                        {
                                            LocalDevice.A2_CoordinatorAssociation = Convert.ToByte(s, 16);
                                            //ST
                                            XBSendString(CXBATCommands.XBAT_GetTimeBeforeSleep);
                                            s = XBGetString(1000);
                                            if (s != null)
                                            {
                                                LocalDevice.ST_TimeBeforeSleep = Convert.ToUInt16(s, 16);

                                                //SP
                                                XBSendString(CXBATCommands.XBAT_GetCyclicSleepPeriod);
                                                s = XBGetString(1000);
                                                if (s != null)
                                                {
                                                    LocalDevice.SP_CyclicSleepPeriode = Convert.ToUInt16(s, 16);

                                                    //SM
                                                    XBSendString(CXBATCommands.XBAT_GetSleepMode);
                                                    s = XBGetString(1000);
                                                    if (s != null)
                                                    {
                                                        LocalDevice.SM_SleepMode = (XBSleepMode)Convert.ToByte(s, 16);

                                                        //SO
                                                        XBSendString(CXBATCommands.XBAT_GetSleepOption);
                                                        s = XBGetString(1000);
                                                        if (s != null)
                                                        {
                                                            LocalDevice.SO_SleepOption = Convert.ToByte(s, 16);

                                                            //DH, DL
                                                            ulong dhdl;
                                                            XBSendString(CXBATCommands.XBAT_GetDestinationAddressHigh);
                                                            s = XBGetString(1000);
                                                            if (s != null)
                                                            {
                                                                dhdl = Convert.ToUInt64(s, 16) << 32;

                                                                XBSendString(CXBATCommands.XBAT_GetDestinationAddressLow);
                                                                s = XBGetString(1000);
                                                                if (s != null)
                                                                {
                                                                    dhdl += Convert.ToUInt64(s, 16);
                                                                    LocalDevice.DestinationAddress = dhdl; ;

                                                                    XBLeaveCommandMode();
                                                                    //if (logger.IsInfoEnabled) logger.Info("Local configuration successfully read");
                                                                    return LocalDevice;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            XBLeaveCommandMode();
            //if (logger.IsInfoEnabled) logger.Info("Local configuration not successfully read");
            return null;
        }

        /// <summary>
        /// Sets the MY-Address of the local device
        /// </summary>
        /// <param name="MyAddress">MY-Address</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetMyAddress2LocalDevice(ushort MyAddress)
        {
            string ATCommand = CXBATCommands.XBAT_SetMyAddress(MyAddress);

            //if (logger.IsInfoEnabled) logger.Info("Set MY-Address to local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                //if (logger.IsInfoEnabled) logger.Info("MY-Address of local device successfully set");
                LocalDevice.MY_MyAddress = MyAddress;
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("MY-Address of local device not successfully set");
            }
            return res;
        }

        /// <summary>
        /// Sets the CE-Register (coordinator-enable) of the local device
        /// </summary>
        /// <param name="CE">CE-Register</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetCoordinatorEnable2LocalDevice(byte CE)
        {
            string ATCommand = CXBATCommands.XBAT_SetCoordinatorEnable(CE);

            //if (logger.IsInfoEnabled) logger.Info("Setting CE-Register of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                LocalDevice.CE_CoordinatorEnable = CE;
                //if (logger.IsInfoEnabled) logger.Info("CE-Register of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("CE-Register of local device not successfully set");
            }
            return res;
        }

        /// <summary>
        /// Sets the A2-Register (coordinator-association) of the local device
        /// </summary>
        /// <param name="A2">A2-Register</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetCoordinatorAssociation2LocalDevice(byte A2)
        {
            string ATCommand = CXBATCommands.XBAT_SetCoordinatorAssociation(A2);

            //if (logger.IsInfoEnabled) logger.Info("Setting the A2-Register of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);
            if (res > 0)
            {
                LocalDevice.A2_CoordinatorAssociation = A2;
                //if (logger.IsInfoEnabled) logger.Info("A2-Register of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("A2-Register of local device not successfully set");
            }
            return res;
        }

        /// <summary>
        /// checks the rules for node-identifier string
        /// </summary>
        /// <remarks>only checks if the first character is not a string</remarks>
        /// <param name="newNodeIDString">new NI-string</param>
        /// <returns>
        /// true: if the string is ok
        /// false: if the string is not ok
        /// </returns>
        public bool XBCheckNodeIdentifierString(string newNodeIDString)
        {
            if (newNodeIDString.Length > 0)
            {
                //string can not start with a space
                if (newNodeIDString[0] == 0x20)
                {
                    //XBSetLastError(12);
                    _Error.SetError_NodeIdentifierString();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the Node-Identifier of the local device
        /// </summary>
        /// <param name="newNodeIdentifier">node-identifier</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetNodeIdentifier2LocalDevice(string newNodeIdentifier)
        {
            if (XBCheckNodeIdentifierString(newNodeIdentifier))
            {
                string ATCommand = CXBATCommands.XBAT_SetNodeIdentifier(newNodeIdentifier);

                //if (logger.IsInfoEnabled) logger.Info("Setting the NI-Register of local device...");
                int res;
                if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                    LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                    res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
                else
                    res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

                if (res > 0)
                {
                    LocalDevice.NI_NodeIdentifier = newNodeIdentifier;
                    //if (logger.IsInfoEnabled) logger.Info("NI-Register of local device successfully set");
                }
                else
                    //if (logger.IsInfoEnabled) logger.Info("NI-Register of local device not successfully set");
                    return res;
            }
            return -1;

        }

        /// <summary>
        /// Sends an command to set an register of an local device
        /// </summary>
        /// <param name="ATCommand">command in AT-format</param>
        /// <remarks>
        /// <para>this methods works in API-mode and in AT-Mode</para>
        /// <para>for the following registers use the specific setter-method: MY, NI, AP, CE, A2, CC, GT, NT</para>
        /// </remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSendSetCommand2LocalDevice(string ATCommand)
        {
            //if (logger.IsInfoEnabled) logger.Info("Setting a register of local deviced...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                //if (logger.IsInfoEnabled) logger.Info("Register of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
            }
            return res;
        }

        /// <summary>
        /// sends an command to local device for setting an register in API-Mode
        /// </summary>
        /// <param name="ATCommand">command in AT-format</param>
        /// <returns>
        /// 1: if register successfully set
        /// -1: an error occurs
        /// </returns>
        private int XBSendSetCommand2LocalDeviceAPIMode(string ATCommand)
        {
            //if (logger.IsInfoEnabled) logger.Info("Setting a register on local device in API-mode...");
            CATCommand request = new()
            {
                ATCommandResponse = true,
                ATCommand = ATCommand
            };
            CATCommandResponse response = (CATCommandResponse)XBSendRequest(request, 500);
            if (response != null)
            {
                switch (response.status)
                {
                    case RXCommandResponseStatus.OK:
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device successfully set");
                        return 1;
                    case RXCommandResponseStatus.ERROR:
                        //XBSetLastError(6);
                        _Error.SetError_ATErrorStatus();
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
                        return -1;
                    case RXCommandResponseStatus.InvalidCommand:
                        //XBSetLastError(7);
                        _Error.SetError_ATInvalidCommand();
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
                        return -1;
                    case RXCommandResponseStatus.InvalidParameter:
                        //XBSetLastError(8);
                        _Error.SetError_ATInvalidParam();
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
                        return -1;
                    default:
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
                        return -1;
                }
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
                return -1;
            }
        }

        /// <summary>
        /// sends an command to local device for setting an register in AT-Mode
        /// </summary>
        /// <param name="ATCommand">command in AT-format</param>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: if register successfully set
        /// -1: an error occurs
        /// </returns>
        private int XBSendSetCommand2LocalDeviceCMDMode(string ATCommand)
        {
            //if (logger.IsInfoEnabled) logger.Info("Setting a register on local device in command-mode...");
            if (XBCheckCommandMode(true) > 0)
            {
                if (XBSendString_CheckOKResponse(ATCommand) > 0)
                {
                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory) > 0)
                    {
                        //if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_ApplyChanges) > 0) // not neccessary if WR+CN was sent
                        //{

                        XBLeaveCommandMode();
                        CModemStatus response = (CModemStatus)XBGetResponse(); //waits for ModemStatus for 500msec.
                        if (response != null)
                        {
                            //if (logger.IsInfoEnabled) logger.Info("Register of local device successfully set");
                            return (int)response.modemStatus;
                        }
                        //if (logger.IsInfoEnabled) logger.Info("Last timeout could be a result of not receiving any modem-status response, which is only sending while specific conditions");
                        //if (logger.IsInfoEnabled) logger.Info("Register of local device successfully set");
                        return 1;
                        //}
                    }
                }
            }
            //if (logger.IsInfoEnabled) logger.Info("Register of local device not successfully set");
            return -1;
        }

        /// <summary>
        /// Sets the CC-Register (common sequ char) of the local device
        /// </summary>
        /// <param name="newCharacter">character</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetCommonSequenceCharacter2LocalDevice(char newCharacter)
        {
            string ATCommand = CXBATCommands.XBAT_SetCommandSequenceCharacter((byte)newCharacter);

            //if (logger.IsInfoEnabled) logger.Info("Setting the CC (Command Sequence Character) of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                _CommonSequenceCharacter = newCharacter;
                //if (logger.IsInfoEnabled) logger.Info("CC of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("CC of local device not successfully set");
            }
            return res;
        }

        /// <summary>
        /// Sets the GT-Register (guard times) of the local device
        /// </summary>
        /// <param name="newGuardTimes">guard times</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetGuardTimes2LocalDevice(ushort newGuardTimes)
        {
            string ATCommand = CXBATCommands.XBAT_SetGuardTimes(newGuardTimes);

            //if (logger.IsInfoEnabled) logger.Info("Setting the GT (Guard Times) of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                _GuardTimes = newGuardTimes;
                //if (logger.IsInfoEnabled) logger.Info("GT of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("GT of local device not successfully set");
            }

            return res;
        }



        /// <summary>
        /// Sets the NT-Register (node discover timeout) of the local device
        /// </summary>
        /// <param name="newNodeDiscoverTime">node discover timeout</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetNodeDiscoverTime2LocalDevice(byte newNodeDiscoverTime)
        {
            string ATCommand = CXBATCommands.XBAT_SetNodeDiscoverTime(newNodeDiscoverTime);

            //if (logger.IsInfoEnabled) logger.Info("Setting the NT (Node Discover Time) of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);

            if (res > 0)
            {
                _NodeDiscoverTime = newNodeDiscoverTime;
                //if (logger.IsInfoEnabled) logger.Info("NT of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("NT of local device not successfully set");
            }

            return res;
        }

        /// <summary>
        /// Sets the API-Mode of the local device
        /// </summary>
        /// <param name="APIMode">API-Mode</param>
        /// <remarks>this methods works in API-mode and in AT-Mode</remarks>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: register successfully set
        /// -1: an error occurs
        /// </returns>
        public int XBSetAPIMode2LocalDevice(XBAPIMode APIMode)
        {
            string ATCommand = CXBATCommands.XBAT_SetAPIMode(APIMode);

            //if (logger.IsInfoEnabled) logger.Info("Setting the API-Mode of local device...");
            int res;
            if (LocalDevice.APIMode == XBAPIMode.Enabled ||
                LocalDevice.APIMode == XBAPIMode.Enabled_w_Escape_Control_Chars)
                res = XBSendSetCommand2LocalDeviceAPIMode(ATCommand);
            else
                res = XBSendSetCommand2LocalDeviceCMDMode(ATCommand);
            if (res > 0)
            {
                LocalDevice.APIMode = APIMode;
                //if (logger.IsInfoEnabled) logger.Info("API-Mode of local device successfully set");
            }
            else
            {
                //if (logger.IsInfoEnabled) logger.Info("API-Mode of local device not successfully set");
            }

            return res;
        }

        /// <summary>
        /// sets the A1 of the endDevice
        /// </summary>
        /// <param name="EndDeviceSerialNumber">serial number of end device</param>
        /// <param name="A1">end device association</param>
        /// <remarks>
        /// after successful setting of A1, it trys to write (WR-Command) changes to the non-vol memory
        /// until end device is reachable again (after setting A1, enddevice is not reachable for about 3 seconds)
        /// </remarks>
        /// <returns>
        /// 1: A1 successfully written
        /// -1: A1 not successfully set (an error occurs)
        /// </returns>
        public int XBSetEndDeviceAssociation2EndDevice(ulong EndDeviceSerialNumber, byte A1)
        {
            //if (logger.IsInfoEnabled) logger.Info("Setting the A1-register of the end device...");

            if (LocalDevice.APIMode == XBAPIMode.Disabled)
            {
                //XBSetLastError(9);
                _Error.SetError_LocalDevNotInApi();
                //if (logger.IsInfoEnabled) logger.Info("Setting A1-register failed");
                return -1;
            }

            CRemoteATCommandRequest remoteCommand = new()
            {
                ApplyChangesOnRemote = true,
                ATCommandResponse = true,
                DestinationAddress64 = EndDeviceSerialNumber,

                //SetEndDeviceAssocation
                ATCommand = CXBATCommands.XBAT_SetEndDeviceAssociation(A1)
            };
            CRemoteCommandResponse response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
            if (response != null && response.status == RXCommandResponseStatus.OK)
            {
                //try to write the A1-command and waits until the end device is reachable again
                do
                {
                    remoteCommand.ATCommand = CXBATCommands.XBAT_Write_to_nonvolatilememory;
                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                    if (response != null && response.status == RXCommandResponseStatus.OK)      //WRITE successful
                    {
                        CurrentEndDevice.A1_EndDeviceAssociation = A1;
                        return 1;
                    }
                }
                while (response != null && response.status == RXCommandResponseStatus.NoResponse);
                //until API-packet with WR-command returns a "No response" 
            }

            if (response == null)
            {
                //if (logger.IsInfoEnabled) logger.Info("Setting the A1-register not successfully finished");
                return -1;
            }
            else
            {
                SetResponseError(response);
                //if (logger.IsInfoEnabled) logger.Info("Setting the A1-register not successfully finished");
                return -1;
            }
        }


        /// <summary>
        /// Configures remote device
        /// </summary>
        /// <param name="EndDeviceSerialNumber">Serial number of end device(64bit)</param>
        /// <param name="newEndDeviceConfig">new configuration (AP, CE, A1, NI, Baud, SM, SP, ST, DH, DL must be set)</param>
        /// <remarks>
        /// also commits the WR command
        /// </remarks>
        /// <returns>
        /// 1: if configuration was successful
        /// -1: an error occurs
        /// </returns>
        public int XBConfigureRemoteDevice(ulong EndDeviceSerialNumber, CXBNodeInformation newEndDeviceConfig)
        {
            if (LocalDevice.APIMode == XBAPIMode.Disabled)
            {
                //XBSetLastError(9);
                _Error.SetError_LocalDevNotInApi();
                //if (logger.IsInfoEnabled) logger.Info("Configuration of end-device not successfully finished");
                return -1;
            }

            /*
            if (XBCheckNodeIdentifierString(newEndDeviceConfig.NI_NodeIdentifier) == false)
            {
                //if (logger.IsInfoEnabled) logger.Info("Configuration of end-device not successfully finished");
                return -1;
            }*/

            CRemoteATCommandRequest remoteCommand = new()
            {
                ApplyChangesOnRemote = false,     //!!!! must be set for the last Command!!!!!
                ATCommandResponse = true,
                DestinationAddress64 = EndDeviceSerialNumber,

                //Set AP
                ATCommand = CXBATCommands.XBAT_SetAPIMode(newEndDeviceConfig.APIMode)
            };
            CRemoteCommandResponse response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
            if (response != null && response.status == RXCommandResponseStatus.OK)
            {
                CurrentEndDevice.APIMode = newEndDeviceConfig.APIMode;

                //Set CE
                remoteCommand.ATCommand = CXBATCommands.XBAT_SetCoordinatorEnable(newEndDeviceConfig.CE_CoordinatorEnable);
                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                if (response != null && response.status == RXCommandResponseStatus.OK)
                {
                    CurrentEndDevice.CE_CoordinatorEnable = newEndDeviceConfig.CE_CoordinatorEnable;

                    //MY Address
                    remoteCommand.ATCommand = CXBATCommands.XBAT_SetMyAddress(newEndDeviceConfig.MY_MyAddress);
                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                    if (response != null && response.status == RXCommandResponseStatus.OK)
                    {
                        CurrentEndDevice.MY_MyAddress = newEndDeviceConfig.MY_MyAddress;

                        //NI Node Identifier
                        if (XBCheckNodeIdentifierString(newEndDeviceConfig.NI_NodeIdentifier) == true)
                        {
                            remoteCommand.ATCommand = CXBATCommands.XBAT_SetNodeIdentifier(newEndDeviceConfig.NI_NodeIdentifier);
                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                        }
                        if (response != null && response.status == RXCommandResponseStatus.OK)
                        {
                            CurrentEndDevice.NI_NodeIdentifier = newEndDeviceConfig.NI_NodeIdentifier;

                            //Set Baud
                            remoteCommand.ATCommand = CXBATCommands.XBAT_SetBaudRate(newEndDeviceConfig.BaudRate);
                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                            if (response != null && response.status == RXCommandResponseStatus.OK)
                            {
                                CurrentEndDevice.BaudRate = newEndDeviceConfig.BaudRate;
                                //SM Set Sleep Mode
                                remoteCommand.ATCommand = CXBATCommands.XBAT_SetSleepMode(newEndDeviceConfig.SM_SleepMode);
                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                {
                                    CurrentEndDevice.SM_SleepMode = newEndDeviceConfig.SM_SleepMode;

                                    //ST
                                    remoteCommand.ATCommand = CXBATCommands.XBAT_SetTimeBeforeSleep(newEndDeviceConfig.ST_TimeBeforeSleep);
                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                    {
                                        CurrentEndDevice.ST_TimeBeforeSleep = newEndDeviceConfig.ST_TimeBeforeSleep;

                                        //SP
                                        remoteCommand.ATCommand = CXBATCommands.XBAT_SetCyclicSleepPeriod(newEndDeviceConfig.SP_CyclicSleepPeriode);
                                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                        if (response != null && response.status == RXCommandResponseStatus.OK)
                                        {
                                            //SO
                                            remoteCommand.ATCommand = CXBATCommands.XBAT_SetSleepOption(newEndDeviceConfig.SO_SleepOption);
                                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                            if (response != null && response.status == RXCommandResponseStatus.OK)
                                            {
                                                //RO
                                                remoteCommand.ATCommand = CXBATCommands.XBAT_SetROPacketizationTimeout(newEndDeviceConfig.R0_PacketizationTimeout);
                                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                                {
                                                    //DH
                                                    remoteCommand.ATCommand = CXBATCommands.XBAT_SetDestinationAddressHigh(newEndDeviceConfig.DestinationAddress);
                                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                                    {
                                                        CurrentEndDevice.DestinationAddress = newEndDeviceConfig.DestinationAddress;
                                                        //DL
                                                        remoteCommand.ATCommand = CXBATCommands.XBAT_SetDestinationAddressLow(newEndDeviceConfig.DestinationAddress);
                                                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                        if (response != null && response.status == RXCommandResponseStatus.OK)
                                                        {
                                                            //A2
                                                            remoteCommand.ATCommand = CXBATCommands.XBAT_SetCoordinatorAssociation(newEndDeviceConfig.A2_CoordinatorAssociation);
                                                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                            if (response != null && response.status == RXCommandResponseStatus.OK)
                                                            {

                                                                /*
                                                                //A1 SetEndDeviceAssocation
                                                                //17.4.2014
                                                                remoteCommand.ATCommand = CXBATCommands.XBAT_SetEndDeviceAssociation(newEndDeviceConfig.A1_EndDeviceAssociation);
                                                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                                                {
                                                                    //Write to memory with AC 
                                                                    remoteCommand.ATCommand = CXBATCommands.XBAT_Apply_Changes;
                                                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                                                    {
                                                                        //Soft Reset
                                                                        remoteCommand.ApplyChangesOnRemote = true;
                                                                        remoteCommand.ATCommand = CXBATCommands.XBAT_SotfwareReset;
                                                                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);   //No response from this command
                                                                        //Wait to come out of reset (3s)
                                                                        Thread.Sleep(3000);

                                                                        return 1;
                                                                    }
                                                                }*/


                                                                //WR Write to Nonvolatile memory
                                                                //remoteCommand.ATCommand = CXBATCommands.XBAT_Write_to_nonvolatilememory;
                                                                //response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                                                {

                                                                    //A1
                                                                    //Issues also write to non volatile memory command
                                                                    if (XBSetEndDeviceAssociation2EndDevice(EndDeviceSerialNumber, newEndDeviceConfig.A1_EndDeviceAssociation) > 0)
                                                                    {
                                                                        CurrentEndDevice.A1_EndDeviceAssociation = newEndDeviceConfig.A1_EndDeviceAssociation;

                                                                        //Soft Reset
                                                                        remoteCommand.ATCommand = CXBATCommands.XBAT_SotfwareReset;
                                                                        _ = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);   //No response from this command
                                                                        //Wait to come out of reset (3s)
                                                                        Thread.Sleep(3000);
                                                                        return 1;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (response != null) SetResponseError(response);
            //if (logger.IsInfoEnabled) logger.Info("Configuration of end-device failed");
            return -1;
        }

        /// <summary>
        /// Reads the configuration of the end device
        /// </summary>
        /// <remarks>
        /// this methods stores the read information to the private member CurrentEndDevice
        /// MY, SH, SL, NI, AP, CE, A2, A1, SM, SP, ST, DH, DL, BaudRate
        /// </remarks>
        /// <param name="EndDeviceSerialNumber">Serial Number of End Device (64bit)</param>
        /// <returns>
        /// configuration of the end device,
        /// null: if an error occurs
        /// </returns>
        public CXBNodeInformation XBGetConfigurationOfRemoteDevice(ulong EndDeviceSerialNumber)
        {

            //if (logger.IsInfoEnabled) logger.Info("Get configuration of end device you want to configurate...");

            if (LocalDevice.APIMode == XBAPIMode.Disabled)
            {
                //XBSetLastError(9);
                _Error.SetError_LocalDevNotInApi();
                //if (logger.IsInfoEnabled) logger.Info("Getting the Configuration of end-device not successfully finished");
                return null;
            }

            CRemoteATCommandRequest remoteCommand = new()
            {
                ApplyChangesOnRemote = false,
                ATCommandResponse = true,
                DestinationAddress64 = EndDeviceSerialNumber,

                //Get AP
                ATCommand = CXBATCommands.XBAT_GetAPIMode
            };
            CRemoteCommandResponse response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 3000);
            if (response != null && response.status == RXCommandResponseStatus.OK)
            {
                if (response.valueOfCommand.Length > 0)
                    CurrentEndDevice.APIMode = (XBAPIMode)response.valueOfCommand[0];

                //Get MY
                remoteCommand.ATCommand = CXBATCommands.XBAT_GetMyAddress;
                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                if (response != null && response.status == RXCommandResponseStatus.OK)
                {
                    if (response.valueOfCommand.Length > 0)
                    {
                        CurrentEndDevice.MY_MyAddress = (ushort)CMyConvert.FromUIntBytestoUInt(response.valueOfCommand[1], response.valueOfCommand[0]);
                    }
                    //Get SH
                    remoteCommand.ATCommand = CXBATCommands.XBAT_GetSerialNumberH;
                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                    if (response != null && response.status == RXCommandResponseStatus.OK)
                    {
                        if (response.valueOfCommand.Length > 0)
                        {
                            CurrentEndDevice.SH_SerialNumberHigh = ((uint)response.valueOfCommand[0]) << 24;
                            CurrentEndDevice.SH_SerialNumberHigh += ((uint)response.valueOfCommand[1]) << 16;
                            CurrentEndDevice.SH_SerialNumberHigh += ((uint)response.valueOfCommand[2]) << 8;
                            CurrentEndDevice.SH_SerialNumberHigh += response.valueOfCommand[3];
                        }
                        //Get SL
                        remoteCommand.ATCommand = CXBATCommands.XBAT_GetSerialNumberL;
                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                        if (response != null && response.status == RXCommandResponseStatus.OK)
                        {
                            if (response.valueOfCommand.Length > 0)
                            {
                                CurrentEndDevice.SL_SerialNumberLow = ((uint)response.valueOfCommand[0]) << 24;
                                CurrentEndDevice.SL_SerialNumberLow += ((uint)response.valueOfCommand[1]) << 16;
                                CurrentEndDevice.SL_SerialNumberLow += ((uint)response.valueOfCommand[2]) << 8;
                                CurrentEndDevice.SL_SerialNumberLow += response.valueOfCommand[3];
                            }

                            //Get CE
                            remoteCommand.ATCommand = CXBATCommands.XBAT_GetCoordinatorEnable;
                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                            if (response != null && response.status == RXCommandResponseStatus.OK)
                            {
                                if (response.valueOfCommand.Length > 0)
                                    CurrentEndDevice.CE_CoordinatorEnable = response.valueOfCommand[0];

                                //Get EndDeviceAssocation
                                remoteCommand.ATCommand = CXBATCommands.XBAT_GetEndDeviceAssociation;
                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                {
                                    if (response.valueOfCommand.Length > 0)
                                        CurrentEndDevice.A1_EndDeviceAssociation = response.valueOfCommand[0];

                                    //Get Node Identifier
                                    remoteCommand.ATCommand = CXBATCommands.XBAT_GetNodeIdentifier;
                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                    {
                                        if (response.valueOfCommand.Length > 0)
                                        {
                                            CurrentEndDevice.NI_NodeIdentifier = CMyConvert.ByteArraytoString(response.valueOfCommand);
                                            if (CurrentEndDevice.NI_NodeIdentifier == " ") CurrentEndDevice.NI_NodeIdentifier = "";
                                        }
                                        //A2 Get CoordinatorAssociation
                                        remoteCommand.ATCommand = CXBATCommands.XBAT_GetCoordinatorAssociation;
                                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                        if (response != null && response.status == RXCommandResponseStatus.OK)
                                        {
                                            if (response.valueOfCommand.Length > 0)
                                                CurrentEndDevice.A2_CoordinatorAssociation = response.valueOfCommand[0];

                                            //ST
                                            remoteCommand.ATCommand = CXBATCommands.XBAT_GetTimeBeforeSleep;
                                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                            if (response != null && response.status == RXCommandResponseStatus.OK)
                                            {
                                                if (response.valueOfCommand.Length > 0)
                                                    CurrentEndDevice.ST_TimeBeforeSleep = (ushort)CMyConvert.FromUIntBytestoUInt(response.valueOfCommand[1], response.valueOfCommand[0]);

                                                //SP
                                                remoteCommand.ATCommand = CXBATCommands.XBAT_GetCyclicSleepPeriod;
                                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                                {
                                                    if (response.valueOfCommand.Length > 0)
                                                        CurrentEndDevice.SP_CyclicSleepPeriode = (ushort)CMyConvert.FromUIntBytestoUInt(response.valueOfCommand[1], response.valueOfCommand[0]);

                                                    //SM
                                                    remoteCommand.ATCommand = CXBATCommands.XBAT_GetSleepMode;
                                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                                    {
                                                        if (response.valueOfCommand.Length > 0)
                                                            CurrentEndDevice.SM_SleepMode = (XBSleepMode)response.valueOfCommand[0];

                                                        //SO
                                                        remoteCommand.ATCommand = CXBATCommands.XBAT_GetSleepOption;
                                                        response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                        if (response != null && response.status == RXCommandResponseStatus.OK)
                                                        {
                                                            if (response.valueOfCommand.Length > 0)
                                                                CurrentEndDevice.SO_SleepOption = response.valueOfCommand[0];

                                                            //BD
                                                            remoteCommand.ATCommand = CXBATCommands.XBAT_GetBaudRate;
                                                            response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                            if (response != null && response.status == RXCommandResponseStatus.OK)
                                                            {
                                                                if (response.valueOfCommand.Length > 0)
                                                                {
                                                                    CurrentEndDevice.BaudRate = CXBATCommands.XBBaudRates.GetBaudRate(BitConverter.ToUInt32(CMyTools.SwapByteArray(response.valueOfCommand), 0));
                                                                    //CurrentEndDevice.BaudRate = CXBATCommands.XBBaudRates[response.valueOfCommand[3]];
                                                                }

                                                                //DH, DL
                                                                ulong dhdl = 0;
                                                                remoteCommand.ATCommand = CXBATCommands.XBAT_GetDestinationAddressHigh;
                                                                response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                                if (response != null && response.status == RXCommandResponseStatus.OK)
                                                                {
                                                                    if (response.valueOfCommand.Length > 0)
                                                                    {
                                                                        dhdl += BitConverter.ToUInt32(CMyTools.SwapByteArray(response.valueOfCommand), 0);
                                                                        dhdl <<= 32;
                                                                    }
                                                                    remoteCommand.ATCommand = CXBATCommands.XBAT_GetDestinationAddressLow;
                                                                    response = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                                                                    if (response != null && response.status == RXCommandResponseStatus.OK)
                                                                    {
                                                                        if (response.valueOfCommand.Length > 0)
                                                                        {
                                                                            dhdl += BitConverter.ToUInt32(CMyTools.SwapByteArray(response.valueOfCommand), 0);
                                                                        }

                                                                        CurrentEndDevice.DestinationAddress = dhdl;

                                                                        //if (logger.IsInfoEnabled) logger.Info("Getting the Configuration of end-device successfully finished.");
                                                                        return CurrentEndDevice;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (response != null) SetResponseError(response);
            //if (logger.IsInfoEnabled) logger.Info("Getting the Configuration of end-device not successfully finished");
            return null;
        }

        private void SetResponseError(CRemoteCommandResponse response)
        {
            switch (response.status)
            {
                case RXCommandResponseStatus.ERROR:
                    //XBSetLastError(6);
                    _Error.SetError_ATErrorStatus();
                    break;
                case RXCommandResponseStatus.InvalidCommand:
                    //XBSetLastError(7);
                    _Error.SetError_ATInvalidCommand();
                    break;
                case RXCommandResponseStatus.InvalidParameter:
                    //XBSetLastError(8);
                    _Error.SetError_ATInvalidParam();
                    break;
                case RXCommandResponseStatus.NoResponse:
                    //XBSetLastError(10);
                    _Error.SetError_ATNoResponse();
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Configures local device
        /// </summary>
        /// <param name="newLocalDeviceConfig">new configuration (AP, MY, CE, A2, NI must be set)</param>
        /// <returns>
        /// >1: if register successfully set and modem-status-message (status=return value) was received
        /// 1: if configuration was successful
        /// -1: an error occurs
        /// </returns>
        public int XBConfigureLocalDevice(CXBNodeInformation newLocalDeviceConfig)
        {
            //if (logger.IsInfoEnabled) logger.Info("Configure local device with new configuration based on new CXBNodeInformation object...");
            if (XBCheckCommandMode(true) > 0)
            {
                //Node Identifier 
                //if (logger.IsDebugEnabled) logger.Debug("NI is set to local");
                if (XBCheckNodeIdentifierString(newLocalDeviceConfig.NI_NodeIdentifier))
                {
                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetNodeIdentifier(newLocalDeviceConfig.NI_NodeIdentifier)) > 0)
                    {
                        LocalDevice.NI_NodeIdentifier = newLocalDeviceConfig.NI_NodeIdentifier;
                    }
                    else
                    {
                        //if (logger.IsErrorEnabled) logger.Error("NodeIdentifier not set");
                        return -1;
                    }
                }

                //AP Enabled Enable AP=1
                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetAPIMode(newLocalDeviceConfig.APIMode)) > 0)
                {
                    LocalDevice.APIMode = newLocalDeviceConfig.APIMode;
                    //MY
                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetMyAddress(newLocalDeviceConfig.MY_MyAddress)) > 0)
                    {
                        LocalDevice.MY_MyAddress = newLocalDeviceConfig.MY_MyAddress;

                        //CE Coordinator Enable
                        if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetCoordinatorEnable(newLocalDeviceConfig.CE_CoordinatorEnable)) > 0)
                        {
                            LocalDevice.CE_CoordinatorEnable = newLocalDeviceConfig.CE_CoordinatorEnable;

                            //A2 Coordinator Accociation 
                            if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetCoordinatorAssociation(newLocalDeviceConfig.A2_CoordinatorAssociation)) > 0)
                            {
                                LocalDevice.A2_CoordinatorAssociation = newLocalDeviceConfig.A2_CoordinatorAssociation;

                                //ST
                                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetTimeBeforeSleep(newLocalDeviceConfig.ST_TimeBeforeSleep)) > 0)
                                {
                                    LocalDevice.ST_TimeBeforeSleep = newLocalDeviceConfig.ST_TimeBeforeSleep;

                                    //SP
                                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetCyclicSleepPeriod(newLocalDeviceConfig.SP_CyclicSleepPeriode)) > 0)
                                    {
                                        LocalDevice.SP_CyclicSleepPeriode = newLocalDeviceConfig.SP_CyclicSleepPeriode;

                                        //SO
                                        if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetSleepOption(newLocalDeviceConfig.SO_SleepOption)) > 0)
                                        {
                                            LocalDevice.SO_SleepOption = newLocalDeviceConfig.SO_SleepOption;

                                            //SM
                                            if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetSleepMode(newLocalDeviceConfig.SM_SleepMode)) > 0)
                                            {
                                                LocalDevice.SM_SleepMode = newLocalDeviceConfig.SM_SleepMode;

                                                //RO
                                                if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetROPacketizationTimeout(newLocalDeviceConfig.R0_PacketizationTimeout)) > 0)
                                                {
                                                    LocalDevice.R0_PacketizationTimeout = newLocalDeviceConfig.R0_PacketizationTimeout;

                                                    //RO
                                                    if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SetROPacketizationTimeout(newLocalDeviceConfig.R0_PacketizationTimeout)) > 0)
                                                    {
                                                        LocalDevice.R0_PacketizationTimeout = newLocalDeviceConfig.R0_PacketizationTimeout;


                                                        //Write to Nonvolatile memory
                                                        if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory) > 0)
                                                        {
                                                            //Reset necessary otherwise does not switch to API mode ... 
                                                            //Kicks device out of API mode
                                                            if (XBSendString_CheckOKResponse(CXBATCommands.XBAT_SotfwareReset) > 0)
                                                            {
                                                                //Should return 2 ModemStatus Strings
                                                                //7E 00 02 8A 01 74 7E 00 02 8A 06 6F
                                                                TimeSentCommandString = DateTime.MinValue;  //Reset API Mode Timer
                                                                Thread.Sleep(400);  //Wait for response
                                                                CModemStatus response = (CModemStatus)XBGetResponse();

                                                                if (response != null)
                                                                {
                                                                    response = (CModemStatus)XBGetResponse();
                                                                    if (response != null)
                                                                    {
                                                                        //if (logger.IsInfoEnabled) logger.Info("Local Device Configured and Reset");
                                                                        return 1;
                                                                    }
                                                                }

                                                            }


                                                            /*
                                                            XBLeaveCommandMode();
                                                            CModemStatus response = (CModemStatus)XBGetResponse();
                                                            if (response != null)
                                                            {
                                                                //if (logger.IsInfoEnabled) logger.Info("Local Device Configured. Modem-Status-Message of type: " + response.modemStatus.ToString() + " received");
                                                                return (int)response.modemStatus;
                                                            }
                                                            //if (logger.IsInfoEnabled) logger.Info("Local Device Configured: BUT failed to get Modem Status");
                                                            return 1;*/
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //if (logger.IsInfoEnabled) logger.Info("Configuration not successfully finished");
            return -1;
        }

        /// <summary>
        /// Pairs Devices
        /// </summary>
        public bool ManageXBeePairing(CXBNodeInformation LocalDeviceDefault_Configuration, CXBNodeInformation RemoteDeviceDefault_Configuration)
        {
            bool ret = false;
            //Possible to open port?
            try
            {
                //Todo: Seriell32 bedient jetzt die DTR Leitung ... sollte aber nicht sein!!!
                //sollte auf open und close in CXBee connection zugreifen

                if (!Seriell32.IsOpen) Seriell32.GetOpen();
                if (Seriell32.IsOpen)
                {
                    //Port OK, now check Baud
                    if (XBCheckBaudrate() == 1)
                    {
                        //Baud = OK
                        ret = true;
                    }
                }
            }
            catch
            {
                ret = false;
            }
            finally
            {
                Seriell32.Close();
            }

            if (ret == false)
            {
                //Either Port or Baud Not OK
                //Search for correct Baud, Port
                //if (logger.IsInfoEnabled) logger.Info("Have to search for Ports ....");
                if (!Search4LocalXBeeModule())
                {
                    return false;
                }
            }

            if (!Seriell32.IsOpen) Seriell32.GetOpen();
            if (Seriell32.IsOpen)
            {
                ret = false; //18.10.2023
                //Now we have a Local Device - lets check first whether it is configured correctly
                if (Check_Set_LocalDeviceConfiguration(LocalDeviceDefault_Configuration))
                {
                    //Check BaudRate - is it the Default BaudRate?
                    if (Seriell32.BaudRate != LocalDeviceDefault_Configuration.BaudRate)
                    {
                        if (!Seriell32.IsOpen) Seriell32.GetOpen();
                        if (Seriell32.IsOpen)
                        {
                            //We have to set default Baud Rate
                            if (XBSetSerialPort2LocalDevice(LocalDeviceDefault_Configuration.BaudRate, System.IO.Ports.Parity.None) == 1)
                            {
                                Seriell32.Close();
                                Seriell32.BaudRate = (int)LocalDeviceDefault_Configuration.BaudRate;
                                if (Seriell32.BaudRate > 57600)
                                {
                                    //XBee Workaround that Node Discovery works at higher baud rates
                                    Seriell32.StopBits = System.IO.Ports.StopBits.Two;
                                }
                                else
                                {
                                    Seriell32.StopBits = System.IO.Ports.StopBits.One;
                                }
                                ret = true;
                            }
                        }
                    }
                    else

                    {
                        //Baud OK
                        ret = true;
                    }

                    if (ret)
                    {
                        //Go for Remote Device
                        ret = Check_Set_RemoteDeviceConfiguration(RemoteDeviceDefault_Configuration);
                    }
                }

                if (Seriell32.IsOpen)
                    Seriell32.Close();
                return ret;
            }
            //if (logger.IsErrorEnabled) logger.Error("Cannot reopen port " + Seriell32.PortName);
            return false;

        }

        /// <summary>
        /// Checks the Configuration of the Local Device and sets it to required parameters
        /// </summary>
        private bool Check_Set_LocalDeviceConfiguration(CXBNodeInformation LocalDeviceConfiguration)
        {
            bool ret = false;
            //Just ckeck parameters currently relevant
            XBGetConfigurationOfLocalDevice();  //ToDo: notwendig?
            CXBNodeInformation NILocal = (CXBNodeInformation)LocalDevice.Clone();
            NILocal.UpdateIfNotDefault(LocalDeviceConfiguration);

            if (NILocal.CompareTo(LocalDevice) != 0)
            {
                //So we have to set Parameters for Local Device
                if (!Seriell32.IsOpen) Seriell32.GetOpen();
                if (Seriell32.IsOpen)
                {
                    if (XBConfigureLocalDevice(NILocal) > 0)  //Set Params
                    {
                        //Success
                        //if (logger.IsInfoEnabled) logger.Info("Configuration successfully finished");
                        ret = true;
                    }
                    else
                    {
                        //if (logger.IsInfoEnabled) logger.Info(_Error.LastError_String);
                        _Error.SetError_XBeeError();
                    }
                }
                else
                {
                    //Port cannnot be re-open
                    //if (logger.IsInfoEnabled) logger.Info("Check_Set_LocalDeviceConfiguration: Port cannnot be re-open");
                    _Error.SetError_PortCannotBeReopened();
                }
            }
            else
            {
                //Params are OK, nothing to do
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Configures the remote XBEE device
        /// </summary>
        /// <param name="RemoteDeviceDefault_Configuration">The remote device default_ configuration.</param>
        /// <returns></returns>
        private bool Check_Set_RemoteDeviceConfiguration(CXBNodeInformation RemoteDeviceDefault_Configuration)
        {
            bool ret = false;
            //Node Discover
            if (!Seriell32.IsOpen) Seriell32.GetOpen();
            List<CXBNodeInformation> NodeInfos = XBNodeDiscoverAPI();


            if ((NodeInfos == null) || (NodeInfos.Count == 0))
            {
                _Error.SetError_NoRemoteDevice();    //Error
                //if (logger.IsInfoEnabled) logger.Info(_Error.LastError_String);
                if (DisplayMessages)
                {
                    //MessageBox.Show(_Error.LastError_String, MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (NodeInfos.Count == 1)
            {
                //one device is discovered
                ulong DestinationAddress64 = NodeInfos[0].SerialNumber;

                //Only One remote device in Range, configure it
                CRemoteATCommandRequest remoteCommand = new()
                {
                    ApplyChangesOnRemote = false,     //!!!! must be set for the last Command!!!!!
                    ATCommandResponse = true,
                    DestinationAddress64 = DestinationAddress64
                };

                CXBNodeInformation remoteDevice = XBGetConfigurationOfRemoteDevice(DestinationAddress64);
                if (remoteDevice != null)
                {
                    CXBNodeInformation def = new(); //(CXBNodeInformation)remoteDevice.Clone();
                    def.UpdateIfNotDefault(RemoteDeviceDefault_Configuration);
                    def.BaudRate = RemoteDeviceDefault_Configuration.BaudRate;  //Assuemes that there a default values stored in it 
                    def.DestinationAddress = LocalDevice.SerialNumber;
                    def.SerialNumber = remoteDevice.SerialNumber;   //that the following CompareTo works

                    //Check if Configuration necessary
                    int res = 1;
                    if (def.CompareTo(remoteDevice) != 0)
                    {
                        res = XBConfigureRemoteDevice(DestinationAddress64, def);
                        //Reset notwendig um Association neu herzustellen und in den 64bit Modus zu kommen
                        if (res > 0)
                        {
                            //res = -1;
                            remoteCommand.ATCommand = CXBATCommands.XBAT_SotfwareReset;
                            _ = (CRemoteCommandResponse)XBSendRequest(remoteCommand, 500);
                            Thread.Sleep(1000);
                            //Todo: Macht der wirklich einen Reset?????
                        }
                    }
                    if (res > 0)
                    {
                        //Configure other Params
                        //if (logger.IsInfoEnabled) logger.Info("End Device configuration successful");
                        ret = true;
                    }
                    else
                    {
                        //if (logger.IsErrorEnabled) logger.Error("End Device configuration failed: " + DestinationAddress64.ToString());
                    }
                }
                else //if (endDevice != null)
                {
                    //if (logger.IsErrorEnabled) logger.Error("Cannot read Configuration of Device: " + DestinationAddress64.ToString());
                }
            }
            else
            {
                //More than 1 remote devices in Range
                _Error.SetError_MoreThan1XBee();

            }
            return ret;
        }

        /// <summary>
        /// Searches the correct baud rate of the local XBee Module
        /// </summary>
        /// <remarks>
        /// 11.11.2011: It is assumed that the presence of 1! module with correct driver name IS ALREADY CHEHECKED
        /// see funcrions in BMTCommunication.CGetComPorts
        /// </remarks>
        /// <returns></returns>
        private bool Search4LocalXBeeModule()
        {
            bool ret = false;
            int[] baudrateOptions = [250000, 9600, 115200, 19200, 38400, 57600];
            //int[] baudrateOptions = { 115200, 250000 };

            //get some available COM-Ports found based on the passed search string
            //List<CComPortInfo> availablePorts = CGetComPorts.GetComPortInfo(DriverName);

            System.IO.Ports.Handshake Handshake_prev = Seriell32.Handshake;

            //TODO: What if there are more than one modules locally connected?
            //try every found port to open it then check for correct baudrate
            //int j = -1;
            //while ((j < availablePorts.Count-1) && (ret == false))
            //{
            //    j++;
            //Seriell32.PortName = availablePorts[j].ComName;
            //Seriell32.PortName = PortName;

            //trys the baudrate
            for (int i = 0; i < baudrateOptions.Length; i++)
            {
                //if (logger.IsInfoEnabled) logger.Info("Checking: " + Seriell32.PortName + " with Baudrate " + baudrateOptions[i] + "\r\n");
                Seriell32.BaudRate = baudrateOptions[i];

                if (Seriell32.BaudRate > 57600)
                {
                    //XBee Workaround that Node Discovery works at higher baud rates
                    Seriell32.StopBits = System.IO.Ports.StopBits.Two;
                }
                else
                {
                    Seriell32.StopBits = System.IO.Ports.StopBits.One;
                }

                //Handshake Option
                //Seriell32.Handshake = System.IO.Ports.Handshake.RequestToSend;
                //4.12.2012 RequestToSend removed, da FTDI sonst Daten nicht an XBEE weiterschickt - Warum auf einmal?????
                //Seriell32.Handshake = System.IO.Ports.Handshake.None;

                try
                {
                    Seriell32.GetOpen();
                }
                catch (Exception)
                {
                    //if (logger.IsInfoEnabled) logger.Info(ee.Message + "\r\n");
                    break;
                }
                if (Seriell32.IsOpen)
                {
                    //XBeeSeries1 = new CXBeeSeries1(Seriell32);
                    //check if the baudrate is correct, by sending to "Enter Command Mode" Command to the XBee module
                    if (XBCheckCommandMode(true) > 0) //works faster than checkBaudrate()
                    {
                        //_XBRemoteDeviceInfo = XBGetConfigurationOfLocalDevice();
                        //this._DataReceiver2InitInfo.ComPortName = Seriell32.PortName;
                        //this._DataReceiver2InitInfo.BaudRate = Seriell32.BaudRate;
                        //Seriell32.Close();
                        ret = true;
                        break;
                    }
                    else
                    {
                        ret = false;
                    }
                    Seriell32.Close();
                }
            }

            Seriell32.Handshake = Handshake_prev;
            return ret;
        }

        /*
        private bool Search4LocalXBeeModule(string DriverName)
        {
            bool ret = false;
            int[] baudrateOptions = { 9600, 19200, 38400, 57600, 115200, 250000 };

            //get some available COM-Ports found based on the passed search string
            List<CComPortInfo> availablePorts = CGetComPorts.GetComPortInfo(DriverName);

            //TODO: What if there are more than one modules locally connected?
            //try every found port to open it then check for correct baudrate
            int j = -1;
            while ((j < availablePorts.Count - 1) && (ret == false))
            {
                j++;
                //Seriell32.PortName = ucComPortSelector1.Text;
                Seriell32.PortName = availablePorts[j].ComName;

                //trys the baudrate
                for (int i = 0; i < baudrateOptions.Length; i++)
                {
                    //if (logger.IsInfoEnabled) logger.Info("Checking: " + availablePorts[j].ComName + " with Baudrate " + baudrateOptions[i] + "\r\n");
                    Seriell32.BaudRate = baudrateOptions[i];

                    if (Seriell32.BaudRate > 57600)
                    {
                        //XBee Workaround that Node Discovery works at higher baud rates
                        Seriell32.StopBits = System.IO.Ports.StopBits.Two;
                    }
                    else
                    {
                        Seriell32.StopBits = System.IO.Ports.StopBits.One;
                    }

                    //Handshake Option
                    Seriell32.Handshake = System.IO.Ports.Handshake.RequestToSend;
                    try
                    {
                        Seriell32.Open();
                    }
                    catch (Exception ee)
                    {
                        //if (logger.IsInfoEnabled) logger.Info(ee.Message + "\r\n");
                        break;
                    }
                    if (Seriell32.IsOpen)
                    {
                        //XBeeSeries1 = new CXBeeSeries1(Seriell32);
                        //check if the baudrate is correct, by sending to "Enter Command Mode" Command to the XBee module
                        if (XBCheckCommandMode(true) > 0) //works faster than checkBaudrate()
                        {
                            //_XBRemoteDeviceInfo = XBGetConfigurationOfLocalDevice();
                            //this._DataReceiver2InitInfo.ComPortName = Seriell32.PortName;
                            //this._DataReceiver2InitInfo.BaudRate = Seriell32.BaudRate;
                            //Seriell32.Close();
                            ret = true;
                            break;
                        }
                        else
                        {
                            ret = false;
                        }
                        Seriell32.Close();
                    }
                }

            }
            return ret;
        }*/

    }
}
