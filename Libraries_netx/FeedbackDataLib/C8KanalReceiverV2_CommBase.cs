using BMTCommunicationLib;
using WindControlLib;
using EnNeuromasterCommand = FeedbackDataLib.C8KanalReceiverCommandCodes.EnNeuromasterCommand;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    public partial class C8KanalReceiverV2_CommBase
    {

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        protected CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);    //10.1.2013

        /// <summary>
        /// Converter for Device clock
        /// </summary>
        public CCDateTime DeviceClock {  get; private set; }

        /// <summary>
        /// TimeOut [ms] in WaitCommandResponse
        /// </summary>
        protected const int WaitCommandResponseTimeOut_ms = 3000; //ScanModules braucht so eine lange Timeout

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

        /// <summary>Cancellation token for called async operations</summary>
        public readonly CancellationToken cancellationToken = CancellationToken.None;


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

        public class CommandProcessedResponseEventArgs(EnNeuromasterCommand command, string message, Color messageColor, bool success, byte[] responseData, byte hWcn = 0xff) : EventArgs
        {
            public EnNeuromasterCommand Command { get; } = command;
            public string Message { get; } = message;
            public Color MessageColor { get; } = messageColor;
            public bool Success { get; } = success;
            public byte[] ResponseData { get; } = responseData;
            public byte HWcn { get; } = hWcn;
        }

        public event EventHandler<CommandProcessedResponseEventArgs>? CommandProcessedResponse;
        protected virtual void OnCommandProcessedResponse(CommandProcessedResponseEventArgs e)
        {
            CommandProcessedResponse?.Invoke(this, e);
        }


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
                    cdi.VirtualID = Device.ModuleInfos[cdi.HWcn].SWChannels[cdi.SWcn].VirtualID;
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

        public delegate void DeviceToPC_ModuleErrorEventHandler(byte HWcn);
        /// <summary>
        /// Buffer of the Device is full, it stops sampling, has to be reinitialised
        /// </summary>
        public event DeviceToPC_ModuleErrorEventHandler? DeviceToPC_ModuleError;
        protected virtual void OnDeviceToPC_ModuleError(byte HWcn)
        {
            DeviceToPC_ModuleError?.Invoke(HWcn);
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
            DeviceClock = new CCDateTime();
            BatteryVoltage = new CBatteryVoltage();
        }

        /// <param name="ComPortName">
        /// "COM1","COM2
        /// </param>
        public C8KanalReceiverV2_CommBase(string ComPortName): this()
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
            RS232Receiver.CommandProcessed += RS232Receiver_CommandProcessed;
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
                    Device.ModuleInfos[di.HWcn].SWChannels[di.SWcn].SWChan_Started = ReceivingStarted;
                    Device.ModuleInfos[di.HWcn].SWChannels[di.SWcn].SynPackagesreceived = cntSyncPackages;
                }

                Device.UpdateTime(di);

                if (di.ChannelStarted != DateTime.MinValue)
                {
                    //Process Data Module specific
                    _DataIn.AddRange(Device.ModuleInfos[di.HWcn].Processdata(di));
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
            for (int HWcn = 0; HWcn < Device.ModuleInfos.Count; HWcn++)
            {
                for (int SW_cn = 0; SW_cn < Device.ModuleInfos[HWcn].SWChannels.Count; SW_cn++)
                {
                    if (Device.ModuleInfos[HWcn].SWChannels[SW_cn].SendChannel == true)
                    {
                        //Count data packets per second
                        double d = Device.ModuleInfos[HWcn].SWChannels[SW_cn].SampleInt;
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
            int hwcn = DataIn.HWcn;// & 0xf0) >> 4;
            //int swcn = (DataIn.HWChannelNumber & 0x0f);
            int swcn = DataIn.SWcn;
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
            DateTime dt_absolute = data_in[0][0].DTAbsolute;
            DateTime earliestStartingTime = dt_absolute;

            int idx_latestEndTime = 0;
            DateTime latestEndTime = data_in[0][^1].DTAbsolute;

            data_value_scaled = [];
            data_time = [];
            if (Device is null) return;

            for (int i = 0; i < data_in.Count; i++)
            {
                //Find channel with highest sample interval
                ushort si = Device.ModuleInfos[data_in[i][0].HWcn].SWChannels[data_in[i][0].SWcn].SampleInt;
                if (si > si_max)
                {
                    si_max = si;
                    idx_highestSampleInt = i;
                }

                //Check Starting times of channels ... for synchronisation
                //Find the channel with earlierst starting time
                if (data_in[i][0].DTAbsolute > earliestStartingTime)
                {
                    earliestStartingTime = data_in[i][0].DTAbsolute;
                }

                //Check Ending times of channels ... 
                //Find the channel with latest ending time
                if (data_in[i][^1].DTAbsolute < latestEndTime)
                {
                    idx_latestEndTime = i;
                    latestEndTime = data_in[i][^1].DTAbsolute;
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
                        if (data_in[i][j].DTAbsolute > latestEndTime)
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
            while (data_in[idx_highestSampleInt][refIndex].DTAbsolute < earliestStartingTime)
            {
                refIndex++;
                if (refIndex >= data_in[idx_highestSampleInt].Count)
                    break;
            }


            //find StartingIndizes closest to refPoint
            if (refIndex < data_in[idx_highestSampleInt].Count)
            {
                DateTime refPoint = data_in[idx_highestSampleInt][refIndex].DTAbsolute;

                //Find indizes of other channels closest to the refPoint
                int[] StartingIndizes = new int[data_in.Count];
                StartingIndizes[idx_highestSampleInt] = refIndex;

                for (int i = 0; i < data_in.Count; i++)
                {
                    //find index closest to refPoint
                    if (i != idx_highestSampleInt)
                    {
                        int j = 0;
                        while ((j < data_in[i].Count) && (data_in[i][j].DTAbsolute < refPoint))
                        {
                            j++;
                        }

                        //if (j < data_in[i].Count)
                        if (j <= EndIndizes[i])
                        {
                            //j points to next higher point
                            //Check if j or j-1 is closer to refPoint
                            TimeSpan tsprev = refPoint - data_in[i][j].DTAbsolute;
                            TimeSpan tsnxt = data_in[i][j].DTAbsolute - refPoint;

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
                        ushort si = Device.ModuleInfos[data_in[i][0].HWcn].SWChannels[data_in[i][0].SWcn].SampleInt;
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

    }
}
