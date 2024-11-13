namespace XBeeLib
{

    /// <summary>
    /// implements the API Type RX Packet 64-bit Address (0x80)
    /// </summary>
    /// <remarks>base class: CRXPacketBasic</remarks>
    public class CRXPacket64 : CRXPacketBasic
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>inits the AP ID</remarks>
        public CRXPacket64()
        {
            APID = CXBAPICommands.RXPacket64bit;
        }

        private ulong _SourceAddress64 = 0;
        /// <summary>
        /// source address (64bit)
        /// </summary>
        /// <remarks>serial number of remote device</remarks>
        virtual public ulong SourceAddress64
        {
            get { return _SourceAddress64; }
            set { _SourceAddress64 = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">receiving frame data</param>
        override public void InitResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //64-bit ResponderAddress
            byte[] be = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            SourceAddress64 = BitConverter.ToUInt64(be, 0);

            InitMembers(listEnum);
        }

    }
}
