using BMTCommunicationLib;

namespace FeedbackDataLib
{


    //***************************************************************
    //
    //              C8KanalReceiverV2_RS232
    //
    //***************************************************************/
    /// <summary>
    /// Basic Component for Insight Instruments 8 Channel Device
    /// </summary>
    public class CNMasterRS232 : IC8Base, IDisposable
    {
        /// <summary>
        /// Serial Port used by this class
        /// </summary>
        private ISerialPort serialPort;
        private readonly int baudRate_RemoteDevice = 250000;
        private readonly int baudRate_LocalDevice = 250000;

        public ISerialPort? SerPort { get => serialPort; }

        /// <summary>
        /// Part of the USB-XBee Driver Name that identifies the Neurolink
        /// </summary>
        //public const string DriverSearchName = "Insight USB";

        public int BaudRate_LocalDevice => baudRate_LocalDevice;
        public int BaudRate_RemoteDevice => baudRate_RemoteDevice;
        /// <summary>
        /// Initializes a new instance of the <see cref="CNMasterRS232" /> class.
        /// </summary>
        /// <param name="ComPortName">
        /// Com Port Name (COM1, COM2, ..) in case of Serial Port Connection
        /// </param>
        public CNMasterRS232(string ComPortName)
        {
            serialPort = new CSerialPortWrapper
            {
                PortName = ComPortName
            };
            Init();
        }

        public void Close()
        {
            serialPort?.Close();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CNMasterRS232" /> class.
        /// </summary>
        /// <param name="SerialPort">The serial port.</param>
        public CNMasterRS232()
        {
            serialPort = new CSerialPortWrapper();
        }

        public void Init(ISerialPort SerialPort)
        {
            serialPort = SerialPort;
            Init();
        }

        /// <summary>
        /// Initialises C8KanalReceiverV2
        /// </summary>
        private void Init()
        {
            if (SerPort is null) { return; }

            SerPort.BaudRate = BaudRate_LocalDevice;
            SerPort.Handshake = System.IO.Ports.Handshake.None;

            SerPort.Parity = System.IO.Ports.Parity.None;
            SerPort.DataBits = 8;
            SerPort.StopBits = System.IO.Ports.StopBits.One;
        }

        public EnConnectionStatus ConnectionStatus
        {
            get
            {
                if (serialPort == null)
                    return EnConnectionStatus.NoConnection;
                if (serialPort.IsOpen)
                    return EnConnectionStatus.Connected_via_RS232;
                return EnConnectionStatus.NoConnection;
            }
        }

        public void Send_to_Sleep()
        {
            if (serialPort != null)
            {
                serialPort.GetOpen();
                serialPort.DtrEnable = false;
                serialPort.Close();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}



