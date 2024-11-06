using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neuromaster_Demo_Library_Reduced__netx
{
    /// <summary>
    /// Has const Length and is sent before another datapacket of variable Length follows
    /// </summary>
    public class CTCPPacketHeader
    {
        public const UInt16 PacketType_Data = 1;
        public const UInt16 PacketType_Error = 2;

        private Guid _UniquePacketID;
        /// <summary>
        /// Unique packer ID automatically added
        /// Can be used to identify answers
        /// </summary>
        public Guid UniquePacketID
        {
            get { return _UniquePacketID; }
        }

        /// <summary>
        /// Length of the packet following the Packet Header
        /// </summary>
        public UInt16 LengthOfFollowingPacket = 0;

        /// <summary>
        /// Type of the packet following the Packet Header
        /// </summary>
        private UInt16 _TCPPacketType;
        public UInt16 TCPPacketType
        {
            get { return _TCPPacketType; }
            set { _TCPPacketType = value; }
        }

        //Guid has 16 bytes
        private static int _sizeofGuid = 16;

        public int _PacketSize;
        /// <summary>
        /// Number of bytes of this packet
        /// </summary>
        public int PacketSize
        {
            get { return _PacketSize; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CTCPPacketHeader"/> class.
        /// </summary>
        public CTCPPacketHeader()
        {
            _UniquePacketID = System.Guid.NewGuid();

            _PacketSize = _sizeofGuid +
                System.Runtime.InteropServices.Marshal.SizeOf(_TCPPacketType) +
                System.Runtime.InteropServices.Marshal.SizeOf(LengthOfFollowingPacket);
        }


        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = new List<byte>();
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            byte[] b = _UniquePacketID.ToByteArray();
            buf.AddRange(b);
            b = [(byte)_TCPPacketType];
            buf.AddRange(b);
            b = BitConverter.GetBytes(LengthOfFollowingPacket);
            buf.AddRange(b);
            buffer = buf.ToArray();
        }

        public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start, uint TCPPacketLength)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            //Guid has 16 bytes
            byte[] b = new byte[_sizeofGuid];
            Buffer.BlockCopy(InBuf, Pointer_To_Array_Start, b, 0, _sizeofGuid);
            _UniquePacketID = new Guid(b);
            ptr += _sizeofGuid;

            TCPPacketType = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(TCPPacketType);
            LengthOfFollowingPacket = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(LengthOfFollowingPacket);
            return ptr;
        }
    }

    public enum enumTCPPacketTypes
    {
        DataPAcket,
        ErrorPacket
    }

    public class CTCPDataPacket: ICloneable
    {

        private CTCPPacketHeader _TCPPacketHeader;
        public CTCPPacketHeader TCPPacketHeader
        {
            get { return _TCPPacketHeader; }
            set { _TCPPacketHeader = value; }
        }

        public CTCPDataPacket()
        {
            _TCPPacketHeader = new CTCPPacketHeader();
        }

        public void Update(CTCPPacketHeader TCPPacketHeader)
        {
            byte[] b = new byte[0];
            TCPPacketHeader.GetByteArray(ref b, 0);
            this.TCPPacketHeader.UpdateFrom_ByteArray(b, 0, 0);
        }


        public new void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = new List<byte>();
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            //Make array for CAN Message
            byte[] arrCANMsg = new byte[0];
            //base.GetByteArray(ref arrCANMsg, 0);

            TCPPacketHeader.LengthOfFollowingPacket = (UInt16)arrCANMsg.Length;
            TCPPacketHeader.TCPPacketType = CTCPPacketHeader.PacketType_Data;
            byte[] arrHeader = new byte[0];
            TCPPacketHeader.GetByteArray(ref arrHeader, 0);
            buf.AddRange(arrHeader);
            buf.AddRange(arrCANMsg);
            buffer = buf.ToArray();
        }

        public int Update_Header_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            return TCPPacketHeader.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start, 0);
        }

        public int Update_Data_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            return 0;// this.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start, TCPPacketHeader.LengthOfFollowingPacket);
        }

        public new object Clone()
        {
            CTCPDataPacket p = new CTCPDataPacket();
            byte[] buf = new byte[0];
            this.TCPPacketHeader.GetByteArray(ref buf, 0);
            p.Update_Header_From_ByteArray(buf, 0);

            this.GetByteArray(ref buf, 0);
            p.Update_Data_From_ByteArray(buf, 0);
            return p;
        }
    }

    public class CTCPErrorPacket : ICloneable
    {

        private CTCPPacketHeader _TCPPacketHeader;
        public CTCPPacketHeader TCPPacketHeader
        {
            get { return _TCPPacketHeader; }
            set { _TCPPacketHeader = value; }
        }

        private string _ErrorString;
        public string ErrorString
        {
            get { return _ErrorString; }
            set { _ErrorString = value; }
        }

        public CTCPErrorPacket()
        {
            _TCPPacketHeader = new CTCPPacketHeader();
        }

        public void Update(CTCPPacketHeader TCPPacketHeader)
        {
            byte[] b = new byte[0];
            TCPPacketHeader.GetByteArray(ref b, 0);
            this.TCPPacketHeader.UpdateFrom_ByteArray(b, 0, 0);
        }

        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = new List<byte>();
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            //Make array for CAN Message
            byte[] arrCANMsg = new byte[0];

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            arrCANMsg = enc.GetBytes(ErrorString);

            TCPPacketHeader.LengthOfFollowingPacket = (UInt16)arrCANMsg.Length;
            TCPPacketHeader.TCPPacketType = CTCPPacketHeader.PacketType_Error;
            byte[] arrHeader = new byte[0];
            TCPPacketHeader.GetByteArray(ref arrHeader, 0);
            buf.AddRange(arrHeader);
            buf.AddRange(arrCANMsg);
            buffer = buf.ToArray();
        }

        public int Update_Header_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            return TCPPacketHeader.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start, 0);
        }

        public int Update_Data_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            enc.GetString(InBuf);
            return Pointer_To_Array_Start + InBuf.Length;
        }

        public object Clone()
        {
            CTCPErrorPacket p = new CTCPErrorPacket();
            byte[] buf = new byte[0];
            this.TCPPacketHeader.GetByteArray(ref buf, 0);
            p.Update_Header_From_ByteArray(buf, 0);
            p.ErrorString = ErrorString;
            return p;
        }
    }
}
