using System;
using System.Collections.Generic;

namespace XBeeLib
{
    abstract public class CTXRequestBasic : CBasicAPIRequest
    {

        protected byte _frameId = 1;
        public byte frameId
        {
            get { return _frameId; }
            set { _frameId = value; }
        }

        protected TXRequestOptions _options = TXRequestOptions.noOption;
        virtual public TXRequestOptions options
        {
            get { return _options; }
            set
            {
                _options = value;
            }
        }

        protected List<byte> _rfData = [];
        public List<byte> rfData
        {
            get { return _rfData; }
            set
            {
                if (rfData.Count > 100)
                {
                    throw new Exception("Max. RFData site = 100");
                }
                _rfData = value;
            }
        }

        /// <summary>
        /// Returns complete byte [] to be sent to configure remote device
        /// </summary>
        public override bool checkResponse(CBasicAPIResponse response)
        {
            bool IsResponseCorrect = false;
            switch (response.APID)
            {
                case (CXBAPICommands.TXStatus):

                    CTXStatusResponse responseNew = (CTXStatusResponse)response;

                    if (frameId == responseNew.frameId)
                    {
                        IsResponseCorrect = true;
                    }
                    break;

                default:
                    IsResponseCorrect = false;
                    break;
            }
            return IsResponseCorrect;
        }
    }


    public enum TXRequestOptions
    {
        noOption = 0x00,
        disableACK = 0x01,
        sendPacketWithBroadcastPanID = 0x04
    }
}
