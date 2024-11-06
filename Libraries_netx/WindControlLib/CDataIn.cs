using System.Text.Json.Serialization; //Via NuGet

namespace WindControlLib
{
    /// <summary>
    /// Data as they come from Neuromaster
    /// </summary>
    public class CDataIn : ICloneable
    {
        //Added 10.5.2016 fuer Teststand
        [JsonIgnore]
        public byte[] Value_buffer
        {
            get
            {
                return BitConverter.GetBytes(Value);
            }
        }

        public int Value;

        /// <summary>
        ///  DateTime this value was received at
        ///  Set in: RS232Receiver_DataReadyComm
        /// </summary>
        public DateTime ReceivedAt { get; set; } = DateTime.MinValue;

        /// <summary>
        /// DateTime of the last Sync-Signal that was received prior to this packet
        /// </summary>
        [JsonIgnore]
        public DateTime LastSync { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Timespan since LastSync
        /// </summary>
        [JsonIgnore]
        public TimeSpan TSSinceLastSync { get; set; }

        /// <summary>
        /// DateTime when the Channel started = first Sync 
        /// Used to calculate absolute time
        /// </summary>
        [JsonIgnore]
        public DateTime ChannelStarted { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Timespan since ChannelStarted
        /// </summary>
        [JsonIgnore]
        public TimeSpan TSSinceChannelStarted { get; set; }


        /// <summary>
        /// Absolute DateTime of this data point
        /// Calculated: LastSync + ts_Since_LastSync
        /// Update every time Neuromatser sends Sync Signal
        /// </summary>
        public DateTime DTAbsolute
        {
            get { return LastSync + TSSinceLastSync; }
        }

        /// <summary>
        /// Relative DateTime in respect to the ChannelStarted Time
        /// </summary>
        /// <remarks>
        /// Calculated: ChannelStarted + ts_Since_ChannelStarted
        /// Update only when the channel receives the first SyncPacket
        /// </remarks>
        public DateTime DTRelative
        {
            get { return ChannelStarted + TSSinceChannelStarted; }
        }

        /// <summary>
        /// Hardware channel number where the packet came from
        /// </summary>
        public byte HWcn { get; set; }

        /// <summary>
        /// Software channel number where the packet came from
        /// </summary>
        public byte SWcn { get; set; }

        /// <summary>
        /// Virtual Identifier
        /// </summary>
        /// <remarks>
        /// Added Dec. 2013; see CSWChannel.SetVirtualID
        /// </remarks>
        public uint VirtualID { get; set; }

        /// <summary>
        /// DeviceID
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// If true, this Date packet was resynced
        /// </summary>
        [JsonIgnore]
        public bool Resync { get; set; }

        /// <summary>
        /// Value is SyncVal ms away from from last Sync event
        /// (Neuromasters sends C8KanalRecCommandCodes.cChannelSync)
        /// </summary>
        [JsonIgnore]
        public byte SyncVal { get; set; }

        /// <summary>
        /// Is 1 if this is a Sync data packet
        /// In this case PacketReceived holds the DateTime when the SyncSignal was received 
        /// </summary>
        [JsonIgnore]
        public byte SyncFlag { get; set; }

        /// <summary>
        /// Only for internal usage in Decode4Byte
        /// DO NOT USE
        /// </summary>
        [JsonIgnore]
        public byte Sync7 { get; set; }

        /// <summary>
        /// Extended byte (24bit ...)
        /// DO NOT USE
        /// </summary>
        [JsonIgnore]
        public byte EP { get; set; } = 0;

        //For 24 bit Data
        public byte TypeExtraDat { get; set; } = 0;

        public byte NumExtraDat { get; set; } = 0;

        public byte[] ExtraDat { get; set; } = [8];
        public DateTime DT_relative { get; set; }

        public void Copy(CDataIn DataIn)
        {
            Value = DataIn.Value;

            TSSinceLastSync = DataIn.TSSinceLastSync;
            TSSinceChannelStarted = DataIn.TSSinceChannelStarted;
            HWcn = DataIn.HWcn;
            SWcn = DataIn.SWcn;
            DeviceID = DataIn.DeviceID;
            ChannelStarted = DataIn.ChannelStarted;
            VirtualID = DataIn.VirtualID;

            Resync = DataIn.Resync;
            SyncVal = DataIn.SyncVal;
            Sync7 = DataIn.Sync7;
            SyncFlag = DataIn.SyncFlag;
            LastSync = DataIn.LastSync;
            ReceivedAt = DataIn.ReceivedAt;

            EP = DataIn.EP;
            TypeExtraDat = DataIn.TypeExtraDat;
            NumExtraDat = DataIn.NumExtraDat;
            ExtraDat = new byte[NumExtraDat];

            Array.Copy(DataIn.ExtraDat, ExtraDat, NumExtraDat);
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            CDataIn d = (CDataIn)MemberwiseClone();
            return d;
        }

        #endregion
    }
}
