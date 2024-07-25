using System.Collections.Generic;

namespace XBeeLib
{
    /// <summary>
    /// base class for API responses
    /// </summary>
    abstract public class CBasicAPIResponse
    {
        protected byte _APID;
        /// <summary>
        /// AP-ID
        /// </summary>
        public byte APID
        {
            get { return _APID; }
            set { _APID = value; }
        }

        protected int _length;
        /// <summary>
        /// length of the api frame
        /// </summary>
        /// <remarks>is not in use at the moment</remarks>
        public int length
        {
            get { return _length; }
            set { _length = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">receiving frame data</param>
        abstract public void initResponse(List<byte> recFrameData);

    }
}
