using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


namespace WindControlLib
{
    /// <summary>
    /// Summary description for Diverses.
    /// </summary>

    public class CMyTools
    {
        private static readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Creates a random String
        /// </summary>
        /// <param name="StringLength">Length of the string.</param>
        /// <returns></returns>
        public static string RandomString(int StringLength)
        {
            char[] buffer = new char[StringLength];

            for (int i = 0; i < StringLength; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        /// <summary>
        /// Determines whether the specified byte_to_ test is power_of_2.
        /// </summary>
        /// <param name="Byte_to_Test">byte_to_ test.</param>
        /// <returns>
        ///   <c>true</c> if the specified byte_to_ test is power_of_2; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPower_of_2(byte Byte_to_Test)
        {
            return (Byte_to_Test % 2 == 0); //Return true if num is totally divisible byt 2 with remainder 0
        }

        /// <summary>
        /// Determines whether to_ test is power_of_2
        /// </summary>
        /// <param name="To_Test">to_ test</param>
        /// <returns>
        ///   <c>true</c> if the specified to_ test is power_of_2; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPower_of_2(ulong To_Test)
        {
            return (To_Test != 0) && ((To_Test & (To_Test - 1)) == 0);
        }

        /// <summary>
        /// Gets the next higher number thats a power of 2
        /// </summary>
        /// <param name="val">Value to test</param>
        /// <returns>next higher number thats a power of 2</returns>
        public static int getNearestPowerofTwoVal(double val)
        {
            double Exp = Math.Log(val, 2);
            return (int)Math.Pow(2, Math.Ceiling(Exp));    //Always round to next higher Integer
            //return (int) Math.Pow(2, ((int)(Exp + 0.5)));
        }

        /// <summary>
        /// Swaps the byte array First -> Last ....
        /// </summary>
        /// <param name="buf">Array to swap</param>
        /// <returns>Swapped byte array</returns>
        public static byte[] SwapByteArray(byte[] buf)
        {
            byte[] ret = new byte[buf.Length];
            int j = buf.Length - 1;
            for (int i = 0; i < buf.Length; i++)
            {
                ret[j] = buf[i];
                j--;
            }
            return ret;
        }

        /// <summary>
        /// Calculate the average of all elements in a double array.
        /// </summary>
        /// <param name="dblArray">The double array to get the 
        /// average from.</param>
        /// <returns>The average of the double array</returns>
        public static double getAverageFromDoubleArray(double[] dblArray)
        {
            double dblResult = 0;
            foreach (double dblValue in dblArray)
                dblResult += dblValue;
            return dblResult / dblArray.Length;
        }

        /// <summary>
        /// Formats number according to SI prefixes
        /// </summary>
        /// <param name="number">value</param>
        /// <param name="unitSymbol">The unit symbol.</param>
        /// <param name="groupWeight">The group weight.</param>
        /// <param name="unitMultipliers">The unit multipliers.</param>
        /// <param name="unitDivider">The unit divider.</param>
        /// <returns>Formatted string</returns>
        public static string Format_with_SI_prefixes(double number, string unitSymbol, int groupWeight = 1000, string unitMultipliers = " kMGT", string unitDivider = " mµnp")
        {
            double numberWeigth = (int)Math.Floor(Math.Log(Math.Abs(number)) / Math.Log(groupWeight));
            char? unitWeigthSymbol;
            if (numberWeigth > 0)
            {
                numberWeigth = Math.Min(numberWeigth, unitMultipliers.Length - 1);
                unitWeigthSymbol = unitMultipliers[(int)numberWeigth];
            }
            else
            {
                numberWeigth = -Math.Min(Math.Abs(numberWeigth), unitDivider.Length - 1);
                unitWeigthSymbol = unitDivider[-(int)numberWeigth];
            }

            number /= Math.Pow(groupWeight, numberWeigth);
            //https://code.4noobz.net/c-si-prefixes-because-size-matters/
            string formatted = string.Format("{0:0.000} {1}{2}", number, unitWeigthSymbol, unitSymbol);
            return (formatted);
        }

        private static readonly string[] ms_seperator = { "," };

        /// <summary>Formats DateTime to "12:14:07,786"</summary>
        /// <param name="dt">DateTime</param>
        /// <returns>string with the format "12:14:07,786"</returns>
        public static string Format_DateTime_with_ms(DateTime dt)
        {
            return dt.ToLongTimeString() + ms_seperator[0] +
                dt.Millisecond.ToString("000");
        }

        /// <summary>Reconvert from "12:14:07,786"</summary>
        /// <param name="s">"12:14:07,786" formated string</param>
        /// <param name="Date">Date to be added with time</param>
        /// <returns>DateTime</returns>
        public static DateTime Get_DateTime_from_DateTime_with_ms(string s, string? Date = null)
        {
            DateTime t = DateTime.MinValue;
            string[] ds = s.Split(ms_seperator, StringSplitOptions.None);
            if (ds.Length == 2)
            {
                if (Date == null)
                {
                    t = Convert.ToDateTime(ds[0]);
                }
                else
                {
                    t = DateTime.Parse(Date + " " + ds[0]);
                }
                t = t.AddMilliseconds(Convert.ToInt32(ds[1]));
            }
            return t;
        }
    }

    public class CMyConvert
    {
        public static byte Low0(int val)
        {
            return Convert.ToByte(val & 0xFF);
        }

        public static byte High0(int val)
        {
            return Convert.ToByte((val >> 8) & 0xFF);
        }

        public static byte High1(int val)
        {
            return Convert.ToByte((val >> 16) & 0xFF);
        }

        public static byte High2(int val)
        {
            return Convert.ToByte((val >> 24) & 0xFF);
        }

        public static byte HighByte(int bt)
        {
            return Convert.ToByte((bt >> 8) & 0xFF);
        }
        public static byte HighByte(uint bt)
        {
            return Convert.ToByte((bt >> 8) & 0xFF);
        }
        public static byte LowByte(int bt)
        {
            return Convert.ToByte(bt & 0xFF);
        }
        public static byte LowByte(uint bt)
        {
            return Convert.ToByte(bt & 0xFF);
        }
        public static byte MakeBCDByte(byte val)
        {
            string s = val.ToString("00");
            return (byte)((Convert.ToByte(Convert.ToByte(s.Substring(0, 1)) << 4)) | Convert.ToByte(s.Substring(1, 1)));
        }
        public static int FromIntBytestoInt(byte Low, byte High)
        {
            int res = ((int)High << 8) + Low;
            if (res > 0x7fff) res = (0x10000 - res) * -1;
            return res;
        }
        public static void FromInttoIntBytes(int val, ref byte Low, ref byte High)
        {

            if (val < 0)
            {
                //2er komplement
                //val=val*-1;
                //val=(~(val*-1)+1);
            }
            High = CMyConvert.HighByte(val);
            Low = CMyConvert.LowByte(val);
        }

        public static int FromUIntBytestoInt(byte Low, byte High)
        {
            return ((int)High << 8) + Low;
        }

        public static uint FromUIntBytestoUInt(byte Low, byte High)
        {
            return ((uint)High << 8) + Low;
        }

        public static byte SwapNibbles(byte b)
        {
            return (byte)((byte)(b >> 4) | (byte)(b << 4));

        }


        ///<summary>
        /// Convert a byte array to a string.
        ///</summary>
        public static string ByteArraytoString(byte[] chars)
        {
            int idx = 0;
            //Find 0 that terminates string
            while ((idx < chars.Length) && (chars[idx] != 0))
            {
                idx++;
            }
            byte[] buf = new byte[idx];
            Buffer.BlockCopy(chars, 0, buf, 0, idx);
            char[] c = System.Text.Encoding.UTF8.GetChars(chars, 0, idx);
            return new string(c);
        }

        public static string ByteArrayto_HexString(byte[] chars, bool Stop_at_Zero_chr)
        {
            return ByteArrayto_HexString(chars, Stop_at_Zero_chr, "");
        }

        /// <summary>
        /// Convert a byte array to a hex string.
        /// </summary>
        /// <param name="chars">The chars.</param>
        /// <param name="Stop_at_Zero_chr">Only add chars before zero char</param>
        /// <param name="separator">String beetween bytes</param>
        /// <returns></returns>
        public static string ByteArrayto_HexString(byte[] chars, bool Stop_at_Zero_chr, string separator)
        {
            int idx = chars.Length;

            if (Stop_at_Zero_chr)   //Look if there is a Zero char
            {
                while ((idx < chars.Length) && (chars[idx] != 0))
                {
                    idx++;
                }
            }

            string s = string.Format("{0:X2}", chars[0]);
            for (int i = 1; i < idx; i++)
            {
                s += separator + string.Format("{0:X2}", chars[i]);
            }
            return s;
        }

        /// <summary>
        /// Convert a string to a byte array.
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="digits_to_group">number of digits to group</param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string str, int digits_to_group)
        {
            int i = 0;
            List<byte> btl = new List<byte>();
            while (i + digits_to_group <= str.Length)
            {
                btl.Add(Convert.ToByte((str.Substring(i, digits_to_group)), 16));
                i += digits_to_group;
            }
            return btl.ToArray();
        }

        public static byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        ///<summary>
        ///Convert a hex string to a byte array.
        ///</summary>
        public static byte[] HexStringTo_ByteArray(string str)
        {
            int numBytes = str.Length / 2;
            byte[] Buf = new byte[numBytes];
            for (int i = 0; i < numBytes; i++)
            {
                Buf[i] = Convert.ToByte((str.Substring(2 * i - 1, 2)), 16);
            }
            return Buf;
        }


        public static int HexStringToInt(string hexString)
        {
            return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        //Fornmat Provider mit wählbarem Dezimaltrennzeichen
        public static System.Globalization.NumberFormatInfo GetNumberFormatInfo(string DecimalSeperator)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = DecimalSeperator;
            return ni;
        }

        
    }

    /// <summary>
    /// Legt eine wachsende Liste mit Zahlen an und gibt bei bekannter Zahl den Index zurück
    /// </summary>
    /// <remarks>
    /// Verwendung z.B Zuordnung von Hardwarekanalnummern zu Tracks
    /// </remarks>
    public class CIndexReferencer
    {
        public List<int> idx1;
        //public List<int> idx2;

        public CIndexReferencer()
        {
            idx1 = new List<int>();
            //idx2 = new List<int>();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            idx1.Clear();
        }

        /// <summary>
        /// Add's a refernce.
        /// </summary>
        /// <param name="reference">reference</param>
        /// <returns></returns>
        public int Add_Refernce(int reference)
        {
            idx1.Add(reference);
            return idx1.Count - 1;
        }

        /// <summary>
        /// Gets indxe of the specified reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns>
        /// index, -1 wenn refernce nicht vorhanden
        /// </returns>
        public int Get_idx(int reference)
        {
            for (int i = 0; i < idx1.Count; i++)
            {
                if (idx1[i] == reference)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public class CDiverses
    {
        ///////////////////////////////////////////////////////////////
        //
        // Wait
        //
        ///////////////////////////////////////////////////////////////

        //The GetTickCount function retrieves the number of milliseconds that have elapsed since the system was started
#if COMPACT
        private const string RS232dll = "coredll.dll";		//fuer CE
#else
        private const string RS232dll = "kernel32.dll";		//fuer Windows
#endif
        [DllImport(RS232dll, SetLastError = true, EntryPoint = "GetTickCount")]
        static extern uint GetTickCount();

        public static void Wait(uint Waitms)
        {
            long tsoll, tist;
            tist = GetTickCount();	//This function retrieves the number of milliseconds that have elapsed since Windows was started
            tsoll = tist + Waitms;

            while (tist < tsoll)
            {
                tist = GetTickCount();
            }
        }
    }


#if !NET7_0_OR_GREATER
    public class CSave_Binary_Objects
    {
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
#else
    public class CSave_Binary_Objects
    {
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Pretty print the JSON
            };

            string jsonString = JsonSerializer.Serialize(objectToWrite, options);

            // Determine the file mode based on the append parameter
            FileMode mode = append ? FileMode.Append : FileMode.Create;

            using (FileStream stream = new FileStream(filePath, mode))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(jsonString);
            }
        }

        public static T ReadFromJsonFile<T>(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonString = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(jsonString);
            }
        }
    }

#endif


    /// <summary>
    /// Zur Verwaltung von PID und VID
    /// </summary>
    public class CVID_PID
    {
        public CVID_PID()
        { }
        public CVID_PID(string vID, string pID)
        {
            VID = vID;
            PID = pID;
        }

        public string VID { get; set; }
        public string PID { get; set; }

        //"VID_0403&PID_6010"
        public virtual string VID_PID
        {
            get { return ("VID_" + VID + "&PID_" + PID); }
            set
            {
                string[] st = value.Split(new string[] { "&", "_" }, StringSplitOptions.RemoveEmptyEntries);
                VID = ""; PID = "";
                if ((st != null) && (st.Length >= 4))
                {
                    if (st[0] == "VID")
                        VID = st[1];
                    if (st[2] == "PID")
                        PID = st[3];
                }
            }
        }
    }
}



