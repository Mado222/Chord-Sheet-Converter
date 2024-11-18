using BMTCommunicationLib;
using Microsoft.Extensions.Logging;
using WindControlLib;

namespace FeedbackDataLib
{
    /// <summary>
    /// Packs all C8KanalReceiverV2 with differnt Connections together
    /// </summary>
    public class CNMasterReceiver : IDisposable
    {
        #region declarations
        /// <summary>
        /// Connection type enumerator
        /// </summary>
        //public enum EnumNeuromasterConnectionType
        //{
        //    NoConnection = 0x00,
        //    XBeeConnection = 0x01,
        //    RS232Connection = 0x02,
        //    SDCardConnection = 0x03
        //}
        private const CFTDI_D2xx.FTDI_Types Accepted_FTDI_Single_Device = CFTDI_D2xx.FTDI_Types.FT_DEVICE_232R;
        private const CFTDI_D2xx.FTDI_Types Accepted_FTDI_Dual_Device = CFTDI_D2xx.FTDI_Types.FT_DEVICE_2232;

        /// <summary>
        /// Shows if we are connected to Neuromaster
        /// </summary>
        public bool IsConnected = false;

        #endregion

        #region properties
        private CNMasterRS232? c8RS232;
        private CNMasterXBee? c8XBee;

        public EnConnectionStatus ConnectionResult = EnConnectionStatus.NoConnection;

        private readonly ILogger<CNMasterReceiver> _logger;

        /// <summary>
        /// THE connection to the Neuromaster
        /// </summary>
        public IC8Base? Connection
        {
            get
            {
                switch (ConnectionResult)
                {
                    case EnConnectionStatus.Connected_via_XBee:
                        {
                            return c8XBee;
                        }
                    case EnConnectionStatus.Connected_via_RS232:
                        {
                            return c8RS232;
                        }
                    //case EnumNeuromasterConnectionType.SDCardConnection:
                    //    {
                    //        return _8KanalReceiverV2_SDCard;
                    //    }
                    default:
                        {
                            return null;
                        }
                }
            }
        }

        /// <summary>
        /// FTDI driver class
        /// </summary>
        public CFTDI_D2xx? FTDI_D2xx;

        //public CSDCardConnection? SDCardConnection;

        /// <summary>
        /// Signal Strength [%]
        /// </summary>
        /// <value>
        /// 0..100%
        /// </value>
        public byte RSSI_percent
        {
            get => c8XBee?.RSSI_percent ?? 0;
        }
        /// <summary>
        /// Gets the neurolink serial number.
        /// </summary>
        /// <value>
        /// The neurolink serial number.
        /// </value>
        public string NeurolinkSerialNumber { get; private set; } = "";

        /// <summary>
        /// VID & PID
        /// </summary>
        /// <value>
        /// "VID_0403&PID_6001"
        /// </value>
        public CVidPid VidPid
        {
            get
            {
                if (ComPortInfo != null) return ComPortInfo.VID_PID;

                string vid = FTDI_D2xx == null ? "" : FTDI_D2xx.VID(FTDI_D2xx.IndexOfDeviceToOpen);
                string pid = FTDI_D2xx == null ? "" : FTDI_D2xx.PID(FTDI_D2xx.IndexOfDeviceToOpen);
                return new ()
                {
                    PID = pid,
                    VID = vid
                };
            }
        }

        /// <summary>
        /// Gets the name of the Com port FINALLY used (my be changed during the pairing process)
        /// </summary>
        public string PortName
        {
            get
            {
                return ConnectionResult switch
                {
                    EnConnectionStatus.Connected_via_XBee => c8XBee?.PortName ?? "",
                    EnConnectionStatus.Connected_via_RS232 => c8RS232?.SerPort?.PortName ?? "",
                    _ => ""
                };
            }
        }

        /// <summary>
        /// Connectionstatus
        /// </summary>
        public EnConnectionStatus ConnectionStatus { get; set; }

        public CComPortProcessing.CComPortInfo? ComPortInfo { get; private set; } = null;
        #endregion

        #region events

        public event EventHandler<CVidPid>? DeviceDisconnected;
        /// Occurs when Neurolink is disconnected from USB
        protected virtual void OnDeviceDisconnected(CVidPid pidVid)
        {
            DeviceDisconnected?.Invoke(this, pidVid);
        }


        public event EventHandler<EnConnectionStatus>? DeviceConnected;
        protected virtual void OnDeviceConnected(EnConnectionStatus connectionResult)
        {
            var handler = DeviceConnected;
            handler?.Invoke(this, connectionResult);
        }

        #endregion

        /// <summary>Initializes a new instance of the <see cref="CNMasterReceiver" /> class.</summary>
        public CNMasterReceiver()
        {
            _logger = AppLogger.CreateLogger<CNMasterReceiver>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CNMasterReceiver"/> class.
        /// </summary>
        ~CNMasterReceiver()
        {
            //Make sure that USB monitoring and all other stuff is down
            StopUSBMonitoring();
            Dispose();
        }

        //public EnumConnectionResult Init_via_SDCard()
        //{
        //    /*
        //    SDCardConnection ??= new CSDCardConnection();

        //    ConnectionType = EnumNeuromasterConnectionType.SDCardConnection;
        //    _8KanalReceiverV2_SDCard = new C8KanalReceiverV2_SDCard(SDCardConnection);*/
        //    return EnumConnectionResult.Connected_via_SDCard;
        //}


        /// <summary>
        /// Init function using D2xx driver
        /// Checks first cable connection: if a device is present, this connection is selected otherwiser
        /// </summary>
        /// <param name="DescriptionContains">Might search for other names than "Neurolink" too</param>
        /// <returns></returns>
        public EnConnectionStatus Init_via_D2XX(List<string>? DescriptionContains = null)
        {
            Close();
            List<int> idxNeurolinkDevices = [];
            EnConnectionStatus ret = EnConnectionStatus.No_Active_Neurolink;

            FTDI_D2xx ??= new CFTDI_D2xx();
            int numDevices = FTDI_D2xx.CheckForConnectedDevices();  //fast

            List<string> descriptionContains = [];

            if (DescriptionContains != null)
            {
                descriptionContains.AddRange(DescriptionContains);
            }
            else
            {
                descriptionContains.Add("Neurolink");
                descriptionContains.Add("Serial Converter");
            }

            if (numDevices != 0)
            {
                foreach (string _descriptionContains in descriptionContains)
                {
                    for (int i = 0; i < numDevices; i++)
                    {
                        if (((FTDI_D2xx.Type(i) == Accepted_FTDI_Dual_Device) && FTDI_D2xx.Description(i).Contains(_descriptionContains)) ||
                            ((FTDI_D2xx.Type(i) == Accepted_FTDI_Single_Device) && FTDI_D2xx.Description(i).Contains(_descriptionContains)))
                        {
                            idxNeurolinkDevices.Add(i);
                        }
                    }
                }

                if (idxNeurolinkDevices.Count > 2)
                {
                    ret = EnConnectionStatus.More_than_one_Neurolink_detected;
                }

                if (idxNeurolinkDevices.Count == 2)
                {
                    //Find out whether XBEE connection or RS232 connection to Neuromaster
                    //Differntiation between the 2 devices

                    int idxXBEeeConnection = -1;
                    int idxRS232Connection = -1;

                    for (int i = 0; i < idxNeurolinkDevices.Count; i++)
                    {
                        NeurolinkSerialNumber = FTDI_D2xx.SerialNumber(idxNeurolinkDevices[i]);
                        if (NeurolinkSerialNumber[^1] == 'A')
                        {
                            //XBee Device
                            idxXBEeeConnection = idxNeurolinkDevices[i];
                        }
                        if (NeurolinkSerialNumber[^1] == 'B')
                        {
                            //RS232Connection
                            idxRS232Connection = idxNeurolinkDevices[i];
                        }
                    }
                    NeurolinkSerialNumber = NeurolinkSerialNumber.Remove(NeurolinkSerialNumber.Length - 1);

                    //First look for RS232 because it is faster
                    //Open the related Port
                    ConnectionResult = EnConnectionStatus.Connected_via_RS232;
                    c8RS232 ??= new CNMasterRS232();
                    c8RS232.Init(FTDI_D2xx);

                    FTDI_D2xx.BaudRate = c8RS232.BaudRate_LocalDevice;     //4.2.2016
                    FTDI_D2xx.IndexOfDeviceToOpen = idxRS232Connection;

                    if (c8RS232 is not null && c8RS232.SerPort is not null)
                    {
                        if (CNMaster.Check4Neuromaster(c8RS232.SerPort))   //fast
                        {
                            //RS232 Connection is OK
                            ConnectionResult = EnConnectionStatus.Connected_via_RS232;

                            //Make sure that XBEE is in Sleep
                            CFTDI_D2xx FTDI_D2xx_temp = new()
                            {
                                IndexOfDeviceToOpen = idxXBEeeConnection
                            };
                            c8XBee ??= new CNMasterXBee();
                            c8XBee.Init(FTDI_D2xx_temp);
                            c8XBee.Send_to_Sleep();
                            c8XBee.Close();

                            FTDI_D2xx_temp.Close();
                            FTDI_D2xx_temp.Dispose();
                            StartUSBMonitoring();
                            ret = EnConnectionStatus.Connected_via_RS232;
                        }
                        else
                        {
                            //Go for XBEE
                            //It must be XBee ... leave Connection to the calling routine
                            c8RS232.Close();
                            FTDI_D2xx.IndexOfDeviceToOpen = idxXBEeeConnection;
                            ConnectionResult = EnConnectionStatus.Connected_via_XBee;
                            c8XBee = new CNMasterXBee();
                            c8XBee.Init(FTDI_D2xx);

                            try
                            {
                                if (c8XBee.InitXBee())
                                {
                                    ret = EnConnectionStatus.Connected_via_XBee;
                                    StartUSBMonitoring();
                                }
                                else
                                {
                                    ret = EnConnectionStatus.Error_during_XBee_connection;
                                }
                            }
                            catch
                            {
                                ret = EnConnectionStatus.Error_during_XBee_connection;
                            }
                        }
                    }
                }


                if (idxNeurolinkDevices.Count == 1)
                {
                    throw new InvalidOperationException("Old USB/XBee dongle not supported in this version.");
                    //We assume that it is an old USB/XBee dongle
                    //FTDI_D2xx.IndexOfDeviceToOpen = 0;
                    //ConnectionType = EnumNeuromasterConnectionType.XBeeConnection;
                    //c8XBee = new C8XBee(FTDI_D2xx);
                    //ret = EnumConnectionResult.Connected_via_XBee;
                }
            }
            else
            {
                //numDevices == 0
                ret = EnConnectionStatus.No_Active_Neurolink;
            }
            return ret;
        }

        #region USBMonitoring
        private CUSBDeviceMonitor? usbm = null;
        private CVidPid VID_PID_opened = new (); //Tp back up PID VID for OnDeviceDisconnected Event - IF FTDI is gone, PID VID (this.VID_PID) are also gone


        //private bool USBMonitorIsConnected = false;
        /// <summary>
        /// Starts USB Monitoring via Windows Messages
        /// </summary>
        public void StartUSBMonitoring()
        {
            if (usbm == null)
            {
                usbm = new (VidPid);
                usbm.USBDeviceConnectedEvent += Usbm_USBDeviceConnectedEvent;
                usbm.USBDeviceDisConnectedEvent += Usbm_USBDeviceDisConnectedEvent;
                usbm.StartMonitoring();
                _logger.LogInformation("USBMonitor started");
            }
            VID_PID_opened = VidPid;
        }


        /// <summary>
        /// Stops the usb monitoring.
        /// </summary>
        public void StopUSBMonitoring()
        {
            usbm?.StopMonitoring();
            _logger.LogInformation("USBMonitor stopped");
        }

        /* Events kommen 3x!!!!         */
        private void Usbm_USBDeviceDisConnectedEvent(object? sender, EventArgs e)
        {
            if (ConnectionStatus != EnConnectionStatus.USB_disconnected)
            {
                ConnectionStatus = EnConnectionStatus.USB_disconnected;
                OnDeviceDisconnected(VID_PID_opened);
            }
        }

        private void Usbm_USBDeviceConnectedEvent(object? sender, EventArgs e)
        {
            if (ConnectionStatus != EnConnectionStatus.USB_reconnected)
            {
                ConnectionStatus = EnConnectionStatus.USB_reconnected;
                OnDeviceConnected(EnConnectionStatus.NoConnection);
            }
        }

        #endregion

        /// <summary>
        /// Closes this instance and communication
        /// </summary>
        public void Close()
        {
            //StopUSBMonitoring();
            c8XBee?.Close();
            c8RS232?.Close();
            FTDI_D2xx?.Close();  //2nd Close
        }

        #region IDisposable Members

        public void Dispose()
        {
            DisposeDevice();
            GC.SuppressFinalize(this);
        }

        private void DisposeDevice()
        {
            c8XBee?.Dispose();

            c8RS232?.Dispose();

            FTDI_D2xx?.Dispose();
        }
        #endregion
    }

}

