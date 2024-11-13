namespace FeedbackDataLib
{
    public class CPIC24BootloaderParams
    {
        //ICD3 Command Line Params

        //Für MPLAB 8 
        //const string cmd_Read_Neuromodul_EEG_To_File = @"/P33FJ128GP802 /V3.25 /GPF";    //Add FileName with "" with no space - see Add_File_to_CommandLineParameters
        //const string cmd_Read_Neuromodul_To_File = @"/P24FJ64GA102 /V3.25 /GPF";

        //MPLAB X

        public const string cmd_Read_Neuromodul_EEG_To_File = @"/P33FJ128GP802 /V3.25 /GF";    //Add FileName with "" with no space - see Add_File_to_CommandLineParameters
        public const string cmd_Read_Neuromodul_To_File = @"/P24FJ64GA102 /V3.25 /GF";


        public const string cmd_Flash_Neuromodul = @"/P24FJ64GA102 /V3.25 /M /F";              //Add FileName with "" with no space - see Add_File_to_CommandLineParameters
        public const string cmd_Flash_Neuromodul_EEG = @"/P33FJ128GP802 /V3.25 /M /F";              //Add FileName with "" with no space - see Add_File_to_CommandLineParameters

        //const string cmd_Erase_Neuromaster = @"/P24FJ256GB210 /E";
        public const string cmd_Flash_Neuromaster = @"/P24FJ256GB210 /M /L /F";

        //General
        public const int BOOTLOADER_SW_ADDRESS = 0x200;                //Place of SW Version, same position of software version for all types

        //SD Card Bootloader
        public const int PIC24_NM_BOOTLOADER_ORIGIN = 0x28800;
        public const int PIC24_NM_BOOTLOADER_LENGTH = 0x23F6;

        public const int PIC24_NM_BOOTLOADER_CRC_ADDRESS = 0x287FE;
        public const int PIC24_NM_BOOTLOADER_BL_ADDRESS = 0x2ABE0;
        public const int PIC24_NM_BOOTLOADER_HW_ADDRESS = PIC24_NM_BOOTLOADER_BL_ADDRESS + 2;   //0x2ABE2;
        public const int PIC24_NM_BOOTLOADER_UUID_ADDRESS = PIC24_NM_BOOTLOADER_HW_ADDRESS + 2; //0x2ABE4;

        //Neuromaster
        public const int PIC24_NM_PROGRAM_MEMORY_LENGTH = 0x285FE;

        //UART Bootloader PIC24
        public const int PIC24_BOOTLOADER_ORIGIN = 0xA000;
        public const int PIC24_BOOTLOADER_LENGTH = 0x0BF8;
        public const int PIC24_BOOTLOADER_BL_ADDRESS = 0xABE0;
        public const int PIC24_BOOTLOADER_HW_ADDRESS = PIC24_BOOTLOADER_BL_ADDRESS + 2; //0xABE2;
        public const int PIC24_BOOTLOADER_UUID_ADDRESS = PIC24_BOOTLOADER_HW_ADDRESS + 2; //0xABE4;             //Place of unique ID in PIC24 memory

        //Neuromodul - PIC24
        public const uint PIC24_SWChannelInfo_memory_location = 0x9C00;

        public const int PIC24_PROGRAM_MEMORY_LENGTH = 0x09E00;


        //UART Bootloader dsPIC33
        public const int dsPIC33_BOOTLOADER_ORIGIN = 0x14C00;
        public const int dsPIC33_BOOTLOADER_LENGTH = 0x0A00;
        public const int dsPIC33_BOOTLOADER_CRC_ADDRESS = 0x14BFC;
        public const int dsPIC33_BOOTLOADER_BL_ADDRESS = 0x157E0;
        public const int dsPIC33_BOOTLOADER_HW_ADDRESS = dsPIC33_BOOTLOADER_BL_ADDRESS + 2; //0x157E2;
        public const int dsPIC33_BOOTLOADER_UUID_ADDRESS = dsPIC33_BOOTLOADER_HW_ADDRESS + 2; //0x157E4;              //Place of unique ID in dsPIC memory

        //Neuromodul - dsPIC
        public const uint dsPIC33_SWChannelInfo_memory_location = 0x9C00;

        public const int dsPIC33_PROGRAM_MEMORY_LENGTH = 0x14A00;
    }
}