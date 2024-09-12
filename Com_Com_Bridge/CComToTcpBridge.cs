using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ComToTcpBridge
{
    private SerialPort _comPort;
    private TcpListener _tcpServer;
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private Thread _comToTcpThread;
    private Thread _tcpToComThread;

    // Define an event that reports status and data
    public event EventHandler<string> StatusReported;

    public ComToTcpBridge(string comPortName, int baudRate, int tcpPort)
    {
        // Initialize the COM port
        _comPort = new SerialPort(comPortName, baudRate);
        _comPort.DataReceived += ComPort_DataReceived;

        // Initialize the TCP server
        _tcpServer = new TcpListener(IPAddress.Any, tcpPort);
    }

    public void Start()
    {
        // Open COM port
        if (!_comPort.IsOpen)
        {
            _comPort.Open();
            OnStatusReported($"COM port {_comPort.PortName} opened.");
        }

        // Start TCP server
        _tcpServer.Start();
        OnStatusReported("TCP Server started. Waiting for connection...");

        // Accept a single client connection
        _tcpClient = _tcpServer.AcceptTcpClient();
        _networkStream = _tcpClient.GetStream();
        OnStatusReported("Client connected.");

        // Start threads to transfer data in both directions
        _comToTcpThread = new Thread(ComToTcp);
        _tcpToComThread = new Thread(TcpToCom);
        _comToTcpThread.Start();
        _tcpToComThread.Start();
    }

    private void ComToTcp()
    {
        while (_comPort.IsOpen && _tcpClient.Connected)
        {
            try
            {
                string comData = _comPort.ReadExisting();
                if (!string.IsNullOrEmpty(comData))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(comData);
                    _networkStream.Write(buffer, 0, buffer.Length);
                    OnStatusReported($"COM to TCP: {comData}");
                }
            }
            catch (Exception ex)
            {
                OnStatusReported($"Error COM to TCP: {ex.Message}");
            }
        }
    }

    private void TcpToCom()
    {
        byte[] buffer = new byte[1024];
        while (_comPort.IsOpen && _tcpClient.Connected)
        {
            try
            {
                int bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string tcpData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    _comPort.Write(tcpData);
                    OnStatusReported($"TCP to COM: {tcpData}");
                }
            }
            catch (Exception ex)
            {
                OnStatusReported($"Error TCP to COM: {ex.Message}");
            }
        }
    }

    private void ComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        // Handle any data received on the COM port if needed
    }

    public void Stop()
    {
        // Close the COM port and TCP connection
        if (_comPort.IsOpen)
        {
            _comPort.Close();
            OnStatusReported($"COM port {_comPort.PortName} closed.");
        }

        if (_tcpClient != null)
        {
            _networkStream.Close();
            _tcpClient.Close();
            OnStatusReported("TCP connection closed.");
        }

        _tcpServer.Stop();
        OnStatusReported("TCP server stopped.");
    }

    // Method to trigger the event
    protected virtual void OnStatusReported(string message)
    {
        StatusReported?.Invoke(this, message);
    }
}
