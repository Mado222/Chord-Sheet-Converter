using BMTCommunication;
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

            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232ReceiverThread";

            DateTime Now;
            ISerialPort ThrSeriell32 = Seriell32;

            //CFifoBuffer RS232inBytes
            CFifoBuffer<byte> RS232inBytes = new();

            //ReceiverThread starten
            RS232DataDistributorThread = new BackgroundWorker();
            RS232DataDistributorThread.DoWork += new DoWorkEventHandler(RS232DataDistributorThread_DoWork);
            RS232DataDistributorThread.WorkerSupportsCancellation = true;
            RS232DataDistributorThread.RunWorkerAsync();

            void FillRS232()
            {
                if (ThrSeriell32.BytesToRead >= 0)
                {
                    byte[] buf = new byte[ThrSeriell32.BytesToRead];
                    ThrSeriell32.Read(ref buf, 0, ThrSeriell32.BytesToRead);    //Has Timeout!!!!
                    RS232inBytes.Push(buf);
                }
            }

            bool Check_RS232inBytesCount (int num_byte_required)
            {
                int rep = 5;
                do
                {
                    if (RS232inBytes.Count >= num_byte_required)
                        return true;
                    FillRS232();
                    Thread.Sleep(2);
                }
                while (rep >0);
                return false;
            }

            try
            {
                while (RS232ReceiverThread is not null && !RS232ReceiverThread.CancellationPending
                    && ThrSeriell32 is not null)
                {
                    if (!EnableDataReceiving) Thread.Sleep(20);

                    #region Send_Data
                    byte[]? bout = RPDataOut.Pop();
                    if (bout is not null && bout.Length > 0)
                    {
                        if (!ThrSeriell32.IsOpen)
                            RS232ReceiverThread.CancelAsync();
                        try
                        {
                            //if (!ConnectionBroken)
                            ThrSeriell32.Write(bout, 0, bout.Length);
                        }
                        catch (Exception ee)
                        {
                            //MessageBox.Show("Data send error:" + ee);
                            //log.Error(ee.Message, ee);
                        }
                        finally
                        {
                            DataSent = true;
                        }
                    }
                    #endregion

                    //Alive Signal to Device
                    Now = ThrSeriell32.Now(enumTimQueryStatus.no_Special);

                    if (Now > NextAliveSignalToSend)
                    {
                        //in debug might be taken away
                        if (SendKeepAlive)
                        {
                            ThrSeriell32.Write(AliveSequToSend, 0, AliveSequToSend.Length);
                            NextAliveSignalToSend = Now + AliveSignalToSendInterv;
                        }
                    }

                    //Bytes hereinholen
                    FillRS232();
                    
                    if (RS232inBytes.Count < 4)
                    {
                        Thread.Sleep(10);
                    }

                    CDataIn dataIn = new();
                    bool dataIsValid = false;

                    //Überprüfen ob eine auswertbare Anzahl von Bytes zur Verfügung steht
                    byte[]? bt = PrecheckBuffer(ref RS232inBytes);
                    if (bt is not null)
                    {
                        CInsightDataEnDecoder.DecodePacket(bt, ref dataIn);
                        dataIsValid = true;

                        //Daten akzeptieren
                        if (dataIn.HW_cn == _CommandChannelNo)
                        {
                            dataIsValid = false;
                            //Kommando kommt herein
                            byte[] buf = new byte[dataIn.Value];       //Array fuer die kommenden bytes inkl CRC

                            //Die fehlende bytes holen
                            //Timeout brechnen
                            //double d = Seriell32.BaudRate;
                            //d = 10 * 1 / d * 1000 * buf.Length * 5;   //10bit*1/Baud*1000[ms]*Anzahl der Byte * Sicherheitsfaktor
                            //if (d < 200) d = 200;

                            //Auf hereinkommende Kommando-bytes warten
                            if (Check_RS232inBytesCount(buf.Length))
                            {
                                buf = RS232inBytes.Pop(buf.Length);

                                //CRC check
                                if (buf.Length > 0 && CRC8.Check_CRC8(ref buf, 0, true, true))    //Last Byte is CRC, remove CRC
                                {
                                    //Check for commands that must be processed here
                                    switch (buf[0])
                                    {
                                        case C8KanalReceiverCommandCodes.cChannelSync:
                                            {
                                                LastSyncSignal = ThrSeriell32.Now(enumTimQueryStatus.isSync);
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
                                                RPCommand.Push(buf); //Verspeichern
                                                break;
                                            }
                                    }
                                } //if (CRC8.Check_CRC8(ref buf, 0, true, true))
                                else
                                {
                                    //CRC failed
                                }
                            }
                            else //if (Check_RS232inBytesCount(buf.Length))
                            {
                            }
                        } //if (dataIn.HW_cn == _CommandChannelNo)
                    } //if (numByteRead == 4)

                    if (dataIsValid)
                    {
                        //Daten in Ausgangsfeld Data kopieren
                        if (EnableDataReadyEvent)
                        {
                            dataIn.LastSync = LastSyncSignal;
                            dataIn.Received_at = ThrSeriell32.Now(enumTimQueryStatus.isSync);
                            Data.Push(dataIn);

                        } //if (_EnableDataReadyEvent)
                    }//if (DataAdded > 0)
                }
            }//while (!RS232ReceiverThread.CancellationPending)
            catch (Exception ee)
            {
#if DEBUG
                Debug.WriteLine("RS232ReceiverThread_DoWork Exception: " + ee.Message);
#endif
            }
            finally
            {
                IsConnected = false;
                if (RS232DataDistributorThread != null)
                {
                    RS232DataDistributorThread.CancelAsync();
                    //ConnectionBroken = true;
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
        private byte []? PrecheckBuffer(ref CFifoBuffer<byte> RS232inBytes)
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
                            int numExtradata = (RS232inBytes.Peek(mustbeinbuf - 2) >> 4) &0x07;
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
                    this.RS232ReceiverThread.CancelAsync();
                    //Warten bis thread fertig, aber nicht länger als 3 Sekunden
                    DateTime EndWait = DateTime.Now + new TimeSpan(0, 0, 0, 3, 0);
                    while (RS232ReceiverThread.IsBusy && (DateTime.Now < EndWait)) ;  //Warten bis thread fertig
                }
            }
        }
        #endregion}
    }
}
