namespace XBeeLib
{
    abstract public class CTXRequestBasic : CBasicAPIRequest
    {

        protected byte _frameId = 1;
        public byte FrameId
        {
            get { return _frameId; }
            set { _frameId = value; }
        }

        protected TXRequestOptions _options = TXRequestOptions.noOption;
        virtual public TXRequestOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;
            }
        }

        protected List<byte> _rfData = [];
        public List<byte> RfData
        {
            get { return _rfData; }
            set
            {
                if (RfData.Count > 100)
                {
                    throw new Exception("Max. RFData site = 100");
                }
                _rfData = value;
            }
        }

        /// <summary>
        /// Returns complete byte [] to be sent to configure remote device
        /// </summary>
        public override bool CheckResponse(CBasicAPIResponse response)
        {
            bool IsResponseCorrect = false;
            switch (response.APID)
            {
                case CXBAPICommands.TXStatus:

                    CTXStatusResponse responseNew = (CTXStatusResponse)response;

                    if (FrameId == responseNew.FrameId)
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
