using BMTCommunicationLib;
using System.IO.Ports;

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
    public class C8XBee : IC8Base, IDisposable
    {

        /// <summary>
        /// XBee Connection
        /// </summary>
        private CCommXBee commXBee = new();
        public CCommXBee XBeeConnection { get => commXBee; }
        private readonly int baudRate_LocalDevice = 250000;
        private readonly int baudRate_RemoteDevice = 115200;

        public ISerialPort SerialPort { get => XBeeConnection; }

        /// <summary>
        /// Part of the USB-XBee Driver Name that identifies the Neurolink
        /// </summary>
        //public const string DriverSearchName = "Insight XBEE";
        //public const string DriverSearchName = "USB Serial Port";

        /// <summary>
        /// Baud rate of the Neurolink (Connection to PC)
        /// </summary>
        public int BaudRate_LocalDevice => baudRate_LocalDevice;
        /// <summary>
        /// Baudrate of the XBee module in Neurolink
        /// </summary>
        public int BaudRate_RemoteDevice => baudRate_RemoteDevice;

        private readonly string _LastXBeeErrorString = "";
        public string LastErrorString
        {
            get
            {
                string s = _LastXBeeErrorString;
                if (XBeeConnection != null)
                    s += "; " + XBeeConnection.LastErrorString;
                return s;
            }
        }

        public C8XBee()
        { }

        // Optional: Destructor to release unmanaged resources if Dispose is not called
        ~C8XBee()
        {
            Close();
        }

        public void Close()
        {
            XBeeConnection?.Close();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8XBee" /> class.
        /// </summary>
        /// <param name="SerialPort">Serial Port</param>
        public void Init(ISerialPort SerialPort, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            commXBee = new CCommXBee(SerialPort, BaudRate_LocalDevice,
                            BaudRate_RemoteDevice);
                            //CommandChannelNo,
                            //ConnectSequToSend,
                            //ConnectSequToReturn);

            Init(); 
        }


        public void Init (string ComPortName, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            commXBee = new CCommXBee(BaudRate_LocalDevice,
                BaudRate_RemoteDevice)
                //CommandChannelNo,
                //ConnectSequToSend,
                //ConnectSequToReturn)
            {
                PortName = ComPortName
            };
            Init();
        }
        
        /// <summary>
        ///         /// Function for Constructor
        /// </summary>
        /// <remarks></remarks>
        private void Init()
        {
            XBeeConnection.BaudRate = BaudRate_LocalDevice;
            XBeeConnection.StopBits = StopBits.One;
            if (BaudRate_LocalDevice > 57600)
                XBeeConnection.StopBits = StopBits.Two;

            XBeeConnection.Handshake = Handshake.RequestToSend;
        }

        /// <summary>
        /// Opens Com and looks for device
        /// Starts in case of success tryToConnectWorker
        /// </summary>
        //public bool CheckConnection_Start_trytoConnectWorker()
        //{
        //    if (XBeeConnection == null)
        //    {
        //        throw new InvalidOperationException("XBee connection is not initialized. Ensure that the connection is established before attempting this operation.");
        //    }

        //    bool ret = false;
        //    XBeeConnection.ConfigureEndDeviceTo = CCommXBee.EnumConfigureEnDeviceTo.Neuromaster;
        //    if (XBeeConnection.InitXBee())
        //    {
        //        //Jetzt Verbindung herstellen
        //        //Debug
        //        //Connect_via_tryToConnectWorker();
        //        ret = true;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            if (XBeeConnection.XBeeSeries1 is not null)
        //            {
        //                _LastXBeeErrorString = XBeeConnection.XBeeSeries1.XBGetLastErrorTxt();
        //            }
        //            else
        //            {
        //                _LastXBeeErrorString = "XBeeConnection.XBeeSeries1 is null";
        //            }
        //        }
        //        catch
        //        {
        //            ret = false;
        //        }
        //        finally { }
        //    }

        //    return ret;
        //}

        public EnumConnectionStatus ConnectionStatus
        {
            get
            {
                if (commXBee == null)
                    return EnumConnectionStatus.Not_Connected;
                if (commXBee.IsOpen)
                    return EnumConnectionStatus.Connected;
                return EnumConnectionStatus.Not_Connected;
            }
        }

        public void Send_to_Sleep()
        {
            if (commXBee != null)
            {
                commXBee.GetOpen();
                commXBee.DtrEnable = false;
                commXBee.Close();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this); // Suppress the finalizer.
        }

        #endregion
    }

}

