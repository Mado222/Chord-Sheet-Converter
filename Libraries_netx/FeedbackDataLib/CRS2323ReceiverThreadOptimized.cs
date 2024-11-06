using BMTCommunicationLib;
using System.Buffers;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        private readonly CFifoConcurrentQueue<CDataIn> _measurementDataQueue = new();
        private CFifoConcurrentQueue<byte[]> _commandResponseQueue = new();
        private readonly CFifoConcurrentQueue<byte[]> RPDeviceCommunicationToPC = new();
        private readonly CFifoConcurrentQueue<byte[]> _sendingQueue = new();
        private CFifoConcurrentQueue<byte> rs232InBytes = new();
        private CDataIn? dataInTemp;
        private int numCommandData = 0;

        enum ReceiverState
        {
            None,
            WaitingForCommandData
        }

        private ReceiverState _receiverState = ReceiverState.None;

        // Declare cancellationTokenSourceReceiver as a private property
        private CancellationTokenSource? CancellationTokenSourceReceiver { get; set; }

        // Reinitialize the BufferBlock to clear it
        public void ClearCommandResponseQueue() => _commandResponseQueue = new();

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
                                                _measurementDataQueue.Push(dataInTemp);
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
                                            case C8KanalReceiverCommandCodes.cChannelSync:
                                                {
                                                    LastSyncSignal = Seriell32.Now(EnumTimQueryStatus.isSync);
                                                    break;
                                                }
                                            case C8KanalReceiverCommandCodes.cDeviceAlive:
                                                {
                                                    break;
                                                }
                                            case C8KanalReceiverCommandCodes.cNeuromasterToPC:
                                                {
                                                    RPDeviceCommunicationToPC.Push(buf);
                                                    break;
                                                }
                                            default:
                                                {
                                                    Debug.WriteLine("default: " + BitConverter.ToString(buf).Replace("-", " "));
                                                    _commandResponseQueue.Push(buf); // Store command for further processing
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
                StopRS232DistributorThread();
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
    }
}