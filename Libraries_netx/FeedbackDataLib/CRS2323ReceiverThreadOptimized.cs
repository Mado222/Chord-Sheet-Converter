using BMTCommunicationLib;
using Microsoft.VisualBasic.ApplicationServices;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;
using WindControlLib;

namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        private readonly CFifoConcurrentQueue<CDataIn> _measurementDataQueue = new();
        private readonly CFifoConcurrentQueue<byte> _commandResponseQueue = new();
        private readonly CFifoConcurrentQueue<byte> RPDeviceCommunicationToPC = new();
        private CFifoConcurrentQueue<byte> rs232InBytes = new();
        private readonly CFifoConcurrentQueue<byte[]> RPDataOut = new();
        private bool DataSent = false;

        private TaskCompletionSource<byte[]>? _responseTcs;

        // Declare cancellationTokenSourceReceiver as a private property
        private CancellationTokenSource? cancellationTokenSourceReceiver { get; set; }

        private async Task RS232ReceiverThread_DoWorkAsync()
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232ReceiverThread";

            DateTime Now;
            var cancellationToken = cancellationTokenSourceReceiver?.Token ?? CancellationToken.None;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!EnableDataReceiving)
                    {
                        await Task.Delay(20, cancellationToken); // Wait if data receiving is disabled
                        continue;
                    }

                    // Handle outgoing data
                    byte[]? bout = RPDataOut.Pop();
                    if (bout is not null && bout.Length > 0)
                    {
                        if (!await SendAsync(bout)) // Replace with actual sending logic
                        {
                           
                        }
                        else
                        {
                            DataSent = true; // Indicate that data was sent
                        }
                    }

                    Now = DateTime.Now;
                    // Send "alive" signal periodically
                    if (SendKeepAlive && Now > NextAliveSignalToSend)
                    {
                        await SendAsync(AliveSequToSend);
                        NextAliveSignalToSend = Now + AliveSignalToSendInterv;
                    }

                    // Receive and categorize incoming data
                    
                    FillRS232();
                    
                    if (rs232InBytes.Count >= 4)
                    {
                        ProcessIncomingData();
                    }

                    // Brief delay to avoid high CPU usage if no data is pending
                    if (rs232InBytes.Count < 4)
                    {
                        await Task.Delay(10, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RS232ReceiverThread Error: " + ex.Message);
            }
        }

        // Method to read data from RS232 and add to FIFO buffer
        private void FillRS232()
        {
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
        }

        // Method to check if there are enough bytes in the buffer and fill if necessary
        private bool CheckRS232InBytesCount(int numBytesRequired)
        {
            int attempts = 5;
            var cancellationToken = cancellationTokenSourceReceiver?.Token ?? CancellationToken.None;
            do
            {
                if (rs232InBytes.Count >= numBytesRequired)
                    return true;
                FillRS232();
                Task.Delay(2, cancellationToken).Wait(cancellationToken); // Short delay to avoid busy waiting
                attempts--;
            }
            while (attempts > 0);

            return false;
        }

        private void ProcessIncomingData()
        {
            byte[]? precheckedBuffer = PrecheckBuffer(ref rs232InBytes);

            // Proceed only if precheckedBuffer contains a valid packet
            if (precheckedBuffer is not null)
            {
                CDataIn dataIn = new();
                CInsightDataEnDecoder.DecodePacket(precheckedBuffer, ref dataIn);

                // Check if dataIn is valid and if it's on the command channel
                if (dataIn.HW_cn == _CommandChannelNo)
                {
                    // This packet is on the command channel, so treat it as a response or command
                    byte[] buf = new byte[dataIn.Value];

                    // Wait until enough bytes are available for the command packet
                    if (CheckRS232InBytesCount(buf.Length))
                    {
                        // Pop the required bytes for this packet
                        buf = rs232InBytes.Pop(buf.Length) ?? [];

                        // Validate CRC to ensure data integrity
                        if (buf.Length > 0 && CRC8.Check_CRC8(ref buf, 0, true, true))
                        {
                            Debug.WriteLine("Valid data in: " + BitConverter.ToString(buf).Replace("-", " "));
                            // Process based on the command code (buf[0])
                            switch (buf[0])
                            {
                                case C8KanalReceiverCommandCodes.cChannelSync:
                                    LastSyncSignal = Seriell32.Now(EnumTimQueryStatus.isSync);
                                    //Debug.WriteLine("cChannelSync");
                                    break;
                                case C8KanalReceiverCommandCodes.cDeviceAlive:
                                    //Debug.WriteLine("Alive");
                                    break;
                                case C8KanalReceiverCommandCodes.cNeuromasterToPC:
                                    //Debug.WriteLine("cNeuromasterToPC: " + BitConverter.ToString(buf).Replace("-", " "));
                                    RPDeviceCommunicationToPC.Push(buf);
                                    break;
                                default:
                                    // Enqueue command response in RPCommand
                                    // If we’re waiting for a response, fulfill the TaskCompletionSource
                                    if (_responseTcs != null && !_responseTcs.Task.IsCompleted)
                                    {
                                        //Debug.WriteLine("_responseTcs: " + BitConverter.ToString(buf).Replace("-", " "));
                                        _responseTcs.TrySetResult(buf);
                                    }
                                    break;
                            }
                        }
                        else
                        {
#if DEBUG
                            Debug.WriteLine("CRC check failed for received buffer.");
#endif
                        }
                    }
                }
                else
                {
                    // This is continuous measurement data (not on the command channel)
                    // Process and enqueue as measurement data if valid
                    if (EnableDataReadyEvent)
                    {
                        dataIn.LastSync = LastSyncSignal;
                        dataIn.Received_at = Seriell32.Now(EnumTimQueryStatus.isSync);
                        _measurementDataQueue.Push(dataIn);
                    }
                }
            }
        }

        private int mustbeinbuf = -1;
        private int issync = 0;
        private int isEP = 0;
        private byte[]? PrecheckBuffer(ref CFifoConcurrentQueue<byte> RS232inBytes)
        {
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
            }
            else
            {
                RS232inBytes.Pop();
            }
            return null;
        }

        private async Task<bool> SendAsync(byte[] data)
        {
            try
            {
                Debug.WriteLine("Data out: " + BitConverter.ToString(data).Replace("-", " "));
                await Seriell32.WriteAsync(data, 0, data.Length, CancellationToken.None); // Adjust cancellation token as needed
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Data send error: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> SendCommandAsync(byte[] command, TimeSpan? timeout = null)
        {
            TimeSpan ts = timeout ?? DataReceiverTimeout;

            _responseTcs = new TaskCompletionSource<byte[]>();

            // Send the command
            if (!await SendAsync(command))
            {
                throw new InvalidOperationException("Failed to send the command.");
            }

            using CancellationTokenSource cts = new(ts);
            cts.Token.Register(() => _responseTcs.TrySetCanceled());

            try
            {
                return await _responseTcs.Task; // Await response asynchronously
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Response timed out.");
            }
        }

        // Method to start the RS232ReceiverThread
        public void StartRS232ReceiverThread()
        {
            cancellationTokenSourceReceiver = new CancellationTokenSource();
            Task.Run(RS232ReceiverThread_DoWorkAsync);
        }

        // Method to stop the RS232ReceiverThread
        public void StopRS232ReceiverThread()
        {
            if (cancellationTokenSourceReceiver != null)
            {
                cancellationTokenSourceReceiver.Cancel();
                cancellationTokenSourceReceiver.Dispose();
                cancellationTokenSourceReceiver = null;
            }
        }
    }
}