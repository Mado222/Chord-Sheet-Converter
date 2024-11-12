using BMTCommunicationLib;
using System.Buffers;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{
    public class CRS232Receiver : IDisposable
    {
        private readonly CFifoConcurrentQueue<CDataIn> _measurementDataQueue = new();
        private CFifoConcurrentQueue<byte[]> commandResponseQueue = new();
        private readonly CFifoConcurrentQueue<byte[]> _commandToPCQueue = new();


        private CFifoConcurrentQueue<byte> rs232InBytes = new();
        private CDataIn? dataInTemp;
        private int numCommandData = 0;

        private readonly TimeSpan DataReceiverTimeout = new(0, 0, 0, 2, 0);
        private byte _CommandChannelNo = 0; //Wird nur im Konstruktor übereschrieben

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        public CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);


        /// <summary>
        /// Last Sync Signal received from Device = when Device timer has full second
        /// comes via command channel
        /// </summary>
        private DateTime LastSyncSignal = DateTime.MinValue;

        private ISerialPort? seriell32;
        /// <summary>
        /// Holds the Serial Interface
        /// </summary>
        public ISerialPort Seriell32
        {
            get
            {
                if (seriell32 == null)
                {
                    //throw new InvalidOperationException("The serial connection is not set.");
                    Debug.WriteLine("The serial connection is not set.");
                }
                return seriell32;
            }
            set => seriell32 = value;
        }


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

        enum ReceiverState
        {
            None,
            WaitingForCommandData
        }

        private ReceiverState _receiverState = ReceiverState.None;

        // Declare cancellationTokenSourceReceiver as a private property
        private CancellationTokenSource? CancellationTokenSourceReceiver { get; set; }
        public CFifoConcurrentQueue<byte[]> CommandResponseQueue { get => commandResponseQueue; set => commandResponseQueue = value; }
        public CFifoConcurrentQueue<CDataIn> MeasurementDataQueue => _measurementDataQueue;

        public CFifoConcurrentQueue<byte[]> CommandToPCQueue => _commandToPCQueue;

        // Reinitialize the BufferBlock to clear it
        public void ClearCommandResponseQueue() => CommandResponseQueue = new();

        /// <summary>
        /// CRS232Receiver constructor
        /// </summary>
        /// <param name="CommandChannelNo">Command channel no</param>
        /// <param name="SerialPort">Related serial Port</param>
        public CRS232Receiver(byte CommandChannelNo, ISerialPort SerialPort)
        {
            _CommandChannelNo = CommandChannelNo;
            Seriell32 = SerialPort;
        }

        private async Task RS232ReceiverThread_DoWorkAsync()
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232ReceiverThread";

            var cancellationToken = CancellationTokenSourceReceiver?.Token ?? CancellationToken.None;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!EnableDataReceiving)
                    {
                        await Task.Delay(20, cancellationToken); // Wait if data receiving is disabled
                        continue;
                    }

                    // Receive and buffer incoming data
                    int bytesToRead = Seriell32.BytesToRead;
                    if (bytesToRead > 0)
                    {
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(bytesToRead);
                        int bytesRead = Seriell32.Read(ref buffer, 0, bytesToRead);

                        if (bytesRead > 0)
                        {
                            rs232InBytes.Push(buffer.Take(bytesRead).ToArray());
                        }

                        ArrayPool<byte>.Shared.Return(buffer);
                    }

                    //Receiver state machine
                    switch (_receiverState)
                    {
                        case ReceiverState.None:
                            // Process incoming data if available
                            if (rs232InBytes.Count >= 4)
                            {
                                // Prepare to decode incoming data
                                // Check if we have enough bytes to decode
                                byte[]? precheckedBuffer = PrecheckBuffer(ref rs232InBytes); //Enough bytes to decode one CData in 

                                if (precheckedBuffer is not null)
                                {
                                    // Decode the packet and mark the data as valid
                                    if (CInsightDataEnDecoder.DecodePacket(precheckedBuffer) is CDataIn di)
                                    {
                                        dataInTemp = di;
                                        // Process command if it matches _CommandChannelNo
                                        if (dataInTemp.HWcn == _CommandChannelNo)
                                        {
                                            _receiverState = ReceiverState.WaitingForCommandData;
                                            // Allocate buffer for the expected data length including CRC
                                            numCommandData = dataInTemp.Value;
                                        }
                                        else
                                        {
                                            if (EnableDataReadyEvent)
                                            {
                                                //Store Measurement Data
                                                dataInTemp.LastSync = LastSyncSignal;
                                                dataInTemp.ReceivedAt = Seriell32.Now(EnumTimQueryStatus.isSync);
                                                MeasurementDataQueue.Push(dataInTemp);
                                            }
                                            _receiverState = ReceiverState.None;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                await Task.Delay(10, cancellationToken);
                            }
                            break;

                        case ReceiverState.WaitingForCommandData:
                            if (rs232InBytes.Count >= numCommandData)
                            {
                                byte[]? buf = rs232InBytes.Pop(numCommandData); //ArrayPool<byte>.Shared.Rent(numCommandData);
                                //rs232InBytes.Pop(ref buf);
                                _receiverState |= ReceiverState.None;
                                if (buf != null && buf.Length > 0)
                                {
                                    // Check CRC of the received buffer
                                    if (buf.Length > 0 && CRC8.Check_CRC8(ref buf, 0, true, true))
                                    {
                                        // Process different command codes
                                        switch (buf[0])
                                        {
                                            case C8CommBase.cChannelSync:
                                                {
                                                    LastSyncSignal = Seriell32.Now(EnumTimQueryStatus.isSync);
                                                    break;
                                                }
                                            case C8CommBase.cDeviceAlive:
                                                {
                                                    break;
                                                }
                                            case C8CommBase.cNeuromasterToPC:
                                                {
                                                    CommandToPCQueue.Push(buf);
                                                    break;
                                                }
                                            default:
                                                {
                                                    Debug.WriteLine("default: " + BitConverter.ToString(buf).Replace("-", " "));
                                                    CommandResponseQueue.Push(buf); // Store command for further processing
                                                    break;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        // CRC check failed, handle accordingly
                                        Debug.WriteLine("CRC check failed for received buffer.");
                                    }
                                    _receiverState = ReceiverState.None;
                                    //ArrayPool<byte>.Shared.Return(buf);
                                }
                            }
                            else
                            {
                                await Task.Delay(10, cancellationToken);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e}");
            }
            finally
            {
                //StopRS232DistributorThread();
            }
        }

        private static byte[]? PrecheckBuffer(ref CFifoConcurrentQueue<byte> RS232inBytes)
        {
            int mustbeinbuf = -1;
            int issync = 0;
            int isEP = 0;

            if (RS232inBytes.Count >= 1)
            {
                if ((RS232inBytes.Peek() & 0x80) == 0)
                {
                    if (RS232inBytes.Count >= 4)
                    {
                        //base packet
                        issync = (byte)((RS232inBytes.Peek() >> 6) & 0x01);
                        isEP = (byte)((RS232inBytes.Peek(1) >> 5) & 0x1);
                        if ((issync == 1) && (isEP == 1))
                        {
                            mustbeinbuf = 7; //24bit sync
                        }
                        else if (issync == 1)
                        {
                            mustbeinbuf = 5; //16 bit sync
                        }
                        else if (isEP == 1)
                        {
                            mustbeinbuf = 6;  //24 bit
                        }
                        else
                        {
                            mustbeinbuf = 4; //16 bit
                        }
                    }

                    if (RS232inBytes.Count >= mustbeinbuf)
                    {
                        if (isEP == 1)
                        {
                            int numExtradata = (RS232inBytes.Peek(mustbeinbuf - 2) >> 4) & 0x07;
                            if (numExtradata > 0)
                                mustbeinbuf += numExtradata;
                        }
                        if (RS232inBytes.Count >= mustbeinbuf)
                        {
                            return RS232inBytes.Pop(mustbeinbuf);
                        }
                        return null;
                    }
                }

                else
                {
                    RS232inBytes.Pop();
                }
            }
            return null;
        }

        // Method to start the RS232ReceiverThread
        public void StartRS232ReceiverThread()
        {
            CancellationTokenSourceReceiver = new CancellationTokenSource();
            Task.Run(RS232ReceiverThread_DoWorkAsync);
        }

        // Method to stop the RS232ReceiverThread
        public void StopRS232ReceiverThread()
        {
            if (CancellationTokenSourceReceiver != null)
            {
                CancellationTokenSourceReceiver.Cancel();
                CancellationTokenSourceReceiver.Dispose();
                CancellationTokenSourceReceiver = null;
            }
        }

        /// <summary>
        /// Closes tryToConnectWorker thread or RS232ReceiverThread
        /// </summary>
        private void CloseAll()
        {
            if (Seriell32 != null)
            {
                StopRS232ReceiverThread();
                Seriell32.Close();  //1st Close, 4th close
            }
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

        public int ReceiverTimerInterval => 0;

        public bool EnableDataReadyEvent { set; get; } = false;

        //Die dieses Interface implementierende Komponente empfängt keine Daten!
        public bool EnableDataReceiving { set; get; } = true;

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
