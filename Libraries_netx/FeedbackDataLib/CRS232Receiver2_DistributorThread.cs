using System.ComponentModel;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        #region DistributorThread
        /// <summary>
        /// RS232 worker thread, started in tryToConnectWorker_DoWork
        /// </summary>
        private BackgroundWorker RS232DataDistributorThread;

        /// <summary>
        /// This thread continously gets the data from the RS232ReceiverThread and raises OnDataReadyComm
        /// Thread is required since RS232WorkerThread should not call events due to unpredictable time delays
        /// </summary>
        void RS232DataDistributorThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "RS232DataDistributorThread";

            try
            {
                while (!RS232DataDistributorThread.CancellationPending)
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
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ee)
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


        #endregion
    }
}
