using BMTCommunicationLib;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;


namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        #region ConnectThread
        /// <summary>
        /// Thread to Connect to Device
        /// </summary>
        /// <remarks>
        /// Ends if device is detected
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e">DoWorkEventArgs</param>
        private void TryToConnectWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "tryToConnectWorker";

            if (tryToConnectWorker is null)
            {
                throw new InvalidOperationException("The connection worker has not been initialized.");
            }

            if (Seriell32 is null)
            {
                throw new InvalidOperationException("The serial connection is not set.");
            }

            while (!tryToConnectWorker.CancellationPending)
            {
                if (!IsConnected)
                {
                    ConnectionStatus = EnumConnectionStatus.Connecting;
                    try
                    {
                        if (Seriell32 != null && Seriell32.IsOpen)
                            Seriell32.Close();
                        if (Seriell32 != null && !Seriell32.GetOpen())
                        {
                            ConnectionStatus = EnumConnectionStatus.PortError;
                            IsConnected = false;
                            //Thread stoppen
                            Stop_RS232ReceiverThread();
                            tryToConnectWorker.CancelAsync();   //In diesem Falle nicht weiter probieren
                        }
                    }
                    catch (Exception)
                    {
                        ConnectionStatus = EnumConnectionStatus.PortError;
                        IsConnected = false;
                        //Thread stoppen
                        Stop_RS232ReceiverThread();
                        Seriell32?.Close();
                        //log.Error(ee.Message, ee);
                        tryToConnectWorker.CancelAsync();   //In diesem Falle nicht weiter probieren
                    }

                    if (Seriell32 != null)
                    {
                        if (Check4Neuromaster_RS232())
                        {
                            //Succeeded
                            //Gerät da

                            //Thread starten
                            /*
                                                        RS232ReceiverThread = new BackgroundWorker();
                            #pragma warning disable CS8622
                                                        RS232ReceiverThread.DoWork += new DoWorkEventHandler(RS232ReceiverThread_DoWork);
                            #pragma warning restore CS8622
                                                        RS232ReceiverThread.WorkerSupportsCancellation = true;
                                                        RS232ReceiverThread.RunWorkerAsync();
                            */
                            // Run the async method as a new task
                            Start_RS232ReceiverThread ();

                            //Diesen Thread beenden
                            tryToConnectWorker.CancelAsync();

                            IsConnected = true;
                        }
                        else
                        {
                            //failed
                        }
                    }
                    if (!IsConnected)
                    {
                        ConnectionStatus = EnumConnectionStatus.Not_Connected;
                        IsConnected = false;
                        Seriell32?.Close();
                        //ggf Thread stoppen
                        Stop_RS232ReceiverThread();
                    }
                }//if (!this.IsConnected)
            }   //while (!tryToConnectWorker.CancellationPending)
            if (IsConnected)
            {
                //CDelay.Delay_ms(3000);
                ConnectionStatus = EnumConnectionStatus.Connected;
            }
#if DEBUG
            Debug.WriteLine("tryToConnectWorker Closed");
#endif

        }

        /// <summary>
        /// Checks if Neuromaster is present
        /// Keeps serial port status (opened or closed)
        /// </summary>
        /// <returns></returns>
        public virtual bool Check4Neuromaster_RS232()
        {
            if (Seriell32 != null)
            {
                bool WasOpen = Seriell32.IsOpen;
                if (!WasOpen)
                {
                    //Open
                    if (!Seriell32.GetOpen())
                        return false;
                }
                bool ret = CNeuromaster.Check4Neuromaster(Seriell32, ConnectSequToSend, ConnectSequToReturn, _CommandChannelNo);
                if (!WasOpen)
                    Seriell32.Close();
                return ret;
            }
            return false;
        }

        #endregion
    }
}
