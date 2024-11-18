using BMTCommunicationLib;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Xml.Serialization;
using WindControlLib;
using XBeeLib;

namespace FeedbackDataLib
{
    //***************************************************************
    //
    //              C8KanalReceiverV2_XBee
    //
    //***************************************************************/
    /// <summary>
    /// Basic Component for Insight Instruments "Neuromaster" with XBee Connection
    /// </summary>
    /// <remarks></remarks>
    public class CNMasterXBee : IC8Base, ISerialPort, IDisposable
    {

        /// <summary>
        /// XBee Connection
        /// </summary>
        private readonly int baudRateLocalDevice = 250000;
        private readonly int baudRateRemoteDevice = 115200;
        private readonly int NoRetriesinWrite = 10;

        private CXBeeSeries1? _XBeeSeries1;
        public CXBeeSeries1? XBeeSeries1 => _XBeeSeries1;

        private bool PairingSuceeded = false;

        private readonly CFifoConcurrentQueue<CTXStatusResponse> TXStatusResponseBuffer = new();
        private readonly CFifoConcurrentQueue<byte> XBRFDataBuffer = new();

        private ISerialPort? _SerPort;
        public ISerialPort? SerPort { get => this; }

        private readonly EnConnectionStatus _ConnectionStatus = EnConnectionStatus.NoConnection;
        public EnConnectionStatus ConnectionStatus { get => _ConnectionStatus; }

        byte frameID;

        private readonly CHighPerformanceDateTime hp_Timer = new();

        private readonly ILogger<CNMasterXBee> _logger;

        public CNMasterXBee()
        {
            _logger = AppLogger.CreateLogger<CNMasterXBee>();
        }

        // Optional: Destructor to release unmanaged resources if Dispose is not called
        ~CNMasterXBee()
        {
            Close();
        }

        public void Close()
        {
            if (_SerPort != null)
            {
                try
                {
                    _SerPort.DtrEnable = false;   //Sleep 11.12.2012
                    _SerPort.Close();
                    StopSerialXBeeReceiver();
                }

                catch { };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CNMasterXBee" /> class.
        /// </summary>
        /// <param name="SerialPort">Serial Port</param>
        public void Init(ISerialPort SerialPort)
        {
            _SerPort = SerialPort;
            _SerPort.BaudRate = baudRateLocalDevice;
            _SerPort.StopBits = StopBits.One;
            if (baudRateLocalDevice > 57600)
                _SerPort.StopBits = StopBits.Two;
            _SerPort.Handshake = Handshake.RequestToSend;
            StartSerialXBeeReceiver();
        }

        public void Send_to_Sleep()
        {
            if (_SerPort != null)
            {
                _SerPort.GetOpen();
                _SerPort.DtrEnable = false;
                _SerPort.Close();
            }
        }

        #region CCommXBee
        //RSSI range: 0x17-0x5C (XBee)
        private const float _RSSI_percent_max = 100;
        private const float _RSSI_percent_min = 0;
        private const float _RSSI_max = -0x17;
        private const float _RSSI_min = -0x5C;

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
                float f = -_RSSI;
                f = _RSSI_k * f + _RSSI_d; //[%]
                return (byte)f;
            }
        }

        private Thread? _SerialXBeeReceiverThread;
        private CancellationTokenSource? _cancellationTokenSource;

        public void StartSerialXBeeReceiver()
        {
            if (_SerialXBeeReceiverThread != null && _SerialXBeeReceiverThread.IsAlive)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _SerialXBeeReceiverThread = new Thread(() => SerialXBeeReceiverAsync(_cancellationTokenSource.Token))
            {
                IsBackground = true,
                Name = "SerialXBeeReceiverThread"
            };
            _SerialXBeeReceiverThread.Start();
        }

        public void StopSerialXBeeReceiver()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _SerialXBeeReceiverThread?.Join(); // Wait for the thread to finish
                _SerialXBeeReceiverThread = null;
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void SerialXBeeReceiverAsync(CancellationToken token)
        {
            
            _logger.LogInformation("SerialXBeeReceiverThread Started");
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_SerPort is not null && _SerPort.IsOpen)
                    {
                        if (PairingSuceeded && (_SerPort.BytesToRead > 0))
                        {
                            ReadSeriellBufferUntilEmpty();
                            Thread.Sleep(2); // Adjust sleep time as needed
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SerialXBeeReceiverThread: {Message}", ex.Message);
            }
            finally
            {
               _logger.LogInformation("SerialXBeeReceiverThread Closed");
            }
        }

        public enum EnumConfigureEnDeviceTo
        {
            BackRelaxer_Sleeping = 0,
            Neuromaster = 1
        }

        public EnumConfigureEnDeviceTo ConfigureEndDeviceTo { get; set; }

        private CXBNodeInformation GetDefaultRemoteConfiguration_Neuromaster()
        {
            CXBNodeInformation ni = new()
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
                BaudRate = (uint)baudRateRemoteDevice
            };
            return ni;
        }

        private CXBNodeInformation GetDefaultLocalConfiguration()
        {
            CXBNodeInformation ni = new()
            {
                APIMode = XBAPIMode.Enabled,
                CE_CoordinatorEnable = 1,  //CE
                A2_CoordinatorAssociation = 4, //A2
                NI_NodeIdentifier = "",
                SP_CyclicSleepPeriode = 0,//15;
                SM_SleepMode = XBSleepMode.PinDoze,
                BaudRate = (uint)baudRateLocalDevice
            };
            return ni;
        }

        private bool Start_XBee_Pairing()
        {
            if (_SerPort is null) { return false; }
            
            PairingSuceeded = false;
            //Check if COM Port is valid
            try
            {
                //24.7.2014 repeat it a few times ... in case of USB reconnect
                int cnt = _pairing_retries;
                while ((cnt != 0) && !_SerPort!.IsOpen)
                {
                    _SerPort?.GetOpen();
                    cnt--;
                    if (!_SerPort!.IsOpen)  Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //Comport not valid
                _logger.LogError("Start_XBee_Pairing: {Message}", ex.Message);
                PairingSuceeded = false;
            }

            try
            {
                //Find out if pairing is necessary
                if (_XBeeSeries1 is not null && _XBeeSeries1.Seriell32.IsOpen)
                {
                    CXBNodeInformation? LocalDevice = new();
                    CXBNodeInformation? RemoteDevice = new();
                    PairingSuceeded = false;
                    int cnt = _pairing_retries;
                    if (ReadConfigData(ref LocalDevice, ref RemoteDevice))
                    {
                        if (LocalDevice is not null && RemoteDevice is not null && _XBeeSeries1 is not null)
                        {
                            //Assume Remote and Local Device have saved configutation
                            _XBeeSeries1.LocalDevice = (CXBNodeInformation?)LocalDevice.Clone();
                            _XBeeSeries1.CurrentEndDevice = (CXBNodeInformation?)RemoteDevice.Clone();
                        }

                        //Actual Configuration and Saved File is equal
                        //Test, if everything works properly
                        //If Everything is as expected
                        //Take End device config from file
                        while ((cnt != 0) && !PairingSuceeded)
                        {
                            //24.6.2014 try it more than once, because after switch off sometimes the first communication fails
                            PairingSuceeded = CNMaster.Check4Neuromaster(this);
                            cnt--;
                        }
                    }

                    if (!PairingSuceeded && _XBeeSeries1 is not null && _XBeeSeries1.LocalDevice is not null)
                    {
                        //Not a valid connection - reason can be manyfold => start the whole pairing process
                        _XBeeSeries1.CurrentEndDevice = new CXBNodeInformation();

                        switch (ConfigureEndDeviceTo)
                        {
                            case EnumConfigureEnDeviceTo.Neuromaster:
                                {
                                    if (_XBeeSeries1.ManageXBeePairing(GetDefaultLocalConfiguration(), GetDefaultRemoteConfiguration_Neuromaster()))    //Closes Com
                                    {
                                        SaveConfigData(_XBeeSeries1.LocalDevice, _XBeeSeries1.CurrentEndDevice);
                                        PairingSuceeded = true;
                                    }
                                    else
                                    {
                                        _logger.LogWarning( "ManageXBeePairing: Failed");
                                    }
                                    break;
                                }
                        }//switch (ConfigureEndDeviceTo)
                    }//if (!PairingSuceeded)
                    else
                    {
                        _logger.LogWarning("Start_XBee_Pairing: Pairing did not succeed");
                    }
                }//if (IsOpen)
                else
                {
                    _logger.LogInformation("Start_XBee_Pairing: IsOpen==false");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Start_XBee_Pairing: {Message}", ex.Message);
                PairingSuceeded = false;
            }
            finally
            {
                //Clean up stuff needed for pairing
            }
            return PairingSuceeded;
        }

        public bool InitXBee()
        {
            if (_SerPort is not null)
            {
                bool ret = true;
                try
                {
                    if (_XBeeSeries1 == null)
                    {
                        //Close();
                        _XBeeSeries1 = new CXBeeSeries1(_SerPort);
                        //Does the whole pairing process
                        ret = Start_XBee_Pairing();
                    }
                    else
                    {
                        _logger.LogWarning("InitXBee: _XBeeSeries1 must be null");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("InitXBee: {Message}", ex.Message);
                    ret = false;
                }
                finally
                {
                    Close();
                }
                return ret;
            }
            return false;
        }


        public static void GetConfigPath(ref string LocalDevPath, ref string RemoteDevPath)
        {
            //string fullPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Insight";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            LocalDevPath = fullPath + @"\LocalDevice.xml";
            RemoteDevPath = fullPath + @"\RemoteDevice.xml";
        }

        private static bool SaveConfigData(CXBNodeInformation LocalDevice, CXBNodeInformation RemoteDevice)
        {
            /* Create a StreamWriter to write with. First create a FileStream
               object, and create the StreamWriter specifying an Encoding to use. */
            try
            {
                string LocalDevPath = "";
                string RemoteDevPath = "";
                GetConfigPath(ref LocalDevPath, ref RemoteDevPath);

                FileStream fs = new(LocalDevPath, FileMode.Create);
                TextWriter writer = new StreamWriter(fs);
                XmlSerializer ser = new(typeof(CXBNodeInformation));
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

        private static bool ReadConfigData(ref CXBNodeInformation? LocalDevice, ref CXBNodeInformation? RemoteDevice)
        {
            bool ret = false;//19.09.2023
            try
            {
                string LocalDevPath = "";
                string RemoteDevPath = "";
                GetConfigPath(ref LocalDevPath, ref RemoteDevPath);

                if (File.Exists(LocalDevPath))
                {
                    FileStream fs = new(LocalDevPath, FileMode.Open);
                    TextReader reader = new StreamReader(fs);
                    XmlSerializer ser = new(typeof(CXBNodeInformation));
                    LocalDevice = (CXBNodeInformation?)ser.Deserialize(reader);
                    reader.Close();

                    if (File.Exists(RemoteDevPath))
                    {
                        fs = new FileStream(RemoteDevPath, FileMode.Open);
                        reader = new StreamReader(fs);
                        ser = new XmlSerializer(typeof(CXBNodeInformation));
                        RemoteDevice = (CXBNodeInformation?)ser.Deserialize(reader);
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
            if (_XBeeSeries1 is not null && _SerPort is not null)
            {
                while (_SerPort.BytesToRead > 0)
                {
                    CRXPacketBasic? barp = null;
                    object? o = _XBeeSeries1.XBGetResponse();
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
                            catch (Exception ex)
                            {
                                _logger.LogError("ReadSeriellBufferUntilEmpty: {Message}", ex.Message);
                            }
                        }

                        if (barp != null)
                        {
                            XBRFDataBuffer.Push(barp.RfData);
                        }
                    } //if (o != null)
                    //while ((_SerialPort.BytesToRead > 0) && (barp != null));
                }   //_SerialPort.BytesToRead > 0
            }//IsOPen
            return 0;
        }
        #endregion

        #region ISerialPort Members

        public bool GetOpen()
        {
            bool ret = false;

            if ((_SerPort != null) && (!_SerPort.IsOpen))
            {
                _SerPort.GetOpen();
                _SerPort.DtrEnable = true;   //Awake from Sleep 11.12.2012
                XBRFDataBuffer.Clear();
                if (_SerPort.IsOpen && PairingSuceeded)
                    StartSerialXBeeReceiver();
                ret = _SerPort.IsOpen;
            }
            else
            {
                if (_SerPort == null)
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

        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Write(buffer, offset, count);
            }, cancellationToken);
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
            if (_XBeeSeries1 is not null && _XBeeSeries1.CurrentEndDevice is not null)
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
        public bool Write(byte[] buffer, int offset, int count, ulong EndDevSerialNumber, bool CheckXBeeresponse)
        {
            bool ret = true;
            //LocalNodeInformation is empty
            CTXRequest64 TXRequest64 = new(EndDevSerialNumber);
            if (CheckXBeeresponse)
            {
                TXRequest64.Options= TXRequestOptions.noOption;    //Activate Response Packet
                frameID++; if (frameID == 0) frameID = 1;
                TXRequest64.FrameId = frameID;
            }
            else
            {
                TXRequest64.Options = TXRequestOptions.disableACK;    //DeActivate Response Packet
                TXRequest64.FrameId = 0; //No response

            }
            byte[] buf = new byte[count];
            if ((offset != 0) || (count != buffer.Length))
            {
                Buffer.BlockCopy(buffer, offset, buf, 0, count);
            }
            else
            {
                buf = buffer;
            }

            if (_XBeeSeries1 is not null && _XBeeSeries1.LocalDevice is not null && _SerPort is not null)
            {
                buf = TXRequest64.Get_TX_TransmmitRequest_DataFrame(_XBeeSeries1.LocalDevice.APIMode, buf);
                _SerPort.Write(buf, 0, buf.Length);

                //Waitfor TX Transmit status with correct frame ID
                if (CheckXBeeresponse)
                {
                    DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, 300); //300ms muss länger als der längste Befehl sein (getconfig)
                    while (DateTime.Now < dt)
                    {
                        if (!PairingSuceeded)   //Moved here 23.7.2014
                            ReadSeriellBufferUntilEmpty();

                        while (TXStatusResponseBuffer.Count > 0)
                        {
                            CTXStatusResponse? txsr = TXStatusResponseBuffer.Pop();
                            if (txsr is not null && txsr.FrameId == frameID)
                            {
                                if (txsr.TXStatus != EnTXStatusOptions.Success)
                                {
                                    ret = false;
                                }
                                goto Finish;
                            }
                        }
                    }
                    ret = false;
                }
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
            if (_SerPort is null) return 0;

            if (_SerPort.ReadTimeout == -1)
            {
                return Read(ref buffer, offset, count, 50);
            }
            return Read(ref buffer, offset, count, _SerPort.ReadTimeout);
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
            if (_SerPort is not null && _SerPort.IsOpen)
                _SerPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            if (_SerPort is not null && _SerPort.IsOpen)
                _SerPort.DiscardOutBuffer();
        }


        public int BytesToRead => XBRFDataBuffer.Count;

        public string PortName
        {
            get { return _SerPort?.PortName ?? string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(value) && _SerPort != null)
                {
                    _SerPort.PortName = value;
                }
            }
        }

        public int BaudRate
        {
            get { return _SerPort?.BaudRate ?? 0; } // Return a default value (e.g., 0) if _SerialPort is null
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.BaudRate = value;
                }
            }
        }


        public bool IsOpen
        {
            get { return _SerPort?.IsOpen ?? false; }
        }

        public int ReadTimeout
        {
            get { return _SerPort?.ReadTimeout ?? 0; } // Return a default value (e.g., 0) if _SerialPort is null
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.ReadTimeout = value;
                }
            }
        }

        public int WriteTimeout
        {
            get { return _SerPort?.WriteTimeout ?? 0; }
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.WriteTimeout = value;
                }
            }
        }

        public StopBits StopBits
        {
            get { return _SerPort?.StopBits ?? StopBits.None; } // Use a default value if _SerialPort is null
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.StopBits = value;
                }
            }
        }

        public Parity Parity
        {
            get { return _SerPort?.Parity ?? Parity.None; } // Use a default value if _SerialPort is null
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.Parity = value;
                }
            }
        }

        public int DataBits
        {
            get => _SerPort?.DataBits ?? 8; // Default to 8 if _SerialPort is null
            set
            {
                if (value < 5 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "DataBits must be between 5 and 8.");
                }

                if (_SerPort != null)
                {
                    _SerPort.DataBits = value;
                }
                else
                {
                    _logger.LogWarning("Warning: Attempted to set DataBits, but _SerialPort is null.");
                }
            }
        }


        public Handshake Handshake
        {
            get { return _SerPort?.Handshake ?? Handshake.None; }
            set
            {
                if (_SerPort != null)
                {
                    if ((value == Handshake.RequestToSend) && (_SerPort.WriteTimeout == -1))
                    {
                        _SerPort.WriteTimeout = 100; //[ms]
                    }
                    _SerPort.Handshake = value;
                }
            }
        }

        public bool RtsEnable
        {
            get { return _SerPort?.RtsEnable ?? false; }
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.RtsEnable = value;
                }
            }
        }

        public bool DtrEnable
        {
            get { return _SerPort?.DtrEnable ?? false; }
            set
            {
                if (_SerPort != null)
                {
                    _SerPort.DtrEnable = value;
                }
            }
        }

        public bool DsrHolding
        {
            get { return _SerPort?.DsrHolding ?? false; }
        }

        public event SerialDataReceivedEventHandler? SerialDataReceivedEvent;


        private string _LastErrorString = "";
        public string LastErrorString
        {
            get { return _LastErrorString; }
            set { _LastErrorString = value; }
        }

        public DateTime Now(EnumTimQueryStatus TimQueryStatus)
        {
            return hp_Timer.Now;
        }

        #endregion



        #region IDisposable Members

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this); // Suppress the finalizer.
        }

        #endregion
    }

}

