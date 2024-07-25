using BMTCommunicationLib;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Xml.Serialization;
using WindControlLib;
using XBeeLib;


namespace BMTCommunication
{
    public class CXBeeConnection : ISerialPort, IDisposable
    {

        //RSSI range: 0x17-0x5C (XBee)
        private const float _RSSI_percent_max = (float)100;
        private const float _RSSI_percent_min = (float)0;
        private const float _RSSI_max = (float)-0x17;
        private const float _RSSI_min = (float)-0x5C;

        private const float _RSSI_k = (_RSSI_percent_max - _RSSI_percent_min) / (_RSSI_max - _RSSI_min);
        private const float _RSSI_d = _RSSI_percent_max - _RSSI_k * _RSSI_max;

        private const int _pairing_retries = 6; //18.10.2023, früher 3

        /// <summary>
        /// Signal strength
        /// </summary>
        /// <remarks>
        /// Received Signal Strength. Read signal level [in dB] of last good packet received
        /// (RSSI). Absolute value is reported. (For example: 0x58 = -88 dBm) Reported value is
        /// accurate between -40 dBm and RX sensitivity.
        /// 
        /// Received Signal Strength Indicator -
        /// Hexadecimal equivalent of (-dBm) value.
        /// (For example: If RX signal strength = -40
        /// dBm, “0x28” (40 decimal) is returned)
        /// </remarks>
        private byte _RSSI = (byte)-_RSSI_min;

        /// <summary>
        /// Signal Strength [%]
        /// </summary>
        /// <value>
        /// 0..100%
        /// </value>
        public byte RSSI_percent
        {
            get
            {
                float f = (float)-_RSSI;
                f = _RSSI_k * f + _RSSI_d; //[%]
                return (byte)f;
            }
        }

        private CXBeeSeries1 _XBeeSeries1;
        public CXBeeSeries1 XBeeSeries1
        {
            get { return _XBeeSeries1; }
        }

        private readonly ISerialPort _SerialPort;
        private int _BaudRateDefault_RemoteDevice;
        private int _BaudRateDefault_LocalDevice = 0;
        private readonly int NoRetriesinWrite = 10;
        private bool PairingSuceeded = false;
        byte frameID = 0;
        private readonly CFifoBuffer<CTXStatusResponse> TXStatusResponseBuffer = new();
        private readonly CFifoBuffer<byte> XBRFDataBuffer = new();

        private byte[] _ConnectSequToSend;
        private byte[] _ConnectToReturn;
        private byte _CommandChannelNo;

        private CHighPerformanceDateTime hp_Timer;

        public CXBeeConnection()
        {
            //Just to use some functions
        }

        public CXBeeConnection(ISerialPort SerialPort, int BaudRateDefault_LocalDevice, int BaudRateDefault_RemoteDevice, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            _SerialPort = SerialPort;
            this.InitSerielPort(BaudRateDefault_LocalDevice, BaudRateDefault_RemoteDevice, CommandChannelNo, ConnectSequToSend, ConnectSequToReturn);
        }

        public CXBeeConnection(int BaudRateDefault_LocalDevice, int BaudRateDefault_RemoteDevice, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            _SerialPort = new CSerialPortWrapper();
            this.InitSerielPort(BaudRateDefault_LocalDevice, BaudRateDefault_RemoteDevice, CommandChannelNo, ConnectSequToSend, ConnectSequToReturn);
        }


        /// <summary>
        /// Thread instead of using the SerialDataReceivedEvent
        /// </summary>
        private BackgroundWorker SerialDataReceived_BackgroundWorker;
        private void InitSerielPort(int BaudRateDefault_LocalDevice, int BaudRateDefault_RemoteDevice, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            //_SerialPort.SerialDataReceivedEvent += new SerialDataReceivedEventHandler(_SerialPort_SerialDataReceivedEvent);
            SerialDataReceived_BackgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            SerialDataReceived_BackgroundWorker.DoWork += new DoWorkEventHandler(SerialDataReceived_DoWork);


            _BaudRateDefault_RemoteDevice = BaudRateDefault_RemoteDevice;
            _BaudRateDefault_LocalDevice = BaudRateDefault_LocalDevice;
            _ConnectSequToSend = ConnectSequToSend;
            _ConnectToReturn = ConnectSequToReturn;
            _CommandChannelNo = CommandChannelNo;

            hp_Timer = new CHighPerformanceDateTime();
        }

        void SerialDataReceived_DoWork(object sender, DoWorkEventArgs e)
        //void _SerialPort_SerialDataReceivedEvent(object sender, SerialDataReceivedEventArgs e)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "SerialDataReceived_BackgroundWorker";

#if DEBUG
            Debug.WriteLine("SerialDataReceived_BackgroundWorker Started");
#endif


            while (!SerialDataReceived_BackgroundWorker.CancellationPending)
            {
                if (_SerialPort.IsOpen)
                {
                    if (PairingSuceeded && (_SerialPort.BytesToRead > 0))
                    {
                        ReadSeriellBufferUntilEmpty();
                        Thread.Sleep(5); //15
                    }
                    else
                    {
                        Thread.Sleep(20);//100      
                    }
                }
                else
                {
                    Thread.Sleep(20);//100
                }
            }
#if DEBUG
            Debug.WriteLine("SerialDataReceived_BackgroundWorker Closed");
#endif
        }

        public enum enumConfigureEnDeviceTo
        {
            BackRelaxer_Sleeping = 0,
            Neuromaster = 1
        }

        private enumConfigureEnDeviceTo _ConfigureEndDeviceTo;
        public enumConfigureEnDeviceTo ConfigureEndDeviceTo
        {
            get { return _ConfigureEndDeviceTo; }
            set { _ConfigureEndDeviceTo = value; }
        }


        /*
        private string _DriverName;
        public string DriverName
        {
            get { return _DriverName; }
            set { _DriverName = value; }
        }*/

        /*
        private CXBNodeInformation GetDefaultRemoteConfiguration_BackRelaxer()
        {
            CXBNodeInformation ni = new CXBNodeInformation();

            ni.APIMode = XBAPIMode.Disabled;
            ni.CE_CoordinatorEnable = 0;    //CE 
            ni.A1_EndDeviceAssociation = 0xC; //A1 ParameterXBeeSeries1.LocalDevice.SerialNumber;
            ni.A2_CoordinatorAssociation = 0;
            //def.MY_MyAddress = CXBAPICommands.Default16BitAddress;
            //Wir bleiben im 16 bit Addressing beim End-Deviceni.MY_MyAddress = 0;
            ni.NI_NodeIdentifier = "";
            ni.SP_CyclicSleepPeriode = 0xf;
            ni.ST_TimeBeforeSleep = 0xA;
            ni.SM_SleepMode = XBSleepMode.PinDoze;
            ni.SO_SleepOption = 0;
            ni.BaudRate = (uint) _BaudRateDefault_RemoteDevice;
            ni.R0_PacketizationTimeout = 10;
            return ni;
        }*/

        private CXBNodeInformation GetDefaultRemoteConfiguration_Neuromaster()
        {
            CXBNodeInformation ni = new CXBNodeInformation
            {
                APIMode = XBAPIMode.Disabled,
                CE_CoordinatorEnable = 0,    //CE 
                                             //ni.A1_EndDeviceAssociation = 0xC; //A1 0b1100
                A1_EndDeviceAssociation = 0x0, //A1 0b1100   23.7.2014
                                               //ni.A1_EndDeviceAssociation = 0x4; //A1 0b0100
                A2_CoordinatorAssociation = 0,
                //def.MY_MyAddress = CXBAPICommands.Default16BitAddress;
                //Wir bleiben im 16 bit Addressing beim End-Device
                MY_MyAddress = 0xFFFE,
                NI_NodeIdentifier = "",
                //ni.SM_SleepMode = XBSleepMode.NoSleep;
                SM_SleepMode = XBSleepMode.PinHibernate,
                ST_TimeBeforeSleep = 0xffff,
                R0_PacketizationTimeout = 100,   //Default 3
                BaudRate = (uint)_BaudRateDefault_RemoteDevice
            };
            return ni;
        }

        private CXBNodeInformation GetDefaultLocalConfiguration()
        {
            CXBNodeInformation ni = new CXBNodeInformation
            {
                APIMode = XBAPIMode.Enabled,
                CE_CoordinatorEnable = 1,  //CE
                A2_CoordinatorAssociation = 4, //A2
                NI_NodeIdentifier = "",
                SP_CyclicSleepPeriode = 0,//15;
                SM_SleepMode = XBSleepMode.PinDoze,
                BaudRate = (uint)_BaudRateDefault_LocalDevice
            };
            return ni;
        }

        private bool Start_XBee_Pairing()
        {
            PairingSuceeded = false;

            //Check if COM Port is valid
            try
            {
                //24.7.2014 repeat it a few times ... in case of USB reconnect
                int cnt = _pairing_retries;
                while ((cnt != 0) && (!IsOpen))
                {
                    Open();
                    cnt--;
                    if (!IsOpen)
                        Thread.Sleep(1000);
                }
            }
            catch (Exception)
            {
                //Comport not valid
                PairingSuceeded = false;
            }

            try
            {
                //Find out if pairing is necessary
                if (IsOpen)
                {
                    CXBNodeInformation LocalDevice = new CXBNodeInformation();
                    CXBNodeInformation RemoteDevice = new CXBNodeInformation();
                    PairingSuceeded = false;
                    int cnt = _pairing_retries;
                    if (ReadConfigData(ref LocalDevice, ref RemoteDevice))
                    {
                        //Assume Remote and Local Device have saved configutation
                        _XBeeSeries1.LocalDevice = (CXBNodeInformation)LocalDevice.Clone();
                        _XBeeSeries1.CurrentEndDevice = (CXBNodeInformation)RemoteDevice.Clone();

                        //Actual Configuration and Saved File is equal
                        //Test, if everything works properly
                        //If Everything is as expected
                        //Take End device config from file
                        while ((cnt != 0) && !PairingSuceeded)
                        {
                            //24.6.2014 try it more than once, because after switch off sometimes the first communication fails
                            PairingSuceeded = Check4Neuromaster_XBEE();
                            cnt--;
                        }
                    }

                    if (!PairingSuceeded)
                    {
                        //Not a valid connection - reason can be manyfold => start the whole pairing process
                        _XBeeSeries1.CurrentEndDevice = new CXBNodeInformation();

                        switch (ConfigureEndDeviceTo)
                        {
                            case enumConfigureEnDeviceTo.Neuromaster:
                                {
                                    if (_XBeeSeries1.ManageXBeePairing(GetDefaultLocalConfiguration(), GetDefaultRemoteConfiguration_Neuromaster()))    //Closes Com
                                    {
                                        SaveConfigData(_XBeeSeries1.LocalDevice, _XBeeSeries1.CurrentEndDevice);
                                        PairingSuceeded = true;
                                    }
                                    break;
                                }
                        }//switch (ConfigureEndDeviceTo)
                    }//if (!PairingSuceeded)
                    else
                    {
                        LastErrorString = "Start_XBee_Pairing: Pairing did not succeed";
                    }
                }//if (IsOpen)
                else
                {
                    LastErrorString = "Start_XBee_Pairing: IsOpen==false";
                }
            }
            catch (Exception ee)
            {
                //log.Error("XBPairing: " + ee.Message);
                PairingSuceeded = false;
            }
            finally
            {
                //Clean up stuff needed for pairing
            }
            return PairingSuceeded;
        }

        /*
        private bool Enter_EnergySavingMode(CXBNodeInformation LocalDevice, UInt64 RemoteAddress)
        {
            bool PairingSucceeded = false;
            //Set it back to 0 but dont save to eeprom
            int res = _XBeeSeries1.XBSendAPIString_CheckOKResponse(CXBATCommands.XBAT_SetCyclicSleepPeriod(0), true);
            if (res > 0)
            {
                if (Check4Neuromaster_XBEE())
                {
                    //Now set appropriate Sleeping Parameters on Local device
                    //for energy saving mode (Without writing to EEPROM!)
                    if (_XBeeSeries1.XBSendAPIString_CheckOKResponse(CXBATCommands.XBAT_SetCyclicSleepPeriod(15), false) > 0)
                    {
                        if (_XBeeSeries1.XBSendAPIString_CheckOKResponse(CXBATCommands.XBAT_SetTimeBeforeSleep(0xa), false) > 0)
                        {
                            //Write to Nonvolatile memory
                            if (_XBeeSeries1.XBSendAPIString_CheckOKResponse(CXBATCommands.XBAT_Write_to_nonvolatilememory, true) > 0)
                            {
                                _XBeeSeries1.LocalDevice.SP_CyclicSleepPeriode = 15;
                                _XBeeSeries1.LocalDevice.ST_TimeBeforeSleep = 0xa;
                                PairingSucceeded = true;
                            }
                        }
                    }
                }
            }
            return PairingSucceeded;
        }*/


        /// <summary>
        /// //Local is assumed to be configured and paired, test it
        /// </summary>
        private bool Check4Neuromaster_XBEE()
        {
            //Local device seems to be configured and paired, test it
            //After switching it on the device is not in Energy saving mode
            return CNeuromaster.Check4Neuromaster(this, _ConnectSequToSend, _ConnectToReturn, _CommandChannelNo);
        }

        //private static readonly ILog log = LogManager.GetLogger(typeof(CXBeeConnection));


        public bool InitXBee()
        {
            bool ret = true;
            try
            {
                if (_XBeeSeries1 == null)
                {
                    Close();
                    _XBeeSeries1 = new CXBeeSeries1(_SerialPort)
                    {
                        DisplayMessages = false
                    };
                    //Does the whole pairing process
                    ret = Start_XBee_Pairing();
                }
                else
                {
                    LastErrorString = "_XBeeSeries1 != null";
                }
            }
            catch (Exception ee)
            {
                LastErrorString = ee.Message;
                ret = false;
            }
            finally
            {
                Close();
            }
            return ret;
        }


        public void GetConfigPath(ref string LocalDevPath, ref string RemoteDevPath)
        {
            //string fullPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Insight";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            LocalDevPath = fullPath + @"\LocalDevice.xml";
            RemoteDevPath = fullPath + @"\RemoteDevice.xml";
        }

        private bool SaveConfigData(CXBNodeInformation LocalDevice, CXBNodeInformation RemoteDevice)
        {
            /* Create a StreamWriter to write with. First create a FileStream
               object, and create the StreamWriter specifying an Encoding to use. */
            try
            {
                string LocalDevPath = "";
                string RemoteDevPath = "";
                GetConfigPath(ref LocalDevPath, ref RemoteDevPath);

                FileStream fs = new FileStream(LocalDevPath, FileMode.Create);
                TextWriter writer = new StreamWriter(fs);
                XmlSerializer ser = new XmlSerializer(typeof(CXBNodeInformation));
                ser.Serialize(writer, LocalDevice);
                writer.Close();
                fs = new FileStream(RemoteDevPath, FileMode.Create);
                writer = new StreamWriter(fs);
                ser.Serialize(writer, RemoteDevice);
                writer.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool ReadConfigData(ref CXBNodeInformation LocalDevice, ref CXBNodeInformation RemoteDevice)
        {
            bool ret = false;//19.09.2023
            try
            {
                string LocalDevPath = "";
                string RemoteDevPath = "";
                GetConfigPath(ref LocalDevPath, ref RemoteDevPath);

                if (File.Exists(LocalDevPath))
                {
                    FileStream fs = new FileStream(LocalDevPath, FileMode.Open);
                    TextReader reader = new StreamReader(fs);
                    XmlSerializer ser = new XmlSerializer(typeof(CXBNodeInformation));
                    LocalDevice = (CXBNodeInformation)ser.Deserialize(reader);
                    reader.Close();

                    if (File.Exists(RemoteDevPath))
                    {
                        fs = new FileStream(RemoteDevPath, FileMode.Open);
                        reader = new StreamReader(fs);
                        ser = new XmlSerializer(typeof(CXBNodeInformation));
                        RemoteDevice = (CXBNodeInformation)ser.Deserialize(reader);
                        reader.Close();
                        ret = true; //19.09.2023
                    }
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Reads from _SerialPort until empty, decodes Packets and puts results to XBRFDataBuffer
        /// </summary>
        /// <returns></returns>
        public int ReadSeriellBufferUntilEmpty()
        {
            if (_SerialPort.IsOpen)
            {
                while (_SerialPort.BytesToRead > 0)
                {
                    CRXPacketBasic barp = null;
                    object o = _XBeeSeries1.XBGetResponse();
                    if (o != null)
                    {
                        if (o.GetType() == typeof(CRXPacket16))
                        {
                            barp = (CRXPacket16)o;
                            _RSSI = barp.RSSI;
                            //Debug.WriteLine("ReadSeriellBufferUntilEmpty: CRXPacket16: " + barp.rfData.Count + " RFData Received");
                        }
                        else if (o.GetType() == typeof(CRXPacket64))
                        {
                            barp = (CRXPacket64)o;
                            _RSSI = barp.RSSI;
                            //Debug.WriteLine("ReadSeriellBufferUntilEmpty: CRXPacket64: " + barp.rfData.Count + " RFData Received");
                        }
                        else
                        {
                            try
                            {
                                CTXStatusResponse s = (CTXStatusResponse)o;
                                TXStatusResponseBuffer.Push(s);
                            }
                            catch
                            { };
                            //Debug.WriteLine("ReadSeriellBufferUntilEmpty: FrameID: " + s.frameId.ToString() + " Status: " + s.TXStatus.ToString());
                        }

                        if (barp != null)
                        {
                            XBRFDataBuffer.Push(barp.rfData);
                        }
                    } //if (o != null)
                    //while ((_SerialPort.BytesToRead > 0) && (barp != null));
                }   //_SerialPort.BytesToRead > 0
            }//IsOPen
            return 0;
        }



        #region ISerialPort Members

        public bool Open()
        {
            bool ret = false;

            if ((_SerialPort != null) && (!_SerialPort.IsOpen))
            {
                _SerialPort.Open();
                _SerialPort.DtrEnable = true;   //Awake from Sleep 11.12.2012
                XBRFDataBuffer.Clear();
                if (!SerialDataReceived_BackgroundWorker.IsBusy && _SerialPort.IsOpen && PairingSuceeded)
                    SerialDataReceived_BackgroundWorker.RunWorkerAsync();
                ret = _SerialPort.IsOpen;
            }
            else
            {
                if (_SerialPort == null)
                {
                    LastErrorString = "_SerialPort == null";
                }
                else
                {
                    LastErrorString = "_SerialPort not open";
                }
            }
            return ret;
        }

        public void Close()
        {
            if (_SerialPort != null)
            {
                try
                {
                    _SerialPort.DtrEnable = false;   //Sleep 11.12.2012
                    _SerialPort.Close();
                    SerialDataReceived_BackgroundWorker.CancelAsync();
                }

                catch { };
            }
        }

        /// <summary>
        /// Writes data in buffer to XBee connection, checks response
        /// </summary>
        /// <param name="buffer">Data to send</param>
        /// <param name="offset">Pointer start position in buffer </param>
        /// <param name="count">Number of bytes to send</param>
        /// <returns></returns>
        /// <remarks>if Write failes its is repeated - NoRetriesinWrite times </remarks>
        public bool Write(byte[] buffer, int offset, int count)
        {
            int c = 0;
            while (c < NoRetriesinWrite)   //try to resend Message
            {
                if (Write(buffer, offset, count, _XBeeSeries1.CurrentEndDevice.SerialNumber, true))
                {
                    return true;
                }
                Thread.Sleep(50);   //Sonst Probleme mit den Verzögerungen
                c++;
            }
            return false;
        }

        /// <summary>
        /// Writes data in buffer to XBee connection, checks response
        /// </summary>
        /// <param name="buffer">Data to send</param>
        /// <param name="offset">Pointer start position in buffer </param>
        /// <param name="count">Number of bytes to send</param>
        /// <param name="EndDevSerialNumber">End device serial number</param>
        /// <param name="CheckXBeeresponse">if set to <c>true</c> succeeded</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Write(byte[] buffer, int offset, int count, UInt64 EndDevSerialNumber, bool CheckXBeeresponse)
        {
            bool ret = true;
            //LocalNodeInformation is empty
            CTXRequest64 TXRequest64 = new CTXRequest64(EndDevSerialNumber);
            if (CheckXBeeresponse)
            {
                TXRequest64.options = TXRequestOptions.noOption;    //Activate Response Packet
                frameID++; if (frameID == 0) frameID = 1;
                TXRequest64.frameId = frameID;
            }
            else
            {
                TXRequest64.options = TXRequestOptions.disableACK;    //DeActivate Response Packet
                TXRequest64.frameId = 0; //No response

            }
            //debug
            //DiscardInBuffer();
            byte[] buf = new byte[count];
            if ((offset != 0) || (count != buffer.Length))
            {
                Buffer.BlockCopy(buffer, offset, buf, 0, count);
            }
            else
            {
                buf = buffer;
            }

            buf = TXRequest64.Get_TX_TransmmitRequest_DataFrame(_XBeeSeries1.LocalDevice.APIMode, buf);
            _SerialPort.Write(buf, 0, buf.Length);
            //Debug.WriteLine("frameID sent: " + frameID.ToString());

            //Waitfor TX Transmit status with correct frame ID
            if (CheckXBeeresponse)
            {
                DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, 300); //300ms muss länger als der längste Befehl sein (getconfig)
                while (DateTime.Now < dt)
                //while (true)
                {
                    if (!PairingSuceeded)   //Moved here 23.7.2014
                        ReadSeriellBufferUntilEmpty();

                    while (TXStatusResponseBuffer.Count > 0)
                    {
                        CTXStatusResponse txsr = (CTXStatusResponse)TXStatusResponseBuffer.Pop();
                        if (txsr.frameId == frameID)
                        {
                            if (txsr.TXStatus != TXStatusOptions.Success)
                            {
                                ret = false;
                            }
                            goto Finish;
                        }
                    }
                    //if (!PairingSuceeded)
                    //ReadSeriellBufferUntilEmpty();
                }
                ret = false;
            }
        Finish:
            return ret;

        }

        /// <summary>
        /// Reads from _SerialPort until empty, decodes Packets and puts results to XBRFDataBuffer, returns (in buffer) the requested number of bytes (count)
        /// </summary>
        /// <param name="buffer">byte buffer to hold result</param>
        /// <param name="offset">Bytes are written to buffer from this index</param>
        /// <param name="count">Number of bytes to write into buffer</param>
        /// <returns></returns>
        /// <remarks>see public int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)</remarks>
        public int Read(ref byte[] buffer, int offset, int count)
        {
            if (_SerialPort.ReadTimeout == -1)
            {
                return Read(ref buffer, offset, count, 50);
            }
            return Read(ref buffer, offset, count, _SerialPort.ReadTimeout);
        }

        /// <summary>
        /// Reads from _SerialPort until empty, decodes Packets and puts results to XBRFDataBuffer, returns (in buffer) the requested number of bytes (count)
        /// </summary>
        /// <param name="buffer">byte buffer to hold result</param>
        /// <param name="offset">Bytes are written to buffer from this index</param>
        /// <param name="count">Number of bytes to write into buffer</param>
        /// <param name="WaitMax_ms">Timeout [ms]</param>
        /// <returns></returns>
        public int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, WaitMax_ms);

            while (DateTime.Now < dt)
            {
                //Get the requested bytes from XBRFDataBuffer
                if (XBRFDataBuffer.Count >= count)
                {
                    for (int i = offset; i < offset + count; i++)
                    {
                        buffer[i] = XBRFDataBuffer.Pop();
                    }
                    return count;
                }
                else
                {
                    //Not enough data in XBRFDataBuffer -> get some
                    //ReadSeriellBufferUntilEmpty();
                    if (!PairingSuceeded)
                    {
                        if (ReadSeriellBufferUntilEmpty() <= 0)
                            Thread.Sleep(5);
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }
                }
            }
            return 0;
        }


        public void DiscardInBuffer()
        {
            XBRFDataBuffer.Clear();
            if (_SerialPort.IsOpen)
                _SerialPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            if (_SerialPort.IsOpen)
                _SerialPort.DiscardOutBuffer();
        }


        public int BytesToRead
        {
            get
            {
                return XBRFDataBuffer.Count;
            }
        }

        public string PortName
        {
            get { return _SerialPort.PortName; }
            set
            {
                if (value != "")
                {
                    _SerialPort.PortName = value;
                }
            }
        }

        public int BaudRate
        {
            get { return _SerialPort.BaudRate; }
            set { _SerialPort.BaudRate = value; }
        }

        public bool IsOpen
        {
            get { return _SerialPort.IsOpen; }
        }

        public int ReadTimeout
        {
            get { return _SerialPort.ReadTimeout; }
            set { _SerialPort.ReadTimeout = value; }
        }

        public int WriteTimeout
        {
            get { return _SerialPort.WriteTimeout; }
            set { _SerialPort.WriteTimeout = value; }
        }

        public StopBits StopBits
        {
            get { return _SerialPort.StopBits; }
            set { _SerialPort.StopBits = value; }
        }

        public Parity Parity
        {
            get { return _SerialPort.Parity; }
            set { _SerialPort.Parity = value; }
        }

        public int DataBits
        {
            get { return _SerialPort.DataBits; }
            set { _SerialPort.DataBits = value; }
        }

        public Handshake Handshake
        {
            get { return _SerialPort.Handshake; }
            set
            {
                if ((value == Handshake.RequestToSend) && (_SerialPort.WriteTimeout == -1))
                {
                    _SerialPort.WriteTimeout = 100; //[ms]
                }
                _SerialPort.Handshake = value;
            }
        }

        public bool RtsEnable
        {
            get { return _SerialPort.RtsEnable; }
            set { _SerialPort.RtsEnable = value; }
        }

        public bool DtrEnable
        {
            get { return _SerialPort.DtrEnable; }
            set { _SerialPort.DtrEnable = value; }
        }

        public bool DsrHolding
        {
            get { return _SerialPort.DsrHolding; }
        }

        public event SerialDataReceivedEventHandler SerialDataReceivedEvent;


        private string _LastErrorString = "";
        public string LastErrorString
        {
            get { return _LastErrorString; }
            set { _LastErrorString = value; }
        }

        public DateTime Now(enumTimQueryStatus TimQueryStatus)
        {
            return hp_Timer.Now;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                //closeConnection();
                Close();
                _SerialPort.Dispose();
                if (_XBeeSeries1 != null)
                {
                    _XBeeSeries1.Dispose();
                }
            }
            catch (Exception)
            {
                //OnLogError(e.Message);
                //log.Error(e.Message, e);
            }
        }

        #endregion

    }
}
