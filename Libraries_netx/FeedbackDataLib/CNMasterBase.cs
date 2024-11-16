using BMTCommunicationLib;
using WindControlLib;
using Microsoft.Extensions.Logging;
using FeedbackDataLib.Modules;

namespace FeedbackDataLib
{
    /// <summary>
    /// Base class for 8 Channel Neuromaster
    /// </summary>
    public partial class CNMaster
    {
        /// <summary>
        /// Handles USB, XBee´and Cable connection
        /// </summary>
        public CNMasterReceiver NMReceiver { get; set; } = new(); //Just to keep it away from null

        /// <summary>
        /// Receiver Thread
        /// </summary>
        private CRS232Receiver? RS232Receiver { get; set; }

        public EnConnectionStatus ConnectionType { get => NMReceiver.ConnectionResult; }


        /// <summary>
        /// Converter for Device clock
        /// </summary>
        public CCDateTime DeviceClock { get; private set; }

        /// <summary>
        /// TimeOut [ms] in WaitCommandResponse
        /// </summary>
        protected const int WaitCommandResponseTimeOutMs = 3000; //ScanModules braucht so eine lange Timeout

        /// <summary>
        /// The number of SW channels sent by Neuromaster
        /// </summary>
        public const int NumSWChannelsSentByHW = 4;

        /// <summary>
        /// That is the maximum number of SW Channels one module can have
        /// </summary>
#if VIRTUAL_EEG
        public const int maxNumSWChannels = 12;
#else
        public const int MaxNumSWChannels = 4;

#endif

        /// <summary>
        /// That is the maximum number od HW Channels one module can have
        /// </summary>
        public const int MaxNumHWChannels = 7;

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

        /// <summary>Cancellation token for called async operations</summary>
        public readonly CancellationToken cancellationToken = CancellationToken.None;

        private readonly ILogger<CNMaster> _logger;


        /// <summary>
        /// Basic Constructor
        /// </summary>
        public CNMaster()
        {
            _logger = AppLogger.CreateLogger<CNMaster>();
            DeviceClock = new CCDateTime();
            BatteryVoltage = new CBatteryVoltage();

            for (int i = 0; i < MaxNumHWChannels; i++)
            {
                moduleInfos.Add(new CModuleBase());
            }
        }
      
        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            SendCloseConnection();
            Thread.Sleep(1000);  // 1000 milliseconds = 1 second
            StopDistributorThreadAsync();
            NMReceiver.Close();
        }

        public EnConnectionStatus ConnectionStatus {get => NMReceiver.ConnectionStatus; set => NMReceiver.ConnectionStatus = value; }

        public EnConnectionStatus Connect()
        {
            if (NMReceiver is null) return EnConnectionStatus.NoConnection;

            EnConnectionStatus conres = NMReceiver.Init_via_D2XX();
            
            if (NMReceiver.Connection is null || NMReceiver.Connection.SerPort is null) return conres;

            switch (conres)
            {
                case EnConnectionStatus.Connected_via_RS232:
                case EnConnectionStatus.Connected_via_XBee:
                    {
                        var sp = NMReceiver.Connection.SerPort;

                        if (Check4Neuromaster(sp))
                        {
                            //Succeeded
                            //Gerät da
                            StartDistributorThreadAsync();
                            NMReceiver.StartUSBMonitoring();
                        }
                        else
                        {
                            if (conres == EnConnectionStatus.Connected_via_RS232)
                                conres = EnConnectionStatus.Error_during_RS232_connection;
                            else
                                conres = EnConnectionStatus.Error_during_XBee_connection;

                        }

                        break;
                    }
                case EnConnectionStatus.Error_during_RS232_connection:
                case EnConnectionStatus.Error_during_XBee_connection:
                    break;
                default:
                    if (ConnectionType == EnConnectionStatus.Connected_via_SDCard)
                    {
                        //if (_8KanalReceiverV2_SDCard != null)
                        //{
                        //    _8KanalReceiverV2_SDCard.CheckConnection_Start_trytoConnectWorker();
                        //    return EnumConnectionResult.Connected_via_SDCard;
                        //}
                        ConnectionStatus = EnConnectionStatus.Error_read_SDCard;
                    }

                    break;
            }
            ConnectionStatus = conres;
            return conres;
        }


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
                catch (Exception ex)
                {
                    var logger = AppLogger.CreateLogger<CNMaster>(); // Create a logger for this class
                    logger.LogError("Check4Neuromaster: {Message}", ex.Message);
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
