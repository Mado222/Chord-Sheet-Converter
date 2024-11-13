using System.Text;
using WindControlLib;

namespace XBeeLib
{

    /// <summary>
    /// implements the AT-Command (API Identifier Value: 0x08)
    /// </summary>
    /// <remarks>inherits from CBasicAPICommand</remarks>
    public class CATCommand : CBasicAPIRequest
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>inits the AP-ID</remarks>
        public CATCommand() => APID = CXBAPICommands.ATCommand;

        //private bool _ApplyCommandImmediately = true; // default: AT Command
        /// <summary>
        /// to distinguish between "AT Command" and "AT Command - Queue Parameter Value"
        /// </summary>
        //private bool ApplyCommandImmediately
        //{
        //    get { return _ApplyCommandImmediately; }
        //    set
        //    {
        //        _ApplyCommandImmediately = value;
        //        if (value == true)
        //            APID = CXBAPICommands.ATCommand;
        //        else
        //            APID = CXBAPICommands.ATCommandQueueParameterValue;
        //    }
        //}

        private bool _ATCommandResponse;
        /// <summary>
        /// Whether the remote device should answer with a "AT Command Response"
        /// </summary>
        public bool ATCommandResponse
        {
            get { return _ATCommandResponse; }
            set
            {
                _ATCommandResponse = value;
                if (value == true)
                    FrameId = 1;
                else
                    FrameId = 0;
            }
        }

        /// <summary>
        /// frame-id
        /// </summary>
        /// <remarks>is set to 1 if ATCommandResponse is set to true
        /// is set to 0 if ATCommandResponse is set to false</remarks>
        public byte FrameId { get; set; } = 1;

        /// <summary>
        /// AT-Command (in AT-format)
        /// </summary>
        /// <remarks>use the AT-Commands specified in the CXATCommands class</remarks>
        public string ATCommand { get; set; } = "";

        /// <summary>
        /// makes the basic data frama for sending them on serial port
        /// </summary>
        /// <param name="FrameData">Frama Data</param>
        /// <returns>byte array, ready to send on serial port</returns>
        override protected byte[] MakeBasicDataFrame(List<byte> FrameData)
        {
            return base.MakeBasicDataFrame(FrameData);
        }

        /// <summary>
        /// Returns complete byte [] to be sent to over the serial port
        /// </summary>
        /// <param name="ApiMode">API-Mode</param>
        /// <returns>byte array, ready to sent over the serial port</returns>
        override public byte[] Get_CommandRequest_DataFrame(XBAPIMode ApiMode)
        {
            List<byte> FrameData =
            [
                //Start to build FrameData
                //FrameData Byte 0
                APID,
                //FrameData Byte 1
                FrameId,
                .. Command2ByteList(),
            ];

            return MakeBasicDataFrame(FrameData, ApiMode);
        }

        /// <summary>
        /// Returns a list of bytes containing the command and parameter of command
        /// </summary>
        /// <returns>Returns a list of bytes containing the command and parameter of command</returns>
        protected List<byte> Command2ByteList()
        {
            List<byte> Command = [];
            //FrameData Byte 6-7: Command Name

            //Remove all special chars
            StringBuilder sb = new();
            for (int i = 0; i < ATCommand.Length; i++)
            {
                if (char.IsLetterOrDigit(ATCommand[i])) // e.g. "ATMY 0001\r" --> ATMY0001
                    sb.Append(ATCommand[i]);
            }
            byte[] be = CMyConvert.StringToByteArray(sb.ToString());

            //be[0,1]= "AT"
            Command.Add(be[2]); //Command Name 1  e.g. ATMY0001 ---> MY is cut out
            Command.Add(be[3]); //Command Name 2

            //Is there a Parameter to Add?
            string s = sb.ToString().Remove(0, 4); // e.g. ATDL00000FFF ---> 00000FFF
            if (s.Length % 2 != 0)  //Even or Odd number od chars
            {
                //Odd
                s = "0" + s;    //add zero as first char
            }
            /*
            if (s.Length == 1)  //only one number? uneven number?
                s = "0" + s;    //add zero as first char
             */
            if (s != "")
            {
                // if command is NI or DN
                if ((be[2] == 'N' && be[3] == 'I') ||
                    (be[2] == 'D' && be[3] == 'N'))
                {
                    be = CMyConvert.StringToByteArray(s);
                    for (int i = 0; i < be.Length; i++)
                    {
                        Command.Add(be[i]);
                    }
                } // else 
                else
                {
                    ulong number = Convert.ToUInt32(s, 16);
                    be = BitConverter.GetBytes(number);
                    for (int i = (s.Length / 2) - 1; i >= 0; i--)
                        Command.Add(be[i]);

                }
            }
            return Command;
        }

        /// <summary>
        /// checks if the received response corresponse to the request
        /// </summary>
        /// <param name="response">received response</param>
        /// <returns>
        /// true: if the response corresponses the the request
        /// false: if not</returns>
        override public bool CheckResponse(CBasicAPIResponse response)
        {
            bool IsResponseCorrect = false;
            switch (response.APID)
            {
                case CXBAPICommands.ATCommandResponse:
                    CATCommandResponse responseNew = (CATCommandResponse)response;

                    //if (responseNew.status == RXCommandResponseStatus.OK)
                    //{
                    if (FrameId == responseNew.FrameId)
                    {
                        if (ATCommand.Substring(2, 2) == responseNew.Command)
                        {
                            IsResponseCorrect = true;
                        }
                    }
                    //}
                    break;
                default:
                    IsResponseCorrect = false;
                    break;
            }
            return IsResponseCorrect;
        }
    }
}
