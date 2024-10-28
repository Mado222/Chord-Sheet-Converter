using BMTCommunication;
using System.IO.Ports;
using WindControlLib;

namespace BMTCommunicationLib
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
        void DiscardInBuffer();
        void DiscardOutBuffer();
        void Dispose();
        bool Write(byte[] buffer, int offset, int count);
        int BytesToRead { get; }
        //event LogErrorEventHandler LogError;
        //string UniqueIdentifier { get; }
        event SerialDataReceivedEventHandler SerialDataReceivedEvent;
        string LastErrorString { get; set; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }
        bool DsrHolding { get; }
        DateTime Now(enumTimQueryStatus TimQueryStatus);
    }


    /// <summary>
    /// Implements ISerialPort for Microsoft class: System.IO.Ports.SerialPort
    /// </summary>
    public class CSerialPortWrapper : ISerialPort
    {
        private readonly SerialPort _SerialPort;
        private readonly CHighPerformanceDateTime hp_Timer;

        /// <summary>
        /// Object to lock Serial Port
        /// </summary>
        public readonly object RS232ReadLock = new();
        public readonly object RS232WriteLock = new();
        public readonly object RS232SettingsLock = new();
        public readonly object RS232EventLock = new();

        public CSerialPortWrapper()
        {
            _SerialPort = new SerialPort
            {
                ReadBufferSize = 8192,
                DtrEnable = true
            };
            hp_Timer = new CHighPerformanceDateTime();
        }

        #region ISerialPort Members

        public string PortName
        {
            get => _SerialPort != null ? _SerialPort.PortName : "";
            set
            {
                if (value != null && value != "")
                {
                    _SerialPort.PortName = value;
                }
            }
        }

        public int BaudRate
        {
            get => _SerialPort.BaudRate;
            set => _SerialPort.BaudRate = value;
        }

        public bool IsOpen
        {
            get => _SerialPort.IsOpen; 
        }
        public void Close()
        {
            if (_SerialPort == null) return;

            try
            {
                lock (RS232SettingsLock)
                {
                    _SerialPort.Close();
                }
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
            }
        }

        public virtual bool Open()
        {
            if (_SerialPort == null) return false;

            try
            {
                lock (RS232SettingsLock)
                {
                    _SerialPort.Open();
                }
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
            }

            return _SerialPort?.IsOpen ?? false;
        }


        public int ReadTimeout
        {
            get => _SerialPort.ReadTimeout;
            set => _SerialPort.ReadTimeout = value;
        }

        public int WriteTimeout
        {
            get => _SerialPort.WriteTimeout;
            set => _SerialPort.WriteTimeout = value;
        }

        public StopBits StopBits
        {
            get => _SerialPort.StopBits;
            set => _SerialPort.StopBits = value;
        }

        public Parity Parity
        {
            get => _SerialPort.Parity;
            set => _SerialPort.Parity = value;
        }

        public int DataBits
        {
            get => _SerialPort.DataBits;
            set => _SerialPort.DataBits = value;
        }
        public Handshake Handshake
        {
            get => _SerialPort.Handshake;
            set
            {
                if (value == Handshake.RequestToSend && _SerialPort.WriteTimeout == -1)
                {
                    _SerialPort.WriteTimeout = 100; // Set default WriteTimeout if needed [ms]
                }
                _SerialPort.Handshake = value;
            }
        }

        public virtual int Read(ref byte[] buffer, int offset, int count)
        {
            lock (RS232ReadLock)
            {
                // Using the other overloaded method with ReadTimeout as WaitMax_ms
                return Read(ref buffer, offset, count, _SerialPort.ReadTimeout);
            }
        }

        public virtual int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen) return 0;

            DateTime timeout = DateTime.Now + TimeSpan.FromMilliseconds(WaitMax_ms);

            // Wait until the required number of bytes are available or timeout occurs
            while (_SerialPort.IsOpen && _SerialPort.BytesToRead < count && DateTime.Now < timeout)
            {
                // Uncomment if Application.DoEvents() is needed, but be cautious as it can cause reentrancy issues.
                // Application.DoEvents(); 
            }

            if (_SerialPort.IsOpen && _SerialPort.BytesToRead >= count)
            {
                buffer = new byte[count];
                return _SerialPort.Read(buffer, offset, count);
            }

            return 0;
        }

        public void DiscardInBuffer()
        {
            if (_SerialPort?.IsOpen == true)
                _SerialPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            if (_SerialPort?.IsOpen == true)
                _SerialPort.DiscardOutBuffer();
        }

        public void Dispose()
        {
            _SerialPort?.Dispose();
        }


        public virtual bool Write(byte[] buffer, int offset, int count)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen) return false;

            try
            {
                lock (RS232WriteLock)
                {
                    _SerialPort.Write(buffer, offset, count);
                }
                return true;
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
                return false;
            }
        }

        public virtual bool WriteByteByByte(byte[] buffer, int offset, int count, int delayBetweenBytesMs)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen) return false;

            try
            {
                lock (RS232WriteLock)
                {
                    int byteLengthMs = (int)(1.0 / _SerialPort.BaudRate * 10.0 * 1000.0);
                    int waitMs = byteLengthMs + delayBetweenBytesMs;

                    for (int i = 0; i < count; i++)
                    {
                        _SerialPort.Write(buffer, offset + i, 1);
                        CDelay.Delay_ms(waitMs);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
                return false;
            }
        }

        public virtual int BytesToRead => _SerialPort?.BytesToRead ?? 0;

        public bool RtsEnable
        {
            get => _SerialPort?.RtsEnable ?? false;
            set
            {
                if (_SerialPort != null)
                {
                    _SerialPort.RtsEnable = value;
                }
            }
        }

        public bool DtrEnable
        {
            get => _SerialPort?.DtrEnable ?? false;
            set
            {
                if (_SerialPort != null)
                {
                    _SerialPort.DtrEnable = value;
                }
            }
        }

        public bool DsrHolding => _SerialPort?.DsrHolding ?? false;

        public event SerialDataReceivedEventHandler? SerialDataReceivedEvent;

        protected virtual void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (RS232EventLock)
            {
                SerialDataReceivedEvent?.Invoke(this, e);
            }
        }

        private string _lastErrorString = "";
        public string LastErrorString
        {
            get => _lastErrorString;
            set => _lastErrorString = value;
        }

        public DateTime Now(enumTimQueryStatus timQueryStatus) => hp_Timer.Now;


        #endregion
    }

}
