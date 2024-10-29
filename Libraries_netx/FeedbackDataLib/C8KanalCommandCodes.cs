using System;
using System.Collections;
using System.Collections.Generic;
using WindControlLib;

namespace FeedbackDataLib
{
    public class C8KanalReceiverCommandCodes
    {
        //configs
        /// <summary>
        /// Returnvalue for some functions: Error
        /// </summary>
        public const byte ERRORchar = 1;
        /// <summary>
        /// Returnvalue for some functions: OK
        /// </summary>
        public const byte OKchar = 0;

        /// <summary>
        /// Command channel number
        /// </summary>
        public const byte cCommandChannelNo = 0x0f;

        ////////////////////
        // Commando Codes to Neuromaster
        ////////////////////

        /// <summary>
        /// Commandcode: Signals that device is alive
        /// </summary>
        public const byte cDeviceAlive = 0xFE;

        /// <summary>
        /// Connecting to Device
        /// </summary>
        public const byte cConnectToDevice = 0xFA;

        /// <summary>
        /// Commandocode: The following code is a Command byte
        /// </summary>
        /// <remarks>Ein code ausserhalb der verwendeten ASCII Buchstaben (da BT Modul beim herstellen einer Verbindung allerhand Zeiechen ausgibt)</remarks>
        public const byte CommandCode = 0x11;

        /// <summary>
        /// Neuromaster sends every Second a message to synchronize data to
        /// </summary>
        public const byte cChannelSync = 0xFC;

        /// <summary>
        /// Neuromaster performs reset
        /// </summary>
        public const byte cReset = 0xDD;

        #region NeuromasterToPC

        /// <summary>
        /// Neuromaster sends Info to PC (Errors, Status, ...)
        /// </summary>
        public const byte cNeuromasterToPC = 0xFB;

        public class CNMtoPCCommands
        {
            /// <summary>
            /// Error in Device
            /// </summary>
            public const byte cModuleError = 0xFF;

            /// <summary>
            /// Buffer Fulll in Neuromaster
            /// </summary>
            /// <remarks>
            /// Neuromaster stops sampling, keep alive continues
            /// </remarks>
            public const byte cBufferFull = 0xBF;

            /// <summary>
            /// Neuromaster sends battery status
            /// </summary>
            public const byte cBatteryStatus = 0xDF;

            /// <summary>
            /// Neuromaster is going to switch off (or starts to record to SD Card)
            /// </summary>
            public const byte cNMOffline = 0xDE;

            /// <summary>
            /// NM has detected that Module Configuration is differnt from Configuration on the SD Card
            /// </summary>
            public const byte cModuleConfigChanged = 0xE0;
        }
        #endregion

        /// <summary>
        /// Buffer Fulll in Neuromaster
        /// </summary>
        /// <remarks>
        /// Neuromaster stops sampling, keep alive continues
        /// </remarks>
        public const byte cNeuromasterBufferFull = 0xBF;


        /// <summary>
        /// Commandocode: Set Clock
        /// </summary>
        public const byte cSetClock = 0x87;
        /// <summary>
        /// Commandocode: Read Clock
        /// </summary>
        public const byte cGetClock = 0x88;

        /// <summary>
        /// Neuromaster sends Firmware Version
        /// </summary>
        public const byte cGetFirmwareVersion = 0x89;

        /// <summary>
        /// Neuromaster sendes SD Card Info
        /// </summary>
        public const byte cGetSDCardInfo = 0x8B;

        //public const byte cCommandError = 0x00;             //Sent back in NM-to-PC-Ack as Command Code

        /// <summary>
        /// Set Module Configuration
        public const byte cSetModuleConfig = 0x93;

        /// <summary>
        /// Reads Module Configuration
        /// </summary>
        public const byte cGetChannelConfig = 0x94;

        /// <summary>
        /// Tells NM that PC is closing the Connection
        /// </summary>
        public const byte cSetConnectionClosed = 0x96;

        /// <summary>
        /// Write-Read Command for Modules - only passed through from Neuromaster
        /// </summary>
        /// <remarks>
        ///     +1byte HW_cn
        ///     +1byte number of bytes to send (inkl Module command)
        ///     +1byte number of bytes to read
        ///     ---- bytes to send:
        ///     +1byte ModuleCommand
        ///     + additional bytes
        /// </remarks>
        public const byte cWrRdModuleCommand = 0x97;

        /// <summary>
        /// Returns the specific information of one module
        /// </summary>
        //public const byte cGetModuleInfoSpecific = 0x9B;

        /// <summary>
        /// Sets the specific information of one module
        /// </summary>
        //public const byte cSetModuleInfoSpecific = 0x9C;

        /// <summary>
        /// get configuration of the selected module (all sw channels)
        /// </summary>
        public const byte cGetModuleConfig = 0x98;

        public const byte cScanModules = 0x99;              //Scans connected Modules

        //*****************************************************
        //***************** Communication to PC  **************
        //*****************************************************
        /*
         * Is sent with 3 bytes to pC via Command channel
         * 1 byte: cDeviceCommunicationToPC
         * 1 byte: cDeviceCommunicationToPC
         * 1 byte: cDeviceCommunicationToPC
         */


        //*****************************************************
        //***************** Module Specific Commands **********
        // Used in combination with cWrRdModuleCommand
        //*****************************************************

        /// <summary>
        /// Commandocode: Configure SCL
        /// </summary>
        /// <remarks>
        /// + 1 byte:
        /// bit 0: =0: Measuring
        ///        =1: SCL in Test Mode, switched to resistors
        /// bit 1: =0: Low Resistor
        ///        =1: High Resistor
        /// </remarks>
        //public const byte cMulti_SCLTest = 0x53;

        /// <summary>
        /// Commandocode: Configure Pulse in Multisensor
        /// </summary>
        /// <remarks>
        /// + 1 byte:
        /// bit 0: =0: Integrator is running
        ///        =1: Integrator short circuited
        /// bit 1: =0: Amplification Low
        ///        =1: Amplification High
        /// </remarks>
        //public const byte cMulti_PulseRun = 0x54;	        //Kurschluss Integrator


        /// <summary>
        /// Returns Module specific data
        /// </summary>
        public const byte cModule_GetSpecific = 0x55;

        /// <summary>
        /// Sets Module specific data
        /// </summary>
        public const byte cModule_SetSpecific = 0x56;

        //*****************************************************
        //***************** Predefined Sequences **************
        //*****************************************************

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        protected static WindControlLib.CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);    //10.1.2013

        /// <summary>
        /// Alive Sequence to be sent to Neuromaster
        /// </summary>
        public static byte[] AliveSequToSend()
        {
            //{ 0x11, 0xFE };//CommandCode, cDeviceAlive
            //return new byte[] { C8KanalReceiverCommandCodes.CommandCode, C8KanalReceiverCommandCodes.cDeviceAlive, 1, 0};

            byte[] buf = [C8KanalReceiverCommandCodes.CommandCode, C8KanalReceiverCommandCodes.cDeviceAlive, 1, 0];

            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 2);
            return buf;
        }

        /// <summary>
        /// Answer from Neuromaster to AliveSequToSend
        /// </summary>
        public static byte[] AliveSequToReturn()
        {
            //cDeviceAlive
            //return new byte[] { 0x7, 0xC0, 0x80, 0x82, C8KanalReceiverCommandCodes.cConnectToDevice, 0 };
            //+CRC
            //return new byte[] { C8KanalReceiverCommandCodes.cConnectToDevice, 0};

            byte[] buf = [C8KanalReceiverCommandCodes.cDeviceAlive, 0];
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 2);
            return buf;
        }

        /// <summary>
        /// Connect Sequence to be sent to Neuromaster
        /// </summary>
        public static byte[] ConnectSequToSend()
        {
            //{ 0x11, 0xFA, one byte, CRC};
            byte[] buf = [C8KanalReceiverCommandCodes.CommandCode, C8KanalReceiverCommandCodes.cConnectToDevice, 1, 0];
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 2);
            return buf;
        }

        /// <summary>
        /// Answer from Neuromaster to ConnectSequToSend
        /// </summary>
        public static byte[] ConnectSequToReturn()
        {
            //return new byte[] { 0x7, 0xC0, 0x80, 0x82, C8KanalReceiverCommandCodes.cConnectToDevice, 0};
            //+ CRC
            //return new byte[] { C8KanalReceiverCommandCodes.cConnectToDevice, 0xE8 };
            byte[] buf = [C8KanalReceiverCommandCodes.cConnectToDevice, 0];
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 2);
            return buf;
        }

    }
}
