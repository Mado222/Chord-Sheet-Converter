﻿using BMTCommunication;
using BMTCommunicationLib;
using MathNet.Numerics;
using System.ComponentModel;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        #region RS232ReceiverThread
        // 
        /// <summary>
        /// Thread that handles RS232 communication (receiving, sending)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">DoWorkEventArgs</param>
        /// <remarks>Main work loop of the class.</remarks>
        void RS232ReceiverThread_DoWork(object sender, DoWorkEventArgs e)
        {
            // Set thread name if it has not been set
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232ReceiverThread";

            DateTime now;
            ISerialPort thrSeriell32 = Seriell32;

            // Initialize RS232 FIFO buffer to hold incoming bytes
            CFifoBuffer<byte> rs232InBytes = new();

            // Start the Data Distributor Thread
            RS232DataDistributorThread = new BackgroundWorker();
            RS232DataDistributorThread.DoWork += new DoWorkEventHandler(RS232DataDistributorThread_DoWork);
            RS232DataDistributorThread.WorkerSupportsCancellation = true;
            RS232DataDistributorThread.RunWorkerAsync();

            // Method to read data from RS232 and add to FIFO buffer
            void FillRS232()
            {
                int bytesToRead = thrSeriell32.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = thrSeriell32.Read(ref buffer, 0, bytesToRead);

                    if (bytesRead > 0)
                    {
                        rs232InBytes.Push(buffer);
                    }
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
                    Thread.Sleep(2); // Short sleep to avoid busy waiting
                    attempts--;
                }
                while (attempts > 0);

                return false;
            }

            try
            {
                // Main loop to handle incoming and outgoing data
                while (RS232ReceiverThread is not null && !RS232ReceiverThread.CancellationPending
                    && thrSeriell32 is not null)
                {
                    if (!EnableDataReceiving)
                    {
                        Thread.Sleep(20); // If data receiving is disabled, sleep to prevent high CPU usage
                        continue;
                    }

                    #region Send Data
                    // Handle outgoing data
                    byte[]? bout = RPDataOut.Pop();
                    if (bout is not null && bout.Length > 0)
                    {
                        if (!thrSeriell32.IsOpen)
                        {
                            RS232ReceiverThread.CancelAsync(); // Cancel if the serial port is closed
                        }

                        try
                        {
                            thrSeriell32.Write(bout, 0, bout.Length);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the error related to data transmission failure
#if DEBUG
                            Debug.WriteLine($"Data send error: {ex.Message}");
#endif
                        }
                        finally
                        {
                            DataSent = true; // Indicate that the data was sent
                        }
                    }
                    #endregion

                    // Send "alive" signal to the device periodically
                    now = thrSeriell32.Now(enumTimQueryStatus.no_Special);

                    if (now > NextAliveSignalToSend)
                    {
                        if (SendKeepAlive)
                        {
                            thrSeriell32.Write(AliveSequToSend, 0, AliveSequToSend.Length);
                            NextAliveSignalToSend = now + AliveSignalToSendInterv;
                        }
                    }

                    // Fill buffer with incoming bytes
                    FillRS232();

                    // If the buffer has fewer than 4 bytes, sleep briefly to wait for more data
                    if (rs232InBytes.Count < 4)
                    {
                        Thread.Sleep(10);
                    }

                    // Prepare to decode incoming data
                    CDataIn dataIn = new();
                    bool dataIsValid = false;

                    // Check if we have enough bytes to decode
                    byte[]? precheckedBuffer = PrecheckBuffer(ref rs232InBytes);
                    if (precheckedBuffer is not null)
                    {
                        // Decode the packet and mark the data as valid
                        CInsightDataEnDecoder.DecodePacket(precheckedBuffer, ref dataIn);
                        dataIsValid = true;

                        // Process command if it matches the expected hardware channel number
                        if (dataIn.HW_cn == _CommandChannelNo)
                        {
                            dataIsValid = false;

                            // Allocate buffer for the expected data length including CRC
                            byte[] buf = new byte[dataIn.Value];

                            // Wait for the remaining command bytes to arrive
                            if (CheckRS232InBytesCount(buf.Length))
                            {
                                buf = rs232InBytes.Pop(buf.Length);

                                // Check CRC of the received buffer
                                if (buf.Length > 0 && CRC8.Check_CRC8(ref buf, 0, true, true))
                                {
                                    // Process different command codes
                                    switch (buf[0])
                                    {
                                        case C8KanalReceiverCommandCodes.cChannelSync:
                                            {
                                                LastSyncSignal = thrSeriell32.Now(enumTimQueryStatus.isSync);
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
                                                RPCommand.Push(buf); // Store command for further processing
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    // CRC check failed, handle accordingly
#if DEBUG
                                    Debug.WriteLine("CRC check failed for received buffer.");
#endif
                                }
                            }
                        }
                    }

                    // If data is valid, copy it to the output collection
                    if (dataIsValid)
                    {
                        if (EnableDataReadyEvent)
                        {
                            dataIn.LastSync = LastSyncSignal;
                            dataIn.Received_at = thrSeriell32.Now(enumTimQueryStatus.isSync);
                            Data.Push(dataIn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions that occur during thread operation
#if DEBUG
                Debug.WriteLine("RS232ReceiverThread_DoWork Exception: " + ex.Message);
#endif
            }
            finally
            {
                // Cleanup on thread exit
                IsConnected = false;
                if (RS232DataDistributorThread is not null)
                {
                    RS232DataDistributorThread.CancelAsync();
                    ConnectionStatus = enumConnectionStatus.Dis_Connected;
                }

#if DEBUG
                Debug.WriteLine("RS232ReceiverThread_DoWork Closed");
#endif
            }
        }



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

        private void Stop_RS232ReceiverThread()
        {
            if (RS232ReceiverThread != null)
            {
                if (RS232ReceiverThread.IsBusy)
                {
                    RS232ReceiverThread.CancelAsync();
                    //Warten bis thread fertig, aber nicht länger als 3 Sekunden
                    DateTime EndWait = DateTime.Now + new TimeSpan(0, 0, 0, 3, 0);
                    while (RS232ReceiverThread.IsBusy && (DateTime.Now < EndWait)) ;  //Warten bis thread fertig
                }
            }
        }
        #endregion}
    }
}
