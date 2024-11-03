using System.Runtime.InteropServices;

namespace FeedbackDataLib
{
    /// <summary>
    /// Hardware specific class - matches "struct sSWChannelInfo" in PIC
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CSWChannelInfo : ICloneable
    {
        /// <summary>
        /// Resolution of the AD (number of bits)
        /// </summary>
        public ushort ADResolution
        {
            get { return _ADResolution; }
            set { _ADResolution = value; }
        }
        private ushort _ADResolution;

        /// <summary>
        /// Mid of Range of hex values
        /// </summary>
        public ushort MidofRange
        {
            get { return _MidofRange; }
            set { _MidofRange = value; }
        }
        private ushort _MidofRange;

        /// <summary>
        /// Reference voltage source installed
        /// </summary>
        public double uref
        {
            get { return _uref; }
            set { _uref = value; }
        }
        private double _uref;

        /// <summary>
        /// Hex- Offset
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public short Offset_hex
        {
            get { return _Offset_hex; }
            set { _Offset_hex = value; }
        }
        private short _Offset_hex;

        /// <summary>
        /// Scaling factor
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public double SkalValue_k
        {
            get { return _SkalValue_k; }
            set { _SkalValue_k = value; }
        }
        private double _SkalValue_k;

        /// <summary>
        /// Offset
        /// </summary>
        /// <remarks>ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d</remarks>
        public double Offset_d
        {
            get { return _Offset_d; }
            set { _Offset_d = value; }
        }
        private double _Offset_d;

        /// <summary>
        /// Fill properies according to corresponding structure in Device
        /// </summary>
        public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            ADResolution = BitConverter.ToUInt16(InBuf, ptr); ptr += Marshal.SizeOf(ADResolution);
            Offset_hex = BitConverter.ToInt16(InBuf, ptr); ptr += Marshal.SizeOf(Offset_hex);
            MidofRange = BitConverter.ToUInt16(InBuf, ptr); ptr += Marshal.SizeOf(MidofRange);
            uref = BitConverter.ToDouble(InBuf, ptr); ptr += Marshal.SizeOf(uref);
            SkalValue_k = BitConverter.ToDouble(InBuf, ptr); ptr += Marshal.SizeOf(SkalValue_k);
            Offset_d = BitConverter.ToDouble(InBuf, ptr); ptr += Marshal.SizeOf(Offset_d);
            return ptr;
        }

        public byte[] GetBytes()
        {
            List<byte> Buf =
            [
                .. BitConverter.GetBytes(ADResolution),
                .. BitConverter.GetBytes(Offset_hex),
                .. BitConverter.GetBytes(MidofRange),
                .. BitConverter.GetBytes(uref),
                .. BitConverter.GetBytes(SkalValue_k),
                .. BitConverter.GetBytes(Offset_d),
            ];
            return [.. Buf];
        }

        public static uint get_size_of()
        {
            CSWChannelInfo c = new();
            int ptr = 0;
            foreach (var propert in c.GetType().GetProperties())
            {
                ptr += Marshal.SizeOf(propert.PropertyType);
            }
            return (uint)ptr;
        }


        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            CSWChannelInfo ci = (CSWChannelInfo)MemberwiseClone();
            return ci;
        }

        #endregion

    }
}
