using BMTCommunicationLib;
using System.ComponentModel;
using WindControlLib;
using System.Diagnostics;
using System.Buffers;

namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        /// <summary>
        /// RS232 receiver thread, started in TryToConnect
        /// </summary>
        //private BackgroundWorker? RS232ReceiverThread;


        #region RS232ReceiverThread
        // 
        /// <summary>
        /// Thread that handles RS232 communication (receiving, sending)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DoWorkEventArgs</param>
        /// <remarks>Main work loop of the class.</remarks>
        //private void RS232ReceiverThread_DoWork(object sender, DoWorkEventArgs e)
        private async Task RS232ReceiverThread_DoWorkAsync(CancellationToken cancellationToken)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232ReceiverThread";

            if (Seriell32 is null)
            {
                throw new InvalidOperationException("The serial connection is not set.");
            }
            ISerialPort thrSeriell32 = Seriell32;

            CFifoBuffer<byte> rs232InBytes = new();

            // Method to read data from RS232 and add to FIFO buffer
            void FillRS232()
            {
                int bytesToRead = thrSeriell32.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(bytesToRead);
                    int bytesRead = thrSeriell32.Read(ref buffer, 0, bytesToRead);

                    if (bytesRead > 0)
                    {
                        rs232InBytes.Push(buffer.Take(bytesRead).ToArray());
                    }

                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            // Method to check if there are enough bytes in the buffer and fill if necessary
            bool CheckRS232InBytesCount(int numBytesRequired)
            {
                int attempts = 5;
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

            try
            {
                while (!cancellationToken.IsCancellationRequested && thrSeriell32 is not null)
                {
                    if (!EnableDataReceiving)
                    {
                        await Task.Delay(20, cancellationToken); // Sleep to prevent high CPU usage
                        continue;
                    }

                    #region Send Data
                    byte[]? bout = RPDataOut.Pop();
                    if (bout is not null && bout.Length > 0)
                    {
                        if (!thrSeriell32.IsOpen)
                        {
                            throw new InvalidOperationException("Serial port closed unexpectedly.");
                        }

                        try
                        {
                            await thrSeriell32.WriteAsync(bout, 0, bout.Length, cancellationToken);
                            DataSent = true;
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Debug.WriteLine($"Data send error: {ex.Message}");
#endif
                        }
                    }

                    DateTime now = thrSeriell32.Now(EnumTimQueryStatus.no_Special);

                    if (now > NextAliveSignalToSend && SendKeepAlive)
                    {
                        await thrSeriell32.WriteAsync(AliveSequToSend, 0, AliveSequToSend.Length, cancellationToken);
                        NextAliveSignalToSend = now + AliveSignalToSendInterv;
                    }
                    #endregion

                    FillRS232();

                    if (rs232InBytes.Count < 4)
                    {
                        await Task.Delay(10, cancellationToken);
                        continue;
                    }

                    CDataIn dataIn = new();
                    bool dataIsValid = false;

                    byte[]? precheckedBuffer = PrecheckBuffer(ref rs232InBytes);
                    if (precheckedBuffer is not null)
                    {
                        CInsightDataEnDecoder.DecodePacket(precheckedBuffer, ref dataIn);
                        dataIsValid = true;

                        if (dataIn.HW_cn == _CommandChannelNo)
                        {
                            dataIsValid = false;
                            byte[] buf = new byte[dataIn.Value];

                            if (CheckRS232InBytesCount(buf.Length))
                            {
                                buf = rs232InBytes.Pop(buf.Length) ?? [];

                                if (buf.Length > 0 && CRC8.Check_CRC8(ref buf, 0, true, true))
                                {
                                    switch (buf[0])
                                    {
                                        case C8KanalReceiverCommandCodes.cChannelSync:
                                            LastSyncSignal = thrSeriell32.Now(EnumTimQueryStatus.isSync);
                                            break;

                                        case C8KanalReceiverCommandCodes.cNeuromasterToPC:
                                            RPDeviceCommunicationToPC.Push(buf);
                                            break;

                                        default:
                                            RPCommand.Push(buf);
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
                    }

                    if (dataIsValid && EnableDataReadyEvent)
                    {
                        dataIn.LastSync = LastSyncSignal;
                        dataIn.Received_at = thrSeriell32.Now(EnumTimQueryStatus.isSync);
                        Data.Push(dataIn);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("RS232ReceiverThread_DoWorkAsync Exception: " + ex.Message);
#endif
            }
            finally
            {
                IsConnected = false;
                ConnectionStatus = EnumConnectionStatus.Dis_Connected;

#if DEBUG
                Debug.WriteLine("RS232ReceiverThread_DoWorkAsync Closed");
#endif
            }
        }

        private CancellationTokenSource? cancellationTokenReceiver;

        public void Start_RS232ReceiverThread()
        {
            cancellationTokenReceiver = new CancellationTokenSource();
            Task.Run(() => RS232ReceiverThread_DoWorkAsync(cancellationTokenReceiver.Token));
        }

        public void Stop_RS232ReceiverThread()
        {
            cancellationTokenReceiver?.Cancel();
        }
        #endregion

        private int mustbeinbuf = -1;
        private int issync = 0;
        private int isEP = 0;
        private byte[]? PrecheckBuffer(ref CFifoBuffer<byte> RS232inBytes)
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

    }
}
