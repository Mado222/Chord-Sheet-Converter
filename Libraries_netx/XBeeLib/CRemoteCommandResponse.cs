using System.Text;

namespace XBeeLib
{
    /// <summary>
    /// implements the API Type Remote Command Response
    /// </summary>
    /// <remarks>base class: CATCommandResponse</remarks>
    public class CRemoteCommandResponse : CATCommandResponse
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>
        /// inits the AP ID
        /// </remarks>
        public CRemoteCommandResponse()
        {
            APID = CXBAPICommands.RemoteCommandResponse;
        }

        private ushort _ResponderAddress16 = 0;
        /// <summary>
        /// responder address (16bit)
        /// </summary>
        virtual public ushort ResponderAddress16
        {
            get { return _ResponderAddress16; }
            set { _ResponderAddress16 = value; }
        }

        private ulong _ResponderAddress64 = 0;
        /// <summary>
        /// responder address (64bit)
        /// </summary>
        virtual public ulong ResponderAddress64
        {
            get { return _ResponderAddress64; }
            set { _ResponderAddress64 = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame data
        /// </summary>
        /// <param name="recFrameData">received frame data</param>
        override public void InitResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //Frame ID
            listEnum.MoveNext();
            FrameId = listEnum.Current;

            //64-bit ResponderAddress
            byte[] be = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            ResponderAddress64 = BitConverter.ToUInt64(be, 0);

            //16-bit ResponderAddress
            be = new byte[2];
            for (int i = 1; i >= 0; i--)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            ResponderAddress16 = BitConverter.ToUInt16(be, 0);

            //AT Command
            be = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            Command = ASCIIEncoding.ASCII.GetString(be);

            //Status
            listEnum.MoveNext();
            Status = (RXCommandResponseStatus)listEnum.Current;

            //Value   
            List<byte> ByteListOfValue = [];
            while (listEnum.MoveNext())
            {
                ByteListOfValue.Add(listEnum.Current);
            }
            ValueOfCommand = [.. ByteListOfValue];

        }


    }
}
