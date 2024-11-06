using WindControlLib;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Concurrent;


namespace FeedbackDataLib
{
    public partial class CRS232Receiver2
    {
        // Define a custom struct or class to hold input data and TCS
        public class CommandRequest
        {
            public EnNeuromasterCommand Command { get; set; }
            public byte[] SendData { get; set; }

            public CommandRequest(EnNeuromasterCommand command, byte[] sendData)
            {
                Command = command;
                SendData = sendData;
            }
        }

        private readonly ConcurrentQueue<CommandRequest> _commandQueue = new ();

        public class CommandProcessedEventArgs(EnNeuromasterCommand command,
            byte[] responseData, 
            bool success, 
            byte hWcn = 0xff) : EventArgs
        {
            public EnNeuromasterCommand Command { get; } = command;
            public byte[] ResponseData { get; } = responseData;
            public bool Success { get; set; } = success;

            public byte HWcn { get; } = hWcn;
        }

        public event EventHandler<CommandProcessedEventArgs> CommandProcessed;
        protected virtual void OnCommandProcessed(CommandProcessedEventArgs e)
        {
            CommandProcessed?.Invoke(this, e);
        }


        public void ProcessCommand(EnNeuromasterCommand command, byte[] additionalData)
        {
            // Create a new CommandRequest object
            var commandRequest = new CommandRequest(command, additionalData);

            // Enqueue the request
            _commandQueue.Enqueue(commandRequest);
        }

        private EnNeuromasterCommand RunningCommand { get; set; }
        private DateTime RunningEnd;
        private TimeSpan TsCommandTimeout = TimeSpan.FromSeconds(3);

        private void StateMachine()
        {
            if (RunningCommand == EnNeuromasterCommand.None)
            {
                if (!_commandQueue.IsEmpty)
                {
                    if (_commandQueue.TryDequeue(out var commandRequest))
                    {
                        RunningCommand = commandRequest.Command;
                        //Prepare sending of the command
                        _sendingQueue.Push(commandRequest.SendData);
                        RunningEnd = DateTime.Now + TsCommandTimeout;
                    }
                }
            }
            else {
                //Any data coming in?
                if (!_commandResponseQueue.IsEmpty)
                {
                    var pk = _commandResponseQueue.Peek();
                    if (pk != null && pk[0] == (byte) RunningCommand)
                    {
                        //Correct response
                        // Notify the waiting GUI
                        byte[]? res = _commandResponseQueue.Pop();
                        if (res != null)
                        {
                            OnCommandProcessed(new CommandProcessedEventArgs(RunningCommand, res[1..], true));
                        }
                        else
                        {
                            OnCommandProcessed(new CommandProcessedEventArgs(RunningCommand, Array.Empty<byte>(), false));
                        }
                        RunningCommand = EnNeuromasterCommand.None;
                    }
                }
                if (DateTime.Now > RunningEnd)
                {
                    //Timeout
                    _= _commandResponseQueue.Pop();
                    OnCommandProcessed(new CommandProcessedEventArgs(RunningCommand, Array.Empty<byte>(), false));
                    RunningCommand = EnNeuromasterCommand.None;
                }
            }
        }

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

            DateTime Now;

            while (!cancellationToken.IsCancellationRequested)
            {

                try
                {
                    if (RPDeviceCommunicationToPC is not null && RPDeviceCommunicationToPC.Count > 0)
                    {
                        //Fire event
                        OnDeviceCommunicationToPC(buf: RPDeviceCommunicationToPC.Pop());
                    }


                    // Distribute measurement data
                    if (_measurementDataQueue is { Count: > 0 })
                    {
                        CDataIn[]? buffer = _measurementDataQueue.PopAll();
                        if (buffer?.Length > 0)
                        {
                            OnDataReadyComm(new List<CDataIn>(buffer));
                        }
                    }

                    // Handle outgoing data
                    if (_sendingQueue is { Count: > 0 })// && DataSent)
                    {
                        byte[]? bout = _sendingQueue.Pop();
                        if (bout is not null && bout.Length > 0)
                        {
                            //DataSent = false;
                            Seriell32.Write(bout, 0, bout.Length); // Adjust cancellation token as needed
                        }
                    }

                    Now = DateTime.Now;
                    // Send "alive" signal periodically
                    if (SendKeepAlive && Now > NextAliveSignalToSend)
                    {
                        _sendingQueue.Push(AliveSequToSend);
                        NextAliveSignalToSend = Now + AliveSignalToSendInterv;
                    }

                    else
                    {
                        await Task.Delay(10, cancellationToken); // Avoid high CPU usage
                    }

                    StateMachine();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("RS232DistributorThread Error: " + ex.Message);
                }
                finally
                {
                    
                }
            }
            Debug.WriteLine("RS232DistributorThread Closed");
        }

        private static void FireAndForgetTask(Func<Task> asyncFunc)
        {
            asyncFunc().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Log or handle the exception if needed
                    Debug.WriteLine($"Error in task: {task.Exception?.GetBaseException().Message}");
                }
            }, TaskScheduler.Default);
        }

        private CancellationTokenSource? cancellationTokenDistributor;

        public void StartRS232DistributorThread()
        {
            cancellationTokenDistributor = new CancellationTokenSource();
            Task.Run(() => RS232DistributorThread_DoWorkAsync(cancellationTokenDistributor.Token));
        }

        public void StopRS232DistributorThread()
        {
            cancellationTokenDistributor?.Cancel();
        }
        #endregion

        public enum EnNeuromasterCommand : byte
        {
            None = 0,
            ////////////////////
            // Dummy Commands for easier Call back processing
            ////////////////////
            GetDeviceConfig = 1,
            SetConfigAllModules= 2,
            GetModuleInfoSpecific = 3,


            ////////////////////
            // Commando Codes to Neuromaster
            ////////////////////

            /// <summary>Neuromaster sends battery status</summary>
            BatteryStatus = 0xDF,

            /// <summary>Buffer Full in Neuromaster</summary>
            /// <remarks>Neuromaster stops sampling, keep alive continues</remarks>
            BufferFull = 0xBF,

            /// <summary>Neuromaster sends every second a message to synchronize data</summary>
            ChannelSync = 0xFC,

            /// <summary>The following code is a Command byte</summary>
            CommandCode = 0x11,

            /// <summary>Connecting to Device</summary>
            ConnectToDevice = 0xFA,

            /// <summary>Signals that device is alive</summary>
            DeviceAlive = 0xFE,

            /// <summary>Read Clock</summary>
            GetClock = 0x88,

            /// <summary>Reads Module Configuration</summary>
            GetChannelConfig = 0x94,

            /// <summary>Neuromaster sends Firmware Version</summary>
            GetFirmwareVersion = 0x89,

            /// <summary>Get configuration of the selected module (all sw channels)</summary>
            GetModuleConfig = 0x98,

            /// <summary>Neuromaster sends SD Card Info</summary>
            GetSDCardInfo = 0x8B,

            /// <summary>NM has detected that Module Configuration is different from SD Card configuration</summary>
            ModuleConfigChanged = 0xE0,

            /// <summary>Error in Device</summary>
            ModuleError = 0xFF,

            /// <summary>Returns Module specific data</summary>
            Module_GetSpecific = 0x55,

            /// <summary>Sets Module specific data</summary>
            Module_SetSpecific = 0x56,

            /// <summary>Neuromaster is going to switch off (or starts to record to SD Card)</summary>
            NMOffline = 0xDE,

            /// <summary>Neuromaster sends Info to PC (Errors, Status, ...)</summary>
            NeuromasterToPC = 0xFB,

            /// <summary>Reset Neuromaster</summary>
            Reset = 0xDD,

            /// <summary>Scans connected Modules</summary>
            ScanModules = 0x99,

            /// <summary>Set Clock</summary>
            SetClock = 0x87,

            /// <summary>Tells NM that PC is closing the Connection</summary>
            SetConnectionClosed = 0x96,

            /// <summary>Set Module Configuration</summary>
            SetModuleConfig = 0x93,

            /// <summary>Write-Read Command for Modules - only passed through from Neuromaster</summary>
            /// <remarks> +1byte HW_cn, +1byte number of bytes to send, +1byte number of bytes to read</remarks>
            WrRdModuleCommand = 0x97,

        }

    }
}
