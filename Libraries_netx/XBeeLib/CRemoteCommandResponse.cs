using System;
using System.Collections.Generic;
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

        private UInt16 _ResponderAddress16 = 0;
        /// <summary>
        /// responder address (16bit)
        /// </summary>
        virtual public UInt16 ResponderAddress16
        {
            get { return _ResponderAddress16; }
            set { _ResponderAddress16 = value; }
        }

        private UInt64 _ResponderAddress64 = 0;
        /// <summary>
        /// responder address (64bit)
        /// </summary>
        virtual public UInt64 ResponderAddress64
        {
            get { return _ResponderAddress64; }
            set { _ResponderAddress64 = value; }
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
            command = ASCIIEncoding.ASCII.GetString(be);

            //Status
            listEnum.MoveNext();
            status = (RXCommandResponseStatus)listEnum.Current;

            //Value   
            List<byte> ByteListOfValue = new List<byte>();
            while (listEnum.MoveNext())
            {
                ByteListOfValue.Add(listEnum.Current);
            }
            valueOfCommand = ByteListOfValue.ToArray();

        }


    }
}
