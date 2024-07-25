using System;
using System.Collections.Generic;

namespace FeedbackDataLib
{
    /// <summary>
    /// Configuration for each SW Channel - match "struct sSWConfigChannel" in PIC
    /// </summary>
    /// <remarks></remarks>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CSWConfigChannel : ICloneable
    {
        /// <summary>
        /// Sample Interval
        /// </summary>
        /// <value>Interval [ms]</value>
        /// <remarks></remarks>
        public ushort SampleInt
        {
            get { return _SampleInt; }
            set { _SampleInt = value; }
        }
        private ushort _SampleInt;


        /// <summary>
        /// Flag: Channel data is sent
        /// </summary>
        /// <value><c>true</c> if [Channel should be sent]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Flag is extracted from _ConfigByte
        /// </remarks>
        public bool SendChannel
        {
            get
            {
                if ((_ConfigByte & 0x0001) != 0) return true;
                return false;
            }
            set
            {
                _ConfigByte &= 0xFFFE;
                if (value) _ConfigByte = (ushort)(_ConfigByte | 0x0001);
            }
        }

        /// <summary>
        /// Flag: Channel data is sved on SD card
        /// </summary>
        public bool SaveChannel
        {
            get
            {
                if ((_ConfigByte & 0x0100) != 0) return true;
                return false;
            }
            set
            {
                _ConfigByte &= 0xFEFF;
                if (value) _ConfigByte = (ushort)(_ConfigByte | 0x0100);
            }
        }
        protected ushort _ConfigByte;

        /// <summary>
        /// Fills properties from received byte array
        /// </summary>
        public virtual int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            SampleInt = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(SampleInt);
            _ConfigByte = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(_ConfigByte);
            return ptr;
        }

        /// <summary>
        /// Array as it can be sent to Device
        /// </summary>
        public void GetByteArray(ref byte[] buffer, int Index_where_to_start_filling)
        {
            List<byte> buf = [];
            if (Index_where_to_start_filling > 0)
            {
                //Reserve place
                for (int i = 0; i < Index_where_to_start_filling; i++)
                    buf.Add(buffer[i]);
            }

            byte[] b = BitConverter.GetBytes(SampleInt);
            buf.AddRange(b);
            b = BitConverter.GetBytes(_ConfigByte);
            buf.AddRange(b);
            buffer = [.. buf];
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
            CSWConfigChannel cc = (CSWConfigChannel)base.MemberwiseClone();
            return cc;
        }

        #endregion
    }
}
