using System.Collections.Generic;

namespace XBeeLib
{

    /// <summary>
    /// implements the API type TX Status Response (0x89)
    /// </summary>
    public class CTXStatusResponse : CBasicAPIResponse
    {
        private byte _frameId;
        /// <summary>
        /// frame id
        /// </summary>
        virtual public byte frameId
        {
            get { return _frameId; }
            set { _frameId = value; }
        }

        private TXStatusOptions _TXStatus;
        /// <summary>
        /// transmit status
        /// </summary>
        virtual public TXStatusOptions TXStatus
        {
            get { return _TXStatus; }
            set { _TXStatus = value; }
        }

        /// <summary>
        /// constructor 
        /// </summary>
        /// <remarks>inits the AP ID</remarks>
        public CTXStatusResponse()
        {
            APID = CXBAPICommands.TXStatus;
        }


        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">receiving frame data</param>
        override public void initResponse(List<byte> recFrameData)
        {
            List<byte>.Enumerator listEnum = recFrameData.GetEnumerator();

            //Frame ID
            if (listEnum.MoveNext())
            {
                frameId = listEnum.Current;
                
                //Status
                if (listEnum.MoveNext())
                {
                    TXStatus = (TXStatusOptions)listEnum.Current;
                }
            }
        }
    }

    public enum TXStatusOptions
    {
        Success,
        NoACKReceived,
        CCAFailure,
        Purged
    }

}
