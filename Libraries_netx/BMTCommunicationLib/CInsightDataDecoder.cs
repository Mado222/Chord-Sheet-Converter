using WindControlLib;


namespace BMTCommunicationLib
{
    public class CInsightDataEnDecoder
    {
        /********************************************************************/
        /************************** ENCODING ********************************/
        /********************************************************************/

        public static int Convert24BitToInt(byte bt2, byte bt1, byte bt0)
        {
            uint result = (uint)((bt2 << 16) | (bt1 << 8) | bt0);
            return Convert24BitToInt(result);
        }

        public static int Convert24BitToInt(uint val)
        {
            // If the sign bit (bit 23) is set, we need to extend the sign to make it a proper 32-bit signed integer
            if ((val & 0x800000) != 0) // Check if the 24th bit is set (sign bit)
            {
                val |= 0xFF000000; // Sign-extend to 32 bits
            }
            return (int)val;
        }

        public static byte[] EncodeHeader(ushort value, byte HW_cn, byte SW_cn)
        {
            byte[] buffer = new byte[4];
            byte SendBH = CMyConvert.HighByte(value);
            byte SendBL = CMyConvert.LowByte(value);

            byte DeviceID = 0;

            buffer[0] = (byte)(0x3F & (((DeviceID << 3) & 0x18) | ((HW_cn >> 1) & 0x07)));
            buffer[1] = (byte)(0x80 | ((HW_cn << 6) & 0x40) | ((SW_cn << 2) & 0x3C) | (SendBH >> 6));
            buffer[2] = (byte)(0x80 | ((SendBH << 1) & 0x7E) | (SendBL >> 7));
            buffer[3] = (byte)(0x80 | (SendBL & 0x7F));
            return buffer;
        }

        public static byte[] EncodeData(byte command, byte[] payload)
        {
            CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);
            byte[] buffer = new byte[payload.Length + 2];
            buffer[0] = command;
            Buffer.BlockCopy(payload, 0, buffer, 1, payload.Length);
            buffer[^1] = CRC8.Calc_CRC8(buffer, buffer.Length - 2);
            return buffer;
        }

        /********************************************************************/
        /************************** DECODING ********************************/
        /********************************************************************/

        public static byte[] EncodeTo7Bit(byte[] inByte)
        {
            int length = inByte.Length;
            int outLength = (length * 8 + 6) / 7; // Calculate the required size for the output array
            byte[] outByte = new byte[outLength];

            int outIndex = 0;
            int bitBuffer = 0;
            int bitCount = 0;

            for (int i = 0; i < length; i++)
            {
                bitBuffer |= inByte[i] << bitCount;
                bitCount += 8;

                while (bitCount >= 7)
                {
                    outByte[outIndex++] = (byte)((bitBuffer & 0x7F) | 0x80);
                    bitBuffer >>= 7;
                    bitCount -= 7;
                }
            }

            if (bitCount > 0)
            {
                outByte[outIndex++] = (byte)((bitBuffer & 0x7F) | 0x80);
            }

            return outByte;
        }

        public static byte[] DecodeFrom7Bit(byte[] inByte)
        {
            int length = inByte.Length;
            int outLength = length * 7 / 8; // Calculate the required size for the output array
            byte[] outByte = new byte[outLength];

            int outIndex = 0;
            int bitBuffer = 0;
            int bitCount = 0;

            for (int i = 0; i < length; i++)
            {
                bitBuffer |= (inByte[i] & 0x7F) << bitCount;
                bitCount += 7;

                while (bitCount >= 8)
                {
                    outByte[outIndex++] = (byte)(bitBuffer & 0xFF);
                    bitBuffer >>= 8;
                    bitCount -= 8;
                }
            }

            return outByte;
        }

        public static bool Parse4Byte(CFifoBuffer<byte> inBuf, ref CDataIn dataIn)
        {
            if (inBuf == null || inBuf.Count < 4)
            {
                return false; // Not enough data to parse
            }
            return Parse4Byte([inBuf.Peek(0), inBuf.Peek(1), inBuf.Peek(2), inBuf.Peek(3)], ref dataIn);
        }


        public static bool Parse4Byte_funct(byte[] src, ref CDataIn DI)
        {
            // Check if the MSB of the first 4 bytes are 1 0 0 0
            if (!((src[0] & 0x80) == 0x00 && (src[1] & 0x80) != 0x00 && (src[2] & 0x80) != 0x00 && (src[3] & 0x80) != 0x00))
            {
                return false;
            }

            uint temp, temp_val = 0;
            // Decode Byte 0
            DI.SyncFlag = (byte)((src[0] >> 6) & 0x01);
            DI.DeviceID = (byte)((src[0] >> 3) & 0x03);
            DI.HW_cn = (byte)(((src[0] & 0x07) << 1) | ((src[1] >> 6) & 0x01));

            // Decode Byte 1
            DI.SW_cn = (byte)((src[1] >> 2) & 0x07);
            DI.EP = (byte)((src[1] >> 5) & 0x1);
            temp = (uint)(src[1] & 0x03);
            temp <<= 14;
            temp_val |= temp;

            // Decode Byte 2
            temp = (uint)(src[2] & 0x7F);
            temp <<= 7;
            temp_val |= temp;

            // Decode Byte 3
            temp_val |= (uint)(src[3] & 0x7F);

            DI.Value = (int)temp_val;

            return true;
        }

        public static bool Parse4Byte(byte[] src, ref CDataIn DI)
        {
            // Validate input length to avoid potential index out of range exceptions.
            if (src.Length < 4) return false;

            // Check if MSB of the first 4 bytes are 1 0 0 0 (using a combined mask)
            if ((src[0] & 0x80) != 0x00 || (src[1] & 0x80) == 0x00 || (src[2] & 0x80) == 0x00 || (src[3] & 0x80) == 0x00)
            {
                return false;
            }

            // Decode each byte and directly assign values to CDataIn fields
            DI.SyncFlag = (byte)((src[0] >> 6) & 0x01);
            DI.DeviceID = (byte)((src[0] >> 3) & 0x03);
            DI.HW_cn = (byte)(((src[0] & 0x07) << 1) | ((src[1] >> 6) & 0x01));
            DI.SW_cn = (byte)((src[1] >> 2) & 0x07);
            DI.EP = (byte)((src[1] >> 5) & 0x1);

            // Combine temp_val calculation into a single expression for improved readability
            DI.Value = ((src[1] & 0x03) << 14) | ((src[2] & 0x7F) << 7) | (src[3] & 0x7F);

            return true;
        }


        public static void DecodePacket_funct(byte[] src, ref CDataIn dataIn)
        {
            int temp;

            Parse4Byte(src, ref dataIn);

            int idx = 4;
            if (dataIn.SyncFlag == 1)
            {
                dataIn.SyncVal = (byte)(src[idx] & 0x7F);
                dataIn.SyncVal |= (byte)((src[0] & 0x20) << 2);
                if (dataIn.EP == 0) return;
                idx++;
            }
            else if (dataIn.EP == 0)
                return;

            // Decode Byte 4 / 5
            dataIn.NumExtraDat = (byte)((src[idx] >> 4) & 0x07);
            dataIn.TypeExtraDat = (byte)((src[idx] >> 1) & 0x07);
            temp = src[idx] & 0x01;
            temp <<= 23;
            dataIn.Value |= temp;
            idx++;

            // Decode Byte 5 / 6
            temp = src[idx] & 0x7F;
            temp <<= 16;
            dataIn.Value |= temp;
            // Check if the sign bit (bit 23) is set
            if ((dataIn.Value & 0x800000) != 0)
            {
                // Extend the sign to 32 bits
                dataIn.Value |= unchecked((int)0xFF000000);
            }

            idx++;

            // Decode Extra Data
            if (dataIn.NumExtraDat > 0)
            {
                dataIn.ExtraDat = new byte[dataIn.NumExtraDat];
                for (int i = 0; i < dataIn.NumExtraDat; ++i)
                {
                    dataIn.ExtraDat[i] = (byte)(src[idx + i] & 0x7F); // Clear the first bit
                }
            }
        }

        public static void DecodePacket(byte[] src, ref CDataIn dataIn)
        {
            // Ensure input is parsed first
            Parse4Byte(src, ref dataIn);

            int idx = 4;

            // Handle SyncFlag and early exit for EP == 0
            if (dataIn.SyncFlag == 1)
            {
                dataIn.SyncVal = (byte)((src[idx] & 0x7F) | ((src[0] & 0x20) << 2));
                if (dataIn.EP == 0) return;
                idx++;
            }
            else if (dataIn.EP == 0)
            {
                return;
            }

            // Decode Byte 4 / 5
            dataIn.NumExtraDat = (byte)((src[idx] >> 4) & 0x07);
            dataIn.TypeExtraDat = (byte)((src[idx] >> 1) & 0x07);
            dataIn.Value |= (src[idx] & 0x01) << 23;
            idx++;

            // Decode Byte 5 / 6
            dataIn.Value |= (src[idx] & 0x7F) << 16;

            // Sign extend if bit 23 is set
            if ((dataIn.Value & 0x800000) != 0)
            {
                dataIn.Value |= unchecked((int)0xFF000000);
            }
            idx++;

            // Decode Extra Data only if NumExtraDat > 0
            if (dataIn.NumExtraDat > 0)
            {
                dataIn.ExtraDat = new byte[dataIn.NumExtraDat];
                for (int i = 0; i < dataIn.NumExtraDat; ++i)
                {
                    dataIn.ExtraDat[i] = (byte)(src[idx + i] & 0x7F); // Clear the MSB
                }
            }
        }

    }
}



