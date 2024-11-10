using FeedbackDataLib.Modules;
using System.Collections.Concurrent;
using WindControlLib;
using EnNeuromasterCommand = FeedbackDataLib.C8KanalReceiverCommandCodes.EnNeuromasterCommand;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    /// 

    public partial class C8CommBase
    {
        private int cntDeviceConfigs = 0;
        private readonly List<byte> allDeviceConfigData = [];
        private readonly ConcurrentQueue<byte[]> getModulesqueue = new();


        public event EventHandler<List<CDataIn>>?  DataReadyResponse;
        /// <summary>
        /// Data Ready
        /// </summary>
        /// <remarks>
        /// Limits of the Data Range:
        /// 0x0000 ... input amplifier is neg saturated
        /// 0xFFFF ... input amplifier is pos saturated
        /// if output stage is saturated, 0x0001 and 0xFFFE
        /// </remarks>
        protected virtual void OnDataReadyResponse(List<CDataIn>? DataRead)
        {
            if (DataRead != null)
                DataReadyResponse?.Invoke(this, DataRead);
        }

        #region Events_related_to_Commands
        public event EventHandler<(CNMFirmwareVersion?, ColoredText msg)>? GetFirmwareVersionResponse;
        protected virtual void OnGetFirmwareVersionResponse(CNMFirmwareVersion? fw, ColoredText msg)
        {
            var handler = GetFirmwareVersionResponse;
            handler?.Invoke(this, (fw, msg));
        }

        public event EventHandler<(List<CModuleBase>? cmb, ColoredText msg)>? GetDeviceCongigResponse;
        protected virtual void OnGetDeviceConfigResponse(List<CModuleBase> cmb, ColoredText msg)
        {
            var handler = GetDeviceCongigResponse;
            handler?.Invoke(this, (cmb, msg));
        }

        // Event for GetClock with DateTime response
        public event EventHandler<(DateTime? clock, ColoredText msg)>? GetClockResponse;

        protected virtual void OnGetClockResponse(DateTime? clock, ColoredText msg)
        {
            var handler = GetClockResponse;
            handler?.Invoke(this, (clock, msg));
        }

        // Event for SetClock with bool response
        public event EventHandler<(bool success, ColoredText msg)>? SetClockResponse;

        protected virtual void OnSetClockResponse(bool success, ColoredText msg)
        {
            var handler = SetClockResponse;
            handler?.Invoke(this, (success, msg));
        }

        // Event for SetConnectionClosed with bool response
        public event EventHandler<(bool isClosed, ColoredText msg)>? SetConnectionClosedResponse;

        protected virtual void OnSetConnectionClosedResponse(bool isClosed, ColoredText msg)
        {
            var handler = SetConnectionClosedResponse;
            handler?.Invoke(this, (isClosed, msg));
        }

        // Event for ScanModules with bool response
        public event EventHandler<(bool isSuccessful, ColoredText msg)>? ScanModulesResponse;

        protected virtual void OnScanModulesResponse(bool isSuccessful, ColoredText msg)
        {
            var handler = ScanModulesResponse;
            handler?.Invoke(this, (isSuccessful, msg));
        }

        // Event for ScanModules with bool response
        public event EventHandler<(byte[] response, ColoredText msg)>? GetModuleConfigResponse;

        protected virtual void OnGetModuleConfigResponse(byte[] response, ColoredText msg)
        {
            var handler = GetModuleConfigResponse;
            handler?.Invoke(this, (response, msg));
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
            if (RS232Receiver is null) return false;
            return RS232Receiver.IsConnected;
        }
        #endregion

        #region Commands_to_NM
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

        /// <summary>
        /// Sets configuration of all SW Channels of one Module
        /// </summary>
        /// <param name="HWcn">Hardware channel number</param>
        /// <returns>true if successful</returns>
        /// <remarks>Checks if ModuleType != cModuleTypeEmpty</remarks>
        public bool SetModuleConfig(int HWcn)
        {
            if (HWcn == 0xff || Device is null)
            {
                return false;
            }

            var moduleInfo = Device.ModuleInfos[HWcn];
            if (moduleInfo.ModuleType == enumModuleType.cModuleTypeEmpty)
            {
                return false;
            }

            SendCommand(EnNeuromasterCommand.SetModuleConfig, moduleInfo.Get_SWConfigChannelsByteArray());
            return true;
        }


        /// <summary>
        /// Sets configuration of all Modules
        /// according to Device.ModuleInfos
        /// </summary>
        /// <returns></returns>
        public bool SetConfigAllModules()
        {
            if (Device is null) return false;

            for (int HWcn = 0; HWcn < Device.ModuleInfos.Count; HWcn++)
            {
                if (!SetModuleConfig(HWcn)) return false;
            }
            return true;
        }

        private void GetModuleConfig(int HWcn)
        {
            SendCommand(EnNeuromasterCommand.GetModuleConfig, [(byte) HWcn]);
        }

        /// <summary>
        /// Gets ConfigModules and puts result into Device
        /// </summary>
        private async Task GetDeviceConfigAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            cntDeviceConfigs = max_num_HWChannels;
            allDeviceConfigData.Clear();
            getModulesqueue.Clear();

            // Subscribe to the response event
            GetModuleConfigResponse += C8KanalReceiverV2_CommBase_GetModuleConfigResponse;

            // Initiate the configuration process for each module
            for (int hwCn = 0; hwCn < max_num_HWChannels; hwCn++)
            {
                // Trigger each device configuration retrieval here
                GetModuleConfig(hwCn);
            }

            int cntIncomingdata = 0;
            bool failed = false;
            DateTime timeout = DateTime.Now + TimeSpan.FromMilliseconds(WaitCommandResponseTimeOutMs);

            // Asynchronously poll for incoming data until all expected data is received or timeout is reached
            while (cntIncomingdata < max_num_HWChannels && DateTime.Now < timeout)
            {
                // Check if data is available in the queue
                while (!getModulesqueue.IsEmpty)
                {
                    if (getModulesqueue.TryDequeue(out var responseData))
                    {
                        if (responseData == null)
                        {
                            failed = true;
                            responseData = Array.Empty<byte>();
                        }

                        allDeviceConfigData.AddRange(responseData);
                        cntIncomingdata++;
                    }
                }

                // Delay to avoid a tight polling loop and allow other asynchronous tasks to run
                await Task.Delay(10);
            }

            // Unsubscribe from the response event after processing is complete
            GetModuleConfigResponse -= C8KanalReceiverV2_CommBase_GetModuleConfigResponse;

            ColoredText msg;
            if (cntIncomingdata < max_num_HWChannels)
            {
                // Timeout occurred
                msg = new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: Timeout", Color.Red);
                OnGetDeviceConfigResponse([], msg);
            }
            else
            {
                // Process the received data
                Device ??= new C8Device();
                Device.UpdateModuleInfoFromByteArray(allDeviceConfigData.ToArray());
                Device.Calculate_SkalMax_SkalMin(); // Calculate max and mins

                msg = failed
                    ? new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: Failed", Color.Red)
                    : new ColoredText($"{EnNeuromasterCommand.GetDeviceConfig}: OK", Color.Green);

                if (Device?.ModuleInfos is not null)
                    OnGetDeviceConfigResponse(Device.ModuleInfos, msg);
            }

            // Complete the TaskCompletionSource based on the result
            tcs.SetResult(cntIncomingdata >= max_num_HWChannels);

            // Await completion of the operation
            await tcs.Task;
        }

        private void C8KanalReceiverV2_CommBase_GetModuleConfigResponse(object? sender, (byte[] response, ColoredText msg) e)
        {
            getModulesqueue.Enqueue(e.response);
        }

        /// <summary>
        /// Sends Command to specific module and
        /// Reads data back: ByteIn.Length defines number of bytes to read back
        /// ByteOut can be null
        /// </summary>
        /// <param name="Hwcn">HW channel number</param>
        /// <param name="ModuleCommand">Command Code</param>
        /// <param name="ByteOut">Byytes to send</param>
        /// <param name="ByteIn">Bytes to receive</param>
        /// <returns></returns>
        protected void ModuleCommand(byte Hwcn, byte ModuleCommand, byte[] ByteOut, byte numByteIn)
        {
            int l = ByteOut?.Length ?? 0;

            byte[] OutBuf = new byte[l + 4];
            OutBuf[0] = Hwcn;
            OutBuf[1] = (byte)(l + 1);
            OutBuf[2] = numByteIn;
            OutBuf[3] = ModuleCommand;
            if (ByteOut != null && ByteOut.Length > 0)
            {
                Buffer.BlockCopy(ByteOut, 0, OutBuf, 4, l);
            }
            //Anzahl der bytes die zu lesen sind
            //OutBuf[OutBuf.Length - 1] = (byte)ByteIn.Length;
            SendCommand(EnNeuromasterCommand.WrRdModuleCommand, OutBuf);
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
        public bool SetModuleInfoSpecific(int HWcn, int get_Imp_chan_x = -1)
        {
            bool ret = false;

            if (Device?.ModuleInfos[HWcn].ModuleType == enumModuleType.cModuleTypeEmpty)
                return ret;

            if (Device is not null)
            {
                byte[] Data = Device.ModuleInfos[HWcn].GetModuleSpecific();

                if (get_Imp_chan_x >= 0)
                {
                    Data[0] = (byte)get_Imp_chan_x;
                }

                //Data[Data.Length - 1] = ...
                Data[^1] = CRC8.Calc_CRC8(Data, Data.Length - 2);
                ModuleCommand((byte)HWcn, C8KanalReceiverCommandCodes.cModuleSetInfoSpecific, Data, 1);
            }
            return ret;
        }

        private bool UpdateModuleInfo = false;
        private byte HWcnGetModuleInfoSpecific = 0xff;
        /// <summary>
        /// Gets the Module specific information
        /// </summary>
        public void GetModuleInfoSpecific(int HWcn, bool UpdateModuleInfo)
        {
            ModuleCommand((byte)HWcn, (byte)C8KanalReceiverCommandCodes.EnModuleCommand.ModuleGetInfoSpecific, [(byte) HWcn], 17);
            this.UpdateModuleInfo = UpdateModuleInfo;
            HWcnGetModuleInfoSpecific = (byte) HWcn;
        }

        #endregion
    }
}
