using FeedbackDataLib.Modules;
using System.Diagnostics;
using EnNeuromasterCommand = FeedbackDataLib.C8KanalReceiverCommandCodes.EnNeuromasterCommand;
using static FeedbackDataLib.CRS232Receiver2;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    public partial class C8KanalReceiverV2_CommBase
    {
        private void RS232Receiver_CommandProcessed(object? sender, CommandProcessedEventArgs e)
        {
            void SendSuccess()
            {
                if (e == null || e.ResponseData == null) return;
                if (e.Success)
                {
                    OnCommandProcessedResponse(new(e.Command, "OK", Color.Green, true, e.ResponseData));
                }
                else
                {
                    OnCommandProcessedResponse(new(e.Command, "Failed", Color.Red, false, e.ResponseData));
                }
            }

            switch (e.Command)
            {
                case EnNeuromasterCommand.SetClock:
                    SendSuccess();
                    break;

                case EnNeuromasterCommand.GetClock:
                    if (e.Success)
                    {
                        DeviceClock.UpdateFrom_ByteArray(e.ResponseData, 0);
                    }
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.SetConnectionClosed:
                    break;
                case EnNeuromasterCommand.ChannelSync:
                    break;
                case EnNeuromasterCommand.Reset:
                    break;
                case EnNeuromasterCommand.GetFirmwareVersion:
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.SetModuleConfig:
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.GetChannelConfig:
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.WrRdModuleCommand:
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.GetModuleConfig:
                    //Collect data of all HW channels
                    cntDeviceConfigs--;
                    if (e.ResponseData != null)
                        allDeviceConfigData.AddRange(e.ResponseData);

                    if (cntDeviceConfigs == 0)
                    {
                        //All data in
                        try
                        {
                            Device ??= new C8KanalDevice2();
                            Device.UpdateModuleInfoFromByteArray([.. allDeviceConfigData]);
                            Device.Calculate_SkalMax_SkalMin(); // Calculate max and mins
                            OnCommandProcessedResponse(new(EnNeuromasterCommand.GetDeviceConfig, "OK", Color.Green, true, [.. allDeviceConfigData]));

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("C8KanalReceiverV2_CommBase_#01: " + ex.Message);
                            e.Success = false;
                            OnCommandProcessedResponse(new(EnNeuromasterCommand.GetDeviceConfig, "Failed", Color.Red, false, []));
                            Device = null;
                        }
                    }
                    break;
                case EnNeuromasterCommand.ScanModules:
                    SendSuccess();
                    break;
                case EnNeuromasterCommand.Module_GetSpecific:
                    if (e.Success)
                    {
                        try
                        {
                            byte[] btin = new byte[CModuleBase.ModuleSpecific_sizeof];
                            Buffer.BlockCopy(e.ResponseData, 1, btin, 0, CModuleBase.ModuleSpecific_sizeof);
                            //12.11.2020 Check CRC
                            string s = "GetModuleInfoSpecific: ";
                            for (int i = 0; i < btin.Length; i++)
                            {
                                s += btin[i].ToString("X2") + ", ";
                            }
                            byte crc = CRC8.Calc_CRC8(btin, btin.Length - 2);

                            s += "    /  CalcCRC=" + crc.ToString("X2");
                            Debug.WriteLine(s);

                            if (Device is not null && UpdateModuleInfo)
                                Device.ModuleInfos[HWcnGetModuleInfoSpecific].SetModuleSpecific(btin);
                            
                            OnCommandProcessedResponse(new(e.Command, "OK", Color.Green, true, e.ResponseData, HWcnGetModuleInfoSpecific));
                        }
                        catch
                        {
                        }
                    }

                    SendSuccess();
                    break;
                case EnNeuromasterCommand.Module_SetSpecific:



                    SendSuccess();
                    break;
            }
        }

        protected byte[] BuildNMCommand(EnNeuromasterCommand neuromasterCommand, byte[]? additionalData)
        {
            // Validate AdditionalDataToSend size early
            additionalData ??= [];
            if (additionalData.Length > 250)
            {
                throw new ArgumentException("Size of AdditionalDataToSend must be <= 250", nameof(additionalData));
            }

            const int overhead = 4; // CommandCode, Command, Length, CRC
            int bytestosend = overhead + additionalData.Length;
            byte[] buf = new byte[bytestosend];

            // Build the command buffer
            buf[0] = C8KanalReceiverCommandCodes.CommandCode;  // Add the base command code
            buf[1] = (byte) neuromasterCommand;                    // Add the specific command code
            buf[2] = (byte)(additionalData.Length + 1);  // Length byte (+CRC)

            // Copy additional data into the buffer
            Buffer.BlockCopy(additionalData, 0, buf, overhead - 1, additionalData.Length);

            // Calculate and set CRC
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 1);
            return buf;
        }

        private void SendCommand(EnNeuromasterCommand neuromasterCommand, byte[]? additionalData)
        {
            RS232Receiver?.ProcessCommand(neuromasterCommand, BuildNMCommand(neuromasterCommand, additionalData));
        }

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

        private int cntDeviceConfigs = 0;
        List<byte> allDeviceConfigData = [];


        /// <summary>
        /// Gets ConfigModules and puts result into Device
        /// </summary>
        public virtual void GetDeviceConfig()
        {
            cntDeviceConfigs = max_num_HWChannels;
            allDeviceConfigData.Clear();
            // Get Info Module by Module
            for (int hwCn = 0; hwCn < max_num_HWChannels; hwCn++)
            {
                GetModuleConfig(hwCn);
            }
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
                ModuleCommand((byte)HWcn, C8KanalReceiverCommandCodes.cModule_SetSpecific, Data, 1);
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
            ModuleCommand((byte)HWcn, (byte) EnNeuromasterCommand.Module_GetSpecific, [(byte) HWcn], 17);
            this.UpdateModuleInfo = UpdateModuleInfo;
            HWcnGetModuleInfoSpecific = (byte) HWcn;
        }

        #endregion
    }
}
