/* 8.10.2014
 * Problem bei der Umskalierung: (siehe "BPM statt IPI.docx")
 * Um eine korrekte Weiterverarbeitung der Daten (insbesondere HRV) wird das erkannte IPI nur einmal zurück gegeben, wenn der Herzschlag erkannt wurde. Dazwischen wird 0 zurückgegeben.
 * Bei der Umrechnung auf BPM würde das bedeuten, es ein „unendlich“ … der max Wert zurück.
 * 
 * 0 könnte in der Routine GetScaledValue ausgenommen werden, dann liefert aber auch GetMinScaledValue einen falschen Wert.
*/

namespace FeedbackDataLib
{

    /// <summary>
    /// Class to help interchanging data between Qinno mdb database and importable values for calibration
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CSerialModule_MDB
    {
        public string SerialNumber { get; set; } = "";
        public enumModuleType Type { get; set; } = enumModuleType.cModuleTypeEmpty;
        public float Ampl1 { get; set; } = 0;
        public float Ampl1_soll { get; set; }
        public float Ampl2 { get; set; } = 0;
        public float Ampl2_soll { get; set; }
        public float Ampl3 { get; set; } = 0;
        public float Ampl3_soll { get; set; }
        public float Ampl4 { get; set; } = 0;
        public float Ampl4_soll { get; set; }
        public float DCOffset { get; set; } = 0;
        public float SCLLow { get; set; } = 0;
        public float SCLLow_soll { get; set; }
        public float SCLHigh { get; set; } = 0;
        public float SCLHigh_soll { get; set; }
        public float EEGTheat { get; set; } = 0;
        public float EEGTheat_soll { get; set; }
        public float EEGAlpha { get; set; } = 0;
        public float EEGAlpha_soll { get; set; }
        public float EEGBeta { get; set; } = 0;
        public float EEGBeta_soll { get; set; }
    }

    /// <summary>
    /// DateTime that Implements time.h (from GNUC)
    /// </summary>
    public class CCDateTime
    {
        #region time.h
        /* FROM: <TIME.H> DATE AND TIME FUNCTIONS*/
        private short tm_sec;/*seconds after the minute ( 0 to 61 )*/  /*allows for up to two leap seconds*/
        private short tm_min;/*minutes after the hour ( 0 to 59 )*/
        private short tm_hour;/*hours since midnight ( 0 to 23 )*/
        private short tm_mday;/*day of month ( 1 to 31 )*/
        private short tm_mon;/*month ( 0 to 11 where January = 0 )*/
        private short tm_year;/*years since 1900*/
        private short tm_wday;/*day of week ( 0 to 6 where Sunday = 0 )*/
        private short tm_yday;/*day of year ( 0 to 365 where January 1= 0 )*/
        private short tm_isdst;/*Daylight Savings Time flag*/
        /* If tm_isdst is a positive value, Daylight Savings is in effect. If it is 
         * zero, Daylight Saving time is not in effect. If it is a negative value, the
         * status of Daylight Saving Time is not known.*/
        #endregion

        /// <summary>
        /// Gets or sets the dt.
        /// </summary>
        public DateTime Dt
        {
            get { return _dt; }
            set { UpdateFromDateTime(value); }
        }
        private DateTime _dt;

        /// <summary>
        /// Fill properies according to corresponding structure in Device
        /// </summary>
        public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            tm_sec = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_sec);
            tm_min = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_min);
            tm_hour = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_hour);
            tm_mday = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_mday);
            tm_mon = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_mon);
            tm_year = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_year);
            tm_wday = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_wday);
            tm_yday = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_yday);
            tm_isdst = BitConverter.ToInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(tm_isdst);

            UpdateFrom_time_h();
            return ptr;
        }

        private void UpdateFrom_time_h()
        {
            try
            {
                _dt = new DateTime(tm_year + 1900, tm_mon + 1, tm_mday, tm_hour, tm_min, tm_sec);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                _dt = new DateTime(1800, 1, 1, 0, 0, 0, 0);
            }
            finally
            {
            }
        }

        private void UpdateFromDateTime(DateTime dt)
        {
            tm_sec = (short)dt.Second;
            tm_min = (short)dt.Minute;
            tm_hour = (short)dt.Hour;
            tm_mday = (short)dt.Day;
            tm_mon = (short)(dt.Month - 1);
            tm_year = (short)(dt.Year - 1900);
            tm_wday = (short)dt.DayOfWeek;
            tm_yday = (short)dt.DayOfYear;
            tm_isdst = 0;
            _dt = dt;
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

            byte[] b = BitConverter.GetBytes(tm_sec); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_min); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_hour); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_mday); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_mon); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_year); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_wday); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_yday); buf.AddRange(b);
            b = BitConverter.GetBytes(tm_isdst); buf.AddRange(b);
            buffer = [.. buf];
        }
    }

    /// <summary>
    /// Holds Firmware Version of NeuroMaster 
    /// </summary>
    public class CNMFirmwareVersion
    {

        private readonly byte[] _uuid = new byte[16];
        /// <summary>
        /// UUID set in Neuromaster during programing the boot loader
        /// ASCII chars
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Uuid => System.Text.Encoding.ASCII.GetString(_uuid);

        /// <summary>
        /// Hardware Version of Neuromaster
        /// </summary>
        /// <value>
        /// The hw version.
        /// </value>
        public ushort HWVersion { get; private set; }

        /// <summary>
        /// Hardware Version string 
        /// </summary>
        /// <remarks>
        /// siehe MM config.h
        /// Higher 4 Byte: Major revision
        /// Lower 12 Byte: Minor revision
        /// </remarks>
        public string HWVersionString
        {
            get
            {
                int maj = (HWVersion & 0xFFF0) >> 4;
                int min = HWVersion & 0x000F;

                return $"{maj}.{min}";
            }
        }

        /// <summary>
        /// Software Version of Neuromaster
        /// </summary>
        /// <value>
        /// The sw version.
        /// </value>
        public ushort SWVersion { get; private set; } = 0;

        /// <summary>
        /// Software Version string 
        /// </summary>
        /// <remarks>
        /// siehe MM config.h
        /// Higher 4 Byte: Major revision
        /// Lower 12 Byte: Minor revision
        /// </remarks>
        public string SWVersionString
        {
            get
            {
                int maj = (SWVersion & 0xF000) >> 12;
                int min = SWVersion & 0x0FFF;

                return $"{maj}.{min}";
            }
        }

        public string GetFWVersionString()
        {
            return $"Neuromaster: {Uuid} connected{Environment.NewLine}" +
                   $"Neuromaster HW-Version: {HWVersionString}{Environment.NewLine}" +
                   $"Neuromaster SW-Version: {SWVersionString}";
        }


        /// <summary>
        /// Fill properies according to corresponding structure in Device
        /// </summary>
        public int UpdateFromByteArray(byte[] InBuf, int Pointer_To_Array_Start)
        {
            int ptr = Pointer_To_Array_Start; //Array Pointer
            Array.Copy(InBuf, Pointer_To_Array_Start, _uuid, 0, _uuid.Length);
            ptr += _uuid.Length;

            HWVersion = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(HWVersion);
            SWVersion = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(SWVersion);
            return ptr;
        }
    }

    /// <summary>
    /// Holds information about SD card in Neuromaster
    /// </summary>
    //public class CSDCardInfo
    //{
    //    private int _sector_count;  //32bit long
    //    private int _free_sector_count;
    //    private ushort _sector_size; //unsigned short 
    //    private byte _type;          //unsigned char

    //    private byte _error;     //keep it for future use

    //    /// <summary>
    //    /// SD Card size [bytes]
    //    /// </summary>
    //    public ulong SDCardSize_bytes
    //    {
    //        get
    //        {
    //            return _sector_size * ((ulong)_sector_count);
    //        }
    //    }

    //    /// <summary>
    //    /// SD Card free space [bytes]
    //    /// </summary>
    //    public ulong SDFree_bytes
    //    {
    //        get
    //        {
    //            return _sector_size * ((ulong)_free_sector_count);
    //        }
    //    }

    //    /// <summary>
    //    /// Possible SD Card types according to used PIC library
    //    /// </summary>
    //    public enum EnSDCardType
    //    {
    //        CT_MMC = 0x01,                      /* MMC ver 3 */
    //        CT_SD1 = 0x02,                      /* SD ver 1 */
    //        CT_SD2 = 0x04,                      /* SD ver 2 */
    //        CT_SDC = CT_SD1 | CT_SD2,          /* SD */
    //        CT_BLOCK = 0x08                    /* Block addressing */
    //    }

    //    /// <summary>
    //    /// SD Card types according to used PIC library
    //    /// </summary>
    //    public EnSDCardType SDCardType
    //    {
    //        get { return (EnSDCardType)_type; }
    //    }

    //    /// <summary>
    //    /// Fill properies according to corresponding structure in Device
    //    /// </summary>
    //    public int UpdateFrom_ByteArray(byte[] InBuf, int Pointer_To_Array_Start)
    //    {
    //        int ptr = Pointer_To_Array_Start; //Array Pointer
    //        _sector_count = BitConverter.ToInt32(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(_sector_count);
    //        _free_sector_count = BitConverter.ToInt32(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(_free_sector_count);
    //        _sector_size = BitConverter.ToUInt16(InBuf, ptr); ptr += System.Runtime.InteropServices.Marshal.SizeOf(_sector_size);
    //        _type = InBuf[ptr]; ptr++;
    //        _error = InBuf[ptr]; ptr++;
    //        return ptr;
    //    }
    //}
}
