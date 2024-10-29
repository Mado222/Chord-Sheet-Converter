using System.Collections.Generic;

namespace XBeeLib
{
    /// <summary>
    /// superclass of CRXPacket64 and CRXPacket16
    /// </summary>
    abstract public class CRXPacketBasic : CBasicAPIResponse
    {
        protected byte _RSSI;
        /// <summary>
        /// RSSI
        /// </summary>
        /// <value>
        /// Received Signal Strength Indicator -
        /// Hexadecimal equivalent of (-dBm) value.
        /// (For example: If RX signal strength = -40
        /// dBm, “0x28” (40 decimal) is returned)
        /// </value>
        virtual public byte RSSI
        {
            get { return _RSSI; }
            set { _RSSI = value; }
        }

        protected bool _IsAddressBroadcast = false;
        /// <summary>
        /// address broadcast flag
        /// </summary>
        virtual public bool IsAddressBroadcast
        {
            get { return _IsAddressBroadcast; }
            set { _IsAddressBroadcast = value; }
        }

        protected bool _IsPANBroadcast = false;
        /// <summary>
        /// pan broadcast flag
        /// </summary>
        virtual public bool IsPANBroadcast
        {
            get { return _IsPANBroadcast; }
            set { _IsPANBroadcast = value; }
        }

        protected List<byte> _rfData = [];
        /// <summary>
        /// received RF data
        /// </summary>
        virtual public List<byte> rfData
        {
            get { return _rfData; }
            set { _rfData = value; }
        }

        /// <summary>
        /// inits the members of this class
        /// </summary>
        /// <param name="listEnum">enumerator of a byte array (containing all bytes of framedata beginning with the RSSI)</param>
        virtual protected void initMembers(List<byte>.Enumerator listEnum)
        {
            //RSSI
            listEnum.MoveNext();
            RSSI = listEnum.Current;

            //Options
            listEnum.MoveNext();
            byte options = listEnum.Current;
            if ((options & 0x02) == 0x02)
                IsAddressBroadcast = true;
            if ((options & 0x04) == 0x04)
                IsPANBroadcast = true;

            //RF-Data 
            List<byte> ByteListOfRFData = [];
            while (listEnum.MoveNext())
            {
                ByteListOfRFData.Add(listEnum.Current);
            }
            rfData = ByteListOfRFData;
        }

    }

}
