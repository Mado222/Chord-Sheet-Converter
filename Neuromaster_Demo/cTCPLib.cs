using System.Text;

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
        private static readonly int _sizeofGuid = 16;

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


        public void GetByteArray(ref byte[] buffer, int indexWhereToStartFilling)
        {
            List<byte> buf = [];

            // Reserve space if needed
            if (indexWhereToStartFilling > 0)
            {
                buf.AddRange(buffer.Take(indexWhereToStartFilling));
            }

            // Append the data sequentially
            buf.AddRange(_UniquePacketID.ToByteArray());
            buf.Add((byte)_TCPPacketType);
            buf.AddRange(BitConverter.GetBytes(LengthOfFollowingPacket));

            // Update the buffer with the newly constructed byte array
            buffer = [.. buf];
        }


        public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)//, uint TCPPacketLength)
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

    public enum EnumTCPPacketTypes
    {
        DataPAcket,
        ErrorPacket
    }

    public class CTCPDataPacket : ICloneable
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
            byte[] b = [];
            TCPPacketHeader.GetByteArray(ref b, 0);
            this.TCPPacketHeader.UpdateFrom_ByteArray(b, 0);
        }


        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = [];
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            //Make array for CAN Message
            byte[] arrCANMsg = Array.Empty<byte>();
            //base.GetByteArray(ref arrCANMsg, 0);

            TCPPacketHeader.LengthOfFollowingPacket = (UInt16)arrCANMsg.Length;
            TCPPacketHeader.TCPPacketType = CTCPPacketHeader.PacketType_Data;
            byte[] arrHeader = Array.Empty<byte>();
            TCPPacketHeader.GetByteArray(ref arrHeader, 0);
            buf.AddRange(arrHeader);
            buf.AddRange(arrCANMsg);
            buffer = [.. buf];
        }

        public int Update_Header_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            return TCPPacketHeader.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start);
        }

        public object Clone()
        {
            CTCPDataPacket p = new();
            byte[] buf = [];
            TCPPacketHeader.GetByteArray(ref buf, 0);
            p.Update_Header_From_ByteArray(buf, 0);
            GetByteArray(ref buf, 0);
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

        private string _ErrorString = "";
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
            byte[] b = Array.Empty<byte>();
            TCPPacketHeader.GetByteArray(ref b, 0);
            this.TCPPacketHeader.UpdateFrom_ByteArray(b, 0);
        }

        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = [];
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            //Make array for CAN Message
            byte[] arrCANMsg;

            ASCIIEncoding enc = new();
            arrCANMsg = enc.GetBytes(ErrorString);

            TCPPacketHeader.LengthOfFollowingPacket = (ushort)arrCANMsg.Length;
            TCPPacketHeader.TCPPacketType = CTCPPacketHeader.PacketType_Error;
            byte[] arrHeader = Array.Empty<byte>();
            TCPPacketHeader.GetByteArray(ref arrHeader, 0);
            buf.AddRange(arrHeader);
            buf.AddRange(arrCANMsg);
            buffer = [.. buf];
        }

        public int Update_Header_From_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            return TCPPacketHeader.UpdateFrom_ByteArray(InBuf, Pointer_To_Array_Start);
        }

        public static int UpdateDataFromByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            ASCIIEncoding enc = new();
            enc.GetString(InBuf);
            return Pointer_To_Array_Start + InBuf.Length;
        }

        public object Clone()
        {
            CTCPErrorPacket p = new();
            byte[] buf = Array.Empty<byte>();
            TCPPacketHeader.GetByteArray(ref buf, 0);
            p.Update_Header_From_ByteArray(buf, 0);
            p.ErrorString = ErrorString;
            return p;
        }
    }
}
