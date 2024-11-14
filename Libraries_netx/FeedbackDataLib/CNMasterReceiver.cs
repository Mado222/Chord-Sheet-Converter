using BMTCommunicationLib;
using System.IO.Ports;
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
        public string LastErrorString { get; private set; } = "";



        private const CFTDI_D2xx.FTDI_Types Accepted_FTDI_Single_Device = CFTDI_D2xx.FTDI_Types.FT_DEVICE_232R;
        private const CFTDI_D2xx.FTDI_Types Accepted_FTDI_Dual_Device = CFTDI_D2xx.FTDI_Types.FT_DEVICE_2232;

        /// <summary>
        /// Shows if we are connected to Neuromaster
        /// </summary>
        public bool IsConnected = false;

        #endregion

        #region properties
        private CNMasterRS232? c8RS232;
        private CXBee? c8XBee;

        public EnConnectionResult ConnectionResult = EnConnectionResult.NoConnection;

        /// <summary>
        /// THE connection to the Neuromaster
        /// </summary>
        public IC8Base? Connection
        {
            get
            {
                switch (ConnectionResult)
                {
                    case EnConnectionResult.Connected_via_XBee:
                        {
                            return c8XBee;
                        }
                    case EnConnectionResult.Connected_via_RS232:
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
            get => c8XBee?.XBeeConnection.RSSI_percent ?? 0;
        }
        /// <summary>
        /// Gets the neurolink serial number.
        /// </summary>
        /// <value>
        /// The neurolink serial number.
        /// </value>
        public string NeurolinkSerialNumber { get; private set; } = "";

        /// <summary>
        /// FTDI Product ID (PID)
        /// </summary>
        public string PID
        {
            get
            {
                if (ComPortInfo != null)
                {
                    return new CVID_PID { VID_PID = ComPortInfo.VID_PID }.PID;
                }

                return FTDI_D2xx == null ? "" : FTDI_D2xx.PID(FTDI_D2xx.IndexOfDeviceToOpen);
            }
        }


        /// <summary>
        /// FTDI Vendor ID (VID)
        /// </summary>
        public string VID
        {
            get
            {
                if (ComPortInfo != null)
                {
                    return new CVID_PID { VID_PID = ComPortInfo.VID_PID }.VID;
                }

                return FTDI_D2xx == null ? "" : FTDI_D2xx.VID(FTDI_D2xx.IndexOfDeviceToOpen);
            }
        }


        /// <summary>
        /// VID & PID
        /// </summary>
        /// <value>
        /// "VID_0403&PID_6001"
        /// </value>
        public string VID_PID
        {
            get
            {
                if (ComPortInfo != null)
                    return ComPortInfo.VID_PID;

                return new CVID_PID
                {
                    PID = PID,
                    VID = VID
                }.VID_PID;
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
                    EnConnectionResult.Connected_via_XBee => c8XBee?.XBeeConnection?.PortName ?? "",
                    EnConnectionResult.Connected_via_RS232 => c8RS232?.SerialPort?.PortName ?? "",
                    _ => ""
                };
            }
        }


        /// <summary>
        /// Connectionstatus
        /// </summary>
        public EnConnectionResult ConnectionStatus { get; set; }

        public CComPortInfo? ComPortInfo { get; private set; } = null;
        #endregion

        #region events

        public event EventHandler<string>? DeviceDisconnected;
        /// Occurs when Neurolink is disconnected from USB
        protected virtual void OnDeviceDisconnected(string pidVid)
        {
            DeviceDisconnected?.Invoke(this, pidVid);
        }


        public event EventHandler<EnConnectionResult>? DeviceConnected;
        protected virtual void OnDeviceConnected(EnConnectionResult connectionResult)
        {
            var handler = DeviceConnected;
            handler?.Invoke(this, connectionResult);
        }

        #endregion

        /// <summary>
        /// Finalizes an instance of the <see cref="CNMasterReceiver"/> class.
        /// </summary>
        ~CNMasterReceiver()
        {
            //Make sure that USB monitoring and all other stuff is down
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
        public EnConnectionResult Init_via_D2XX(List<string>? DescriptionContains = null)
        {
            Close();
            List<int> idxNeurolinkDevices = [];
            EnConnectionResult ret = EnConnectionResult.No_Active_Neurolink;

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
                    ret = EnConnectionResult.More_than_one_Neurolink_detected;
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
                    ConnectionResult = EnConnectionResult.Connected_via_RS232;
                    c8RS232 ??= new CNMasterRS232();
                    c8RS232.Init(
                        FTDI_D2xx,
                        CNMaster.CommandChannelNo,
                        CNMaster.AliveSequToSend(),
                        CNMaster.AliveSequToReturn()
                    );

                    FTDI_D2xx.BaudRate = c8RS232.BaudRate_LocalDevice;     //4.2.2016
                    FTDI_D2xx.IndexOfDeviceToOpen = idxRS232Connection;

                    if (c8RS232 is not null)
                    {
                        if (CNMaster.Check4Neuromaster(c8RS232.SerialPort))   //fast
                        {
                            //RS232 Connection is OK
                            ConnectionResult = EnConnectionResult.Connected_via_RS232;

                            //Make sure that XBEE is in Sleep
                            CFTDI_D2xx FTDI_D2xx_temp = new()
                            {
                                IndexOfDeviceToOpen = idxXBEeeConnection
                            };
                            c8XBee ??= new CXBee();
                            c8XBee.Init(
                                FTDI_D2xx_temp,
                                CNMaster.CommandChannelNo,
                                CNMaster.AliveSequToSend(),
                                CNMaster.AliveSequToReturn()
                            );
                            c8XBee.Send_to_Sleep();
                            c8XBee.Close();

                            FTDI_D2xx_temp.Close();
                            FTDI_D2xx_temp.Dispose();
                            StartUSBMonitoring();
                            ret = EnConnectionResult.Connected_via_USBCable;
                        }
                        else
                        {
                            //Go for XBEE
                            //It must be XBee ... leave Connection to the calling routine
                            c8RS232.Close();
                            FTDI_D2xx.IndexOfDeviceToOpen = idxXBEeeConnection;
                            ConnectionResult = EnConnectionResult.Connected_via_XBee;
                            c8XBee = new CXBee();
                            c8XBee.Init(
                                FTDI_D2xx,
                                CNMaster.CommandChannelNo,
                                CNMaster.AliveSequToSend(),
                                CNMaster.AliveSequToReturn()
                            );

                            try
                            {
                                if (c8XBee.XBeeConnection.InitXBee())
                                {
                                    



                                    ret = EnConnectionResult.Connected_via_XBee;
                                    StartUSBMonitoring();
                                }
                                else
                                {
                                    ret = EnConnectionResult.Error_during_XBee_connection;
                                }
                            }
                            catch
                            {
                                ret = EnConnectionResult.Error_during_XBee_connection;
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
            return ret;
        }

        #region USBMonitoring
        private USBMonitor? usbm = null;
        private string VID_PID_opened = ""; //Tp back up PID VID for OnDeviceDisconnected Event - IF FTDI is gone, PID VID (this.VID_PID) are also gone

        public CNMasterReceiver()
        {
        }

        //private bool USBMonitorIsConnected = false;
        /// <summary>
        /// Starts USB Monitoring via Windows Messages
        /// </summary>
        public void StartUSBMonitoring()
        {
            if (usbm == null)
            {
                usbm = new USBMonitor();
                usbm.USBDeviceConnectedEvent += Usbm_USBDeviceConnectedEvent;
                usbm.USBDeviceDisConnectedEvent += Usbm_USBDeviceDisConnectedEvent;
            }
            VID_PID_opened = VID_PID;
        }


        /// <summary>
        /// Stops the usb monitoring.
        /// </summary>
        public void StopUSBMonitoring()
        {
            usbm?.Close();
        }

        /* Events kommen 3x!!!!         */
        private void Usbm_USBDeviceDisConnectedEvent(object? sender, EventArgs e)
        {
            if (ConnectionStatus != EnConnectionResult.USB_disconnected)
            {
                ConnectionStatus = EnConnectionResult.USB_disconnected;
                OnDeviceDisconnected(VID_PID_opened);
            }
        }

        private void Usbm_USBDeviceConnectedEvent(object? sender, EventArgs e)
        {
            if (ConnectionStatus != EnConnectionResult.USB_reconnected)
            {
                ConnectionStatus = EnConnectionResult.USB_reconnected;
                OnDeviceConnected(EnConnectionResult.NoConnection);
            }
        }

        #endregion

        /// <summary>
        /// Closes this instance and communication
        /// </summary>
        public void Close()
        {
            StopUSBMonitoring();
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

