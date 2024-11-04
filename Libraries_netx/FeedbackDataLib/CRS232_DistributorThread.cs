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
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Distribute command responses
                    while (_commandResponseQueue.TryDequeue(out var commandData))
                    {
                        OnDeviceCommunicationToPC(commandData);
                    }

                    // Distribute measurement data
                    if (_measurementDataQueue.Count > 0)
                    {
                        List<CDataIn> di = [.. _measurementDataQueue];
                        _measurementDataQueue.Clear();
                        OnDataReadyComm(di);
                    }
                    else
                    {
                        await Task.Delay(10, cancellationToken); // Avoid high CPU usage
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RS232DistributorThread Error: " + ex.Message);
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
