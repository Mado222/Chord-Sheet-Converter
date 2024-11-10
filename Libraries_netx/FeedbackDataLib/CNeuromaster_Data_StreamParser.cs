using BMTCommunicationLib;
using System;
using WindControlLib;

namespace FeedbackDataLib
{
    public class CNeuromaster_Data_StreamParser
    {
        /// <summary>
        /// Last Sync Signal received from Device = when Device timer has full second
        /// </summary>
        private DateTime LastSyncSignal;

        /// <summary>
        /// Buffer for incoming data
        /// </summary>
        List<byte> DataInBuffer = [];

        private CHighPerformanceDateTime hp_Timer = new();

        private const int Length_of_DataPack = 4;

        private int numByteRead = 0;

        private DateTime FakeTime_Start;
        private TimeSpan FakeTime_Incement;

        /// <summary>
        /// Just to hold and process Sample Interval
        /// </summary>
        private class CSkalData
        {
            public TimeSpan SmpleInt_ms = new(0, 0, 0, 0, 20);
            public TimeSpan SinceLastSync;
            public void IncrSinceLastSync()
            {
                SinceLastSync += SmpleInt_ms;
            }
        }

        /// <summary>
        /// Data about sample Intervals of all channels
        /// </summary>
        private CSkalData[][] ChanData = new CSkalData[C8KanalReceiverV2_CommBase.max_num_HWChannels][];

        private bool _isFakeTime = false;

        /// <summary>
        /// true if fake time is generated
        /// Set_FakeTime must be called 
        /// </summary>
        public bool isFakeTime
        {
            get { return _isFakeTime; }
            set { _isFakeTime = value; }
        }

        public CNeuromaster_Data_StreamParser()
        {
            for (int i = 0; i < ChanData.Length; i++)
            {
                ChanData[i] = new CSkalData[C8KanalReceiverV2_CommBase.maxNumSWChannels];
                for (int j = 0; j < C8KanalReceiverV2_CommBase.maxNumSWChannels; j++)
                {
                    ChanData[i][j] = new CSkalData();
                }
            }
        }


        public void Set_FakeTime(DateTime FakeTime_Start, TimeSpan FakeTime_Incement)
        {
            isFakeTime = true;
            this.FakeTime_Start = FakeTime_Start;
            LastSyncSignal = FakeTime_Start;
            this.FakeTime_Incement = FakeTime_Incement;
        }

        public void SetSampleInterval(int SampleIntervall_ms, int hw_cn, int sw_cn)
        {
            ChanData[hw_cn][sw_cn].SmpleInt_ms = new TimeSpan(0, 0, 0, 0, SampleIntervall_ms);
        }


        public List<CDataIn> AddBytes(byte[] data)
        {
            List<CDataIn> ret = [];
            DataInBuffer.AddRange(data);

            //Check for a valid packet
            while (DataInBuffer.Count >= numByteRead)
            {
                if (CInsightDataEnDecoder.Parse4Byte([.. DataInBuffer.GetRange(0, 4)]) is CDataIn di)
                {
                    //it is a valid packet

                    //Command channel or data channel?
                    if (di.HWcn == 0x0f)
                    {
                        //Command is coming in, we need more bytes
                        int numAdditionalBytes = di.Value;

                        //Are already enough bytes in the buffer?
                        if (DataInBuffer.Count >= Length_of_DataPack + numAdditionalBytes)
                        {
                            //Enough bytes are here, read them
                            byte[] buf = [.. DataInBuffer.GetRange(Length_of_DataPack, numAdditionalBytes)];
                            //Check for commands that must be processed here
                            switch (buf[0])
                            {
                                case C8KanalReceiverCommandCodes.cChannelSync:
                                    {
                                        if (isFakeTime)
                                        {
                                            LastSyncSignal += FakeTime_Incement;
                                        }
                                        else
                                        {
                                            LastSyncSignal = hp_Timer.Now;
                                        }
                                        break;
                                    }
                            }
                            //Remove bytes
                            DataInBuffer.RemoveRange(0, Length_of_DataPack + numAdditionalBytes);
                            numByteRead = Length_of_DataPack;
                        }
                        else
                        {
                            //Not enogh bytes in, lets go for another round
                            numByteRead = Length_of_DataPack + numAdditionalBytes;
                        }

                    }   //if (di.HWChannelNumber == 0x0f)
                    else
                    {
                        //Its data ... but if sync Sequence it requires a 5th byte
                        if (di.SyncFlag == 1)
                        {
                            //Now we need a 5th byte ... is it already there??
                            if (DataInBuffer.Count >= Length_of_DataPack + 1)
                            {
                                //Yes, we can proceed
                                int sync = DataInBuffer[Length_of_DataPack];   //byte wit idx 4 = 5th byte
                                sync = sync & 0x7f;
                                if (di.Sync7 == 1) sync += 128;
                                di.SyncVal = (byte)sync;

                                //All required data here, remove bytes from buffer
                                DataInBuffer.RemoveRange(0, Length_of_DataPack + 1);
                                di.LastSync = LastSyncSignal;
                                if (isFakeTime)
                                {
                                    //Fake Time
                                    ChanData[di.HWcn][di.SWcn].SinceLastSync = new TimeSpan(0, 0, 0, 0, di.SyncVal);
                                    di.TSSinceLastSync = ChanData[di.HWcn][di.SWcn].SinceLastSync;
                                }
                                else
                                {
                                    //Real Time
                                    di.TSSinceLastSync = hp_Timer.Now - LastSyncSignal;
                                }
                                ret.Add(di);
                                numByteRead = Length_of_DataPack;
                            }
                            else
                            {
                                //5th byte not yet in, go for another loop
                                numByteRead = Length_of_DataPack + 1;
                            }
                        } //if (di.SyncFlag == 1)
                        else
                        {
                            //It is a standard data packet, remove bytes from buffer
                            DataInBuffer.RemoveRange(0, Length_of_DataPack);
                            di.LastSync = LastSyncSignal;
                            if (isFakeTime)
                            {
                                //Fake Time
                                ChanData[di.HWcn][di.SWcn].IncrSinceLastSync();
                                di.TSSinceLastSync = ChanData[di.HWcn][di.SWcn].SinceLastSync;
                            }
                            else
                            {
                                //Real Time
                                di.TSSinceLastSync = hp_Timer.Now - LastSyncSignal;
                            }
                            ret.Add(di);
                            numByteRead = Length_of_DataPack;
                        }
                    }
                } //if (CDecodeBytes.Decode4Byte(DataInBuffer.GetRange(0, 4).ToArray(), ref di))
                else
                {
                    //Decode failed, lets assume first byte is invalid, remove it
                    DataInBuffer.RemoveAt(0);
                    numByteRead = 1;        //We need one more byte for next trial
                }
            } //if (DataInBuffer.Count >= numByteRead)

            return ret;
        }



    }
}
