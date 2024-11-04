using BMTCommunicationLib;
using System.Diagnostics;

namespace FeedbackDataLib
{

    public partial class CRS232Receiver2
    {
        private CancellationTokenSource? cancellationTokenConnector;

        #region ConnectTask
        /// <summary>
        /// Task to Connect to Device
        /// </summary>
        /// <remarks>
        /// Ends if device is detected
        /// </remarks>
        private async Task TryToConnectAsync(CancellationToken cancellationToken)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "tryToConnectTask";

            while (!cancellationToken.IsCancellationRequested)
            {
                if (Seriell32 is null)
                {
                    throw new InvalidOperationException("The serial connection is not set.");
                }

                if (!IsConnected)
                {
                    ConnectionStatus = EnumConnectionStatus.Connecting;
                    try
                    {
                        if (Seriell32.IsOpen)
                            Seriell32.Close();

                        if (!Seriell32.GetOpen())
                        {
                            ConnectionStatus = EnumConnectionStatus.PortError;
                            IsConnected = false;
                            StopRS232ReceiverThread();
                            break; // Exit the loop, stop further attempts
                        }
                    }
                    catch (Exception)
                    {
                        ConnectionStatus = EnumConnectionStatus.PortError;
                        IsConnected = false;
                        StopRS232ReceiverThread();
                        Seriell32?.Close();
                        break; // Exit the loop, stop further attempts
                    }

                    if (Check4Neuromaster_RS232())
                    {
                        // Succeeded, device detected
                        StartRS232ReceiverThread();
                        IsConnected = true;
                    }
                    else
                    {
                        // Connection attempt failed
                        ConnectionStatus = EnumConnectionStatus.Not_Connected;
                        IsConnected = false;
                        Seriell32?.Close();
                        StopRS232ReceiverThread();
                    }
                }


                if (IsConnected)
                {
                    await Task.Delay(3000, cancellationToken); // Delay to stabilize connection if needed
                    ConnectionStatus = EnumConnectionStatus.Connected;
                    StopConnectionThread();
                }
            }

#if DEBUG
            Debug.WriteLine("tryToConnectTask Closed");
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
                bool wasOpen = Seriell32.IsOpen;
                if (!wasOpen)
                {
                    // Try to open the serial connection
                    if (!Seriell32.GetOpen())
                        return false;
                }
                bool result = CNeuromaster.Check4Neuromaster(Seriell32, ConnectSequToSend, ConnectSequToReturn, _CommandChannelNo);
                if (!wasOpen)
                    Seriell32.Close();
                return result;
            }
            return false;
        }
        #endregion

        #region StartStopMethods
        /// <summary>
        /// Starts the connection task
        /// </summary>
        public void StartConnectionThread()
        {
            if (cancellationTokenConnector != null)
            {
                throw new InvalidOperationException("Connection task is already running.");
            }

            cancellationTokenConnector = new CancellationTokenSource();
            _ = TryToConnectAsync(cancellationTokenConnector.Token);
        }

        /// <summary>
        /// Stops the connection task
        /// </summary>
        public void StopConnectionThread()
        {
            if (cancellationTokenConnector == null)
            {
                throw new InvalidOperationException("Connection task is not running.");
            }

            cancellationTokenConnector.Cancel();
            cancellationTokenConnector = null;
        }
        #endregion
    }
}