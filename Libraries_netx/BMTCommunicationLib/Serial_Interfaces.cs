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
        bool GetOpen();

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

        // Async method
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        event SerialDataReceivedEventHandler SerialDataReceivedEvent;
        string LastErrorString { get; set; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }
        bool DsrHolding { get; }
        DateTime Now(EnumTimQueryStatus TimQueryStatus);
    }
    public enum EnumTimQueryStatus : int
    {
        no_Special,
        isSync
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
            get => SerialPort != null ? SerialPort.PortName : "";
            set
            {
                if (value != null && value != "")
                {
                    SerialPort.PortName = value;
                }
            }
        }

        public int BaudRate
        {
            get => SerialPort.BaudRate;
            set => SerialPort.BaudRate = value;
        }

        public bool IsOpen
        {
            get => SerialPort.IsOpen;
        }
        public void Close()
        {
            if (SerialPort == null) return;

            try
            {
                lock (RS232SettingsLock)
                {
                    SerialPort.Close();
                }
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
            }
        }

        public virtual bool GetOpen()
        {
            if (SerialPort == null) return false;

            try
            {
                lock (RS232SettingsLock)
                {
                    SerialPort.Open();
                }
            }
            catch (Exception ex)
            {
                LastErrorString = ex.Message;
            }

            return SerialPort?.IsOpen ?? false;
        }

        public int ReadTimeout
        {
            get => SerialPort.ReadTimeout;
            set => SerialPort.ReadTimeout = value;
        }

        public int WriteTimeout
        {
            get => SerialPort.WriteTimeout;
            set => SerialPort.WriteTimeout = value;
        }

        public StopBits StopBits
        {
            get => SerialPort.StopBits;
            set => SerialPort.StopBits = value;
        }

        public Parity Parity
        {
            get => SerialPort.Parity;
            set => SerialPort.Parity = value;
        }

        public int DataBits
        {
            get => SerialPort.DataBits;
            set => SerialPort.DataBits = value;
        }

        public bool RtsEnable
        {
            get => SerialPort?.RtsEnable ?? false;
            set => SerialPort.RtsEnable = value;
        }
        public Handshake Handshake
        {
            get => SerialPort.Handshake;
            set
            {
                if (value == Handshake.RequestToSend && SerialPort.WriteTimeout == -1)
                {
                    SerialPort.WriteTimeout = 100; // Set default WriteTimeout if needed [ms]
                }
                SerialPort.Handshake = value;
            }
        }

        public virtual int Read(ref byte[] buffer, int offset, int count)
        {
            lock (RS232ReadLock)
            {
                // Using the other overloaded method with ReadTimeout as WaitMax_ms
                return Read(ref buffer, offset, count, SerialPort.ReadTimeout);
            }
        }

        public virtual int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            if (SerialPort == null || !SerialPort.IsOpen) return 0;

            DateTime timeout = DateTime.Now + TimeSpan.FromMilliseconds(WaitMax_ms);

            // Wait until the required number of bytes are available or timeout occurs
            while (SerialPort.IsOpen && SerialPort.BytesToRead < count && DateTime.Now < timeout)
            {
                // Uncomment if Application.DoEvents() is needed, but be cautious as it can cause reentrancy issues.
                // Application.DoEvents(); 
            }

            if (SerialPort.IsOpen && SerialPort.BytesToRead >= count)
            {
                buffer = new byte[count];
                return SerialPort.Read(buffer, offset, count);
            }

            return 0;
        }

        public void DiscardInBuffer()
        {
            if (SerialPort?.IsOpen == true)
                SerialPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            if (SerialPort?.IsOpen == true)
                SerialPort.DiscardOutBuffer();
        }

        public void Dispose()
        {
            SerialPort?.Dispose();
        }


        public virtual bool Write(byte[] buffer, int offset, int count)
        {
            if (SerialPort == null || !SerialPort.IsOpen) return false;

            try
            {
                lock (RS232WriteLock)
                {
                    SerialPort.Write(buffer, offset, count);
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
            if (SerialPort == null || !SerialPort.IsOpen) return false;

            try
            {
                lock (RS232WriteLock)
                {
                    int byteLengthMs = (int)(1.0 / SerialPort.BaudRate * 10.0 * 1000.0);
                    int waitMs = byteLengthMs + delayBetweenBytesMs;

                    for (int i = 0; i < count; i++)
                    {
                        SerialPort.Write(buffer, offset + i, 1);
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

        public virtual int BytesToRead => SerialPort?.BytesToRead ?? 0;

        public bool DtrEnable
        {
            get => SerialPort?.DtrEnable ?? false;
            set
            {
                if (SerialPort != null)
                {
                    SerialPort.DtrEnable = value;
                }
            }
        }

        public bool DsrHolding => SerialPort?.DsrHolding ?? false;

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

        public SerialPort SerialPort => SerialPort1;

        public SerialPort SerialPort1 => _SerialPort;

        public DateTime Now(EnumTimQueryStatus timQueryStatus) => hp_Timer.Now;

        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                SerialPort.Write(buffer, offset, count);
            }, cancellationToken);
        }
        #endregion
    }
}
