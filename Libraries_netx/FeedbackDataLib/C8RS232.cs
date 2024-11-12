using BMTCommunicationLib;


/* ModuleCommand noch nicht getestet - O.K. 18.7.2006

//* Problem bei setzen des Command-Modus: Wie kann ich auf Info die über den Kommandokanal kommt warten - eigener Thread?
//CDataReceiver2 ist da, damit die ursprüngliche SW nicht beeinträchtigt wird
//Kommandoprotokoll: Kanalnummer, Anzahl der Byte, ..... ?
//=> durch eigenen Thread gelöst

GetVersion implementieren
  
 Problem: Während Daten hereinkommen werden die KOmmandos praktisch nicht akzeptiert
 Grund: WaitCommandResponse wartet auf ANtwort - dieser Thread kann aber währenddessen keine
 Events abarbeiten und hält damit den Empfängerthread auf - dieser kann die Daten nicht abarbeiten und
 und die Kommandodaten nicht schicken!
 
 Abhilfe:
 In der Receiver worker loop DataReadyComm(this, DataAdded)
 durch
 ThreadPool.QueueUserWorkItem(new WaitCallback(RaiseDataReadyComm), DataAdded);
 
 ersetzen - Bild ruckelt dann aber anscheinend mehr!!
  
*/

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
    public class C8RS232: IC8Base, IDisposable
    {
        /// <summary>
        /// Serial Port used by this class
        /// </summary>
        private ISerialPort serialPort;
        private readonly int baudRate_RemoteDevice = 250000;
        private readonly int baudRate_LocalDevice = 250000;

        public ISerialPort SerialPort { get => serialPort; }

        /// <summary>
        /// Part of the USB-XBee Driver Name that identifies the Neurolink
        /// </summary>
        //public const string DriverSearchName = "Insight USB";

        public int BaudRate_LocalDevice => baudRate_LocalDevice;
        public int BaudRate_RemoteDevice => baudRate_RemoteDevice;
        /// <summary>
        /// Initializes a new instance of the <see cref="C8RS232" /> class.
        /// </summary>
        /// <param name="ComPortName">
        /// Com Port Name (COM1, COM2, ..) in case of Serial Port Connection
        /// </param>
        public C8RS232(string ComPortName)
        {
            serialPort = new CSerialPortWrapper
            {
                PortName = ComPortName
            };
            Init();
        }

        public string LastErrorString
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8RS232" /> class.
        /// </summary>
        /// <param name="SerialPort">The serial port.</param>
        public C8RS232()
        {
            serialPort = new CSerialPortWrapper();
        }

        public void Init(ISerialPort SerialPort, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
        }

        public void Init(string ComPortName, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            serialPort = new CSerialPortWrapper
            {
                PortName = ComPortName
            };
            Init();
        }

        /// <summary>
        /// Initialises C8KanalReceiverV2
        /// </summary>
        private void Init()
        {
            SerialPort.BaudRate = BaudRate_LocalDevice;
            SerialPort.Handshake = System.IO.Ports.Handshake.None;

            SerialPort.Parity = System.IO.Ports.Parity.None;
            SerialPort.DataBits = 8;
            SerialPort.StopBits = System.IO.Ports.StopBits.One;
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



