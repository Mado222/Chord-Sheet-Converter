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
        private CCommXBee XBeeConnection = new();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="C8XBee" /> class.
        /// </summary>
        /// <param name="ComPortName">Com Port Name (COM1, COM2, ..) in case of Serial Port Connection</param>
        /// <remarks></remarks>
        public C8XBee(string ComPortName)
        {

        }

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
            XBeeConnection = new CCommXBee(SerialPort, BaudRate_LocalDevice,
                            BaudRate_RemoteDevice,
                            CommandChannelNo,
                            ConnectSequToSend,
                            ConnectSequToReturn);

            Init(ConnectSequToSend, ConnectSequToReturn); 
        }


        public void Init (string ComPortName, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            XBeeConnection = new CCommXBee(BaudRate_LocalDevice,
                BaudRate_RemoteDevice,
                CommandChannelNo,
                ConnectSequToSend,
                ConnectSequToReturn)
            {
                PortName = ComPortName
            };
            Init(ConnectSequToSend, ConnectSequToReturn);
        }
        
        /// <summary>
        ///         /// Function for Constructor
        /// </summary>
        /// <remarks></remarks>
        private void Init(byte[] ConnectSequToSend, byte[] ConnectSequToReturn)
        {
            XBeeConnection.BaudRate = BaudRate_LocalDevice;
            XBeeConnection.StopBits = System.IO.Ports.StopBits.One;
            if (BaudRate_LocalDevice > 57600)
                XBeeConnection.StopBits = System.IO.Ports.StopBits.Two;

            XBeeConnection.Handshake = System.IO.Ports.Handshake.RequestToSend;
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

        #region IDisposable Members

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this); // Suppress the finalizer.
        }

        #endregion
    }

}

