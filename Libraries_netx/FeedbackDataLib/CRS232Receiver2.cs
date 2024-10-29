using BMTCommunication;
using BMTCommunicationLib;
using System.ComponentModel;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{

    /// <summary>
    /// Summary description for CRS232Receiver2
    /// 
    /// Diese Klasse sollte nicht direkt instantiiert werden sondern nur abgeleitet werden
    /// um die Schnittstelleneinstellung zu implementieren
    /// 
    /// Der Konstruktor initialisiert die Komponente NICHT!!
    /// 
    /// 5.6.2005
    /// Klasse überarbeitet:
    /// Nach der Feststellung dass ein Timer nicht öfter als ca. alle 15ms kommt Empfangsroutine so modifizieren,
    /// dass bei einem Zugriff _RS232BytetoRead bytes gelesen werden, diese werden ausgewertet und in einem Event zurückgegeben.
    /// 
    /// 6.1.2006
    /// Entsprechend dem Projekt "Sehne" die Komponente auf einen eigenen Thread auslagern
    /// 
    ///Command-Daten werden in RPCommand als byte array gespeichert
    ///GetCommand gibt Daten aus RPCommand zurück, null wenn leer
    ///
    ///Kommen 4 bytes am _CommunicationChannel herein, so ist der value (siehe decode) die Anzahl der
    ///unmittelbar darauf folgende bytes in uncodierter Form 
    ///
    ///TODO:
    ///Erkennen wenn Bluetooth Verbindung abreisst und dann den Thread zumachen - 
    ///Thread bleibt nämlich hängen wenn TransmitByte aufgerufen wird und die BT Verbindung bereits weg ist
    /// 
    /// Umstieg auf DotNet 2.0:
    /// Neue Serielle Kompoenente verwenden
    /// 
    /// 6.7.2006:
    /// KeepAlive implementiert - aber ob das sinnvoll ist, da es ohnedies ein "Connected" und "No Datalink" gibt
    /// Lösungsansatz:
    /// Gerät sendet Dummy byte auf dem _KeepAliveChannel und Empfänger wertet das wie ein hereinkommendes Datenbyte
    /// (zurücksetzen des "Not Connected" Zählers
    /// 
    /// 17.4.2007:
    /// Keep Alive Signal entfernen
    /// Sync- Signal kommt mit fixer Abtastfrequenz herein und wird benuzt um Daten in "Echtzeit" zu skaliern
    /// Der Wert der im Sync-Signal mitkommt ist in ms
    /// 
    /// 4.5.2007
    /// Implementierung eines autom. Öffnen und schliessens der RS232 Verbindung
    /// Definieren einer "Alive Sequenz" die jedes Gerät implementieren MUSS!!!!
    /// 
    /// Aufruf von Connect() -> tmrConnect.Enabled=true
    /// Seriell32.Open() wirft exception -> ConnectionType.PortError, tmrConnect.Enabled=false!!
    /// Seriell32.IsOpen=true -> Abfrage ob Gerät da - bevor alles andere startet
    /// Gerät da: Connected, sonst Not_Connected
    /// 
    /// Weiters: Regelmässig AliveSequToSend schicken damit Device ggf Verbindung zu macht und BT Modul resettet
    /// 
    /// tmrConnect nur einmal nach Connect() laufen lassen - legt sonst Applikation lahm
    /// 
    /// 11.5.2007
    /// Sync-Channel fliegt wieder raus da er zuviele Ressourcen braucht
    /// Auslagern der Zeitbasisgenerierung in CDataChannel
    /// 
    /// 31.5.2007
    /// tmrConnect entfernen
    /// Verbindungsversuch auf TryToConnect auslagern
    /// TryToConnect in Connect mit eigenen Thread aufrufen damit GUI nicht blockiert wird
    /// 
    /// 24.7.2008
    /// Alive Schleife um cAliveSignalToSendInterv, NextAliveSignalToSend erweitern
    /// Handshake= RequestToSend;
    /// 
    /// 19.10.2009
    /// lock (DataLock) für this.Data einführen - da sich sonst der 
    /// RunRS232Worker thread und der RunReceiver thread in die Quere kommen - Daten verwürfeln
    /// 
    /// 10.2.2010
    /// Not necessary to send an Alive signal if data is coming in in regular intervals
    /// !! not true, Device needs a keep alive signal to recognise broken link
    /// 
    /// 15.7.2014
    /// Removed ConnectionBroken
    /// </summary>

    public partial class CRS232Receiver2 : ICommunication, IDisposable
    {

        private readonly TimeSpan DataReceiverTimeout = new(0, 0, 0, 2, 0);
        private readonly TimeSpan AliveSignalToSendInterv = new(0, 0, 0, 0, 500);
        private byte _CommandChannelNo = 0; //Wird nur im Konstruktor beschrieben

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        public CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);

        public bool SendKeepAlive { get; set; } = true;

        /// <summary>
        /// Time when next Alive Signal is due
        /// </summary>
        private DateTime NextAliveSignalToSend = DateTime.Now;

        /// <summary>
        /// Last Sync Signal received from Device = when Device timer has full second
        /// comes via command channel
        /// </summary>
        private DateTime LastSyncSignal = DateTime.MinValue;

        /// <summary>
        /// Shows if we are connected to Neuromaster
        /// </summary>
        public bool IsConnected = false;

        /// <summary>
        /// Holds the Serial Interface
        /// </summary>
        public ISerialPort? Seriell32;

        /// <summary>
        /// Object to lock buffers
        /// </summary>
        public readonly object RS232Lock = new();

        private readonly CFifoBuffer<byte[]> RPCommand = new();
        private readonly CFifoBuffer<byte[]> RPDataOut = new();
        private readonly CFifoBuffer<byte[]> RPDeviceCommunicationToPC = new();
        private bool DataSent = false;

        /// <summary>
        /// Speichert CDataIn Werte
        /// </summary>
        private readonly CFifoBuffer<CDataIn> Data = new();

        /// <summary>
        /// Datenarray in dem die empfangenen Daten gespeichert werden: Groesse: _AnzReturnBlocks*_RS232Values
        /// </summary>
        private CDataIn[] intDataArray = [];

        /// <summary>
        /// Anzahl der Byte die bei einem RS232 Zugriff gelesen werden
        /// </summary>

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0414
        private readonly int _RS232BytetoRead = 4;
#pragma warning restore CS0414
#pragma warning restore IDE0051 // Remove unused private members

        private EnumConnectionStatus _ConnectionStatus = EnumConnectionStatus.Not_Connected;
        private EnumConnectionStatus _ConnectionStatusOld = EnumConnectionStatus.Not_Connected;


        /// <summary>
        /// Gets or sets the ConnectionStatus
        /// </summary>
        private EnumConnectionStatus ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set
            {
                SetConnectionStatus(value);
            }
        }

        private void SetConnectionStatus(EnumConnectionStatus value)
        {
            _ConnectionStatus = value;
            if (_ConnectionStatus != _ConnectionStatusOld)
            {
                _ConnectionStatusOld = _ConnectionStatus;
                OnStatusChangedComm();
            }
        }


        /// <summary>
        /// RS232 receiver thread, started in TryToConnect
        /// </summary>
        private BackgroundWorker? RS232ReceiverThread;

        /// <summary>
        /// Connection thread
        /// </summary>
        private BackgroundWorker? tryToConnectWorker;

        /// <summary>
        /// Lock StatusChangedEvent
        /// </summary>
        //readonly object StatusChangedLock = new object();

        /// <summary>
        /// ={ 0x11, 0xFE };//CommandCode, cDeviceAlive
        /// </summary>
        public byte[] AliveSequToSend = [];

        /// <summary>
        /// = { 0xFE, 0, 0 };//CommandCode, cDeviceAlive, 0
        /// </summary>
        public byte[] AliveSequToReturn = [];

        /// <summary>
        /// ={ 0x11, 0xFA };//CommandCode, cConnectToDevice
        /// </summary>
        public byte[] ConnectSequToSend = [];

        /// <summary>
        /// = { 0xFA, 0, 0 };//cConnectToDevice, 0, 0
        /// </summary>
        public byte[] ConnectSequToReturn = [];

        /// <summary>Finalizes an instance of the <see cref="CRS232Receiver2" /> class.</summary>
        ~CRS232Receiver2()
        {
            CloseAll();
        }

        public CRS232Receiver2(byte CommandChannelNo, ISerialPort SerialPort)
        {
            CRS232Receiver_Constructor(CommandChannelNo, SerialPort);
        }

        /// <summary>
        /// Closes tryToConnectWorker thread or RS232ReceiverThread
        /// </summary>
        private void CloseAll()
        {
            if (Seriell32 != null)
            {
                tryToConnectWorker?.CancelAsync();
                Stop_RS232ReceiverThread();
                Seriell32.Close();  //1st Close, 4th close
            }
        }


        /// <summary>
        /// CRS232Receiver constructor
        /// </summary>
        /// <param name="CommandChannelNo">Command channel no</param>
        /// <param name="SerialPort">Related serial Port</param>
        protected void CRS232Receiver_Constructor(byte CommandChannelNo, ISerialPort SerialPort)
        {
            _CommandChannelNo = CommandChannelNo;
            InitBuffer();
            Seriell32 = SerialPort;
        }

        /// <summary>
        /// Inits the RS232inBytes buffer
        /// </summary>
        private void InitBuffer()
        {
            lock (RS232Lock)
            {
                intDataArray = new CDataIn[5];
                for (int i = 0; i < intDataArray.Length; i++)
                {
                    intDataArray[i] = new CDataIn();
                }
            }
        }

        /// <summary>
        /// Connect to Neuromaster
        /// </summary>
        /// <remarks>
        /// Starts own thread (tryToConnectWorker) that repaets trials until succesful
        /// </remarks>
        public void Connect_via_tryToConnectWorker()
        {

            if (tryToConnectWorker == null)
            {
                tryToConnectWorker = new BackgroundWorker();
#pragma warning disable CS8622
                tryToConnectWorker.DoWork += new DoWorkEventHandler(TryToConnectWorker_DoWork);
#pragma warning restore CS8622
                tryToConnectWorker.WorkerSupportsCancellation = true;
            }
            if (!tryToConnectWorker.IsBusy)
                tryToConnectWorker.RunWorkerAsync();

        }

        public void Send_to_Sleep()
        {
            if (Seriell32 != null)
            {
                Seriell32.GetOpen();
                Seriell32.DtrEnable = false;
                Seriell32.Close();
            }
        }

        #region ICommunication Members

        public event DataReadyEventHandler? DataReadyComm = null;
        /// <summary>
        /// Data Ready
        /// </summary>
        /// <remarks>
        /// Limits of the Data Range:
        /// 0x0000 ... input amplifier is neg saturated
        /// 0xFFFF ... input amplifier is pos saturated
        /// if output stage is saturated, 0x0001 and 0xFFFE
        /// </remarks>
        protected virtual void OnDataReadyComm(List<CDataIn> DataRead)
        {
            DataReadyComm?.Invoke(this, DataRead);
        }

        public event StatusChangedEventHandler? StatusChangedComm = null;
        protected virtual void OnStatusChangedComm()
        {
            StatusChangedComm?.Invoke(this);
        }

        public event DeviceCommunicationToPCEventHandler? DeviceCommunicationToPC = null;
        protected virtual void OnDeviceCommunicationToPC(byte[]? buf)
        {
            if (buf != null)
                DeviceCommunicationToPC?.Invoke(this, buf);
        }

        public EnumConnectionStatus GetConnectionStatus() => ConnectionStatus;

        /// <summary>
        /// Holt alle Daten vom Typ CDataIn aus dem internen Ringpuffer
        /// </summary>
        public void GetData(ref List<CDataIn> Data)
        {
            Data = new List<CDataIn>(this.Data.PopAll());
        }

        public void InitReceiverBuffer(int ReceiverTimerInterval, int Dummy, int BytetoRead, int Dummy2) => InitBuffer();

        public int ReceiverTimerInterval => 0;

        public bool EnableDataReadyEvent { set; get; } = false;

        public byte[] GetCommand => RPCommand.Pop() ?? [];


        //Die dieses Interface implementierende Komponente empfängt keine Daten!
        public bool EnableDataReceiving { set; get; } = true;

        public int SendByteData(byte[] DataOut, int NumData)		//Ermöglicht den direkten Zugriff auf die Kommunikation
        {
            DataSent = false;
            if (NumData != DataOut.Length)
            {
                byte[] buf = new byte[NumData];
                Array.Copy(DataOut, buf, NumData);
                RPDataOut.Push(buf);
            }
            else
            {
                RPDataOut.Push(DataOut);
            }
            DateTime dt = DateTime.Now + DataReceiverTimeout;
            while ((DateTime.Now < dt) && (!DataSent))
            {
                Thread.Sleep(1);
            }
            if (!DataSent)
                return -1;

            return 0;
        }
        /// <summary>
        /// Liest direkt von System.IO.Ports.SerialPort
        public int GetByteData(ref byte[] DataIn, int NumData, int Offset)
        {
            if (Seriell32 == null) return 0; // Return 0 if Seriell32 is null

            return Seriell32.Read(ref DataIn, Offset, NumData);
        }

        /// <summary>
        /// Setzt vorher ReadTimeout von System.IO.Ports.SerialPort
        /// </summary>
        public int GetByteDataTimeOut(ref byte[] DataIn, int NumData, int Offset, uint TimeOut)
        {
            if (Seriell32 is null)
            {
                throw new InvalidOperationException("The serial connection is not set.");
            }

            int i = Seriell32.ReadTimeout;
            int res = Seriell32.Read(ref DataIn, Offset, NumData);
            Seriell32.ReadTimeout = i;
            return res;
        }
        /// <summary>
        /// Clears the receive buffer.
        /// </summary>
        public void ClearReceiveBuffer()
        {
            Seriell32?.DiscardInBuffer();
        }
        /// <summary>
        /// Clears the transmit buffer.
        /// </summary>
        public void ClearTransmitBuffer()
        {
            Seriell32?.DiscardOutBuffer();
        }
        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            CloseAll();
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            CloseAll();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
