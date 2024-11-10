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
    public class C8RS232 : C8CommBase, IDisposable
    {
        /// <summary>
        /// Serial Port used by this class
        /// </summary>
        public ISerialPort SerialPort;

        /// <summary>
        /// Part of the USB-XBee Driver Name that identifies the Neurolink
        /// </summary>
        public const string DriverSearchName = "Insight USB";

        private readonly int _RS232_Neurolink_BaudRate = 250000;
        public int RS232_Neurolink_BaudRate
        {
            get { return _RS232_Neurolink_BaudRate; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8RS232" /> class.
        /// </summary>
        /// <param name="ComPortName">
        /// Com Port Name (COM1, COM2, ..) in case of Serial Port Connection
        /// </param>
        public C8RS232(string ComPortName)
        {
            SerialPort = new CSerialPortWrapper
            {
                PortName = ComPortName
            };
            Initialise_C8KanalReceiverV2_RS232();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8RS232" /> class.
        /// </summary>
        /// <param name="ComPortName">Com Port Name (COM1, COM2, ..) in case of Serial Port Connection</param>
        /// <param name="BaudRate">The baud rate.</param>
        public C8RS232(string ComPortName, int BaudRate)
        {
            SerialPort = new CSerialPortWrapper
            {
                PortName = ComPortName
            };
            _RS232_Neurolink_BaudRate = BaudRate;
            Initialise_C8KanalReceiverV2_RS232();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="C8RS232" /> class.
        /// </summary>
        /// <param name="SerialPort">The serial port.</param>
        public C8RS232(ISerialPort SerialPort)
        {
            this.SerialPort = SerialPort;
            Initialise_C8KanalReceiverV2_RS232();
        }

        /// <summary>
        /// Initialises C8KanalReceiverV2
        /// </summary>
        private void Initialise_C8KanalReceiverV2_RS232()
        {
            SerialPort.BaudRate = RS232_Neurolink_BaudRate;
            SerialPort.Handshake = System.IO.Ports.Handshake.None;

            SerialPort.Parity = System.IO.Ports.Parity.None;
            SerialPort.DataBits = 8;
            SerialPort.StopBits = System.IO.Ports.StopBits.One;

            RS232Receiver = new CRS232Receiver(C8KanalReceiverCommandCodes.cCommandChannelNo, SerialPort);
            base.C8KanalReceiverV2_Construct(); //calls CDataReceiver2_Construct();
            RS232Receiver.AliveSequToReturn = C8KanalReceiverCommandCodes.AliveSequToReturn();
            RS232Receiver.AliveSequToSend = C8KanalReceiverCommandCodes.AliveSequToSend();
            RS232Receiver.ConnectSequToReturn = C8KanalReceiverCommandCodes.ConnectSequToReturn();
            RS232Receiver.ConnectSequToSend = C8KanalReceiverCommandCodes.ConnectSequToSend();
            RS232Receiver.CRC8 = CRC8;
        }

        /// <summary>
        /// Opens Com and looks for device
        /// </summary>
        public bool CheckConnection_Start_trytoConnectWorker()
        {
            //Jetzt Verbinding herstellen
            Connect_via_tryToConnectWorker();
            return true;
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



