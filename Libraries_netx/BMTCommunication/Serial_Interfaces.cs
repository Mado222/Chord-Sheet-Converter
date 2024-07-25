using System.IO.Ports;
using WindControlLib;

namespace BMTCommunication
{
    /// <summary>
    /// Interface for Serial Ports
    /// </summary>
    public interface ISerialPort
    {
        string PortName { get; set; }
        int BaudRate { get; set; }
        bool IsOpen { get; }
        void Close();
        bool Open();
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        StopBits StopBits { get; set; }
        Parity Parity { get; set; }
        int DataBits { get; set; }
        Handshake Handshake { get; set; }
        int Read(ref byte[] buffer, int offset, int count);
        int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms);
        void DiscardInBuffer ();
        void DiscardOutBuffer ();
        void Dispose () ;
        bool Write (byte[] buffer,int offset, int count);
        int BytesToRead { get; }
        //event LogErrorEventHandler LogError;
        //string UniqueIdentifier { get; }
        event SerialDataReceivedEventHandler SerialDataReceivedEvent;
        string LastErrorString { get; set; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }
        bool DsrHolding { get; }
        DateTime Now (enumTimQueryStatus TimQueryStatus);
    }


    /// <summary>
    /// Implements ISerialPort for Microsoft class: System.IO.Ports.SerialPort
    /// </summary>
    public class CSerialPortWrapper: ISerialPort
    {
        private readonly System.IO.Ports.SerialPort _SerialPort;
        private readonly CHighPerformanceDateTime hp_Timer;

        /// <summary>
        /// Object to lock Serial Port
        /// </summary>
        public readonly object RS232ReadLock = new object();
        public readonly object RS232WriteLock = new object();
        public readonly object RS232SettingsLock = new object();
        public readonly object RS232EventLock = new object();

        public CSerialPortWrapper()
        {
            _SerialPort = new SerialPort
            {
                ReadBufferSize = 8192,
                //_SerialPort.DataReceived += new SerialDataReceivedEventHandler(_SerialPort_DataReceived);
                DtrEnable = true
            };
            hp_Timer = new CHighPerformanceDateTime();
        }

        void _SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnSerialDataReceived(sender, e);
        }

        #region ISerialPort Members

        /*
        public event LogErrorEventHandler LogError;
        protected virtual void OnLogError(string ErrorText)
        {
            if (LogError != null) LogError(this, ErrorText);
        }*/

        public string? PortName
        {
            get { return _SerialPort.PortName; }
            set
            {
                if (value !=null && value != "")
                {
                    _SerialPort.PortName = value;
                }
            }
        }

        public int BaudRate
        {
            get { return _SerialPort.BaudRate; }
            set { _SerialPort.BaudRate = value; }
        }

        public bool IsOpen
        {
            get { return _SerialPort.IsOpen; }
        }

        public void Close()
        {
            if (_SerialPort != null)
            {
                try 
                {
                    lock (RS232SettingsLock)
                    {
                        _SerialPort.Close();
                    }
                }
                catch (Exception ee) 
                {
                    LastErrorString = ee.Message;
                }
            }
        }

        public virtual bool Open()
        {
            if (_SerialPort != null)
            {
                try
                {
                    lock (RS232SettingsLock)
                    {
                        _SerialPort.Open();
                    }
                }
                catch (Exception ee)
                {
                    LastErrorString = ee.Message;
                }

                return _SerialPort.IsOpen;
            }
            return false;
        }

        public int ReadTimeout
        {
            get { return _SerialPort.ReadTimeout; }
            set { _SerialPort.ReadTimeout = value; }
        }

        public int WriteTimeout
        {
            get { return _SerialPort.WriteTimeout; }
            set { _SerialPort.WriteTimeout = value; }
        }

        public StopBits StopBits
        {
            get { return _SerialPort.StopBits; }
            set { _SerialPort.StopBits = value; }
        }

        public Parity Parity
        {
            get { return _SerialPort.Parity; }
            set { _SerialPort.Parity = value; }
        }

        public int DataBits
        {
            get { return _SerialPort.DataBits; }
            set { _SerialPort.DataBits = value; }
        }

        public Handshake Handshake
        {
            get { return _SerialPort.Handshake; }
            set {
                if ((value == Handshake.RequestToSend) && (_SerialPort.WriteTimeout == -1))
                {
                    _SerialPort.WriteTimeout = 100; //[ms]
                }
                _SerialPort.Handshake = value; }
        }

        public virtual int Read(ref byte[] buffer, int offset, int count)
        {
            int ret = 0;
            lock (RS232ReadLock)
            {
                ret = Read(ref buffer, offset, count, _SerialPort.ReadTimeout);
            }
             return ret;
         }

        public virtual int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            int ret = 0;
            DateTime Timeout = DateTime.Now + new TimeSpan(0, 0, 0, 0, WaitMax_ms);
            if (_SerialPort.IsOpen)
            {
                while (_SerialPort.IsOpen && _SerialPort.BytesToRead < count && (DateTime.Now < Timeout))
                {
                    //Application.DoEvents();
                }
                if (_SerialPort.IsOpen && (_SerialPort.BytesToRead >= count))
                {
                    buffer = new byte[count];
                    ret = _SerialPort.Read(buffer, offset, count);
                }
            }
            return ret;
        }

        public void DiscardInBuffer()
        {
            if (_SerialPort.IsOpen)
                _SerialPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            if (_SerialPort.IsOpen)            
                _SerialPort.DiscardOutBuffer();
        }

        public void Dispose()
        {
            if (_SerialPort != null)
                _SerialPort.Dispose();
        }

        public virtual bool Write(byte[] buffer, int offset, int count)
        {
            try
            {
                lock (RS232WriteLock)
                {
                    if (_SerialPort.IsOpen)
                        _SerialPort.Write(buffer, offset, count);
                }
                return true;
            }
            catch (Exception ee)
            {
                LastErrorString = ee.Message;
            }

            return false;
        }

        public virtual bool Write_byte_by_byte(byte[] buffer, int offset, int count, int Delay_Between_Bytes_ms)
        {
            try
            {
                lock (RS232WriteLock)
                {
                    if (_SerialPort.IsOpen)
                    {
                        int byte_Length_ms = (int)(1.0 / (double) _SerialPort.BaudRate * 10.0 * 1000.0);
                        int wait_ms = byte_Length_ms + Delay_Between_Bytes_ms;
                        for (int i = 0; i < count; i++)
                        {
                            _SerialPort.Write(buffer, offset + i, 1);
                            CDelay.Delay_ms(wait_ms);
                        }
                    }
                }
                return true;
            }
            catch (Exception ee)
            {
                LastErrorString = ee.Message;
            }

            return false;
        }


        public virtual int BytesToRead
        {
            get { return _SerialPort.BytesToRead; }
        }

        public bool RtsEnable
        {
            get { return _SerialPort.RtsEnable; }
            set { _SerialPort.RtsEnable = value; }
        }
        
        public bool DtrEnable
        {
            get { return _SerialPort.DtrEnable; }
            set { _SerialPort.DtrEnable = value; }
        }

        public bool DsrHolding
        {
            get { return _SerialPort.DsrHolding; }
        }

        public event SerialDataReceivedEventHandler SerialDataReceivedEvent;
        protected virtual void OnSerialDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            lock (RS232EventLock)
            {
                SerialDataReceivedEvent?.Invoke(this, e);
            }
        }

        private string _LastErrorString="";
        public string LastErrorString
        {
            get { return _LastErrorString; }
            set { _LastErrorString = value; }
        }

        public DateTime Now (enumTimQueryStatus TimQueryStatus)
        {
            return hp_Timer.Now;
        }


        #endregion
    }
    
}
