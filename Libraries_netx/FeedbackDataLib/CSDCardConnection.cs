using System;
using BMTCommunication;
using WindControlLib;
using System.IO.Ports;
using BMTCommunicationLib;


namespace FeedbackDataLib
{
    public class CSDCardConnection : ISerialPort, IDisposable
    {
        public CSDCardConnection()
        {
        }

        /// <summary>
        /// Path to the Configuration file on the SD Card
        /// </summary>
        /// <value>
        /// The path to configuration file.
        /// </value>
        public string PathToConfigFile { get; set; }



        /// <summary>
        /// Adds the sd card values.
        /// </summary>
        /// <returns>NUmber of data added</returns>
        public int AddSDCardValues(byte[] SDData)
        {
            int ret = SDData.Length;
            if (SDData.Length > InBufferSD.EmptySpace)
            {
                ret = InBufferSD.EmptySpace;
            }
            InBufferSD.Push(SDData, 0, ret);
            return ret;
        }

        #region ISerialPort Members

        public string PortName { get; set; } = "SDCard";

        public int BaudRate { get; set; } = 115200;

        private bool _IsOpen;
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        public void Close()
        {
            _IsOpen = false;
            //AliveTimer.Enabled = false;
        }

        public bool Open()
        {
            _IsOpen = true;
            return true;
        }

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; }
        public Handshake Handshake { get; set; }

        private readonly CByteRingpuffer InBufferSD = new CByteRingpuffer(1024);
        private readonly CByteRingpuffer OutBufferSD = new CByteRingpuffer(1024);


        public int Read(ref byte[] buffer, int offset, int count)
        {

            int cntbytes = 0;
            
            for (int i = 0; i < count; i++)
            {
                if (InBufferSD.StoredBytes == 0)
                {
                    //return (i - offset);
                    break;
                }
                else
                {
                    buffer[offset + i] = (byte)InBufferSD.Pop();
                    cntbytes++;
                }
            }
            return cntbytes;
        }

        public int Read(ref byte[] buffer, int offset, int count, int WaitMax_ms)
        {
            return Read(ref buffer, offset, count);
        }

        public void DiscardInBuffer()
        {
            InBufferSD.Clear();
        }

        public void DiscardOutBuffer()
        {
            OutBufferSD.Clear();
        }

        public void Dispose()
        {
            //AliveTimer.Enabled = false;
        }

        public bool Write(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < count; i++)
            {
                OutBufferSD.Push(buffer[i]);
            }
            ParseMessage();
            return true;
        }

        public int BytesToRead
        {
            get { return InBufferSD.StoredBytes; }
        }

        public event SerialDataReceivedEventHandler SerialDataReceivedEvent;


        private string _LastErrorString = "";
        public string LastErrorString
        {
            get { return _LastErrorString; }
            set { _LastErrorString = value; }
        }

        private bool _DtrEnable;
        public bool DtrEnable
        {
            get { return _DtrEnable; }
            set { _DtrEnable = value; }
        }


        private bool _RtsEnable;
        public bool RtsEnable
        {
            get { return _RtsEnable; }
            set { _RtsEnable = value; }
        }

        public bool DsrHolding
        {
            get { return true; }
        }


        private DateTime _StartTime = DateTime.MinValue;
        /// <summary>
        /// Start time of the recording
        /// </summary>
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value;
            SyncTime = StartTime;
            }
        }


        private TimeSpan _SyncInterval = new TimeSpan(0, 0, 0, 1, 0);
        /// <summary>
        /// Interval where the Sync events are coming from NM
        /// </summary>
        public TimeSpan SyncInterval
        {
            get { return _SyncInterval; }
            set { _SyncInterval = value; }
        }


        private DateTime SyncTime = DateTime.MinValue;
        public DateTime Now(enumTimQueryStatus TimeQueryStatus)
        {
            switch (TimeQueryStatus)
            {
                case enumTimQueryStatus.no_Special:
                    {
                        break;
                    }
                case enumTimQueryStatus.isSync:
                    {
                        if (SyncTime == DateTime.MinValue)
                        {
                            SyncTime = DateTime.Now;
                        }
                        else
                        {
                            SyncTime += _SyncInterval;
                        }
                        break;
                    }
            }

            return SyncTime;
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {

        }
        #endregion

        private void ParseMessage()
        {
            while (OutBufferSD.StoredBytes != 0)
            {
                byte b = (byte)OutBufferSD.Pop();
                if (b == C8KanalReceiverCommandCodes.CommandCode)
                {
                    byte Command = (byte)OutBufferSD.Pop();
                    byte len = (byte)OutBufferSD.Pop();
                    byte[] payload = new byte[len];
                    for (int i = 0; i < len - 1; i++)
                    {
                        payload[i] = (byte)OutBufferSD.Pop();
                    }
                    _ = (byte)OutBufferSD.Pop();    //CRC

                    //Evaluate
                    switch (Command)
                    {
                        case C8KanalReceiverCommandCodes.cConnectToDevice:
                            {

                                byte[] cmd = C8KanalReceiverCommandCodes.ConnectSequToReturn();
                                byte[] header = CInsightDataEnDecoder.EncodeHeader((ushort)cmd.Length, C8KanalReceiverCommandCodes.cCommandChannelNo, 0); //C8KanalReceiverCommandCodes.AliveSequToReturn();
                                InBufferSD.Push(header, 0, header.Length);
                                InBufferSD.Push(cmd, 0, cmd.Length);
                                //AliveTimer.Enabled = true;
                                break;
                            }
                        case C8KanalReceiverCommandCodes.cDeviceAlive:
                            {
                                byte[] cmd = C8KanalReceiverCommandCodes.AliveSequToReturn();
                                byte[] header = CInsightDataEnDecoder.EncodeHeader((ushort)cmd.Length, C8KanalReceiverCommandCodes.cCommandChannelNo, 0); //C8KanalReceiverCommandCodes.AliveSequToReturn();
                                InBufferSD.Push(header, 0, header.Length);
                                InBufferSD.Push(cmd, 0, cmd.Length);
                                break;
                            }
                        case C8KanalReceiverCommandCodes.cGetChannelConfig:
                            {
                                _ = payload[0]; //hwcn
                                break;
                            }
                        case C8KanalReceiverCommandCodes.cScanModules:
                            {
                                break;
                            }
                    }
                }
            }
        }
    }
}
