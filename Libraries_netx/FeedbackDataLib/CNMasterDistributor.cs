using FeedbackDataLib.Modules;
using Microsoft.Extensions.Logging;
using WindControlLib;


namespace FeedbackDataLib
{
    public partial class CNMaster
    {
        private int cntSyncPackages = 0;                        //Counts the incoming sync packages
        private DateTime OldLastSync = DateTime.MinValue;       //Time when the previous package was received
        private DateTime ReceivingStarted = DateTime.MinValue;  //Receiving started at

        // Define a custom struct or class to hold input data and TCS
        public class CommandRequest
        {
            private byte[] responseData = [];

            public EnNeuromasterCommand Command { get; set; }
            public EnModuleCommand ModuleCommand { get; set; } = EnModuleCommand.None;
            public byte[] SendData { get; set; }
            public byte[] ResponseData { get => responseData; set => responseData = value[1..]; }
            public bool Success { get; set; }
            public byte HWcn { get; set; } = 0xff;
            public DateTime RunningEnd { get; set; } = DateTime.MinValue;

            public CommandRequest(EnNeuromasterCommand command, byte[] sendData)
            {
                Command = command;
                SendData = sendData;
            }

            public CommandRequest()
            {
                Command = EnNeuromasterCommand.None;
#pragma warning disable IDE0301 // Simplify collection initialization
                SendData = Array.Empty<byte>();
#pragma warning restore IDE0301 // Simplify collection initialization
            }
        }


        private readonly TimeSpan TsCommandTimeout = TimeSpan.FromMilliseconds(WaitCommandResponseTimeOutMs);
        private CommandRequest? RunningCommand;

        /// <summary>
        /// Time when next Alive Signal is due
        /// </summary>
        private DateTime NextAliveSignalToSend = DateTime.Now;

        private readonly TimeSpan AliveSignalToSendInterv = new(0, 0, 0, 0, AliveSignalToSendIntervMs);

        private readonly CFifoConcurrentQueue<CommandRequest> _sendingQueue = new();

        private readonly CFifoConcurrentQueue<CommandRequest> _runningCommandsQueue = new();


        //private CFifoConcurrentQueue<CommandRequest> RunningCommandsQueue => _runningCommandsQueue;
        public CFifoConcurrentQueue<CommandRequest> SendingQueue => _sendingQueue;


        public event EventHandler<CommandRequest>? CommandProcessed;
        protected virtual void OnCommandProcessed(CommandRequest e)
        {
            CommandProcessed?.Invoke(this, e);
        }

        private void SendCommand(EnNeuromasterCommand neuromasterCommand, byte[]? additionalData, CommandRequest? cr = null)
        {
            // Validate AdditionalDataToSend size early
#pragma warning disable IDE0301 // Simplify collection initialization
            additionalData ??= Array.Empty<byte>();
#pragma warning restore IDE0301 // Simplify collection initialization
            if (additionalData.Length > 250)
            {
                throw new ArgumentException("Size of AdditionalDataToSend must be <= 250", nameof(additionalData));
            }

            const int overhead = 4; // CommandCode, Command, Length, CRC
            int lengthWithCRC = additionalData.Length + 1;
            int bytesToSend = overhead + additionalData.Length;
            byte[] buf = new byte[bytesToSend];

            // Build the command buffer
            buf[0] = CommandCode;    // Base command code
            buf[1] = (byte)neuromasterCommand;                   // Specific command code
            buf[2] = (byte)lengthWithCRC;                        // Length byte (+CRC)

            // Copy additional data into the buffer
            Buffer.BlockCopy(additionalData, 0, buf, overhead - 1, additionalData.Length);

            // Calculate and set CRC
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 1);

            // Create or update the CommandRequest and enqueue it
            cr ??= new();
            cr.Command = neuromasterCommand;
            cr.SendData = buf;

            _sendingQueue.Push(cr);
        }



        #region DistributorThread
        /// <summary>
        /// This thread continously gets the data from the RS232ReceiverThread and raises OnDataReadyComm
        /// Thread is required since RS232WorkerThread should not call events due to unpredictable time delays
        /// </summary>
        private async Task DistributorThreadAsync(CancellationToken cancellationToken)
        {
            if (NMReceiver is null || NMReceiver.Connection is null || NMReceiver.Connection.SerPort is null) return;

            bool isReportedRS232Threadbroken = false;

            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "DistributorThread";

            if (NMReceiver.Connection == null) throw new Exception("c8Receiver.Connection not allowed to be null");

            if (!NMReceiver.Connection.SerPort.IsOpen) NMReceiver.Connection.SerPort.GetOpen();

            NextAliveSignalToSend = DateTime.Now;
            RS232Receiver = new CRS232Receiver(0x0f, NMReceiver.Connection.SerPort);

            // Start the RS232ReceiverThread and pass the cancellation token
            _ = RS232Receiver.StartRS232ReceiverThreadAsync(cancellationToken).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    // Handle exception (log, etc.)
                    _logger.LogError("DistributorThreadAsync: {Message}", t.Exception.Message);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            _sendingQueue.Clear();
            _runningCommandsQueue.Clear();
            RunningCommand = null;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    DateTime Now = DateTime.Now;
                    //Any data coming in?
                    if (!RS232Receiver.CommandResponseQueue.IsEmpty)
                    {
                        //Correct response?
                        (byte[] btreceived, DateTime dtreceived) = RS232Receiver.CommandResponseQueue.Peek();
                        EnNeuromasterCommand cmdin = EnNeuromasterCommand.None;
                        if (btreceived != null)
                        {
                            cmdin = (EnNeuromasterCommand)btreceived[0];
                            _logger.LogInformation("DistributorThreadAsync Receiving: {Message}", Enum.GetName(typeof(EnNeuromasterCommand), cmdin));
                        }
                        if (btreceived != null && RunningCommand is not null && btreceived[0] == (byte)RunningCommand.Command)
                        {
                            //Yes, get it
                            (byte[]? res, _) = RS232Receiver.CommandResponseQueue.Pop();
                            if (res != null)
                            {
                                RunningCommand.ResponseData = res;
                                RunningCommand.Success = true;
                                if (cmdin != EnNeuromasterCommand.DeviceAlive)
                                    EvalCommandResponse(RunningCommand);
                                else
                                {
                                    _ = RS232Receiver.CommandResponseQueue.Pop();
                                    _logger.LogInformation("Popped DeviceAlive");
                                    RunningCommand = null;
                                }
                            }
                            else
                            {
                                OnCommandProcessed(new());
                            }
                            RunningCommand = null;
                        }
                        else
                        {
                            //Incoming message not processed - timeout, delete
                            if (DateTime.Now > dtreceived + TsCommandTimeout)
                            {
                                (_, _) = RS232Receiver.CommandResponseQueue.Pop();
                                _logger.LogWarning("DistributorThreadAsync: Incoming timeout");
                            }
                        }
                    }

                    //Check Timeout
                    if (RunningCommand != null && Now > RunningCommand.RunningEnd)
                    {
                        //_ = RS232Receiver.CommandResponseQueue.Pop();
                        OnCommandProcessed(RunningCommand);
                        _logger.LogWarning("DistributorThreadAsync: Timeout {Message}", Enum.GetName(typeof(EnNeuromasterCommand), RunningCommand.Command));
                        RunningCommand = null;
                    }

                    //Incoming: Command to PC 
                    if (!RS232Receiver.CommandToPCQueue.IsEmpty)
                    {
                        //Fire event
                        byte[]? buf = RS232Receiver.CommandToPCQueue.Pop();
                        if (buf != null)
                        {
                            EvalCommunicationToPC(buf);
                        }
                    }

                    // Incoming: Distribute measurement data 
                    if (!RS232Receiver.MeasurementDataQueue.IsEmpty)
                    {
                        CDataIn[]? buffer = RS232Receiver.MeasurementDataQueue.PopAll();
                        if (buffer?.Length > 0)
                        {
                            EvalMeasurementData(new List<CDataIn>(buffer));
                        }
                    }

                    // Outgoing: Send data 
                    if (!SendingQueue.IsEmpty)
                    {
                        if (RunningCommand == null) //Wait until running command is finished or timed out
                        {
                            CommandRequest? cr = SendingQueue.Pop();
                            if (cr is not null && cr.SendData.Length > 0)
                            {
                                //if (cr.Command is not EnNeuromasterCommand.DeviceAlive)
                                {
                                    _logger.LogInformation("DistributorThreadAsync Sending: {Message}", Enum.GetName(typeof(EnNeuromasterCommand), cr.Command));
                                }
                                RunningCommand = cr;
                                RunningCommand.RunningEnd = DateTime.Now + TsCommandTimeout;
                                NMReceiver.Connection.SerPort.Write(cr.SendData, 0, cr.SendData.Length); // Adjust cancellation token as needed
                            }
                        }
                    }

                    // Send "alive" signal periodically
                    if (Now > NextAliveSignalToSend)
                    {
                        CommandRequest cr = new(EnNeuromasterCommand.DeviceAlive, AliveSequToSend());
                        SendingQueue.Push(cr);
                        NextAliveSignalToSend = Now + AliveSignalToSendInterv;
                    }

                    if (!RS232Receiver.IsRS232ReceiverThreadRunning && !isReportedRS232Threadbroken)
                    {
                        _logger.LogError("DistributorThreadAsync: RS232ReceiverThreadAsync stopped");
                        isReportedRS232Threadbroken = true;
                    }

                    else
                    {
                        await Task.Delay(10, cancellationToken); // Avoid high CPU usage
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DistributorThreadAsync: {Message}", ex.Message);
            }
            finally
            {
                NMReceiver.Connection.SerPort.Close();
                RS232Receiver.StopRS232ReceiverThread();
                _logger.LogInformation("DistributorThreadAsync Closed");
            }
        }


        protected virtual void EvalCommandResponse(CommandRequest rc)
        {
            if (RunningCommand is null) return;

            //Prepare message text for display
            ColoredText msg;
            string? nm = Enum.GetName(typeof(EnNeuromasterCommand), RunningCommand.Command);
            msg = new($"{nm}: {(rc.Success ? "OK" : "Failed")}", rc.Success ? Color.Green : Color.Red);

            switch (rc.Command)
            {
                case EnNeuromasterCommand.SetConnectionClosed:
                    OnSetConnectionClosedResponse(rc.Success, msg);
                    break;

                case EnNeuromasterCommand.GetFirmwareVersion:
                    if (rc.Success)
                    {
                        CNMFirmwareVersion NMFirmwareVersion = new();
                        NMFirmwareVersion.UpdateFromByteArray(rc.ResponseData, 0);
                        OnGetFirmwareVersionResponse(NMFirmwareVersion, msg);
                    }
                    else
                        OnGetFirmwareVersionResponse(null, msg);
                    break;
                case EnNeuromasterCommand.ScanModules:
                    OnScanModulesResponse(rc.Success, msg);
                    break;

                case EnNeuromasterCommand.GetModuleConfig:
                    if (rc.Success)
                    {
                        try
                        {
                            UpdateModuleFromByteArray(rc.ResponseData);
                            Calculate_SkalMax_SkalMin(); // Calculate max and mins
                            OnGetModuleConfigResponse(ModuleInfos[RunningCommand.HWcn], msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("C8KanalReceiverV2_CommBase_#01: {Message}", ex.Message);
                            rc.Success = false;
                            OnGetModuleConfigResponse(null, msg);
                        }
                    }
                    break;

                case EnNeuromasterCommand.SetModuleConfig:
                    OnSetModuleConfigResponse(rc.Success, rc.HWcn, msg);
                    break;
                case EnNeuromasterCommand.WrRdModuleCommand:
                    nm = Enum.GetName(typeof(EnModuleCommand), rc.ModuleCommand);
                    msg = new ColoredText($"{nm}: {(rc.Success ? "OK" : "Failed")}", rc.Success ? Color.Green : Color.Red);

                    if (rc.Success)
                    {
                        try
                        {
                            switch (RunningCommand.ModuleCommand)
                            {
                                case EnModuleCommand.GetModuleSpecific:
                                    byte[] btin = new byte[CModuleBase.ModuleSpecific_sizeof];
                                    Buffer.BlockCopy(rc.ResponseData, 1, btin, 0, CModuleBase.ModuleSpecific_sizeof);
                                    //12.11.2020 Check CRC
                                    // Check CRC and debug output
                                    byte crc = CRC8.Calc_CRC8(btin, btin.Length - 2);

                                    _logger.LogInformation("GetModuleInfoSpecific: {Message}", string.Join(", ", btin.Select(b => b.ToString("X2"))) +
                                                    $" / CalcCRC={crc:X2}");

                                    if (UpdateModuleInfo)
                                        ModuleInfos[HWcnGetModuleInfoSpecific].SetModuleSpecific(btin);

                                    OnGetModuleSpecificResponse(rc.Success, RunningCommand.HWcn, msg);

                                    break;
                                case EnModuleCommand.SetModuleSpecific:
                                    OnSetModuleSpecificResponse(rc.Success, RunningCommand.HWcn, msg);
                                    break;
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        switch (RunningCommand.ModuleCommand)
                        {
                            case EnModuleCommand.GetModuleSpecific:
                                OnGetModuleSpecificResponse(rc.Success, RunningCommand.HWcn, msg);
                                break;
                            case EnModuleCommand.SetModuleSpecific:
                                OnSetModuleSpecificResponse(rc.Success, RunningCommand.HWcn, msg);
                                break;
                        }
                    }

                    break;

                case EnNeuromasterCommand.GetClock:
                    if (rc.Success)
                    {
                        DeviceClock.UpdateFrom_ByteArray(rc.ResponseData, 0);
                        OnGetClockResponse(DeviceClock.Dt, msg);
                    }
                    else
                        OnGetClockResponse(null, msg);
                    break;
                case EnNeuromasterCommand.SetClock:
                    OnSetClockResponse(rc.Success, msg);
                    break;
                
                case EnNeuromasterCommand.DeviceAlive:
                    RunningCommand = null;
                    break;
            }
        }



        /// <summary>
        /// NM is communicating back to PC
        /// </summary>
        /// <param name="buf">The buf.</param>
        protected virtual void EvalCommunicationToPC(byte[] buf)
        {
            switch (buf[1])
            {
                case CNMtoPCCommands.cModuleError:
                    {
                        if (buf.Length > 2)
                            OnDeviceToPC_ModuleError(buf[2]);
                        break;
                    }

                case CNMtoPCCommands.cBufferFull:
                    {
                        OnDeviceToPC_BufferFull();
                        break;
                    }
                case CNMtoPCCommands.cBatteryStatus:
                    {
                        if (ConnectionType ==  EnConnectionStatus.Connected_via_XBee)
                        {
                            //buf[2] ... Battery Low    
                            //buf[3] ... Battery High [1/10V]
                            //buf[4] ... Supply Low
                            //buf[5] ... Supply High  [1/10V] 

                            uint BatteryVoltage_mV = buf[3];
                            BatteryVoltage_mV = ((BatteryVoltage_mV << 8) + buf[2]) * 10;


                            uint SupplyVoltage_mV = buf[5];
                            SupplyVoltage_mV = ((SupplyVoltage_mV << 8) + buf[4]) * 10;

                            uint BatteryPercentage = (uint)BatteryVoltage.GetPercentage(((double)BatteryVoltage_mV) / 1000);

                            OnDeviceToPC_BatteryStatus(BatteryVoltage_mV, BatteryPercentage, SupplyVoltage_mV);
                        }
                        break;
                    }
                case CNMtoPCCommands.cNMOffline:   //28.7.2014
                    {
                        Close();  //Kommumikation beenden
                        break;
                    }
            }
        }

        /// <summary>
        /// Receives data, takes care of over all data synchronicity
        /// Updates time and forwards event
        /// </summary>
        /// <remarks>
        /// Only forwards data if the first Sync Value is received
        /// </remarks>
        private void EvalMeasurementData(List<CDataIn> DataIn)
        {
            if (DataIn == null) return;

            List<CDataIn> dataIn = [];
            foreach (CDataIn di in DataIn)
            {
                if (di.HWcn >= ModuleInfos.Count) return;
                if (di.SWcn >= ModuleInfos[di.HWcn].SWChannels.Count) return;

                //27.1.2020
                if (di.SyncFlag == 1)
                {
                    if (ReceivingStarted == DateTime.MinValue)
                    {
                        //First Sync Packet of this channel
                        ReceivingStarted = di.LastSync;
                        OldLastSync = di.LastSync;
                        cntSyncPackages = 0;
                    }
                    else
                    {
                        if (di.LastSync != OldLastSync)
                        {
                            OldLastSync = di.LastSync;
                            cntSyncPackages++;
                        }
                    }
                    ModuleInfos[di.HWcn].SWChannels[di.SWcn].SWChan_Started = ReceivingStarted;
                    ModuleInfos[di.HWcn].SWChannels[di.SWcn].SynPackagesreceived = cntSyncPackages;
                }

                UpdateTime(di);
                di.VirtualID = ModuleInfos[di.HWcn].SWChannels[di.SWcn].VirtualID;

                if (di.ChannelStarted != DateTime.MinValue)
                {
                    //Process Data Module specific
                    dataIn.AddRange(ModuleInfos[di.HWcn].Processdata(di));
                }
            }
            OnDataReadyResponse(dataIn);
        }


        private CancellationTokenSource? cancellationTokenDistributor;

        public void StartDistributorThreadAsync()
        {
            cancellationTokenDistributor = new CancellationTokenSource();
            Task.Run(() => DistributorThreadAsync(cancellationTokenDistributor.Token));
        }

        public void StopDistributorThreadAsync()
        {
            cancellationTokenDistributor?.Cancel();
        }
        #endregion

    }
}
