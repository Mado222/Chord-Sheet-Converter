namespace XBeeLib
{
    public class CTXRequest64 : CTXRequestBasic
    {
        public CTXRequest64()
        {
            APID = CXBAPICommands.TXRequest64bitAddress;
        }

        public CTXRequest64(ulong DestinationAddress64)
        {
            this.DestinationAddress64 = DestinationAddress64;
            APID = CXBAPICommands.TXRequest64bitAddress;
        }

        private ulong _DestinationAddress64 = 0;
        /// <summary>
        /// "SH, SL" = Serial number of the Remote device
        /// </summary>
        public ulong DestinationAddress64
        {
            get { return _DestinationAddress64; }
            set { _DestinationAddress64 = value; }
        }

        public override TXRequestOptions options
        {
            get { return _options; }
            set
            {
                _options = value;
                if (value == TXRequestOptions.sendPacketWithBroadcastPanID)
                    DestinationAddress64 = 0x000000000000FFFF;
            }
        }

        /// <summary>
        /// Returns complete byte [] to be sent to configure remote device
        /// </summary>
        public override byte[] Get_CommandRequest_DataFrame(XBAPIMode ApiMode)
        {
            List<byte> FrameData =
            [
                //FrameData Byte 0
                APID,
                //FrameData Byte 1
                frameId
            ];

            //FrameData Byte 2-9: 64 Bit Destination Address
            byte[] be = BitConverter.GetBytes(_DestinationAddress64);
            for (int i = be.Length - 1; i >= 0; i--)
                FrameData.Add(be[i]);

            FrameData.Add((byte)options);
            FrameData.AddRange(rfData);

            return MakeBasicDataFrame(FrameData, ApiMode);
        }

        /// <summary>
        /// Returns complete byte [] to be sent to configure remote device
        /// </summary>
        public byte[] Get_TX_TransmmitRequest_DataFrame(XBAPIMode ApiMode, byte[] RFData)
        {
            rfData = new List<byte>(RFData);
            //rfData = RFData.ToList(); //--> braucht LINQ
            APID = CXBAPICommands.TXRequest64bitAddress;
            return Get_CommandRequest_DataFrame(ApiMode);
        }

    }

}

