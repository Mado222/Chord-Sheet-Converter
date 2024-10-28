using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;
using System.Linq;

//http://www.keil.com/support/docs/1584.htm
//https://en.wikipedia.org/wiki/Intel_HEX

/*
An Intel HEX file is composed of any number of HEX records. Each record is made up of five fields that are arranged in the following format:

:llaaaatt[dd...]cc

Each group of letters corresponds to a different field, and each letter represents a single hexadecimal digit. Each field is composed of at least two hexadecimal digits-which make up a byte-as described below:

    * : is the colon that starts every Intel HEX record.
    * ll is the record-length field that represents the number of data bytes (dd) in the record.
    * aaaa is the address field that represents the starting address for subsequent data in the record.
    * tt is the field that represents the HEX record type, which may be one of the following:
      00 - data record
      01 - end-of-file record
      02 - extended segment address record
      04 - extended linear address record
    * dd is a data field that represents one byte of data. A record may have multiple data bytes. The number of data bytes in the record must match the number specified by the ll field.
    * cc is the checksum field that represents the checksum of the record. The checksum is calculated by summing the values of all hexadecimal digit pairs in the record modulo 256 and taking the two's complement.
*/

namespace WindControlLib
{
    /// <summary>
    /// Class to handle Intel Hex Files
    /// </summary>
    public class CIntelHex
    {
        public List<CHexfileEntry> HexFile;
        public List<string> HexFileTextLines;

        public virtual int MemoryMirror_size { get { return 65563; } }

       /// <summary>
        /// Internal Mirror of the Memory
        /// </summary>
        private CHexMemory HexMemoryMirror;     

        /// <summary>
        /// Reflects one line in the hex-text file
        /// </summary>
        public class CHexfileEntry
        {
            public uint MemoryLocation;
            public byte[]? Values;
            public int LineNo_in_HexFileLines = 0;
        }

        /// <summary>
        /// Holds an Array that reflects the Memory that the Hex File describes
        /// </summary>
        public class CHexMemory
        {
            public byte []? value = null;
            public bool []? accessed = null;

            public CHexMemory(int size)
            {
                value = new byte[size];
                int i = 0;
                while (i<size)
                {
                    value[i] = 0xff;
                    value[i+1] = 0xff;      //was 0x3f
                    i += 2;
                }
                accessed = new bool[size];
            }
        }
        public CIntelHex()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Read the hex text fil2
        /// </summary>
        /// <param name="HEXFileName">Name of the hexadecimal file.</param>
        /// <returns></returns>
        public bool OpenHexFile(string HEXFileName, uint SkipFromAddress = uint.MaxValue, uint SkipToAddress = uint.MinValue)
        {
            string InFileLine;
            int RecordLength;
            int RecordType;
            uint LowerAddress;
            uint HigherAddress = 0;
            bool br;
            byte cc = 0;
            //int l;
            //byte [] Buf= new byte[255];
            //int IndexCount;
            bool EOFRecord;
            int LineNo_in_HexFileLines = -1;

            HexFile = new List<CHexfileEntry>();
            HexFileTextLines = new List<string>();

            System.IO.StreamReader InFile = new System.IO.StreamReader(HEXFileName);

            br = false;
            //IndexCount = 0;
            EOFRecord = false;

            //Validate the file before using it
            while (((InFileLine = InFile.ReadLine()) != null) && !br && !EOFRecord)
            {

                HexFileTextLines.Add(InFileLine);
                LineNo_in_HexFileLines++;
                string chr = ":";
                if (InFileLine[0] != chr[0])
                {
                    br = true;
                }
                else
                {
                    //First charaecter =':'
                    RecordLength = Convert.ToInt32(InFileLine.Substring(1, 2), 16);
                    cc = (byte)RecordLength;
                    LowerAddress = Convert.ToUInt32(InFileLine.Substring(3, 4), 16);
                    cc = (byte)(cc + CMyConvert.HighByte((int)LowerAddress));
                    cc = (byte)(cc + CMyConvert.LowByte((int)LowerAddress));
                    RecordType = Convert.ToInt32(InFileLine.Substring(7, 2), 16);
                    cc = (byte)(cc + RecordType);
                    //l = InFileLine.Length;
                    switch (RecordType)
                    {
                        case 0:          // data record
                            {
                                CHexfileEntry HexfileEntry = new CHexfileEntry
                                {
                                    Values = new byte[RecordLength],
                                    LineNo_in_HexFileLines = LineNo_in_HexFileLines
                                };
                                if (ReadBytes(RecordLength, ref InFileLine, ref cc, ref HexfileEntry.Values))
                                {
                                    HexfileEntry.MemoryLocation = (HigherAddress << 16) + LowerAddress;
                                    if ((HexfileEntry.MemoryLocation < SkipFromAddress) || (HexfileEntry.MemoryLocation >= SkipToAddress) )
                                    {
                                        HexFile.Add(HexfileEntry);
                                        //IndexCount = IndexCount + 1;
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    br = true;
                                }
                                break;

                            }
                        case 1:          //end-of-file record
                            {
                                EOFRecord = true;
                                break;
                            }
                        case 2:          //extended segment address record
                            {
                                br = true;    //Unhandled record
                                break;
                            }
                        case 4:          //extended linear address record
                            {
                                byte[] buf = new byte[20];
                                if (ReadBytes(2, ref InFileLine, ref cc, ref buf))
                                {
                                    HigherAddress = (uint) (buf[0] * 256 + buf[1]);
                                }
                                else
                                {
                                    br = true;
                                }
                                break;
                            }
                    }
                }      //} case
            }        //} while

            bool res = false;
            InFile.Close();
            if ((!br) && (EOFRecord))
            {
                res = true;
            }
            return res;
        }

        /// <summary>
        /// Decode one line of text and check Checksum
        /// </summary>
        /// <param name="numBytes">Number of data bytes.</param>
        /// <param name="InFileLine">Line to decode</param>
        /// <param name="checksum">The checksum.</param>
        /// <param name="Buf">Data Buffer</param>
        /// <returns>Checksum OK</returns>
        private bool ReadBytes(int numBytes, ref string InFileLine, ref byte checksum, ref byte[] Buf)
        {
            int i;
            bool res;

            for (i = 0; i < numBytes; i++)
            {
                Buf[i] = Convert.ToByte((InFileLine.Substring(10 + 2 * i - 1, 2)), 16);
                checksum = (byte)(checksum + Buf[i]);
            }
            checksum = (byte)((checksum ^ 0xff) + 1);
            //Checksumme überprüfen
            res = false;
            if (Convert.ToInt32(InFileLine.Substring(InFileLine.Length - 2, 2), 16) == checksum)
            {
                res = true;
            }
            return res;
        }

        /// <summary>
        /// Replaces a memory area with Data, Updates the Checksum
        /// </summary>
        /// <param name="StartAddress">The start address.</param>
        /// <param name="Data">The data.</param>
        /// <returns></returns>
        public bool PIC16_Replace_Memory_Area(uint StartAddress, byte[] Data)
        {
            bool ret = false;
            //StartAddress = StartAddress * 2;    //PIC interner Speicher -> removed 12.1.2016
            if ((HexFile != null) && (HexFile.Count > 2))
            {
                //Search Address
                for (int i = 0; i < HexFile.Count - 2; i++)
                {
                    if ((HexFile[i].MemoryLocation <= StartAddress) && (HexFile[i + 1].MemoryLocation > StartAddress))
                    {
                        int cnt = 0; //Data.Length;
                        List<int> Edited_HexFile_Entries = new List<int>();

                        //Record gefunden, jetzt ersetzen wir die Bytes
                        while ((cnt < Data.Length) && (i < HexFile.Count))
                        {

                            uint LineStart = HexFile[i].MemoryLocation;
                            uint LineOffset = (uint) (StartAddress - LineStart);
                            Edited_HexFile_Entries.Add(i);

                            while ((LineOffset < HexFile[i].Values.Length) && (cnt < Data.Length))
                            {
                                HexFile[i].Values[LineOffset] = Data[cnt];
                                LineOffset += 2;
                                StartAddress += 2;
                                cnt++;
                            }
                            i++;
                        }
                        if ((i < HexFile.Count) && (Edited_HexFile_Entries.Count > 0))
                        {
                            //Update Edited Lines
                            for (int j = 0; j < Edited_HexFile_Entries.Count; j++)
                            {
                                int HexFileLine_No = HexFile[Edited_HexFile_Entries[j]].LineNo_in_HexFileLines;
                                //string newLine = HexFileTextLines[HexFileLine_No];
                                string newLine = HexFileTextLines[HexFileLine_No].Substring(0, 9);     //9 Bytes of beginning of the Line
                                newLine += CMyConvert.ByteArrayto_HexString(HexFile[Edited_HexFile_Entries[j]].Values, false);
                                //Add Checksum
                                string temp = newLine;
                                temp = temp.Remove(0, 1);     //: weg
                                byte[] bt = CMyConvert.StringToByteArray(temp, 2);
                                byte cs = 0;
                                for (int k = 0; k < bt.Length; k++)
                                {
                                    cs += bt[k];
                                }
                                cs = (byte)((cs ^ 0xff) + 1);
                                newLine += cs.ToString("X2");
                                HexFileTextLines[HexFileLine_No] = newLine;
                            }
                        }
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Reads a specific Memory area
        /// </summary>
        /// <param name="StartAddress">The start address.</param>
        /// <param name="numBytestoRead">The number bytesto read.</param>
        /// <param name="Data">The data.</param>
        /// <returns></returns>
        /*
        public byte[] PIC16_Get_Memory_Area(uint StartAddress, int numBytestoRead)
        {
            List<byte> Data = new List<byte>();
            //StartAddress = StartAddress * 2;    //PIC interner Speicher -> removed 12.1.2016
            if ((HexFile != null) && (HexFile.Count > 2))
            {
                Data.Clear();
                //Search Address
                for (int i = 0; i < HexFile.Count - 2; i++)
                {
                    if ((HexFile[i].MemoryLocation <= StartAddress) && (HexFile[i + 1].MemoryLocation > StartAddress))
                    {
                        int cnt = 0; //Data.Length;

                        //Record gefunden, jetzt ersetzen wir die Bytes
                        while ((cnt < numBytestoRead) && (i < HexFile.Count))
                        {

                            uint LineStart = HexFile[i].MemoryLocation;
                            uint LineOffset = StartAddress - LineStart;

                            while ((LineOffset < HexFile[i].Values.Length) && (cnt < numBytestoRead))
                            {
                                Data.Add(HexFile[i].Values[LineOffset]);
                                LineOffset += 2;
                                StartAddress += 2;
                                cnt++;
                            }
                            i++;
                        }
                        break;
                    }
                }
            }
            return Data.ToArray();
        }*/

        public virtual byte[] Get_Memory_Area(uint StartAddress, uint numBytestoRead)
        {
            //CHexMemory hmem = new CHexMemory(MemoryMirror_size);
            if (this.HexMemoryMirror == null)
            {
                this.HexMemoryMirror = new CHexMemory(MemoryMirror_size);
                this.Make_MemoryMirror(ref this.HexMemoryMirror);
            }
            byte[] Data = new byte[numBytestoRead];
            Array.Copy(HexMemoryMirror.value, StartAddress, Data, 0, numBytestoRead);
            return Data;
        }

        public virtual byte Get_Memory_Value(uint Address)
        {
            return HexMemoryMirror.value[Address];
        }


        /// <summary>
        /// Writes the Modified hex File - when Replace_Memory_Area was used
        /// </summary>
        /// <param name="HEXFileName">Name of the hexadecimal file.</param>
        public void WriteHexFile(string HEXFileName)
        {
            System.IO.StreamWriter OutFile = new System.IO.StreamWriter(HEXFileName, false);
            for (int i = 0; i < HexFileTextLines.Count; i++)
            {
                OutFile.WriteLine(HexFileTextLines[i]);
            }
            OutFile.Close();
        }


        /// <summary>
        /// Puts the read hex file to a memory mirror
        /// </summary>
        public void Make_MemoryMirror()
        {
            HexMemoryMirror = new CHexMemory(MemoryMirror_size);
            Make_MemoryMirror(ref HexMemoryMirror);
        }
        
        /// <summary>
        /// Makes the memory mirror.
        /// </summary>
        public void Make_MemoryMirror(ref CHexMemory HexMem)
        {
            foreach (CHexfileEntry he in this.HexFile)
            {
                for (int i = 0; i < he.Values.Length; i++)
                {
                    HexMem.value[he.MemoryLocation + i] = he.Values[i];
                    HexMem.accessed[he.MemoryLocation + i] = true;
                }
            }
        }

        /// <summary>
        /// Adds to memory mirror.
        /// </summary>
        /// <param name="HexFile">The hexadecimal file.</param>
        /// <param name="BeginAddress">The begin address.</param>
        /// <param name="EndAddress">The end address.</param>
        /// <param name="Overwrite">if set to <c>true</c> [overwrite].</param>
        public virtual void Add_to_MemoryMirror(CHexfileEntry[] HexFile, uint BeginAddress, uint EndAddress, bool Overwrite)
        {
            foreach (CHexfileEntry he in HexFile)
            {
                for (int i = 0; i < he.Values.Length; i++)
                {
                    uint MemoryLocation = (uint) (he.MemoryLocation + i);

                    if ((MemoryLocation >= BeginAddress) && (MemoryLocation <= EndAddress))
                    {
                        if (Overwrite || (HexMemoryMirror.accessed[MemoryLocation]==false))
                        HexMemoryMirror.value[MemoryLocation] = he.Values[i];
                        HexMemoryMirror.accessed[MemoryLocation] = true;
                    }
                }
            }
        }

        public virtual void Add_to_MemoryMirror(byte[] Data, uint BeginAddress, bool Overwrite, byte Addedchar = 0)
        {
            if (HexMemoryMirror == null)
                this.Make_MemoryMirror();

            uint MemoryLocation = BeginAddress;
            for (int i = 0; i < Data.Length; i++)
            {
                if (Overwrite || (HexMemoryMirror.accessed[MemoryLocation] == false))
                {
                    HexMemoryMirror.value[MemoryLocation] = Data[i];
                    HexMemoryMirror.accessed[MemoryLocation] = true;
                }
                MemoryLocation++;
            }
        }

        /// <summary>
        /// Makes new HexFile from HexMemoryMirror
        /// Reworked 24.1.2020 - only regions with HexMemoryMirror.accessed == true are included
        /// </summary>
        /// <param name="HEXFileName">Name of the hexadecimal file.</param>
        public void WriteHexFile_from_Memory(string HEXFileName)
        {
            System.IO.StreamWriter OutFile = new System.IO.StreamWriter(HEXFileName, false);
            //Walk through memory in Blocks of 16
            List<byte> line = new List<byte>();

            ushort ExtendedAdress = 0;
            ushort LastExtendedAdress = 0;
            OutFile.WriteLine(Make_Extended_Address_Line(ExtendedAdress));   //Set extended Adress

            //Lesen in 16er Blocks
            for (int i = 0; i < HexMemoryMirror.value.Length - 16; i += 16)
            {
                //Check if Extended Adress has to be set
                ExtendedAdress = (ushort)(i >> 16);
                if (ExtendedAdress != LastExtendedAdress)
                {
                    OutFile.WriteLine(Make_Extended_Address_Line(ExtendedAdress));
                    LastExtendedAdress = ExtendedAdress;
                }


                List<byte[]> accessedBlocks = new List<byte[]>();
                List<byte> acessedblock = null;
                List<ushort> accessedBlocks_startAdresses = new List<ushort>();


                for (int ptrBlock = 0; ptrBlock < 16; ptrBlock++)
                {
                    if (HexMemoryMirror.accessed[i + ptrBlock])
                    {
                        if (acessedblock == null)
                        {
                            acessedblock = new List<byte>();
                            accessedBlocks_startAdresses.Add((ushort)(i + ptrBlock));
                        }
                        acessedblock.Add(HexMemoryMirror.value[i + ptrBlock]);
                    }
                    else
                    {
                        if (acessedblock != null)
                        {
                            //Block building was starte - stop
                            accessedBlocks.Add(acessedblock.ToArray());
                            acessedblock = null;
                        }
                    }
                }

                if (acessedblock != null)
                {
                    accessedBlocks.Add(acessedblock.ToArray());
                }

                if (accessedBlocks != null)
                {
                    if (accessedBlocks.Count > 0)
                    {
                        //Work through blocks
                        int cntBlocks = 0;
                        foreach (byte[] barr in accessedBlocks)
                        {
                            line.Clear();
                            line.Add((byte)barr.Length);   //number of bytes
                            byte[] adr = BitConverter.GetBytes((ushort)accessedBlocks_startAdresses[cntBlocks]);
                            line.Add(adr[1]);   //Big Endian
                            line.Add(adr[0]);
                            line.Add(0);   //Record Type Data
                            for (int j = 0; j < barr.Length; j++)
                            {
                                line.Add(barr[j]);
                            }
                            OutFile.WriteLine(Make_String_With_Checksum(line));
                            cntBlocks++;
                        }
                    }
                }
            }
            //EOF
            OutFile.WriteLine(":00000001FF");
            OutFile.Close();
        }

        private string Make_Extended_Address_Line (ushort HigherByte)
        {
            //OutFile.WriteLine(":02 0000 04 0000 fa");

            List<byte> line = new List<byte>
            {
                2,    //Always bytes
                0,    //Adress is 0x0000
                0,
                4    //Type
            };
            byte[] adr = BitConverter.GetBytes(HigherByte);
            line.Add(adr[1]);   //Big Endian
            line.Add(adr[0]);
            return Make_String_With_Checksum(line);
        }
        private string Make_String_With_Checksum(List<byte> line)
        {
            //Calc Checksum
            byte checksum = 0;
            for (int j = 0; j < line.Count; j++)
            {
                checksum += line[j];
            }
            checksum = (byte)((checksum ^ 0xff) + 1);
            line.Add(checksum);
            return ":" + CMyConvert.ByteArrayto_HexString(line.ToArray(), false);
        }

    }
}
