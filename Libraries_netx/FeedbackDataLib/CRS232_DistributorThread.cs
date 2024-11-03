using System.ComponentModel;
using WindControlLib;
using System.Diagnostics;


namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        #region DistributorThread
        /// <summary>
        /// RS232 worker thread, started in tryToConnectWorker_DoWork
        /// </summary>
        //private BackgroundWorker RS232DataDistributorThread;

        /// <summary>
        /// This thread continously gets the data from the RS232ReceiverThread and raises OnDataReadyComm
        /// Thread is required since RS232WorkerThread should not call events due to unpredictable time delays
        /// </summary>
        //private void RS232DataDistributorThread_DoWork(object? sender, DoWorkEventArgs? e)
        private async Task RS232DistributorThread_DoWorkAsync(CancellationToken cancellationToken)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232DataDistributorThread";

            try
            {
                //while (RS232DataDistributorThread != null && !RS232DataDistributorThread.CancellationPending)
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (RPDeviceCommunicationToPC is not null)
                    {
                        //Daten holen
                        if (RPDeviceCommunicationToPC.Count > 0)
                        {
                            //Fire event
                            OnDeviceCommunicationToPC(buf: RPDeviceCommunicationToPC.Pop());
                        }
                    }

                    if (Data.Count > 0)
                    {
                        CDataIn[] di = Data.PopAll();
                        if (di != null && di.Length > 0)
                        {
                            OnDataReadyComm([.. di]);
                        }
                    }
                    else
                    {
                        //Thread.Sleep(10);
                        await Task.Delay(10, cancellationToken); // Sleep to prevent high CPU usage
                    }
                }
            }
#pragma warning disable CS0168
            catch (Exception ee)
#pragma warning restore CS0168
            {
#if DEBUG
                Debug.WriteLine("RS232DataDistributorThread Error: " + ee.Message);
#endif
            }

            finally
            {
#if DEBUG
                Debug.WriteLine("RS232DataDistributorThread Closed");
#endif
            }
        }

        private CancellationTokenSource? cancellationTokenDistributor;

        public void Start_RS232DistributorThread()
        {
            cancellationTokenDistributor = new CancellationTokenSource();
            Task.Run(() => RS232DistributorThread_DoWorkAsync(cancellationTokenDistributor.Token));
        }

        public void Stop_RS232Distributorhread()
        {
            cancellationTokenDistributor?.Cancel();
        }


        #endregion
    }
}
