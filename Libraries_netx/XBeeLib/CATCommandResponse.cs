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

        protected byte _frameId;
        /// <summary>
        /// frame id of the received frame
        /// </summary>
        virtual public byte frameId
        {
            get { return _frameId; }
            set { _frameId = value; }
        }

        protected string _command;
        /// <summary>
        /// command of the received frame
        /// </summary>
        virtual public string command
        {
            get { return _command; }
            set { _command = value; }
        }

        protected byte[] _valueOfCommand;
        /// <summary>
        /// value of command of the received frame
        /// </summary>
        virtual public byte[] valueOfCommand
        {
            get { return _valueOfCommand; }
            set { _valueOfCommand = value; }
        }

        protected RXCommandResponseStatus _status;
        /// <summary>
        /// command response status of the received frame
        /// </summary>
        virtual public RXCommandResponseStatus status
        {
            get { return _status; }
            set { _status = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame data
        /// </summary>
        /// <param name="recFrameData">received frame data</param>
        override public void initResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //Frame ID
            listEnum.MoveNext();
            frameId = listEnum.Current;
            
            //AT Command
            byte[] be = new byte[2];
            for (int i=0; i<2;i++)
            {
                listEnum.MoveNext();
                be[i] = listEnum.Current;
            }
            command = ASCIIEncoding.ASCII.GetString(be);

            //Status
            listEnum.MoveNext();
            status = (RXCommandResponseStatus)listEnum.Current;

            //Value   
            List<byte> ByteListOfValue = new List<byte>(); 
            while(listEnum.MoveNext())
            {
                ByteListOfValue.Add(listEnum.Current);
            }
            valueOfCommand = ByteListOfValue.ToArray();

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
