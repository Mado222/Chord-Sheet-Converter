namespace XBeeLib
{


    /************************************************************************************
    * ************************************************************************************
    * CLASS: CBasicAPICommand
    * ************************************************************************************
   ************************************************************************************/


    /// <summary>
    /// base class for API request types
    /// </summary>
    /// <remarks>
    /// Implements the API Mode Date Frame
    /// Byte 0: Start Delimiter
    /// Byte 1: Length MSB
    /// Byte 2: Length LSB
    /// Byte 3-n: Frame Data
    /// Byte n+1: Checksum
    /// </remarks>
    abstract public class CBasicAPIRequest
    {
        protected byte _APID;
        /// <summary>
        /// AP-ID of the frame
        /// </summary>
        virtual public byte APID
        {
            get { return _APID; }
            protected set { _APID = value; }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>not neccessary, because its an abstract class</remarks>
        public CBasicAPIRequest()
        {
        }

        /// <summary>
        /// makes basic data frame of the request
        /// </summary>
        /// <param name="ApiMode">API mode</param>
        /// <param name="FrameData">frame data</param>
        /// <returns>byte array, ready to send over the serial port</returns>
        virtual protected byte[] MakeBasicDataFrame(List<byte> FrameData, XBAPIMode ApiMode)
        {

            List<byte> CompleteAPICommand =
            [
                //API Command Byte 0: Start Delimiter
                CXBAPICommands.StartDelimiter
            ];

            //API Command Byte 1: Length MSB, Byte 2: Length LSB
            byte[] be = BitConverter.GetBytes((ushort)FrameData.Count);
            for (int i = 1; i >= 0; i--)
            {
                if (ApiMode == XBAPIMode.Enabled_w_Escape_Control_Chars &&
                    (be[i] == CXBAPICommands.StartDelimiter ||
                    be[i] == 0x7D ||
                    be[i] == 0x11 ||
                    be[i] == 0x13))
                {
                    CompleteAPICommand.Add(0x7D);
                    CompleteAPICommand.Add((byte)(be[i] ^ 0x20));
                }
                else
                {
                    CompleteAPICommand.Add(be[i]);
                }
            }

            //frame data
            byte Checksum = 0;
            for (int i = 0; i < FrameData.Count; i++)
            {
                Checksum += FrameData[i];
                if (ApiMode == XBAPIMode.Enabled_w_Escape_Control_Chars &&
                    (FrameData[i] == CXBAPICommands.StartDelimiter ||
                    FrameData[i] == 0x7D ||
                    FrameData[i] == 0x11 ||
                    FrameData[i] == 0x13))
                {
                    CompleteAPICommand.Add(0x7D);
                    CompleteAPICommand.Add((byte)(FrameData[i] ^ 0x20));
                }
                else
                {
                    CompleteAPICommand.Add(FrameData[i]);
                }
            }

            //Checksum
            Checksum = (byte)(0xff - Checksum);
            if (ApiMode == XBAPIMode.Enabled_w_Escape_Control_Chars &&
                    (Checksum == CXBAPICommands.StartDelimiter ||
                    Checksum == 0x7D ||
                    Checksum == 0x11 ||
                    Checksum == 0x13))
            {
                CompleteAPICommand.Add(0x7D);
                CompleteAPICommand.Add((byte)(Checksum ^ 0x20));
            }
            else
            {
                CompleteAPICommand.Add(Checksum);
            }

            return [.. CompleteAPICommand];
        }

        /// <summary>
        /// makes basic data frame of the request
        /// </summary>
        /// <param name="FrameData">frame data</param>
        /// <remarks>API mode is set to ENABLED</remarks>
        /// <returns>byte array, ready to send over the serial port</returns>
        virtual protected byte[] MakeBasicDataFrame(List<byte> FrameData)
        {
            return MakeBasicDataFrame(FrameData, XBAPIMode.Enabled);
        }

        /// <summary>
        /// checks if the received response corresponses to the request
        /// </summary>
        /// <param name="response">received response</param>
        /// <returns>
        /// true: if the received response corresponse to the request
        /// false: if not 
        /// </returns>
        abstract public bool checkResponse(CBasicAPIResponse response);

        /// <summary>
        /// returns the byte array representation of the frame
        /// </summary>
        /// <remarks>
        /// returns the byte array representation of the frame, ready to send over the serial port
        /// </remarks>
        /// <param name="ApiMode">API-Mode</param>
        /// <returns>byte array, ready to sent over the serial port</returns>
        abstract public byte[] Get_CommandRequest_DataFrame(XBAPIMode ApiMode);

    }

}
