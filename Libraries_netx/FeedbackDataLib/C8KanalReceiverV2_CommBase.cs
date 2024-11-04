using BMTCommunicationLib;
using FeedbackDataLib.Modules;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    public class C8KanalReceiverV2_CommBase
    {

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        protected CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);    //10.1.2013

        /// <summary>
        /// Converter for Device clock
        /// </summary>
        protected CCDateTime _DeviceClock = new();

        /// <summary>
        /// TimeOut [ms] in WaitCommandResponse
        /// </summary>
        protected const int WaitCommandResponseTimeOut = 3000;

        /// <summary>
        /// RS232Receiver
        /// </summary>
        public CRS232Receiver2? RS232Receiver = null;

        /// <summary>
        /// All Information about 8 Channel Device
        /// </summary>
        /// <remarks>Zugriff: [0,1,...]</remarks>
        public C8KanalDevice2? Device = new();       //Fasst die verfügbaren Kanäle zusammen

        /// <summary>
        /// NM Battery Status in %
        /// </summary>
        public uint BatteryPercentage { get; private set; } = 0;

        /// <summary>
        /// The number of SW channels sent by Neuromaster
        /// </summary>
        public const int num_SWChannels_sent_by_HW = 4;

        /// <summary>
        /// That is the maximum number of SW Channels one module can have
        /// </summary>
#if VIRTUAL_EEG
        public const int max_num_SWChannels = 12;
#else
        public const int max_num_SWChannels = 4;

#endif

        /// <summary>
        /// That is the maximum number od HW Channels one module can have
        /// </summary>
        public const int max_num_HWChannels = 7;

        /// <summary>
        /// Default Multisensor Channel No
        /// </summary>
        public const byte Default_Multisensor_Channel_No = 1;

        /// <summary>
        /// NM sends in this ms Interval the SYNC Signal
        /// </summary>
        public const int SyncInterval_ms = 1000;

        /// <summary>
        /// NM sends in this Interval the SYNC Signal
        /// </summary>
        public TimeSpan SyncInterval = new(0, 0, 0, 0, SyncInterval_ms);

        /// <summary>
        /// Minimum hex Value of SCL Measurement - to Calculate Min and Max
        /// </summary>
        public const int minSCLhexValue = 5000;

        /// <summary>
        /// Calculation of the Battery percentage
        /// </summary>
        private readonly CBatteryVoltage BatteryVoltage = new();

        public string ComPortName { get; private set; } = "";


        /// <summary>
        /// Enables or disables the DataReady event
        /// </summary>
        public bool EnableDataReadyEvent
        {
            get
            {
                if (RS232Receiver != null)
                {
                    return RS232Receiver.EnableDataReadyEvent;
                }
                return false;
            }
            set
            {
                if (RS232Receiver != null)
                {
                    RS232Receiver.EnableDataReadyEvent = value;
                }
            }
        }

        //Events
        #region Events

        /// <summary>
        /// Data Ready
        /// </summary>
        /// <remarks>
        /// Limits of the Data Range:
        /// 0x0000 ... input amplifier is neg saturated
        /// 0xFFFF ... input amplifier is pos saturated
        /// if output stage is saturated, 0x0001 and 0xFFFE
        /// </remarks>
        public event DataReadyEventHandler? DataReady = null;
        protected virtual void OnDataReady(List<CDataIn> DataIn)
        {
            //Add Virtual ID / added Dec. 2013
            if (Device?.ModuleInfos != null)
            {
                foreach (CDataIn cdi in DataIn)
                {
                    cdi.VirtualID = Device.ModuleInfos[cdi.HW_cn].SWChannels[cdi.SW_cn].VirtualID;
                }
            }
            DataReady?.Invoke(this, DataIn);
        }

        /// <summary>
        /// Device is communicating back to PC
        /// </summary>
        /// <param name="buf">The buf.</param>
        protected virtual void OnDeviceCommunicationToPC(byte[] buf)
        {
            switch (buf[1])
            {
                case C8KanalReceiverCommandCodes.CNMtoPCCommands.cModuleError:
                    {
                        if (buf.Length > 2)
                            OnDeviceToPC_ModuleError(buf[2]);
                        break;
                    }

                case C8KanalReceiverCommandCodes.CNMtoPCCommands.cBufferFull:
                    {
                        OnDeviceToPC_BufferFull();
                        break;
                    }
                case C8KanalReceiverCommandCodes.CNMtoPCCommands.cBatteryStatus:
                    {
                        //buf[2] ... Battery Low    
                        //buf[3] ... Battery High [1/10V]
                        //buf[4] ... Supply Low
                        //buf[5] ... Supply High  [1/10V] 

                        uint BatteryVoltage_mV = buf[3];
                        BatteryVoltage_mV = ((BatteryVoltage_mV << 8) + buf[2]) * 10;


                        uint SupplyVoltage_mV = buf[5];
                        SupplyVoltage_mV = ((SupplyVoltage_mV << 8) + buf[4]) * 10;

                        BatteryPercentage = (uint)BatteryVoltage.GetPercentage(((double)BatteryVoltage_mV) / 1000);

                        OnDeviceToPC_BatteryStatus(BatteryVoltage_mV, BatteryPercentage, SupplyVoltage_mV);
                        break;
                    }
                case C8KanalReceiverCommandCodes.CNMtoPCCommands.cNMOffline:   //28.7.2014
                    {
                        Close();  //Kommumikation beenden
                        break;
                    }
            }
        }
        /*
         * Events for communication back to PC
         */
        public delegate void DeviceToPC_BufferFullEventHAndler();
        /// <summary>
        /// Buffer of the Device is full, it stops sampling, has to be reinitialised
        /// </summary>
        public event DeviceToPC_BufferFullEventHAndler? DeviceToPC_BufferFull;
        protected virtual void OnDeviceToPC_BufferFull()
        {
            DeviceToPC_BufferFull?.Invoke();
        }

        public delegate void DeviceToPC_ModuleErrorEventHandler(byte HW_cn);
        /// <summary>
        /// Buffer of the Device is full, it stops sampling, has to be reinitialised
        /// </summary>
        public event DeviceToPC_ModuleErrorEventHandler? DeviceToPC_ModuleError;
        protected virtual void OnDeviceToPC_ModuleError(byte HW_cn)
        {
            DeviceToPC_ModuleError?.Invoke(HW_cn);
        }

        public delegate void DeviceToPC_BatteryStatusEventHandler(uint Battery_Voltage_mV, uint percentage, uint Supply_Voltage_mV);
        /// <summary>
        /// Neuromaster sends battery status
        /// </summary>
        public event DeviceToPC_BatteryStatusEventHandler? DeviceToPC_BatteryStatus;
        /// <summary>
        /// Battery Status
        /// </summary>
        /// <param name="Battery_Voltage_mV">Voltage [mV]</param>
        /// <param name="percentage">Percentage</param>
        /// <param name="Supply_Voltage_mV">Supply Voltage</param>
        protected virtual void OnDeviceToPC_BatteryStatus(uint Battery_Voltage_mV, uint percentage, uint Supply_Voltage_mV)
        {
            DeviceToPC_BatteryStatus?.Invoke(Battery_Voltage_mV, percentage, Supply_Voltage_mV);
        }

        public delegate void Vaso_InfoSpecific_UpdatedEventHandler();
        /// <summary>Occurs when Module Specific Information from Vaso is updated.</summary>
        public event Vaso_InfoSpecific_UpdatedEventHandler? Vaso_InfoSpecific_Updated;
        protected virtual void OnVaso_InfoSpecific_Updated()
        {
            Vaso_InfoSpecific_Updated?.Invoke();
        }


        #endregion


        /// <summary>
        /// Do not call
        /// </summary>
        public C8KanalReceiverV2_CommBase()
        {
            //Base constructor must be empty that the derived class does not call 
            Device = new C8KanalDevice2();
            _DeviceClock = new CCDateTime();
            BatteryVoltage = new CBatteryVoltage();
        }

        /// <param name="ComPortName">
        /// "COM1","COM2
        /// </param>
        public C8KanalReceiverV2_CommBase(string ComPortName)
        {
            C8KanalReceiverV2_Construct();
            this.ComPortName = ComPortName;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public virtual void Close()
        {
            //SendCloseConnection();
            RS232Receiver?.Close();  //1st Close
        }

        /// <summary>
        /// Connect to device
        /// </summary>
        public void Connect_via_tryToConnectWorker()
        {
            RS232Receiver?.Connect_via_tryToConnectWorker();
        }

        /// <summary>
        /// Called by constructors
        /// </summary>
        public virtual void C8KanalReceiverV2_Construct()
        {
            //Log4Net anlegen
            //FileAppender logFile = new FileAppender();
            //logFile.File = Path.GetDirectoryName(Assembly.GetAssembly(typeof(C8KanalReceiverV2_CommBase)).Location) + "\\" + "C8KanalReceiverV2.log";
            //logFile.AppendToFile = true;
            //logFile.Encoding = Encoding.UTF8;
            //logFile.Layout = new PatternLayout("%date [%thread] %-5level [%logger] %message%newline");
            /* Hirarchie
                * ALL
                * DEBUG
                * INFO
                * WARN
                * ERROR
                * FATAL
                * OFF
             */
            //logFile.Threshold = log4net.Core.Level.Off;
            //logFile.ActivateOptions();
            //BasicConfigurator.Configure(logFile);

            if (RS232Receiver == null)
                throw new Exception("RS232Receiver mustbe created before calling constructor");
            RS232Receiver.DataReadyComm += new DataReadyEventHandler(RS232Receiver_DataReadyComm);
            RS232Receiver.DeviceCommunicationToPC += new DeviceCommunicationToPCEventHandler(RS232Receiver_DeviceCommunicationToPC);
        }

        /// <summary>
        /// DeviceCommunicationToPC from RS232 Receiver
        /// </summary>
        /// <remarks>
        /// Only forward event
        /// </remarks>
        protected void RS232Receiver_DeviceCommunicationToPC(object sender, byte[] buf)
        {
            OnDeviceCommunicationToPC(buf);
        }

        //For RS232Receiver_DataReadyComm
        private int cntSyncPackages = 0;                        //Counts the incoming sync packages
        private DateTime OldLastSync = DateTime.MinValue;       //Time when the previous package was received
        private DateTime ReceivingStarted = DateTime.MinValue;  //Receiving started at

        /// <summary>
        /// Receives data, takes care of over all data synchronicity
        /// Updates time and forwards event
        /// </summary>
        /// <remarks>
        /// Only forwards data if the first Sync Value is received
        /// </remarks>
        protected virtual void RS232Receiver_DataReadyComm(object sender, List<CDataIn> DataIn)
        {
            if (Device is null) return;

            List<CDataIn> _DataIn = [];
            foreach (CDataIn di in DataIn)
            {
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
                    Device.ModuleInfos[di.HW_cn].SWChannels[di.SW_cn].SWChan_Started = ReceivingStarted;
                    Device.ModuleInfos[di.HW_cn].SWChannels[di.SW_cn].SynPackagesreceived = cntSyncPackages;
                }

                Device.UpdateTime(di);

                if (di.ChannelStarted != DateTime.MinValue)
                {
                    //Process Data Module specific
                    _DataIn.AddRange(Device.ModuleInfos[di.HW_cn].Processdata(di));
                }
            }

            if (_DataIn.Count > 0)
            {
                OnDataReady(_DataIn);
            }
        }

        /// <summary>
        /// Counts how many data packets per second are sent via XBee channel
        /// </summary>
        /// <returns></returns>
        public int GetChannelCapcity()
        {
            if (Device is null) return 0;
            double ret = 0;
            for (int HW_cn = 0; HW_cn < Device.ModuleInfos.Count; HW_cn++)
            {
                for (int SW_cn = 0; SW_cn < Device.ModuleInfos[HW_cn].SWChannels.Count; SW_cn++)
                {
                    if (Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SendChannel == true)
                    {
                        //Count data packets per second
                        double d = Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SampleInt;
                        if (d > 0) d = 1000 / d; //1/(d/1000)
                        ret += d;
                    }
                }
            }
            return Convert.ToInt32(ret);
        }


        /// <summary>
        /// Gets the scaled value of any CDataIn packet
        /// </summary>
        /// <param name="DataIn">DataIn packet</param>
        /// <returns>
        /// Scaled Value
        /// </returns>
        public double GetScaledValue(CDataIn DataIn)
        {
            if (Device == null) return 0;
            int hwcn = DataIn.HW_cn;// & 0xf0) >> 4;
            //int swcn = (DataIn.HWChannelNumber & 0x0f);
            int swcn = DataIn.SW_cn;
            // ;ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d
            //d = d - Device.ModuleInfos[hwcn].SWChannels[swcn].Offset_hex;
            //d = d * Device.ModuleInfos[hwcn].SWChannels[swcn].SkalValue_k + Device.ModuleInfos[hwcn].SWChannels[swcn].Offset_d;
            double ret = Device.ModuleInfos[hwcn].SWChannels[swcn].GetScaledValue(DataIn.Value);
            return ret;
        }

        /// <summary>
        /// Converts CDataIn channel date to double arrays
        /// Rebuilds time base, equidistantly, from Sample Int
        /// Synchronices starting point, even if time series have different starting point
        /// and differnt length
        /// </summary>
        /// <param name="data_in">Input data</param>
        /// <param name="data_time">Time series according to Data_in</param>
        /// <param name="data_value_scaled">Values scaled series according to Data_in</param>
        /// <remarks>
        /// if data channels data_in do not have a common starting point data_time and data_value_scaled are returned as null
        /// </remarks>
        public void ConvCDataIn_to_Double_arrays(List<List<CDataIn>> data_in, out List<List<double>> data_time, out List<List<double>> data_value_scaled)
        {
            int idx_highestSampleInt = 0;
            int si_max = 0;
            DateTime dt_absolute = data_in[0][0].DT_absolute;
            DateTime earliestStartingTime = dt_absolute;

            int idx_latestEndTime = 0;
            DateTime latestEndTime = data_in[0][^1].DT_absolute;

            data_value_scaled = [];
            data_time = [];
            if (Device is null) return;

            for (int i = 0; i < data_in.Count; i++)
            {
                //Find channel with highest sample interval
                ushort si = Device.ModuleInfos[data_in[i][0].HW_cn].SWChannels[data_in[i][0].SW_cn].SampleInt;
                if (si > si_max)
                {
                    si_max = si;
                    idx_highestSampleInt = i;
                }

                //Check Starting times of channels ... for synchronisation
                //Find the channel with earlierst starting time
                if (data_in[i][0].DT_absolute > earliestStartingTime)
                {
                    earliestStartingTime = data_in[i][0].DT_absolute;
                }

                //Check Ending times of channels ... 
                //Find the channel with latest ending time
                if (data_in[i][^1].DT_absolute < latestEndTime)
                {
                    idx_latestEndTime = i;
                    latestEndTime = data_in[i][^1].DT_absolute;
                }
            }

            //Find end indizes of channels
            int[] EndIndizes = new int[data_in.Count];
            EndIndizes[idx_latestEndTime] = data_in[idx_latestEndTime].Count - 1;
            for (int i = 0; i < data_in.Count; i++)
            {
                if (i != idx_latestEndTime)
                {
                    int j = data_in[i].Count - 1;
                    while (j >= 0)
                    {
                        if (data_in[i][j].DT_absolute > latestEndTime)
                        {
                            j--;
                        }
                        else
                        {
                            EndIndizes[i] = j;
                            break;
                        }
                    }
                }
            }


            //Find nearest time reference point in the channel with the lowest sample rate
            int refIndex = 0; //This point is set as starting point
            while (data_in[idx_highestSampleInt][refIndex].DT_absolute < earliestStartingTime)
            {
                refIndex++;
                if (refIndex >= data_in[idx_highestSampleInt].Count)
                    break;
            }


            //find StartingIndizes closest to refPoint
            if (refIndex < data_in[idx_highestSampleInt].Count)
            {
                DateTime refPoint = data_in[idx_highestSampleInt][refIndex].DT_absolute;

                //Find indizes of other channels closest to the refPoint
                int[] StartingIndizes = new int[data_in.Count];
                StartingIndizes[idx_highestSampleInt] = refIndex;

                for (int i = 0; i < data_in.Count; i++)
                {
                    //find index closest to refPoint
                    if (i != idx_highestSampleInt)
                    {
                        int j = 0;
                        while ((j < data_in[i].Count) && (data_in[i][j].DT_absolute < refPoint))
                        {
                            j++;
                        }

                        //if (j < data_in[i].Count)
                        if (j <= EndIndizes[i])
                        {
                            //j points to next higher point
                            //Check if j or j-1 is closer to refPoint
                            TimeSpan tsprev = refPoint - data_in[i][j].DT_absolute;
                            TimeSpan tsnxt = data_in[i][j].DT_absolute - refPoint;

                            StartingIndizes[i] = j;
                            if (tsprev < tsnxt)
                            {
                                //tsprev is closer
                                StartingIndizes[i] = j - 1;
                            }
                        }
                        else
                        {
                            //Could not find a valid starting point
                            StartingIndizes[i] = -1;
                        }
                    }
                }

                //Build arrays of double, rebuild time base
                data_time = [];
                data_value_scaled = [];

                for (int i = 0; i < data_in.Count; i++)
                {
                    if (StartingIndizes[i] >= 0)
                    {
                        List<double> time = [];
                        List<double> val = [];
                        ushort si = Device.ModuleInfos[data_in[i][0].HW_cn].SWChannels[data_in[i][0].SW_cn].SampleInt;
                        double si_s = ((double)si) / 1000;
                        double cnttime = 0;
                        //for (int j = StartingIndizes[i]; j < data_in[i].Count; j++)
                        for (int j = StartingIndizes[i]; j <= EndIndizes[i]; j++)
                        {
                            time.Add(cnttime);
                            cnttime += si_s;
                            //debug
                            //val.Add(GetScaledValue(data_in[i][j]));
                            val.Add(data_in[i][j].Value);
                        }
                        data_time.Add(time);
                        data_value_scaled.Add(val);
                    }
                    else
                    {
                        //In case of error add empty list that indices stay in order
                        data_time.Add([]);
                        data_value_scaled.Add([]);
                    }
                }
            } //if (refIndex < data_in[idx_highestSampleInt].Count)
            else
            {
                //data_in not have a common starting point
                data_time = [];
                data_value_scaled = [];
            }

        }


        /// <summary>
        /// Gets the connection status
        /// </summary>
        /// <returns>Connection status</returns>
        public EnumConnectionStatus GetConnectionStatus()
        {
            if (RS232Receiver is not null)
                return RS232Receiver.GetConnectionStatus();
            return EnumConnectionStatus.Not_Connected;
        }

        /// <summary>
        /// Sends Command to specific module and
        /// Reads data back: ByteIn.Length defines number of bytes to read back
        /// ByteOut can be null
        /// </summary>
        /// <param name="HWChannelNumber">HW channel number</param>
        /// <param name="ModuleCommand">Command Code</param>
        /// <param name="ByteOut">Byytes to send</param>
        /// <param name="ByteIn">Bytes to receive</param>
        /// <returns></returns>
        protected bool ModuleCommand(byte HWChannelNumber, byte ModuleCommand, byte[] ByteOut, ref byte[] ByteIn)
        {
            return this.ModuleCommand(HWChannelNumber, ModuleCommand, ByteOut, ref ByteIn, C8KanalReceiverCommandCodes.cWrRdModuleCommand);
        }

        private bool ModuleCommand(byte HWChannelNumber, byte ModuleCommand, byte[] ByteOut, ref byte[] ByteIn, byte CommandCode)
        {
            int l = 0;
            if (ByteOut != null) l = ByteOut.Length;

            int m = 0;
            if (ByteIn != null && ByteIn.Length > 0)
                m = ByteIn.Length;
            else
                ByteIn = [];


            byte[] OutBuf = new byte[l + 4];
            OutBuf[0] = HWChannelNumber;
            OutBuf[1] = (byte)(l + 1);
            OutBuf[2] = (byte)m;
            OutBuf[3] = ModuleCommand;
            if (ByteOut != null && ByteOut.Length > 0)
            {
                Buffer.BlockCopy(ByteOut, 0, OutBuf, 4, l);
            }
            //Anzahl der bytes die zu lesen sind
            //OutBuf[OutBuf.Length - 1] = (byte)ByteIn.Length;
            return SendGetCheckData(CommandCode, OutBuf, ref ByteIn);
        }


        /// <summary>
        /// Gets ConfigModules and puts result into Device
        /// </summary>
        public virtual bool GetDeviceConfig()
        {
            bool ret = true;
            byte[] InData;
            byte[] AddData = new byte[1];
            List<byte> AllData = [];

            //Get Info Module by  Module
            for (int hw_cn = 0; hw_cn < max_num_HWChannels; hw_cn++)
            {
                AddData[0] = (byte)hw_cn;
                InData = new byte[1];
                if (SendGetCheckData(C8KanalReceiverCommandCodes.cGetModuleConfig, AddData, ref InData))
                {
                    if ((InData != null) && (InData.Length > 0))
                    {
                        AllData.AddRange(InData);
                    }
                    else
                    {
                        //failed
                        ret = false;
                        break;
                    }
                }
                else
                {
                    //failed
                    ret = false;
                    break;
                }
            }

            if (ret)
            {
                try
                {
                    Device ??= new C8KanalDevice2();
                    ret = Device.UpdateModuleInfoFromByteArray([.. AllData]);
                    if (ret)
                    {
                        Device.Calculate_SkalMax_SkalMin(); //Calculate max and mins
                    }
                }
                catch (Exception e)
                {
#if DEBUG   
                    Console.WriteLine("C8KanalReceiverV2_CommBase_#01: " + e.Message);
#endif
                    Device = null;
                }
            }
            return ret;
        }
        /// <summary>
        /// Sets configuration of a specific channel
        /// according to Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SWConfigChannel
        /// </summary>
        /// <param name="HW_cn">Hardware Channel number</param>
        private bool SetConfigModule(int HW_cn)
        {
            bool ret;
            byte[] InData = [];
            if (Device is null) return false;
            ret = SendGetCheckData(C8KanalReceiverCommandCodes.cSetModuleConfig,
                Device.ModuleInfos[HW_cn].Get_SWConfigChannelsByteArray(),
                ref InData);
            return ret;
        }

        /// <summary>
        /// Sets configuration of all SW Channels of one Module
        /// </summary>
        /// <param name="HW_cn">Hardware channel number</param>
        /// <returns>true if successful</returns>
        /// <remarks>Checks if ModuleType != cModuleTypeEmpty</remarks>
        public bool SetConfigModules(int HW_cn)
        {
            bool ret = true;
            //Check if Module is connected
            if (HW_cn != 0xff && Device is not null)
            {
                if (Device.ModuleInfos[HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    ret = SetConfigModule(HW_cn);
                }
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Sets configuration of all SW Modules
        /// according to Device.ModuleInfos
        /// </summary>
        /// <returns></returns>
        public bool SetConfigChannel()
        {
            bool ret = true;

            if (Device is null)
                return false;

            for (int HW_cn = 0; HW_cn < Device.ModuleInfos.Count; HW_cn++)
            {
                if (!SetConfigModules(HW_cn))
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }


        #region Module Specific Functions
        /// <summary>
        /// Returns the electrode information only if the module is ADS1292 based
        /// </summary>
        /// <param name="ElectrodeImp">The electrode imp.</param>
        /// <param name="HW_cn">Hardware Channel number</param>
        /// <returns>
        /// Electrode information or null if not appropriate or something went werng
        /// </returns>
        public bool GetElectrodeInfo(ref CADS1294x_ElectrodeImp ElectrodeImp, int HW_cn)
        {
            bool ret = true;
            if (Device?.ModuleInfos[HW_cn].ModuleType_Unmodified == enumModuleType.cModuleExGADS94)
            {
                if (SetModuleInfoSpecific(HW_cn, 128)) //Kanalnummer muss nur >= 128 sein
                {
                    CDelay.Delay_ms(1000);   //Damit Impedanzmessung fertig wird dauert ca 600ms

                    //4x lesen
                    for (int i = 0; i < 4; i++)
                    {
                        byte[] btin = GetModuleInfoSpecific(HW_cn, false);
                        if (btin != null)
                        {
                            ElectrodeImp.Update(btin);
                        }
                        else
                            ret = false;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Sets the Module specific information
        /// </summary>
        public bool SetModuleInfoSpecific(int HW_cn, int get_Imp_chan_x = -1)
        {
            bool ret = false;
            byte[] InData = new byte[1];

            if (Device?.ModuleInfos[HW_cn].ModuleType == enumModuleType.cModuleTypeEmpty)
                return ret;

            if (Device is not null)
            {
                byte[] Data = Device.ModuleInfos[HW_cn].GetModuleSpecific();

                if (get_Imp_chan_x >= 0)
                {
                    Data[0] = (byte)get_Imp_chan_x;
                }

                //Data[Data.Length - 1] = ...
                Data[^1] = CRC8.Calc_CRC8(Data, Data.Length - 2);
                ret = ModuleCommand((byte)HW_cn, C8KanalReceiverCommandCodes.cModule_SetSpecific, Data, ref InData);
            }
            return ret;
        }

        /// <summary>
        /// Gets the Module specific information
        /// </summary>
        public byte[] GetModuleInfoSpecific(int HW_cn, bool UpdateModuleInfo)
        {
            byte[] ByteOut = new byte[1];
            //byte[] ByteIn = new byte[18]; //12.11.2020 Startet mit 0x44 0x00 ... dann erst kommen Daten ... warum?
            byte[] ByteIn = new byte[17];
            byte[] btin = [];

            ByteOut[0] = (byte)HW_cn;
            if (ModuleCommand((byte)HW_cn, C8KanalReceiverCommandCodes.cModule_GetSpecific, ByteOut, ref ByteIn))
            {
                try
                {
                    btin = new byte[CModuleBase.ModuleSpecific_sizeof];
                    Buffer.BlockCopy(ByteIn, 1, btin, 0, CModuleBase.ModuleSpecific_sizeof);
                    //12.11.2020 Check CRC
                    string s = "GetModuleInfoSpecific: ";
                    for (int i = 0; i < btin.Length; i++)
                    {
                        s += btin[i].ToString("X2") + ", ";
                    }
                    byte crc = CRC8.Calc_CRC8(btin, btin.Length - 2);

                    s += "    /  CalcCRC=" + crc.ToString("X2");
                    Console.WriteLine(s);



                    if (Device is not null && UpdateModuleInfo)
                        Device.ModuleInfos[HW_cn].SetModuleSpecific(btin);
                }

                catch
                {
                }
            }
            return btin;
        }

        #endregion


        /// <summary>
        /// Forces device to rescan modules
        /// </summary>
        /// <remarks>
        /// Resets all Errors that are recognised in Neuromaster
        /// </remarks>
        public virtual bool ScanModules()
        {
            byte[] buf = [];
            byte[] InData = [];
            return SendGetCheckData(C8KanalReceiverCommandCodes.cScanModules, buf, ref InData);
        }


        private bool SendGetCheckData(byte commandCode, byte[] additionalData, ref byte[] inData)
        {
            // Call the async method and block until it completes
            int numInData = inData.Length;
            (byte[] receivedData, bool success) = Task.Run(() => SendGetCheckDataAsync(commandCode, additionalData, numInData)).GetAwaiter().GetResult();


            // Update inData with the result
            inData = receivedData;
            return success;
        }

        /// <summary>
        /// Asynchronously sends a command along with additional data and awaits a response from the device.
        /// </summary>
        /// <remarks>
        /// This method sends a command code followed by additional data, if provided, and then waits for a response
        /// from the device. The method operates asynchronously to prevent blocking the calling thread during
        /// communication and allows for handling timeouts if the response is not received within the specified period.
        /// </remarks>
        /// <param name="C8KanalRecCommandCode">The specific command code to send (see <c>C8KanalRecCommandCodes</c> for available codes).</param>
        /// <param name="AdditionalDataToSend">Optional additional data to send following the command code.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation. The task result contains the response data
        /// as a byte array if the operation succeeds, or <c>null</c> if the operation times out or fails.
        /// </returns>
        //        protected bool SendGetCheckData(byte C8KanalRecCommandCode, byte[] AdditionalDataToSend, ref byte[] InData)
        protected async Task<(byte[], bool)> SendGetCheckDataAsync(byte C8KanalRecCommandCode, byte[] AdditionalDataToSend, int numInData= 0)
        {
            // Validate AdditionalDataToSend size early
            if (AdditionalDataToSend.Length > 250)
            {
                throw new ArgumentException("Size of AdditionalDataToSend must be <= 250", nameof(AdditionalDataToSend));
            }

            const int overhead = 4; // CommandCode, Command, Length, CRC
            int bytestosend = overhead + AdditionalDataToSend.Length;
            byte[] buf = new byte[bytestosend];

            // Build the command buffer
            buf[0] = C8KanalReceiverCommandCodes.CommandCode;  // Add the base command code
            buf[1] = C8KanalRecCommandCode;                    // Add the specific command code
            buf[2] = (byte)(AdditionalDataToSend.Length + 1);  // Length byte (+CRC)

            // Copy additional data into the buffer
            Buffer.BlockCopy(AdditionalDataToSend, 0, buf, overhead - 1, AdditionalDataToSend.Length);

            // Calculate and set CRC
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 1);

            // Initialize default return values
            byte[] inbuf;
            byte[] returnData = Array.Empty<byte>();
            bool ret = false;

            if (RS232Receiver != null)
            {
                try
                {
                    // Send the command and wait for the response
                    inbuf = await RS232Receiver.SendCommandAsync(buf);

                    // Log response for debugging
                    Debug.WriteLine("Received response: " + BitConverter.ToString(inbuf));
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("The command timed out.");
                    return ([], false);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine("Failed to send the command: " + ex.Message);
                    return (Array.Empty<byte>(), false);
                }

                // Process the response
                if (inbuf.Length > 0 && inbuf[0] == C8KanalRecCommandCode) // Check if the response matches the command code
                {
                    if (numInData > 0)
                    {
                        // Expected data length; extract data from the response
                        returnData = new byte[inbuf.Length - 1];
                        Array.Copy(inbuf, 1, returnData, 0, inbuf.Length - 1);
                    }
                    ret = true; // Set ret to true for both cases (with and without data)
                }
            }
            return (returnData, ret);
        }



        /// <summary>
        /// Reads numBytes from Command channel, result stored in buf
        /// </summary>
        /// <param name="buf">Bytes from command channel; Array size must be >= numbytes</param>
        /// <returns></returns>
        /// <remarks>
        /// Timeout defined by WaitCommandResponseTimeOut
        /// </remarks>
        /*
        private bool WaitCommandResponse(ref byte[] buf)
        {
            //Auf bytes die ueber den Kommandokanal kommen warten
            //int TimeOut = WaitCommandResponseTimeOut / ThreadSleepTime;
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, WaitCommandResponseTimeOut);
            if (RS232Receiver is not null)
            {
                while (DateTime.Now < dt)
                {
                    buf = RS232Receiver.GetGetCommand();
                    if (buf.Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

        /// <summary>
        /// Sends Close Connection to NM
        /// </summary>
        /// <returns></returns>
        public bool SendCloseConnection()
        {
            byte[] InData = [];
            byte[] outbuf = [];

            if (!SendGetCheckData(C8KanalReceiverCommandCodes.cSetConnectionClosed, outbuf, ref InData))
            {
                //Error transmitting
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Clock in Device
        /// </summary>
        /// <param name="dt">DateTime to transmit</param>
        /// <returns></returns>
        public virtual bool SetClock(DateTime dt)
        {
            byte[] InData = [];
            _DeviceClock.Dt = dt;
            byte[] outbuf = [];
            _DeviceClock.GetByteArray(ref outbuf, 0);

            if (!SendGetCheckData(C8KanalReceiverCommandCodes.cSetClock, outbuf, ref InData))
            {
                //Error transmitting
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads Device Clock
        /// </summary>
        /// <param name="dt">DateTime received</param>
        /// <returns></returns>
        public bool GetClock(ref DateTime dt)
        {
            byte[] buf = new byte[1];
            byte[] badd = [];

            if (SendGetCheckData(C8KanalReceiverCommandCodes.cGetClock, badd, ref buf))
            {
                _DeviceClock.UpdateFrom_ByteArray(buf, 0);
                dt = _DeviceClock.Dt;
            }
            else
            {
                //Error
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets Neuromaster Firmware Version
        /// </summary>
        /// <returns></returns>
        public bool GetNMFirmwareVersion(ref CNMFirmwareVersion NMFirmwareVersion)
        {
            bool ret = true;
            byte[] buf = new byte[20];
            byte[] badd = [];

            if (SendGetCheckData(C8KanalReceiverCommandCodes.cGetFirmwareVersion, badd, ref buf))
            {
                NMFirmwareVersion.UpdateFrom_ByteArray(buf, 0);
            }
            else
            {
                //Error
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Gets SD Card Information
        /// </summary>
        /// <returns></returns>
        public bool GetSDCardInfo(ref CSDCardInfo SDCardInfo)
        {
            bool ret = true;
            byte[] buf = new byte[2];
            byte[] badd = [];

            if (SendGetCheckData(C8KanalReceiverCommandCodes.cGetSDCardInfo, badd, ref buf))
            {
                SDCardInfo.UpdateFrom_ByteArray(buf, 0);
            }
            else
            {
                //Error
                ret = false;
            }
            return ret;
        }
    }
}
