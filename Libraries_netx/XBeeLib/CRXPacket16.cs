using System;
using System.Collections.Generic;

namespace XBeeLib
{
    /// <summary>
    /// implements the API Type RX Packet 16-bit Address (0x81)
    /// </summary>
    /// <remarks>base class: CRXPacketBasic</remarks>
    public class CRXPacket16 : CRXPacketBasic
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>inits the AP ID</remarks>
        public CRXPacket16()
        {
            APID = CXBAPICommands.RXPacket16bit;
        }

        private UInt16 _SourceAddress16;
        /// <summary>
        /// source address (16bit)
        /// </summary>
        /// <remarks>MY-Address of the remote device</remarks>
        virtual public UInt16 SourceAddress16
        {
            get { return _SourceAddress16; }
            set { _SourceAddress16 = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">receiving frame data</param>
        override public void initResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //16-bit ResponderAddress
            byte[] be = new byte[2];
            for (int i = 1; i >= 0; i--)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            SourceAddress16 = BitConverter.ToUInt16(be, 0);

            initMembers(listEnum);

        }
    }
}
