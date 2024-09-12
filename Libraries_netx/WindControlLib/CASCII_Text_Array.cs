using System.Text;

namespace WindControlLib
{
    public class CASCII_Text_Array
    {
        // Method to convert byte array to string
        public static string ByteArrayToString(byte[] byteArray)
        {
            ArgumentNullException.ThrowIfNull(byteArray);

            // Converts byte array to string using UTF-8 encoding
            return Encoding.UTF8.GetString(byteArray);
        }

        // Method to convert string to byte array
        public static byte[] StringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            // Converts string to byte array using UTF-8 encoding
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ByteArray_To_Ascii(byte[] byteArray)
        {
            StringBuilder result = new();

            foreach (byte b in byteArray)
            {
                // Handle printable ASCII characters
                if (b >= 32 && b <= 126) // Printable characters range
                {
                    result.Append((char)b);
                }
                // Handle non-printable ASCII characters
                else
                {
                    result.Append(GetNonPrintableCharRepresentation(b));
                }
            }

            return result.ToString();
        }

        private static string GetNonPrintableCharRepresentation(byte b)
        {
            return b switch
            {
                0 => "[NUL]",
                1 => "[SOH]",
                2 => "[STX]",
                3 => "[ETX]",
                4 => "[EOT]",
                5 => "[ENQ]",
                6 => "[ACK]",
                7 => "[BEL]",
                8 => "[BS]",
                9 => "[TAB]",
                10 => "[LF]",
                11 => "[VT]",
                12 => "[FF]",
                13 => "[CR]",
                14 => "[SO]",
                15 => "[SI]",
                16 => "[DLE]",
                17 => "[DC1]",
                18 => "[DC2]",
                19 => "[DC3]",
                20 => "[DC4]",
                21 => "[NAK]",
                22 => "[SYN]",
                23 => "[ETB]",
                24 => "[CAN]",
                25 => "[EM]",
                26 => "[SUB]",
                27 => "[ESC]",
                28 => "[FS]",
                29 => "[GS]",
                30 => "[RS]",
                31 => "[US]",
                _ => "[UNK]",
            };
        }

    }
}
