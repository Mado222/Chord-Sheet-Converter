using FeedbackDataLib.Modules;
using FeedbackDataLib.Modules.CADS1294x;
using WindControlLib;


namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    /// 

    public partial class CNMaster
    {
        //private int cntDeviceConfigs = 0;
        private readonly List<byte> allDeviceConfigData = [];
        //private readonly ConcurrentQueue<byte[]> getModulesqueue = new();


        public event EventHandler<List<CDataIn>>? DataReadyResponse;
        /// <summary>
        /// Data Ready
        /// </summary>
        /// <remarks>
        /// Limits of the Data Range:
        /// 0x0000 ... input amplifier is neg saturated
        /// 0xFFFF ... input amplifier is pos saturated
        /// if output stage is saturated, 0x0001 and 0xFFFE
        /// </remarks>
        protected virtual void OnDataReadyResponse(List<CDataIn> DataRead)
        {
            var handler = DataReadyResponse;
            handler?.Invoke(this, DataRead);
        }

        #region Events_related_to_Commands
        public event EventHandler<(CNMFirmwareVersion? FWVersion, ColoredText msg)>? GetFirmwareVersionResponse;
        protected virtual void OnGetFirmwareVersionResponse(CNMFirmwareVersion? fw, ColoredText msg)
        {
            var handler = GetFirmwareVersionResponse;
            handler?.Invoke(this, (fw, msg));
        }

        public event EventHandler<(List<CModuleBase>? cmb, ColoredText msg)>? GetDeviceConfigResponse;
        protected virtual void OnGetDeviceConfigResponse(List<CModuleBase> cmb, ColoredText msg)
        {
            var handler = GetDeviceConfigResponse;
            handler?.Invoke(this, (cmb, msg));
        }

        public event EventHandler<(bool isSuccess, ColoredText msg)>? SetDeviceConfigResponse;
        protected virtual void OnSetDeviceConfigResponse(bool isSuccess, ColoredText msg)
        {
            var handler = SetDeviceConfigResponse;
            handler?.Invoke(this, (isSuccess, msg));
        }

        /// <summary>
        /// Module Config 
        /// </summary>
        public event EventHandler<(CModuleBase? cmb, ColoredText msg)>? GetModuleConfigResponse;
        protected virtual void OnGetModuleConfigResponse(CModuleBase? cmb, ColoredText msg)
        {
            var handler = GetModuleConfigResponse;
            handler?.Invoke(this, (cmb, msg));
        }

        public event EventHandler<(bool isSuccess, byte HWcn, ColoredText msg)>? SetModuleConfigResponse;
        protected virtual void OnSetModuleConfigResponse(bool isSuccess, byte HWcn, ColoredText msg)
        {
            var handler = SetModuleConfigResponse;
            handler?.Invoke(this, (isSuccess, HWcn, msg));
        }

        /// <summary>
        /// Clock
        /// </summary>
        public event EventHandler<(DateTime? clock, ColoredText msg)>? GetClockResponse;

        protected virtual void OnGetClockResponse(DateTime? clock, ColoredText msg)
        {
            var handler = GetClockResponse;
            handler?.Invoke(this, (clock, msg));
        }

        public event EventHandler<(bool success, ColoredText msg)>? SetClockResponse;

        protected virtual void OnSetClockResponse(bool success, ColoredText msg)
        {
            var handler = SetClockResponse;
            handler?.Invoke(this, (success, msg));
        }

        /// <summary>
        /// Event for SetConnectionClosed with bool response        
        /// </summary>
        public event EventHandler<(bool isClosed, ColoredText msg)>? SetConnectionClosedResponse;

        protected virtual void OnSetConnectionClosedResponse(bool isClosed, ColoredText msg)
        {
            var handler = SetConnectionClosedResponse;
            handler?.Invoke(this, (isClosed, msg));
        }

        /// <summary>
        /// Scann modules
        /// </summary>
        public event EventHandler<(bool isSuccess, ColoredText msg)>? ScanModulesResponse;
        protected virtual void OnScanModulesResponse(bool isSuccess, ColoredText msg)
        {
            var handler = ScanModulesResponse;
            handler?.Invoke(this, (isSuccess, msg));
        }

        /// <summary>
        /// Module Specific
        /// </summary>
        public event EventHandler<(bool isSuccess, byte HWcn, ColoredText msg)>? GetModuleSpecificResponse;
        protected virtual void OnGetModuleSpecificResponse(bool isSuccess, byte HWcn, ColoredText msg)
        {
            var handler = GetModuleSpecificResponse;
            handler?.Invoke(this, (isSuccess, HWcn, msg));
        }

        public event EventHandler<(bool isSuccess, byte HWcn, ColoredText msg)>? SetModuleSpecificResponse;
        protected virtual void OnSetModuleSpecificResponse(bool isSuccess, byte HWcn, ColoredText msg)
        {
            var handler = SetModuleSpecificResponse;
            handler?.Invoke(this, (isSuccess, HWcn, msg));
        }
        #endregion

        #region Events_for_communication_NM_to PC
        /// <summary>
        /// Triggered when the device's buffer is full, indicating sampling has stopped, requiring reinitialization.
        /// </summary>
        public event EventHandler<byte>? DeviceToPC_ModuleError;
        protected virtual void OnDeviceToPC_ModuleError(byte hwcn)
        {
            var handler = DeviceToPC_ModuleError;
            handler?.Invoke(this, hwcn);
        }

        /// <summary>
        /// Triggered when the device's buffer is full, indicating sampling has stopped, requiring reinitialization.
        /// </summary>
        public event EventHandler? DeviceToPC_BufferFull;
        protected virtual void OnDeviceToPC_BufferFull()
        {
            var handler = DeviceToPC_BufferFull;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Neuromaster sends battery status
        /// </summary>
        public event EventHandler<(uint BatteryVoltageMV, uint Percentage, uint SupplyVoltageMV)>? DeviceToPC_BatteryStatus;

        /// <summary>
        /// Battery Status
        /// </summary>
        /// <param name="batteryVoltageMV">Voltage [mV]</param>
        /// <param name="percentage">Percentage</param>
        /// <param name="supplyVoltageMV">Supply Voltage</param>
        protected virtual void OnDeviceToPC_BatteryStatus(uint batteryVoltageMV, uint percentage, uint supplyVoltageMV)
        {
            var handler = DeviceToPC_BatteryStatus;
            handler?.Invoke(this, (batteryVoltageMV, percentage, supplyVoltageMV));
        }

        /// <summary>Occurs when Module Specific Information from Vaso is updated.</summary>
        public event EventHandler? Vaso_InfoSpecific_Updated;
        protected virtual void OnVaso_InfoSpecific_Updated()
        {
            var handler = Vaso_InfoSpecific_Updated;
            handler?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region HelperFunctions
        public bool IsDeviceAvailable()
        {
            if (NMReceiver is null || NMReceiver.Connection is null || NMReceiver.Connection.SerPort is null) return false;
            return NMReceiver.Connection.SerPort.IsOpen;
        }
        #endregion

        #region Commands_to_NM
#pragma warning disable IDE0301 // Simplify collection initialization
        /// <summary>
        /// Set Clock in Device
        /// </summary>
        /// <param name="dt">DateTime to transmit</param>
        /// <returns></returns>
        public void SetClock(DateTime dt)
        {
            DeviceClock.Dt = dt;
            byte[] additionalData = Array.Empty<byte>();
            DeviceClock.GetByteArray(ref additionalData, 0);
            SendCommand(EnNeuromasterCommand.SetClock, additionalData);
        }

        /// <summary>
        /// Reads Device Clock
        /// </summary>
        /// <param name="dt">DateTime received</param>
        /// <returns></returns>
        public void GetClock()
        {
            SendCommand(EnNeuromasterCommand.GetClock, Array.Empty<byte>());
        }

        /// <summary>
        /// Sends Close Connection to NM
        /// </summary>
        /// <returns></returns>
        public void SendCloseConnection()
        {
            SendCommand(EnNeuromasterCommand.SetConnectionClosed, Array.Empty<byte>());
        }

        /// <summary>
        /// Gets Neuromaster Firmware Version
        /// </summary>
        /// <returns></returns>
        public void GetNMFirmwareVersion()
        {
            SendCommand(EnNeuromasterCommand.GetFirmwareVersion, Array.Empty<byte>());
        }

        /// <summary>
        /// Gets SD Card Information
        /// </summary>
        /// <returns></returns>
        public void GetSDCardInfo()
        {
            SendCommand(EnNeuromasterCommand.GetSDCardInfo, Array.Empty<byte>());
        }

        /// <summary>
        /// Forces device to rescan modules
        /// </summary>
        /// <remarks>
        /// Resets all Errors that are recognised in Neuromaster
        /// </remarks>
        public virtual void ScanModules()
        {
            SendCommand(EnNeuromasterCommand.ScanModules, Array.Empty<byte>());
        }
#pragma warning restore IDE0301 // Simplify collection initialization

        /// <summary>
        /// Sets configuration of all SW Channels of one Module
        /// </summary>
        /// <param name="HWcn">Hardware channel number</param>
        /// <returns>true if successful</returns>
        /// <remarks>Checks if ModuleType != cModuleTypeEmpty</remarks>
        public bool SetModuleConfig(int HWcn)
        {
            var moduleInfo = ModuleInfos[HWcn];
            if (moduleInfo.ModuleType == EnModuleType.cModuleTypeEmpty)
            {
                return false;
            }

            CommandRequest cr = new()
            {
                HWcn = (byte)HWcn
            };
            SendCommand(EnNeuromasterCommand.SetModuleConfig, moduleInfo.Get_SWConfigChannelsByteArray(), cr);
            return true;
        }


        private bool SetDeviceConfigAsync_Success = false;
        /// <summary>
        /// Sets configuration of all Modules
        /// </summary>
        /// <returns></returns>
        public async Task SetDeviceConfigAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<(bool success, byte HWcn, ColoredText msg)>? originalHandler = null;
            SetDeviceConfigAsync_Success = true;
            int numChanSet = 0;

            try
            {
                //Backup the original handler
                if (SetModuleConfigResponse != null)
                {
                    foreach (Delegate d in SetModuleConfigResponse.GetInvocationList())
                    {
                        if (d is EventHandler<(bool success, byte HWcn, ColoredText msg)> handler)
                        {
                            originalHandler = handler;
                            SetModuleConfigResponse -= handler;  // Detach the original handler
                        }
                    }
                }

                SetModuleConfigResponse += CNMaster_SetModuleConfigResponse;
                for (int HWcn = 0; HWcn < ModuleInfos.Count; HWcn++)
                {
                    if (ModuleInfos[HWcn].ModuleType != EnModuleType.cModuleTypeEmpty)
                    {
                        SetModuleConfig(HWcn);
                        numChanSet++;
                    }
                }

                cntIncomingdata = 0;
                DateTime timeout = DateTime.Now + TimeSpan.FromMilliseconds(WaitCommandResponseTimeOutMs);

                // Asynchronously poll for incoming data until all expected data is received or timeout is reached
                while (cntIncomingdata < numChanSet && DateTime.Now < timeout)
                {
                    // Delay to avoid a tight polling loop and allow other asynchronous tasks to run
                    await Task.Delay(50);
                }

                ColoredText msg;
                if (cntIncomingdata < numChanSet)
                {
                    // Timeout occurred
                    msg = new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: Timeout", Color.Red);
                }
                else
                {
                    msg = !SetDeviceConfigAsync_Success
                        ? new ColoredText($"{EnNeuromasterCommand.SetDeviceConfig}: Failed", Color.Red)
                        : new ColoredText($"{EnNeuromasterCommand.SetDeviceConfig}: OK", Color.Green);

                    if (ModuleInfos is not null)
                        OnSetDeviceConfigResponse(SetDeviceConfigAsync_Success, msg);
                }
            }
            finally
            {
                // Detach the temporary handler
                SetModuleConfigResponse -= CNMaster_SetModuleConfigResponse;

                // Restore the original handler, if any
                if (originalHandler != null)
                {
                    SetModuleConfigResponse += originalHandler;
                }
            }

            // Await completion of the operation
            await tcs.Task;
        }

        private void CNMaster_SetModuleConfigResponse(object? sender, (bool success, byte HWcn, ColoredText msg) e)
        {
            cntIncomingdata++;
            if (!SetDeviceConfigAsync_Success) SetDeviceConfigAsync_Success = false;
        }

        private void GetModuleConfig(int HWcn)
        {
            CommandRequest crequ = new()
            {
                HWcn = (byte)HWcn
            };
            SendCommand(EnNeuromasterCommand.GetModuleConfig, [(byte)HWcn], crequ);
        }

        private int cntIncomingdata = 0;
        private CModuleBase? incomingCmb;
        /// <summary>
        /// Gets ConfigModules and puts result into Device
        /// </summary>
        public async Task GetDeviceConfigAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            allDeviceConfigData.Clear();

            // Subscribe to the response event
            GetModuleConfigResponse += CNMaster_GetModuleConfigResponse;

            // Initiate the configuration process for each module
            for (int hwCn = 0; hwCn < MaxNumHWChannels; hwCn++)
            {
                GetModuleConfig(hwCn); // Trigger each device configuration
            }

            cntIncomingdata = 0;
            bool failed = false;
            DateTime timeout = DateTime.Now + TimeSpan.FromMilliseconds(WaitCommandResponseTimeOutMs);

            // Asynchronously poll for incoming data until all expected data is received or timeout is reached
            while (cntIncomingdata < MaxNumHWChannels && DateTime.Now < timeout)
            {
                // Delay to avoid a tight polling loop and allow other asynchronous tasks to run
                await Task.Delay(10);
            }

            // Unsubscribe from the response event after processing is complete
            GetModuleConfigResponse -= CNMaster_GetModuleConfigResponse;

            ColoredText msg;
            if (cntIncomingdata < MaxNumHWChannels)
            {
                // Timeout occurred
                msg = new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: Timeout", Color.Red);
            }
            else
            {
                msg = failed
                    ? new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: Failed", Color.Red)
                    : new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: OK", Color.Green);

                if (ModuleInfos is not null)
                    OnGetDeviceConfigResponse(ModuleInfos, msg);
            }

            // Complete the TaskCompletionSource based on the result
            tcs.SetResult(cntIncomingdata >= MaxNumHWChannels);

            // Await completion of the operation
            await tcs.Task;
        }

        private void CNMaster_GetModuleConfigResponse(object? sender, (CModuleBase? cmb, ColoredText msg) e)
        {
            cntIncomingdata++;
            incomingCmb = e.cmb;
        }


        /// <summary>
        /// Sends Command to module and
        /// Reads data back: ByteIn.Length defines number of bytes to read back
        /// ByteOut can be null
        /// </summary>
        /// <param name="Hwcn">HW channel number</param>
        /// <param name="ModuleCommand">Command Code</param>
        /// <param name="ByteOut">Byytes to send</param>
        /// <param name="ByteIn">Bytes to receive</param>
        /// <returns></returns>
        protected void ModuleCommand(byte Hwcn, EnModuleCommand ModuleCommand, byte[] ByteOut, byte numByteIn)
        {
            int l = ByteOut?.Length ?? 0;

            byte[] OutBuf = new byte[l + 4];
            OutBuf[0] = Hwcn;
            OutBuf[1] = (byte)(l + 1);
            OutBuf[2] = numByteIn;
            OutBuf[3] = (byte)ModuleCommand;
            if (ByteOut != null && ByteOut.Length > 0)
            {
                Buffer.BlockCopy(ByteOut, 0, OutBuf, 4, l);
            }

            CommandRequest cr = new()
            {
                ModuleCommand = ModuleCommand,
                HWcn = Hwcn
            };
            SendCommand(EnNeuromasterCommand.WrRdModuleCommand, OutBuf, cr);
        }
        #endregion

        #region Module Specific Functions
        /// <summary>
        /// Returns the electrode information only if the module is ADS1292 based
        /// </summary>
        /// <param name="ElectrodeImp">The electrode imp.</param>
        /// <param name="HWcn">Hardware Channel number</param>
        /// <returns>
        /// Electrode information or null if not appropriate or something went werng
        /// </returns>
        public static void GetElectrodeInfo(ref CADS1294x_ElectrodeImp ElectrodeImp, int HWcn)
        {
            /*
            bool ret = true;
            if (Device?.ModuleInfos[HWcn].ModuleType_Unmodified == enumModuleType.cModuleExGADS94)
            {
                if (SetModuleInfoSpecific(HWcn, 128)) //Kanalnummer muss nur >= 128 sein
                {
                    CDelay.Delay_ms(1000);   //Damit Impedanzmessung fertig wird dauert ca 600ms

                    //4x lesen
                    for (int i = 0; i < 4; i++)
                    {
                        //ToDo: Debug
                        byte[] btin = []; //GetModuleInfoSpecific(HWcn, false);
                        if (btin != null)
                        {
                            ElectrodeImp.Update(btin);
                        }
                        else
                            ret = false;
                    }
                }
            }*/
        }


        /// <summary>
        /// Sets the Module specific information
        /// </summary>
        public bool SetModuleSpecific(int HWcn, int get_Imp_chan_x = -1)
        {
            bool ret = false;

            if (ModuleInfos[HWcn].ModuleType == EnModuleType.cModuleTypeEmpty)
                return ret;


            byte[] Data = ModuleInfos[HWcn].GetModuleSpecific();

            if (get_Imp_chan_x >= 0)
            {
                Data[0] = (byte)get_Imp_chan_x;
            }

            //Data[Data.Length - 1] = ...
            Data[^1] = CRC8.Calc_CRC8(Data, Data.Length - 2);
            ModuleCommand((byte)HWcn, EnModuleCommand.SetModuleSpecific, Data, 1);

            return ret;
        }

        private bool UpdateModuleInfo = false;
        private byte HWcnGetModuleInfoSpecific = 0xff;
        /// <summary>
        /// Gets the Module specific information
        /// </summary>
        public void GetModuleSpecific(int HWcn, bool UpdateModuleInfo)
        {
            ModuleCommand((byte)HWcn, EnModuleCommand.GetModuleSpecific, [(byte)HWcn], 17);
            this.UpdateModuleInfo = UpdateModuleInfo;
            HWcnGetModuleInfoSpecific = (byte)HWcn;
        }

        #endregion
    }
}
