using System.Text;
using WindControlLib;

namespace FeedbackDataLib
{
    public class CIPE_Neuromodul_PIC24 : CIPE_Base
    {
        public const string cmd_Erase_Neuromaster = @"P24FJ256GB210 -E";
        public const string cmd_Flash_Neuromaster = @"P24FJ256GB210 -M -L -F";


        private string cmd_Read_Neuromodul_to_file(enumModuleType ModuleType)
        {
            //Für MPLAB x -> siehe OneNote "ICD3 Command Line"
            //cmd_Read_Neuromodul_EEG_To_File = @"/P33FJ128GP802 /V3.25 /GF";    //Add FileName with "" with no space - see Add_File_to_CommandLineParameters
            //cmd_Read_Neuromodul_To_File = @"/P24FJ64GA102 /V3.25 /GF";
            return "-" + get_processor_type(ModuleType) + " " + mc_programmer.CMDLine_string + @" -W -GF";
        }

        //ipecmd.exe -TPPK3 -P18F4550 -M –F"c:/demo.hex"

        private string cmd_Flash_Neuromodul(enumModuleType ModuleType)
        {
            //@"/P24FJ64GA102 /V3.25 /M /F";              //Add FileName with "" with no space - see Add_File_to_CommandLineParameters
            //-OL ... Release from Reset
            //-M  ... Program entire device
            //-W  ... Power target from tool
            //-F<file>
            return @"-" + get_processor_type(ModuleType) + " " + mc_programmer.CMDLine_string + " -W -OL -M -F";
        }

        public static string get_processor_type(enumModuleType ModuleType)
        {
            if (ModuleType == enumModuleType.cNeuromaster)
            {
                return "P24FJ256GB210";
            }
            else if (isDSPIC(ModuleType))
            {
                return "P33FJ128GP802";
            }
            return "P24FJ64GA102";
        }

        /// <summary>
        /// Determines whether the specified module is a DSPIC
        /// </summary>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns>
        ///   <c>true</c> if the specified module type is dspic; otherwise, <c>false</c>.
        /// </returns>
        public static bool isDSPIC(enumModuleType ModuleType)
        {
            if (ModuleType == enumModuleType.cModuleEEG)
                return true;
            else
                return false;
        }

        public CMicrochip_Programmer mc_programmer { get; set; }

        public CIPE_Neuromodul_PIC24(CMicrochip_Programmer Microchip_Programmer)
        {
            mc_programmer = Microchip_Programmer;
        }


        /// <summary>
        /// Gets the memory location in hex file of channel information.
        /// </summary>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public static uint Get_Memory_Location_of_ChannelInfo(enumModuleType ModuleType)
        {
            uint MemoryLocation = CPIC24_Bootloader_Params.PIC24_SWChannelInfo_memory_location;
            if (isDSPIC(ModuleType))
                MemoryLocation = CPIC24_Bootloader_Params.dsPIC33_SWChannelInfo_memory_location;
            return MemoryLocation;
        }

        /// <summary>HexFilePat_Base is copied to HexFilePath_Target 
        /// If SerialNumber != ""  the serial number will be replaced</summary>
        /// <param name="HexFilePat_Base">The hexadecimal file path</param>
        /// <param name="HexFilePath_Target">The hexadecimal file path with sn - if SerialNumber !=""</param>
        /// <param name="SerialNumber">The serial number.</param>
        /// <param name="StatusString">The status string.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public bool FlashHexFile_Copy_AddSerial(string HexFilePat_Base, string HexFilePath_Target, string SerialNumber, ref string StatusString, enumModuleType ModuleType = enumModuleType.cModuleAtem)
        {
            bool ret = false;

            //Add Serial to hex
            if (File.Exists(HexFilePat_Base))
            {
                if (ModuleType == enumModuleType.cNeuromaster)
                {
                    ret = Copy_hexFile_AddSerial(HexFilePat_Base, HexFilePath_Target, CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_UUID_ADDRESS, SerialNumber);
                }
                else
                {
                    if (isDSPIC(ModuleType))
                    {
                        //Todo Überschreibt File!!!!
                        ret = Copy_hexFile_AddSerial(HexFilePat_Base, HexFilePath_Target, CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_UUID_ADDRESS, SerialNumber);
                    }
                    else
                    {
                        ret = Copy_hexFile_AddSerial(HexFilePat_Base, HexFilePath_Target, CPIC24_Bootloader_Params.PIC24_BOOTLOADER_UUID_ADDRESS, SerialNumber);
                    }
                }
            }
            else
            {
                OnReportMeasurementProgress(HexFilePat_Base + " does not exist!", Color.Red);
                return ret;
            }

            if (ret)
            {
                ret = FlashHexFile(HexFilePath_Target, ref StatusString, ModuleType);
            }
            else
            {
                OnReportMeasurementProgress("Could not build temporary hex with updated Serial number", Color.Red);
            }
            return ret;
        }

        public bool FlashHexFile(string HexFilePath_Target, ref string StatusString, enumModuleType ModuleType)
        {
            bool ret;
            string cmd = cmd_Flash_Neuromodul(ModuleType);
            //if (ModuleType == enumModuleType.cNeuromaster) cmd = cmd_Flash_Neuromaster;
            ret = base.Operate_IPE(Add_File_to_CommandLineParameters(cmd, HexFilePath_Target), ref StatusString);
            return ret;
        }

        public bool ReadHexFile(string path_file_to_read, ref string StatusString, enumModuleType ModuleType)
        {
            return base.Operate_IPE(Add_File_to_CommandLineParameters(cmd_Read_Neuromodul_to_file(ModuleType), path_file_to_read), ref StatusString);
        }


        /// <summary>Copies HexFilePath_Base to HexFilePath_Target and adds Serialnumber if != ""</summary>
        /// <param name="HexFilePath_Base">The hexadecimal file path base.</param>
        /// <param name="HexFilePath_Target">The hexadecimal file path target.</param>
        /// <param name="MemoryLocation">The memory location.</param>
        /// <param name="SerialNumber">The serial number.</param>
        /// <returns></returns>
        private static bool Copy_hexFile_AddSerial(string HexFilePath_Base, string HexFilePath_Target, int MemoryLocation, string SerialNumber = "")
        {
            WindControlLib.CPIC24_IntelHex cih = new();

            if (File.Exists(HexFilePath_Base))
            {
                if (cih.OpenHexFile(HexFilePath_Base))
                {
                    if (SerialNumber != "")
                    {
                        List<byte> bt = new(Encoding.ASCII.GetBytes(SerialNumber))
                        {
                            0  //Null terminierter String
                        };
                        cih.Make_MemoryMirror();
                        //Todo Funktioniert das f PIC24?? vorher Add_Data_to_MemoryMirror
                        cih.Add_to_MemoryMirror([.. bt], (uint)MemoryLocation, true);
                    }
                    cih.WriteHexFile_from_Memory(HexFilePath_Target);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   <para>
        ///  Reads a string from hexFle file.
        /// </para>
        /// </summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="MemoryLocation">The memory location.</param>
        /// <param name="numbytestoread">The numbytestoread.</param>
        /// <returns></returns>
        private static string Read_string_from_hexFile(string readHexfilePath, int MemoryLocation, int numbytestoread)
        {
            string ret = "";

            List<byte> b = Read_byte_from_hexFile(readHexfilePath, MemoryLocation, numbytestoread);
            if (b != null)
            {
                try
                {
                    ret = System.Text.Encoding.ASCII.GetString(b.ToArray()).Trim();
                    string[] ss = ret.Split(new char[] { '\0' }, System.StringSplitOptions.RemoveEmptyEntries);
                    ret = ss[0];
                }
                catch { }
            }

            return ret;
        }

        /// <summary>Reads the byte from hexadecimal file.</summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="MemoryLocation">The memory location.</param>
        /// <param name="numbytestoread">The numbytestoread.</param>
        /// <returns></returns>
        private static List<byte> Read_byte_from_hexFile(string readHexfilePath, int MemoryLocation, int numbytestoread)
        {
            CPIC24_IntelHex cih = new();
            List<byte> b = [];
            if (File.Exists(readHexfilePath))
            {
                if (cih.OpenHexFile(readHexfilePath))
                {
                    //Todo Funktioniert das f PIC24?? vorher Get_Data_from_Memory_Area
                    b = new List<byte>(cih.Get_Memory_Area((uint)MemoryLocation, (uint)numbytestoread));
                }
            }
            return b;
        }

        /// <summary>Gets the serial number from hexadecimal file.</summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public string Get_SerialNumber_from_hexFile(string readHexfilePath, enumModuleType ModuleType)
        {
            if (ModuleType == enumModuleType.cNeuromaster)
                return Read_string_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_UUID_ADDRESS, 16);

            if (isDSPIC(ModuleType))
                return Read_string_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_UUID_ADDRESS, 16);
            else
                return Read_string_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_BOOTLOADER_UUID_ADDRESS, 16);
        }

        /// <summary>Gets the hw version from hexadecimal file.</summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public static string Get_HWVersion_from_hexFile(string readHexfilePath, enumModuleType ModuleType)
        {
            //Neuromodul:
            //#define HWVERSION				(TYPE << 8) | CONF_HWVERSION

            //Neuromaster
            //#define CONF_HWVERSION		CONF_HWVERSION_MAJOR << 4 | CONF_HWVERSION_MINOR

            string s = "";
            List<byte> b;
            if (ModuleType == enumModuleType.cNeuromaster)
            {
                b = Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_HW_ADDRESS, 2);

                if (b != null)
                {
                    uint u = CMyConvert.FromUIntBytestoUInt(b[0], b[1]);
                    string CONF_HWVERSION_Minor = (u & 0x0F).ToString();
                    string CONF_HWVERSION_Major = (u >> 4).ToString();
                    s = CONF_HWVERSION_Major + "." + CONF_HWVERSION_Minor;
                }
            }
            else
            {
                if (isDSPIC(ModuleType))
                    b = Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_HW_ADDRESS, 2);
                else
                    b = Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_BOOTLOADER_HW_ADDRESS, 2);

                if (b != null)
                {
                    string CONF_TYPE = b[1].ToString();
                    string CONF_HWVERSION = b[0].ToString();
                    s = CONF_TYPE + "." + CONF_HWVERSION;
                }
            }
            return s;
        }

        /// <summary>Gets the sw version from hexadecimal file.</summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public static string Get_SWVersion_from_hexFile(string readHexfilePath, enumModuleType ModuleType)
        {
            return Decode_SWVersion(Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.BOOTLOADER_SW_ADDRESS, 2));
        }

        /// <summary>Gets the bootloader sw version from hexadecimal file.</summary>
        /// <param name="readHexfilePath">The read hexfile path.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public static string Get_Bootloader_SWVersion_from_hexFile(string readHexfilePath, enumModuleType ModuleType)
        {
            if (ModuleType == enumModuleType.cNeuromaster)
                return Decode_SWVersion(Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_BL_ADDRESS, 2));

            if (isDSPIC(ModuleType))
                return Decode_SWVersion(Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_BL_ADDRESS, 2));
            else
                return Decode_SWVersion(Read_byte_from_hexFile(readHexfilePath, CPIC24_Bootloader_Params.PIC24_BOOTLOADER_BL_ADDRESS, 2));
        }

        /// <summary>Decodes the sw version</summary>
        /// <param name="b">The b.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        private static string Decode_SWVersion(List<byte> b)
        {
            string s = "";

            if (b != null)
            {
                //#define SWVERSION				(CONF_SWVERSION_MAJOR << 12) | CONF_SWVERSION_MINOR
                //#define CONF_SWVERSION				(((CONF_SWVERSION_MAJOR << CONF_SWVERSION_MAJOR_SHIFT) & CONF_SWVERSION_MAJOR_MASK) | ((CONF_SWVERSION_MINOR << CONF_SWVERSION_MINOR_SHIFT) & CONF_SWVERSION_MINOR_MASK))
                uint u = CMyConvert.FromUIntBytestoUInt(b[0], b[1]);
                string CONF_SWVERSION_MAJOR = (u >> 12).ToString();
                string CONF_SWVERSION_MINOR = (u & 0x0FFF).ToString();
                s = CONF_SWVERSION_MAJOR + "." + CONF_SWVERSION_MINOR;
            }
            return s;
        }



        /// <summary>
        /// Gets the serial number from flashed firmware
        /// </summary>
        /// <param name="tempPath">path to temporary hex file; if empty a temporary path will be generated</param>
        /// <param name="SerialNumber">The serial number.</param>
        /// <param name="SWVersion">The sw version.</param>
        /// <param name="HWVersion">The hw version.</param>
        /// <param name="StatusString">The status string.</param>
        /// <param name="deleteFile">if set to <c>true</c> temp file will be finally deleted.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public bool Get_SerialNumber_from_FlashedFirmware(ref string tempPath, ref string SerialNumber, ref string SWVersion, ref string HWVersion, ref string StatusString, bool deleteFile, enumModuleType ModuleType)
        {
            bool ret = false;

            if (tempPath == "")
                tempPath = System.IO.Path.GetTempPath() + "read_hex.hex";

            if (ReadHexFile(tempPath, ref StatusString, ModuleType))
            {
                SerialNumber = Get_SerialNumber_from_hexFile(tempPath, ModuleType);
                SWVersion = Get_SWVersion_from_hexFile(tempPath, ModuleType);
                HWVersion = Get_HWVersion_from_hexFile(tempPath, ModuleType);
                ret = true;
            }
            else
            {
                SerialNumber = "";
                SWVersion = "";
            }
            if (File.Exists(tempPath) && deleteFile)
                File.Delete(tempPath);

            return ret;
        }

        public class CModuleInformation
        {
            public string SerialNumber = "";
            public string SWVersion = "";
            public string HWVersion_Full = "";
            public string BLVersion = "";

            public string ModuleType
            {
                get
                {
                    if (isNeuromaster)
                    {
                        return Enum.GetName(typeof(enumModuleType), enumModuleType.cNeuromaster) ?? "Unknown";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(HWVersion_Full))
                        {
                            return "Unknown"; // Or handle this case as appropriate
                        }

                        string[] ss = HWVersion_Full.Split('.');
                        if (ss.Length > 0 && uint.TryParse(ss[0], out uint moduleValue))
                        {
                            return Enum.GetName(typeof(enumModuleType), moduleValue) ?? "Unknown";
                        }
                    }
                    return "Unknown"; // Or handle this case as appropriate
                }
            }

            public enumModuleType ModuleType_enum
            {
                get
                {
                    if (isNeuromaster)
                    {
                        return enumModuleType.cNeuromaster;
                    }
                    else
                    {
                        string[] ss = HWVersion_Full.Split('.');
                        return (enumModuleType)Convert.ToUInt32(ss[0]);
                    }
                }
            }


            public string HWVersion
            {
                get
                {
                    if (isNeuromaster)
                    {
                        return HWVersion_Full;
                    }
                    else
                    {
                        string[] ss = HWVersion_Full.Split('.');
                        return ss[1];
                    }
                }
            }


            public bool isNeuromaster = false;
            public bool isValid = true;
        }

        public CModuleInformation Get_FullInfo(string hexPath)
        {

            //try Neuromodul
            CModuleInformation ModuleInfo = new();
            enumModuleType _ModuleType = enumModuleType.cModuleAtem;
            _Get_ModuleInfo(hexPath, _ModuleType, ref ModuleInfo);
            ModuleInfo.isNeuromaster = false;

            if (ModuleInfo.SerialNumber.Contains("?"))
            {
                //Invalid, try EEG
                _ModuleType = enumModuleType.cModuleEEG;
                _Get_ModuleInfo(hexPath, _ModuleType, ref ModuleInfo);

                if (ModuleInfo.SerialNumber.Contains("?"))
                {
                    //Invalid, try Neuromaster
                    _ModuleType = enumModuleType.cNeuromaster;
                    _Get_ModuleInfo(hexPath, _ModuleType, ref ModuleInfo);
                    ModuleInfo.isNeuromaster = true;
                    if (ModuleInfo.SerialNumber.Contains("?"))
                    {
                        ModuleInfo.isValid = false;
                    }
                }
            }
            return ModuleInfo;

        }

        protected void _Get_ModuleInfo(string hexPath, enumModuleType ModuleType, ref CModuleInformation ModuleInfo)
        {
            ModuleInfo.SerialNumber = Get_SerialNumber_from_hexFile(hexPath, ModuleType);
            ModuleInfo.SWVersion = Get_SWVersion_from_hexFile(hexPath, ModuleType);
            ModuleInfo.HWVersion_Full = Get_HWVersion_from_hexFile(hexPath, ModuleType);
            ModuleInfo.BLVersion = Get_Bootloader_SWVersion_from_hexFile(hexPath, ModuleType);
        }

        /// <summary>
        /// Makes the combined hexadecimal file with new channel information.
        /// </summary>
        /// <param name="Path_Original_combined_hex_file">The path to unaltered combined hexadecimal file.</param>
        /// <param name="Path_new_combined_hex_file">The path to new combined hex file.</param>
        /// <param name="ChannelInfo">The channel information.</param>
        /// <param name="ModuleType">Type of the module.</param>
        public static void Make_Combined_Hex_File_with_new_ChannelInfo(string Path_Original_combined_hex_file, string Path_new_combined_hex_file, CSWChannelInfo[] ChannelInfo, enumModuleType ModuleType)
        {
            //Get correct Memory location
            uint MemoryLocation = CIPE_Neuromodul_PIC24.Get_Memory_Location_of_ChannelInfo(ModuleType);

            CPIC24_IntelHex ih_module = new();
            ih_module.OpenHexFile(Path_Original_combined_hex_file);
            ih_module.Make_MemoryMirror();

            List<byte> mem_vals_list = [];
            for (int i = 0; i < ChannelInfo.Length; i++)
            {
                mem_vals_list.AddRange(ChannelInfo[i].GetBytes());
            }

            ih_module.Add_to_MemoryMirror([.. mem_vals_list], MemoryLocation, true);
            //Speichern
            ih_module.WriteHexFile_from_Memory(Path_new_combined_hex_file);
        }

        public static void Make_Combined_Hex_File_with_SerialNumber(string Path_Original_combined_hex_file, string Path_new_combined_hex_file, CSWChannelInfo[] ChannelInfo, enumModuleType ModuleType)
        {
            //Get correct Memory location
            uint MemoryLocation = CIPE_Neuromodul_PIC24.Get_Memory_Location_of_ChannelInfo(ModuleType);

            CPIC24_IntelHex ih_module = new();
            ih_module.OpenHexFile(Path_Original_combined_hex_file);
            ih_module.Make_MemoryMirror();

            List<byte> mem_vals_list = [];
            for (int i = 0; i < ChannelInfo.Length; i++)
            {
                mem_vals_list.AddRange(ChannelInfo[i].GetBytes());
            }

            ih_module.Add_to_MemoryMirror([.. mem_vals_list], MemoryLocation, true);
            //Speichern
            ih_module.WriteHexFile_from_Memory(Path_new_combined_hex_file);
        }


        /// <summary>
        /// Gets the channel information from combined hexadecimal file.
        /// </summary>
        /// <param name="Path_Original_combined_hex_file">The path to unaltered combined hexadecimal file.</param>
        /// <param name="ModuleType">Type of the module.</param>
        /// <returns></returns>
        public static CSWChannelInfo[] Get_ChannelInfo_from_Combined_hex_file(string Path_Original_combined_hex_file, enumModuleType ModuleType)
        {
            CSWChannelInfo[] swcis = new CSWChannelInfo[4];
            CPIC24_IntelHex ih_module = new();

            //Get correct Memory location
            uint MemoryLocation = CIPE_Neuromodul_PIC24.Get_Memory_Location_of_ChannelInfo(ModuleType);


            ih_module.OpenHexFile(Path_Original_combined_hex_file);

            //Read Memory
            byte[] mem_vals = ih_module.Get_Memory_Area(MemoryLocation, (uint)(swcis.Length * CSWChannelInfo.get_size_of()));

            //Deserialize
            int ptr = 0;
            for (int i = 0; i < swcis.Length; i++)
            {
                swcis[i] = new CSWChannelInfo();
                ptr = swcis[i].UpdateFrom_ByteArray(mem_vals, ptr);
            }
            return swcis;
        }

        /// <summary>
        /// Reads the program memory, gets the serial number from connected module.
        /// </summary>
        /// <param name="tempPath">path to temporary hex file; if empty a temporary path will be generated</param>
        /// <param name="SerialNumber">The serial number.</param>
        /// <returns></returns>
        public bool Get_Serial_Number_from_connected_Module(ref string tempPath, ref string SerialNumber, ref string SWVersion, ref string HWVersion, enumModuleType ModuleType)
        {
            string status = "";

            OnReportMeasurementProgress("Reading PIC .... ", Color.Black);
            bool ret = Get_SerialNumber_from_FlashedFirmware(ref tempPath, ref SerialNumber, ref SWVersion, ref HWVersion, ref status, false, ModuleType);
            OnReportMeasurementProgress(status, Color.Black);

            if (!ret)
            {
                //try the other PIC type
                if (ModuleType == enumModuleType.cModuleEEG)
                {
                    //try PIC24
                    OnReportMeasurementProgress("dsPIC33 failed, trying PIC24", Color.Red);
                    ret = Get_SerialNumber_from_FlashedFirmware(ref tempPath, ref SerialNumber, ref SWVersion, ref HWVersion, ref status, false, enumModuleType.cModuleAtem);
                }
                else
                {
                    //try DSPIC
                    OnReportMeasurementProgress("PIC24 failed, trying dsPIC33", Color.Red);
                    ret = Get_SerialNumber_from_FlashedFirmware(ref tempPath, ref SerialNumber, ref SWVersion, ref HWVersion, ref status, false, enumModuleType.cModuleEEG);
                }
            }

            if (ret)
            {
                OnReportMeasurementProgress("Serial: " + SerialNumber, Color.Blue);
                OnReportMeasurementProgress("Firmware Version: " + SWVersion, Color.Blue);
                OnReportMeasurementProgress("Hardware Version: " + HWVersion, Color.Blue);
                //string[] s = { "a", "b" };

                string[] ss = HWVersion.Split(['.']);

                enumModuleType t = (enumModuleType)Convert.ToInt16(ss[0]);

                OnReportMeasurementProgress("Module Type: " + t.ToString(), Color.Blue);
            }
            else
            {
                ret = false;
                OnReportMeasurementProgress("Could not read memory of Neuromodul", System.Drawing.Color.Red);
            }
            return ret;
        }


    }
}
