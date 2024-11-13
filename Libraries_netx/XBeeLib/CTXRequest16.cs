namespace XBeeLib
{
    public class CTXRequest16 : CTXRequestBasic
    {
        public CTXRequest16()
        {
            APID = CXBAPICommands.TXRequest16bitAaddress;
        }

        /// <summary>
        /// </summary>
        private ushort _DestinationAddress16 = CXBAPICommands.Default16BitAddress;
        /// <summary>
        /// "MY" Address of the Remote Device
        /// </summary>
        public ushort DestinationAddress16
        {
            get { return _DestinationAddress16; }
            set { _DestinationAddress16 = value; }
        }

        override public TXRequestOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;
                if (value == TXRequestOptions.sendPacketWithBroadcastPanID)
                    DestinationAddress16 = 0xFFFF;
            }
        }

        /// <summary>
        /// Basic DataFrame
        /// </summary>
        override protected byte[] MakeBasicDataFrame(List<byte> FrameData)
        {
            return base.MakeBasicDataFrame(FrameData);
        }

        /// <summary>
        /// Returns complete byte [] to be sent to configure remote device
        /// </summary>
        override public byte[] Get_CommandRequest_DataFrame(XBAPIMode ApiMode)
        {
            List<byte> FrameData =
            [
                //FrameData Byte 0
                APID,
                //FrameData Byte 1
                FrameId
            ];

            //FrameData Byte 2-3: 16 Bit Destination Address
            byte[] be = BitConverter.GetBytes(_DestinationAddress16);
            for (int i = be.Length - 1; i >= 0; i--)
                FrameData.Add(be[i]);

            FrameData.Add((byte)Options);
            FrameData.AddRange(RfData);

            return MakeBasicDataFrame(FrameData, ApiMode);
        }
    }
}
