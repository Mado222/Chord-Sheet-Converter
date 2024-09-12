using System;
using System.IO.Ports;
using WindControlLib;

namespace Com_Com_Bridge
{

    public class CSerialPortBridge
    {
        private readonly SerialPort _serialPortIn;
        private readonly SerialPort _serialPortOut;
        private Thread? _port1ToPort2Thread;
        private Thread? _port2ToPort1Thread;

        // Event to report status changes and error messages
        public event EventHandler<string>? StatusReported;

        // Event to report sent and received data
        public event EventHandler<DataEventArgs>? DataReported;

        public CSerialPortBridge(string portNameIn, int baudRaten, string portNameOut, int baudRateOut)
        {
            // Initialize the two serial ports
            _serialPortIn = new SerialPort(portNameIn, baudRaten);
            _serialPortOut = new SerialPort(portNameOut, baudRateOut);

            // Set the data received event handler for both ports
            _serialPortIn.DataReceived += SerialPortIn_DataReceived;
            _serialPortOut.DataReceived += SerialPortOut_DataReceived;
        }

        public void Start()
        {
            try
            {
                if (!_serialPortIn.IsOpen)
                {
                    _serialPortIn.Open();
                    OnStatusReported($"Serial port {_serialPortIn.PortName} opened.");
                }

                if (!_serialPortOut.IsOpen)
                {
                    _serialPortOut.Open();
                    OnStatusReported($"Serial port {_serialPortOut.PortName} opened.");
                }

                // Start threads to handle data transfer
                _port1ToPort2Thread = new Thread(Port1ToPort2);
                _port2ToPort1Thread = new Thread(Port2ToPort1);
                _port1ToPort2Thread.Start();
                _port2ToPort1Thread.Start();
            }
            catch (Exception ex)
            {
                OnStatusReported($"Error starting serial ports: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_serialPortIn.IsOpen)
            {
                _serialPortIn.Close();
                OnStatusReported($"Serial port {_serialPortIn.PortName} closed.");
            }

            if (_serialPortOut.IsOpen)
            {
                _serialPortOut.Close();
                OnStatusReported($"Serial port {_serialPortOut.PortName} closed.");
            }
        }

        private void Port1ToPort2()
        {
            while (_serialPortIn.IsOpen && _serialPortOut.IsOpen)
            {
                try
                {
                    
                    string dataFromPort1 = _serialPortIn.ReadExisting();
                    if (dataFromPort1 != "")
                    {
                        byte[] buf = CASCII_Text_Array.StringToByteArray(dataFromPort1);
                        if (!string.IsNullOrEmpty(dataFromPort1))
                        {
                            _serialPortOut.Write(dataFromPort1);
                            OnDataReported(_serialPortIn.PortName, _serialPortOut.PortName, CASCII_Text_Array.ByteArray_To_Ascii(buf));
                        }
                    }

                }
                catch (Exception ex)
                {
                    OnStatusReported($"Error transferring data from {_serialPortIn.PortName} to {_serialPortOut.PortName}: {ex.Message}");
                }
            }
        }

        private void Port2ToPort1()
        {
            while (_serialPortIn.IsOpen && _serialPortOut.IsOpen)
            {
                try
                {
                    string dataFromPort2 = _serialPortOut.ReadExisting();
                    if (dataFromPort2 != "")
                    {
                        byte[] buf = CASCII_Text_Array.StringToByteArray(dataFromPort2);
                        if (!string.IsNullOrEmpty(dataFromPort2))
                        {
                            _serialPortIn.Write(dataFromPort2);
                            OnDataReported(_serialPortOut.PortName, _serialPortIn.PortName, CASCII_Text_Array.ByteArray_To_Ascii(buf));
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnStatusReported($"Error transferring data from {_serialPortOut.PortName} to {_serialPortIn.PortName}: {ex.Message}");
                }
            }
        }

        private void SerialPortIn_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Optionally handle data received from port 1
        }

        private void SerialPortOut_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Optionally handle data received from port 2
        }

        // Trigger the status event
        protected virtual void OnStatusReported(string message)
        {
            StatusReported?.Invoke(this, message);
        }

        // Trigger the data event
        protected virtual void OnDataReported(string fromPort, string toPort, string data)
        {
            DataReported?.Invoke(this, new DataEventArgs(fromPort, toPort, data));
        }
    }

    // Custom event args for data reporting
    public class DataEventArgs : EventArgs
    {
        public string FromPort { get; }
        public string ToPort { get; }
        public string Data { get; }

        public DataEventArgs(string fromPort, string toPort, string data)
        {
            FromPort = fromPort;
            ToPort = toPort;
            Data = data;
        }

        public override string ToString()
        {
            return $"Data sent from {FromPort} to {ToPort}: {Data}";
        }
    }
}
