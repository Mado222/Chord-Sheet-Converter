using BMTCommunicationLib;
using WindControlLib;
using static FeedbackDataLib.C8Receiver;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    public partial class C8CommBase
    {
        /// <summary>
        /// Handles USB, XBee´and Cable connection
        /// </summary>
        public C8Receiver c8Receiver = new(); //Just to keep it away from null

        /// <summary>
        /// Receiver Thread
        /// </summary>
        private CRS232Receiver? receiver;


        /// <summary>
        /// Converter for Device clock
        /// </summary>
        public CCDateTime DeviceClock {  get; private set; }

        /// <summary>
        /// TimeOut [ms] in WaitCommandResponse
        /// </summary>
        protected const int WaitCommandResponseTimeOutMs = 3000; //ScanModules braucht so eine lange Timeout

        /// <summary>
        /// The number of SW channels sent by Neuromaster
        /// </summary>
        public const int numSWChannelsSentByHW = 4;

        /// <summary>
        /// That is the maximum number of SW Channels one module can have
        /// </summary>
#if VIRTUAL_EEG
        public const int maxNumSWChannels = 12;
#else
        public const int maxNumSWChannels = 4;

#endif

        /// <summary>
        /// That is the maximum number od HW Channels one module can have
        /// </summary>
        public const int max_num_HWChannels = 7;

        /// <summary>
        /// Default Multisensor Channel No
        /// </summary>
        public const byte DefaultMultisensorChannelNo = 1;

        /// <summary>NM sends in this ms Interval the SYNC Signal/// </summary>
        public const int SyncInterval_ms = 1000;

        /// <summary>Alive signal PC sends to NM</summary>
        public const int AliveSignalToSendIntervMs = 2000;

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


        /// <param name="ComPortName">
        /// "COM1","COM2
        /// </param>
        public C8CommBase(string ComPortName): this()
        {
            C8KanalReceiverV2_Construct();
            this.ComPortName = ComPortName;
        }

        public C8CommBase(ISerialPort SerialPort) : this ()
        { }

        public C8CommBase(string ComPortName, int BaudRate) : this()
        { }


        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            c8Receiver?.Connection?.Close();  //1st Close
        }

        public EnumConnectionResult Connect()
        {
            var conres = c8Receiver.Init_via_D2XX();
            if (conres == EnumConnectionResult.Connected_via_USBCable || conres == EnumConnectionResult.Connected_via_XBee)
            {
                //Start Threads
                StartDistributorThreadAsync();
            }
            return conres;
        }


        /// <summary>
        /// Called by constructors
        /// </summary>
        public void C8KanalReceiverV2_Construct()
        {
            if (c8Receiver == null)
                throw new Exception("RS232Receiver mustbe created before calling constructor");
        }


        //For RS232Receiver_DataReadyComm
        private int cntSyncPackages = 0;                        //Counts the incoming sync packages
        private DateTime OldLastSync = DateTime.MinValue;       //Time when the previous package was received
        private DateTime ReceivingStarted = DateTime.MinValue;  //Receiving started at


        /// <summary>
        /// Counts how many data packets per second are sent via XBee channel
        /// </summary>
        /// <returns></returns>
        public int GetChannelCapcity()
        {
            double ret = 0;
            for (int HWcn = 0; HWcn < ModuleInfos.Count; HWcn++)
            {
                for (int SW_cn = 0; SW_cn < ModuleInfos[HWcn].SWChannels.Count; SW_cn++)
                {
                    if (ModuleInfos[HWcn].SWChannels[SW_cn].SendChannel == true)
                    {
                        //Count data packets per second
                        double d = ModuleInfos[HWcn].SWChannels[SW_cn].SampleInt;
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
            int hwcn = DataIn.HWcn;// & 0xf0) >> 4;
            //int swcn = (DataIn.HWChannelNumber & 0x0f);
            int swcn = DataIn.SWcn;
            // ;ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d
            //d = d - ModuleInfos[hwcn].SWChannels[swcn].Offset_hex;
            //d = d * ModuleInfos[hwcn].SWChannels[swcn].SkalValue_k + ModuleInfos[hwcn].SWChannels[swcn].Offset_d;
            double ret = ModuleInfos[hwcn].SWChannels[swcn].GetScaledValue(DataIn.Value);
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

            for (int i = 0; i < data_in.Count; i++)
            {
                //Find channel with highest sample interval
                ushort si = ModuleInfos[data_in[i][0].HWcn].SWChannels[data_in[i][0].SWcn].SampleInt;
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
                        ushort si = ModuleInfos[data_in[i][0].HWcn].SWChannels[data_in[i][0].SWcn].SampleInt;
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
            if (c8Receiver is not null && c8Receiver.Connection is not null)
                return c8Receiver.Connection.ConnectionStatus;
            return EnumConnectionStatus.Not_Connected;
        }

        public static bool Check4Neuromaster(ISerialPort Seriell32)
        {
            bool ret = false;
            bool Failed = false;

            bool isOpen = Seriell32.IsOpen;
            if (!isOpen) Seriell32.GetOpen();

            if (Seriell32.IsOpen)
            {
                var SequToSend = AliveSequToSend();
                var SequToReturn = AliveSequToReturn();

                //Hier abfragen ob ein Device da ist
                try
                {
                    Seriell32.DiscardInBuffer();
                    Seriell32.DiscardOutBuffer();
                    Seriell32.Write(SequToSend, 0, SequToSend.Length);
                    //Thread.Sleep(300);
                }
                catch (Exception)
                {
                    //OnLogError(ee.Message);
                    //log.Error(ee.Message, ee);
                    Failed = true;
                }
                if (!Failed)
                {
                    //int DataToReceive = 4 + AliveSequToReturn.Length;
                    int ptr = 0;
                    int DataToReceive = 4;
                    byte[] buffer = new byte[DataToReceive];
                    DateTime Timeout = DateTime.Now + new TimeSpan(0, 0, 1);    //1s Timeout

                    while (DateTime.Now < Timeout)
                    //while (true)
                    {
                        int ReadRes = Seriell32.Read(ref buffer, ptr, DataToReceive, 100);
                        if (ReadRes == DataToReceive)
                        {
                            if (CInsightDataEnDecoder.Parse4Byte(buffer) is CDataIn DI)
                            {
                                if (DI.HWcn == CommandChannelNo)
                                {
                                    //DI.
                                    if (DI.Value == SequToReturn.Length)
                                    {
                                        //Über Kommandokanalkanal kommt die passende Anzahl von bytes
                                        //Bytes hereinholen
                                        byte[] buffer2 = new byte[SequToReturn.Length];
                                        ReadRes = Seriell32.Read(ref buffer2, 0, SequToReturn.Length, 100);

                                        if (ReadRes == SequToReturn.Length)
                                        {
                                            //Bytes kontrollieren
                                            bool ValCorrect = true;
                                            for (int i = 0; i < SequToReturn.Length; i++)
                                            {
                                                if (buffer2[i] != SequToReturn[i]) ValCorrect = false;
                                            }
                                            if (ValCorrect)
                                            {
                                                //Gerät da
                                                ret = true;
                                                break;
                                            }
                                            //Ir
                                        } //if (ReadRes == AliveSequToReturn.Length)
                                    }//if (DI.Value == AliveSequToReturn.Length)
                                } //if (DI.Value == AliveSequToReturn.Length)
                            } //if (CDecodeBytes.Decode4Byte(bh2, bh1, bh0, bl, ref DI))
                        } //if (ReadRes == DataToReceive)
                        if (!ret)
                        {
                            if (ReadRes > 0)
                            {
                                //Nichts verwertbares hereingekommen, weiterschieben 
                                ptr = 3;
                                DataToReceive = 1;
                                for (int i = 0; i < buffer.Length - 1; i++)
                                    buffer[i] = buffer[i + 1];
                            } //if (ReadRes > 0)
                        } //if (!ret)
                    }   //while (DateTime.Now < Timeout)
                } //if (!Failed)

                if (!isOpen)
                {
                    Seriell32.Close();
                }
            }   //if (Seriell32.IsOpen)
            return ret;
        }

    }
}
