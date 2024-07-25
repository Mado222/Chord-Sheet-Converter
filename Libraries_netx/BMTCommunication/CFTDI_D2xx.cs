using FTD2XX_NET;
using System.IO.Ports;
using System.Runtime.InteropServices;

//using log4net;
//using log4net.Config;
//using log4net.Appender;
//using log4net.Layout;

using WindControlLib;



namespace BMTCommunication
{
    /************************************************************************************
     * ************************************************************************************
     * CLASS: CFTDI_D2xx
     * ************************************************************************************
    ************************************************************************************/

    /// <summary>
    /// Acess to FTDI Chip via D2XX Driver
    /// </summary>
    /// <remarks>
    /// http://www.ftdichip.com/Drivers/D2XX.htm
    /// </remarks>
    public class CFTDI_D2xx : ISerialPort, IDisposable
    {
        private readonly FTDI myFtdiDevice;
        private FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;
        private readonly CHighPerformanceDateTime hp_Timer;

        /// <summary>
        /// log4net 
        /// </summary>
        //protected static readonly ILog logger = LogManager.GetLogger(typeof(CFTDI_D2xx));

        /// <summary>
        /// Object to lock myFtdiDevice
        /// </summary>
        public readonly object FTDILock = new object();
        //public readonly object FTDIWriteLock = new object();
        //public readonly object FTDISettingsLock = new object();

        private int _numDevices = -1;
        /// <summary>
        /// Number of devices detected
        /// </summary>
        public int numDevices
        {
            get { return _numDevices; }
        }


        private int _IndexOfDeviceToOpen=-1;
        public int IndexOfDeviceToOpen
        {
            get { return _IndexOfDeviceToOpen; }
            set { _IndexOfDeviceToOpen = value; }
        }


#region Device_Data
        public string Description (int idx)
        {
            return ftdiDeviceList[idx].Description;
        }

        public uint ID (int idx)
        {
            return ftdiDeviceList[idx].ID; 
        }

        public string PID (int idx)
        {
            //0xvvvvpppp
            string PID = ftdiDeviceList[idx].ID.ToString("X8");
            return PID.Substring(PID.Length - 4);
        }

        public string VID(int idx)
        {
            //0xvvvvpppp
            string VID = ftdiDeviceList[idx].ID.ToString("X8");
            return VID.Substring(VID.Length - 8, 4);
        }

        public uint LocId(int idx)
        {
            return ftdiDeviceList[idx].LocId;
        }

        public string SerialNumber(int idx)
        {
            return ftdiDeviceList[idx].SerialNumber;
        }

        public string RelatedCom(int idx)
        {
            string ComPort="";
            //FTDI.FT_STATUS stat= myFtdiDevice.GetCOMPort(out ComPort);
            List<CComPortInfo> ComPortInfo = CGetComPorts.GetComPortInfo(String.Empty, String.Empty, ftdiDeviceList[idx].SerialNumber);

            //Remove not active Ports from the List
            List<string> ActiveComPorts = BMTCommunication.CGetComPorts.GetActiveComPorts(String.Empty);
            for (int i = ComPortInfo.Count - 1; i >= 0; i--)
            {
                bool isActife=false;
                for (int j = 0; j < ActiveComPorts.Count; j++)
                {
                    if (ComPortInfo[i].ComName == ActiveComPorts[j])
                        isActife = true;
                }
                if (!isActife)
                    ComPortInfo.RemoveAt(i);
            }

            //Remove duplicates
            if (ComPortInfo.Count > 1)
            {
                for (int i = ComPortInfo.Count - 1; i > 0; i--)
                {

                    if (ComPortInfo[i].ComName == ComPortInfo[i - 1].ComName)
                        ComPortInfo.RemoveAt(i);
                }
            }


            if (ComPortInfo.Count == 1)
                ComPort = ComPortInfo[0].ComName;
            return ComPort;
        }

        /// <summary>
        /// Copied from class FTDI ... that usiung classes have not to invoke FTDI
        /// </summary>
        public enum FTDI_Types
        {
            FT_DEVICE_BM = 0,
            FT_DEVICE_AM = 1,
            FT_DEVICE_100AX = 2,
            FT_DEVICE_UNKNOWN = 3,
            FT_DEVICE_2232 = 4,
            FT_DEVICE_232R = 5,
            FT_DEVICE_2232H = 6,
            FT_DEVICE_4232H = 7,
            FT_DEVICE_232H = 8,
            FT_DEVICE_X_SERIES = 9,
        }

        public FTDI_Types Type(int idx)
        {
            return (FTDI_Types)ftdiDeviceList[idx].Type;
        }
            

#endregion Device_Data

        /// <summary>
        /// Initializes a new instance of the <see cref="CFTDI_D2xx" /> class.
        /// </summary>
        public CFTDI_D2xx()
        {
            if (myFtdiDevice == null)
            {
                myFtdiDevice = new FTDI();
            }

            /*
            //Log4Net anlegen
            FileAppender logFile = new FileAppender();
            logFile.File = Path.GetDirectoryName(Assembly.GetAssembly(typeof(CFTDI_D2xx)).Location) + "\\" + "CFTDI_D2xx.log";
            logFile.AppendToFile = true;
            logFile.Encoding = Encoding.UTF8;
            logFile.Layout = new PatternLayout("%date [%thread] %-5level [%logger] %message%newline");
            /* Hirarchie
                * ALL
                * DEBUG
                * INFO
                * WARN
                * ERROR
                * FATAL
                * OFF
             */
            //logFile.Threshold = log4net.Core.Level.Off;
            //logFile.ActivateOptions();
            //BasicConfigurator.Configure(logFile);

            hp_Timer = new CHighPerformanceDateTime();

            
            /*
            //Code to implement OnReceive Event
            receivedDataEvent = new AutoResetEvent(false);
            dataReceiveBackgroundWorker = new BackgroundWorker();
            dataReceiveBackgroundWorker.DoWork += new DoWorkEventHandler(dataReceivedHandler_DoWork);
            dataReceiveBackgroundWorker.WorkerSupportsCancellation = true;
             */
        }

#pragma warning disable IDE0052 // Remove unread private members
        private string AdditionalFTDIInfo = "";
#pragma warning restore IDE0052 // Remove unread private members
        private FTDI.FT_STATUS _ftStatus;
        private FTDI.FT_STATUS ftStatus
        {
            get
            {
                return _ftStatus;
            }
            set
            {
                lock (FTDILock)
                {
                    _ftStatus = value;
                }
#if DEBUG
                if (value != FTDI.FT_STATUS.FT_OK)
                {
                    //if (logger.IsErrorEnabled) logger.Error(AdditionalFTDIInfo + ": " + value.ToString());
                }
#endif
            }
        }

        /*
        void dataReceivedHandler_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!dataReceiveBackgroundWorker.CancellationPending)
            {
                // wait until event is fired
                this.receivedDataEvent.WaitOne();
                OnSerialDataReceived(this, null);
            }
        }*/


        /// <summary>
        /// Finalizes an instance of the <see cref="CFTDI_D2xx" /> class.
        /// </summary>
        ~CFTDI_D2xx()
        {
            Close();
        }

        /// <summary>
        /// Checks for connected devices
        /// </summary>
        /// <returns></returns>
        public int CheckForConnectedDevices()
        {
            //lock (FTDISettingsLock)
            lock (FTDILock)
            {
                UInt32 ftdiDeviceCount = 0;
                FTDI.FT_STATUS ftStatustemp = FTDI.FT_STATUS.FT_OK;
                ftStatus = FTDI.FT_STATUS.FT_OK;
                ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[0];

                // Determine the number of FTDI devices connected to the machine
#if DEBUG
                AdditionalFTDIInfo = "GetNumberOfDevices";
#endif
                ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
                ftStatustemp = ftStatus;

/* ==== Folgendes macht keinen Sinn da dazu der Port offen sein muss
#if DEBUG
                AdditionalFTDIInfo = "GetCOMPort";
#endif
                string GetCOMPort = "";
                ftStatus = myFtdiDevice.GetCOMPort(out GetCOMPort);

#if DEBUG
                AdditionalFTDIInfo = "GetDescription";
#endif
                string GetDescription = "";
                ftStatus = myFtdiDevice.GetDescription(out GetDescription);

#if DEBUG
                AdditionalFTDIInfo = "GetDeviceID";
#endif
                uint GetDeviceID = 0;
                ftStatus = myFtdiDevice.GetDeviceID(ref GetDeviceID);
                //FTDI.FT_DeviceType GetDeviceType;
                //myFtdiDevice.GetDeviceType(ref GetDeviceType);

#if DEBUG
                AdditionalFTDIInfo = "GetDriverVersion";
#endif
                uint GetDriverVersion = 0;
                ftStatus = myFtdiDevice.GetDriverVersion(ref GetDriverVersion);

#if DEBUG
                AdditionalFTDIInfo = "GetEventType";
#endif
                uint GetEventType = 0;
                ftStatus = myFtdiDevice.GetEventType(ref GetEventType);

                int GetHashCode = myFtdiDevice.GetHashCode();
                byte GetLineStatus = 0;

#if DEBUG
                AdditionalFTDIInfo = "GetLineStatus";
#endif
                ftStatus = myFtdiDevice.GetLineStatus(ref GetLineStatus);
                byte GetModemStatus = 0;

#if DEBUG
                AdditionalFTDIInfo = "GetModemStatus";
#endif
                ftStatus = myFtdiDevice.GetModemStatus(ref GetModemStatus);

#if DEBUG
                AdditionalFTDIInfo = "GetSerialNumber";
#endif
                string GetSerialNumber;
                ftStatus = myFtdiDevice.GetSerialNumber(out GetSerialNumber);
*/
                // Check status
                if ((ftStatustemp == FTDI.FT_STATUS.FT_OK) && ftdiDeviceCount > 0)
                {
                    // Allocate storage for device info list
                    ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

                    // Populate our device list
#if DEBUG
                    AdditionalFTDIInfo = "GetDeviceList";
#endif

                    ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);
                }
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    ftdiDeviceList = [];
                }

                _numDevices = ftdiDeviceList.Length;
                return _numDevices;
            }
        }


        /// <summary>
        /// Resets the device.
        /// </summary>
        /// <remarks>
        /// see: http://stackoverflow.com/questions/4369679/com-port-is-denied
        /// 
        /// As with other bus controls, there is a wait period of 5-8 seconds after a CyclePort where any API call
        /// that requires direct connection to the device, like GetSerialByIndex() etc, will fail with
        /// FT_INVALID_HANDLE until it has completely stabilized.
        /// The application should account for this wait period, or setup a polling loop to detect the change
        /// in return status.
        /// </remarks>
        public FTDI.FT_STATUS ResetDevice()
        {
            FTDI.FT_STATUS _ftStatus = FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            if (myFtdiDevice != null)
            {
                lock (FTDILock)
                {
                    _ftStatus = myFtdiDevice.ResetDevice();
                    if (!IsOpen)
                        Open();
                }
            }
#if DEBUG
            AdditionalFTDIInfo = "RestDevice";
#endif
            ftStatus = _ftStatus;
            return _ftStatus;
        }

        public FTDI.FT_STATUS ResetPort()
        {
            FTDI.FT_STATUS _ftStatus = FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            if (myFtdiDevice != null)
            {
                lock (FTDILock)
                {
#if DEBUG
                    AdditionalFTDIInfo = "ResetPort";
#endif
                    _ftStatus = myFtdiDevice.ResetPort();
                    if (!myFtdiDevice.IsOpen)
                        Open();
                }
            }

            ftStatus = _ftStatus;
            return _ftStatus;
        }

        public FTDI.FT_STATUS CyclePort()
        {
            FTDI.FT_STATUS _ftStatus = FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
            if (myFtdiDevice != null)
            {
                lock (FTDILock)
                {
                    _ftStatus = myFtdiDevice.CyclePort();
                    //Wait for Device to reappear
                    if (_ftStatus == FTDI.FT_STATUS.FT_OK)
                    {
                        TimeSpan ts = new TimeSpan(0, 0, 0, 8);
                        DateTime endoftrial = DateTime.Now + ts;
                        while (DateTime.Now < endoftrial)
                        {
                            //Reopen Port
                            if (Open())
                            {
                                _ftStatus = FTDI.FT_STATUS.FT_OK;
                                break;
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
#if DEBUG
            AdditionalFTDIInfo = "CyclePort";
#endif
            ftStatus = _ftStatus;
            return _ftStatus;
        }


        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();    //3rd Close
        }

        public FTDI.FT_STATUS GetModemStatus (ref byte ModemStatus)
        {
            lock (FTDILock)
            {
                ftStatus = myFtdiDevice.GetModemStatus(ref ModemStatus);
            }

            return ftStatus;
        }

        #endregion

        #region ISerialPort Members

        private string _PortName;
        public string PortName
        {
            get
            {
                return _PortName;
            }
            set
            {
                _PortName=value;
            }
        }

        public int _BaudRate=0;
        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return _BaudRate;
            }
            set
            {
                _BaudRate = value;
            }
        }

        /// <summary>
        /// FTDI is open
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is open]; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen
        {
            get
            {
                bool b = false;
                lock (FTDILock)
                {
                    b = myFtdiDevice.IsOpen;
                }
                return b;
            }
        }

        /// <summary>
        /// Opens FTDI Device
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            bool res = false;
            //lock (FTDISettingsLock)
            lock(FTDILock)
            {
                if (_IndexOfDeviceToOpen >= 0)
                {
#if DEBUG
                    AdditionalFTDIInfo = "OpenByIndex";
#endif
                    ftStatus = myFtdiDevice.OpenByIndex((uint)_IndexOfDeviceToOpen);
                    if (ftStatus == FTDI.FT_STATUS.FT_OK)
                    {
                        if (SetTimeout())
                        {
#if DEBUG
                            AdditionalFTDIInfo = "SetBaudRate";
#endif
                            ftStatus = myFtdiDevice.SetBaudRate((uint)_BaudRate);

                            //Params can be only set when Port is open
                            SetDataCharacteristics();

                            if (Handshake == System.IO.Ports.Handshake.RequestToSend)
                            {
                                DtrEnable = true;   //Awake from Sleep 11.12.2012
                            }

                            if (ftStatus == FTDI.FT_STATUS.FT_OK)
                            {
#if DEBUG
                                AdditionalFTDIInfo = "SetResetPipeRetryCount";
#endif
                                ftStatus = myFtdiDevice.SetResetPipeRetryCount(10000);
                                res = true;
                            }
                            /*
                            FTDI.FT_STATUS status = myFtdiDevice.SetEventNotification(
                                FTDI.FT_EVENTS.FT_EVENT_RXCHAR, receivedDataEvent);
                             */
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            // Close the device
            //lock (FTDISettingsLock)
            lock (FTDILock)
            {
                if (myFtdiDevice != null)
                {
                    if (IsOpen)
                    {
                        if (Handshake== System.IO.Ports.Handshake.RequestToSend)
                            DtrEnable = false;   //Sleep 11.12.2012
                        //dataReceiveBackgroundWorker.CancelAsync();
#if DEBUG
                        AdditionalFTDIInfo = "Close";
#endif
                        ftStatus= myFtdiDevice.Close();
                    }
                }
            }
        }

        private int _ReadTimeout = 500;
        public int ReadTimeout
        {
            get { return _ReadTimeout; }
            set
            {
                int oldVal = _ReadTimeout;
                _ReadTimeout = value;
                if (!SetTimeout())
                    _ReadTimeout = oldVal;
            }
        }

        private int _WriteTimeout = 500;
        public int WriteTimeout
        {
            get { return _WriteTimeout; }
            set
            {
                int oldVal = _WriteTimeout;
                _WriteTimeout = value;
                if (!SetTimeout())
                    _ReadTimeout = oldVal;
            }
        }

        private bool SetTimeout()
        {
            bool ret = false;
            //lock (FTDISettingsLock)
            lock (FTDILock)
            {
#if DEBUG
                AdditionalFTDIInfo = "SetTimeouts";
#endif
                ftStatus = myFtdiDevice.SetTimeouts((uint)_ReadTimeout, (uint)_WriteTimeout);
                if ((ftStatus == FTDI.FT_STATUS.FT_OK))
                {
                    ret = true;
                }
            }
            return ret;
        }

        public System.IO.Ports.StopBits _StopBits = System.IO.Ports.StopBits.One;
        public System.IO.Ports.StopBits StopBits
        {
            get
            {
                return _StopBits;
            }
            set
            {
                _StopBits = value;
                SetDataCharacteristics();
            }
        }

        private System.IO.Ports.Parity _Parity = System.IO.Ports.Parity.None;
        public System.IO.Ports.Parity Parity
        {
            get
            {
                return _Parity;
            }
            set
            {
                _Parity = value;
                SetDataCharacteristics();
            }
        }

        public int _DataBits = 8;
        public int DataBits
        {
            get
            {
                return _DataBits;
            }
            set
            {
                _DataBits = value;
                SetDataCharacteristics();
            }
        }

        private void SetDataCharacteristics()
        {
            //lock (FTDISettingsLock)
            //lock (FTDILock)
            //{

            //Only works if Port is open
            if (myFtdiDevice.IsOpen)
            {
                byte ftdb = FTDI.FT_DATA_BITS.FT_BITS_7;
                if (_DataBits == 8)
                    ftdb = FTDI.FT_DATA_BITS.FT_BITS_8;

                byte ftparity = FTDI.FT_PARITY.FT_PARITY_NONE;
                switch (_Parity)
                {
                    case System.IO.Ports.Parity.Even:
                        {
                            ftparity = FTDI.FT_PARITY.FT_PARITY_EVEN;
                            break;
                        }
                    case System.IO.Ports.Parity.Mark:
                        {
                            ftparity = FTDI.FT_PARITY.FT_PARITY_MARK;
                            break;
                        }
                    case System.IO.Ports.Parity.Odd:
                        {
                            ftparity = FTDI.FT_PARITY.FT_PARITY_ODD;
                            break;
                        }
                    case System.IO.Ports.Parity.Space:
                        {
                            ftparity = FTDI.FT_PARITY.FT_PARITY_SPACE;
                            break;
                        }
                }

                byte ftstop = FTDI.FT_STOP_BITS.FT_STOP_BITS_1;
                switch (_StopBits)
                {
                    case System.IO.Ports.StopBits.None:
                        {
                            throw new Exception("None stopbits not allowed");
                        }
                    case System.IO.Ports.StopBits.OnePointFive:
                        {
                            throw new Exception("1.5 stopbits not allowed");
                        }
                    case System.IO.Ports.StopBits.Two:
                        {
                            ftstop = FTDI.FT_STOP_BITS.FT_STOP_BITS_2;
                            break;
                        }
                }

                lock (FTDILock)
                {
#if DEBUG
                    AdditionalFTDIInfo = "SetDataCharacteristics";
#endif
                    ftStatus = myFtdiDevice.SetDataCharacteristics(ftdb, ftstop, ftparity);
                }
            }
        }

        private System.IO.Ports.Handshake _Handshake = System.IO.Ports.Handshake.None;
        public System.IO.Ports.Handshake Handshake
        {
            get
            {
                return _Handshake;
            }
            set
            {
                _Handshake = value;
                SetHandshake();
            }
        }

        private void SetHandshake()
        {
            ushort dtflow = FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE;
            switch (_Handshake)
            {
                case Handshake.RequestToSend:
                    {
                        dtflow = FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS;
                        break;
                    }
                case Handshake.RequestToSendXOnXOff:
                    {
                        dtflow = FTDI.FT_FLOW_CONTROL.FT_FLOW_XON_XOFF;
                        break;
                    }
                case Handshake.XOnXOff:
                    {
                        dtflow = FTDI.FT_FLOW_CONTROL.FT_FLOW_XON_XOFF;
                        break;
                    }
            }
            lock (FTDILock)
            {
#if DEBUG
                AdditionalFTDIInfo = "SetFlowControl";
#endif

                if (IsOpen)
                {
                    ftStatus = myFtdiDevice.SetFlowControl(dtflow, 0, 0);
                }
            }
        }

        public int Read(ref byte[] buffer, int offset, int count)
        {
            lock (FTDILock)
            {
                if (IsOpen)
                {
                    byte[] ReadBuffer = new byte[count];
                    uint btRead = 0;
                    int finalBuffersize = offset + count;
                    if (buffer.Length < finalBuffersize)
                        buffer = new byte[finalBuffersize];
#if DEBUG
                    AdditionalFTDIInfo = "Read";
#endif
                    ftStatus = myFtdiDevice.Read(ReadBuffer, (uint)count, ref btRead);
                    if ((ftStatus == FTDI.FT_STATUS.FT_OK) && (btRead == count))
                    {
                        Buffer.BlockCopy(ReadBuffer, 0, buffer, offset, count);
                    }

                    return (int)btRead;
                }
                return -1;
            }
        }

        public int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            int ret = 0;
            DateTime Timeout = DateTime.Now + new TimeSpan(0, 0, 0, 0, WaitMax_ms);
            if (IsOpen)
            {
                while (((IsOpen) && BytesToRead < count) && (DateTime.Now < Timeout))
                {
                    //Application.DoEvents();
                }
                if ((IsOpen) && (BytesToRead >= count))
                {
                    ret = Read(ref buffer, offset, count);
                }
            }
            return ret;
        }

        public void DiscardInBuffer()
        {
            //lock (FTDISettingsLock)
            lock (FTDILock)
            {
#if DEBUG
                AdditionalFTDIInfo = "Purge RX";
#endif
                ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
            }
        }

        public void DiscardOutBuffer()
        {
            //lock (FTDISettingsLock)
            lock (FTDILock)
            {
#if DEBUG
                AdditionalFTDIInfo = "Purge TX";
#endif
                ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_TX);
            }
        }

        public bool Write(byte[] buffer, int offset, int count)
        {
            bool ret = false;
            //lock (FTDIWriteLock)
            lock (FTDILock)
            {
                if (IsOpen)
                {
                    byte[] Outbuffer = new byte[count];

                    Buffer.BlockCopy(buffer, offset, Outbuffer, 0, count);
                    uint numBytesWritten = 0;
#if DEBUG
                    AdditionalFTDIInfo = "Write";
#endif
                    ftStatus = myFtdiDevice.Write(Outbuffer, count, ref numBytesWritten);
                    if ((ftStatus == FTDI.FT_STATUS.FT_OK) && (numBytesWritten == count))
                        ret = true;
                }
                return ret;
            }
        }

        public int BytesToRead
        {
            get
            {
                uint bt = 0;
                lock (FTDILock)
                {
#if DEBUG
                    AdditionalFTDIInfo = "GetRxBytesAvailable";
#endif
                    if (myFtdiDevice.IsOpen)
                    {
                        ftStatus = myFtdiDevice.GetRxBytesAvailable(ref bt);
                    }
                    else
                    {
                        bt = 0;
                    }
                }
                return (int) bt;
            }
        }

        public bool RtsEnable
        {
            get { throw new NotImplementedException (); }
            set
            {
                lock (FTDILock)
                {
#if DEBUG
                    AdditionalFTDIInfo = "SetRTS";
#endif
                    ftStatus= myFtdiDevice.SetRTS(value);
                }
            }
        }

        public bool DtrEnable
        {
            get { throw new NotImplementedException(); }
            set
            {
                lock (FTDILock)
                {
#if DEBUG
                    AdditionalFTDIInfo = "SetDTR";
#endif
                    if (IsOpen)
                    {
                        ftStatus = myFtdiDevice.SetDTR(value);
                    }
                }
            }
        }

        public bool DsrHolding
        {
            get { throw new NotImplementedException(); }
        }

        public event System.IO.Ports.SerialDataReceivedEventHandler SerialDataReceivedEvent;
        protected virtual void OnSerialDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            SerialDataReceivedEvent?.Invoke(this, e);
        }


        private string _LastErrorString="";
        public string LastErrorString
        {
            get { return _LastErrorString; }
            set { _LastErrorString = value; }
        }

        public DateTime Now(enumTimQueryStatus TimQueryStatus)
        {
            return hp_Timer.Now;
        }

        #endregion
    }
}
