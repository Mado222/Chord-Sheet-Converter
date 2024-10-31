using System.Collections.Generic;
using System.Text;

namespace XBeeLib
{
    /// <summary>
    /// implements the AT Command Response (API type: 0x88)
    /// </summary>
    /// <remarks>inherits from the class CBasicAPIResponse</remarks>
    public class CATCommandResponse : CBasicAPIResponse
    {
        /// <summary>
        /// constructor 
        /// </summary>
        /// <remarks>inits the AP-ID</remarks>
        public CATCommandResponse()
        {
            APID = CXBAPICommands.ATCommandResponse;
        }

        /// <summary>
        /// frame id of the received frame
        /// </summary>
        public virtual byte FrameId { get; set; }

        /// <summary>
        /// command of the received frame
        /// </summary>
        public virtual string Command { get; set; } = "";

        /// <summary>
        /// value of command of the received frame
        /// </summary>
        public virtual byte[] ValueOfCommand { get; set; } = [];

        /// <summary>
        /// command response status of the received frame
        /// </summary>
        public virtual RXCommandResponseStatus Status { get; set; }

        /// <summary>
        /// inits the object with the receiving frame data
        /// </summary>
        /// <param name="recFrameData">received frame data</param>
        override public void initResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //Frame ID
            listEnum.MoveNext();
            FrameId = listEnum.Current;

            //AT Command
            byte[] be = new byte[2];
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

    /// <summary>
    /// enum for command response status
    /// </summary>
    public enum RXCommandResponseStatus
    {
        OK,
        ERROR,
        InvalidCommand,
        InvalidParameter,
        NoResponse
    }
}
