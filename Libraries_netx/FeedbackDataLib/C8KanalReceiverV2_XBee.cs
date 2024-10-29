using BMTCommunication;
using BMTCommunicationLib;
using System;


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
    public class C8KanalReceiverV2_XBee : C8KanalReceiverV2_CommBase, IDisposable
    {

        /// <summary>
        /// XBee Connection
        /// </summary>
        public CXBeeConnection XBeeConnection = new();

        /// <summary>
        /// Part of the USB-XBee Driver Name that identifies the Neurolink
        /// </summary>
        //public const string DriverSearchName = "Insight XBEE";
        public const string DriverSearchName = "USB Serial Port";

        /// <summary>
        /// Baud rate of the Neurolink (Connection to PC)
        /// </summary>
        protected const int XBee_BaudRate_LocalDevice = 250000;

        /// <summary>
        /// Baudrate of the XBee module in Neurolink
        /// </summary>
        protected const int XBee_BaudRate_RemoteDevice = 115200;


        private string _LastXBeeErrorString = "";
        public string LastXBeeErrorString
        {
            get
            {
                string s = _LastXBeeErrorString;
                if (XBeeConnection != null)
                    s += "; " + XBeeConnection.LastErrorString;
                return s;
            }
        }


        /// <summary>
        /// Base Constructor
        /// </summary>
        public C8KanalReceiverV2_XBee()
        {
            //Base constructor must be empty that the derived class does not call 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8KanalReceiverV2_XBee" /> class.
        /// </summary>
        /// <param name="ComPortName">Com Port Name (COM1, COM2, ..) in case of Serial Port Connection</param>
        /// <remarks></remarks>
        public C8KanalReceiverV2_XBee(string ComPortName)
        {
            XBeeConnection = new CXBeeConnection(XBee_BaudRate_LocalDevice,
                            XBee_BaudRate_RemoteDevice,
                            C8KanalReceiverCommandCodes.cCommandChannelNo,
                            C8KanalReceiverCommandCodes.ConnectSequToSend(),
                            C8KanalReceiverCommandCodes.ConnectSequToReturn())
            {
                PortName = ComPortName
            };
            Initialise_C8KanalReceiverV2_XBee();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="C8KanalReceiverV2_XBee" /> class.
        /// </summary>
        /// <param name="SerialPort">Serial Port</param>
        public C8KanalReceiverV2_XBee(ISerialPort SerialPort)
        {
            XBeeConnection = new CXBeeConnection(SerialPort, XBee_BaudRate_LocalDevice,
                            XBee_BaudRate_RemoteDevice,
                            C8KanalReceiverCommandCodes.cCommandChannelNo,
                            C8KanalReceiverCommandCodes.ConnectSequToSend(),
                            C8KanalReceiverCommandCodes.ConnectSequToReturn());

            Initialise_C8KanalReceiverV2_XBee();
        }

        // Optional: Destructor to release unmanaged resources if Dispose is not called
        ~C8KanalReceiverV2_XBee()
        {
            Close();
        }

        /// <summary>
        /// Function for Constructor
        /// </summary>
        /// <remarks></remarks>
        private void Initialise_C8KanalReceiverV2_XBee()
        {
            XBeeConnection.BaudRate = XBee_BaudRate_LocalDevice;
            XBeeConnection.StopBits = System.IO.Ports.StopBits.One;
            if (XBee_BaudRate_LocalDevice > 57600)
                XBeeConnection.StopBits = System.IO.Ports.StopBits.Two;

            XBeeConnection.Handshake = System.IO.Ports.Handshake.RequestToSend;

            RS232Receiver = new CRS232Receiver2(C8KanalReceiverCommandCodes.cCommandChannelNo, XBeeConnection);
            base.C8KanalReceiverV2_Construct(); //calls CDataReceiver2_Construct();
            RS232Receiver.AliveSequToReturn = C8KanalReceiverCommandCodes.AliveSequToReturn();
            RS232Receiver.AliveSequToSend = C8KanalReceiverCommandCodes.AliveSequToSend();
            RS232Receiver.ConnectSequToSend = C8KanalReceiverCommandCodes.ConnectSequToSend();
            RS232Receiver.ConnectSequToReturn = C8KanalReceiverCommandCodes.ConnectSequToReturn();
            RS232Receiver.CRC8 = CRC8;
        }

        /// <summary>
        /// Opens Com and looks for device
        /// Starts in case of success tryToConnectWorker
        /// </summary>
        public bool CheckConnection_Start_trytoConnectWorker()
        {
            if (XBeeConnection == null)
            {
                throw new InvalidOperationException("XBee connection is not initialized. Ensure that the connection is established before attempting this operation.");
            }

            bool ret = false;
            XBeeConnection.ConfigureEndDeviceTo = CXBeeConnection.EnumConfigureEnDeviceTo.Neuromaster;
            if (XBeeConnection.InitXBee())
            {
                //Jetzt Verbinding herstellen
                Connect_via_tryToConnectWorker();
                ret = true;
            }
            else
            {
                try
                {
                    if (XBeeConnection.XBeeSeries1 is not null)
                    {
                        _LastXBeeErrorString = XBeeConnection.XBeeSeries1.XBGetLastErrorTxt();
                    }
                    else
                    {
                        _LastXBeeErrorString = "XBeeConnection.XBeeSeries1 is null";
                    }
                }
                catch
                {
                    ret = false;
                }
                finally { }
            }

            return ret;
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

