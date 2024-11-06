using BMTCommunicationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindControlLib;

namespace FeedbackDataLib
{
 /*   public enum NeuromasterCommand : byte
    {
        None = 0,
        ////////////////////
        // Commando Codes to Neuromaster
        ////////////////////

        /// <summary>Signals that device is alive</summary>
        DeviceAlive = 0xFE,

        /// <summary>Connecting to Device</summary>
        ConnectToDevice = 0xFA,

        /// <summary>The following code is a Command byte</summary>
        CommandCode = 0x11,

        /// <summary>Neuromaster sends every second a message to synchronize data</summary>
        ChannelSync = 0xFC,

        /// <summary>Neuromaster performs reset</summary>
        Reset = 0xDD,

        // Neuromaster to PC
        /// <summary>Neuromaster sends Info to PC (Errors, Status, ...)</summary>
        NeuromasterToPC = 0xFB,

        // Neuromaster-to-PC Commands
        /// <summary>Error in Device</summary>
        ModuleError = 0xFF,

        /// <summary>Buffer Full in Neuromaster</summary>
        /// <remarks>Neuromaster stops sampling, keep alive continues</remarks>
        BufferFull = 0xBF,

        /// <summary>Neuromaster sends battery status</summary>
        BatteryStatus = 0xDF,

        /// <summary>Neuromaster is going to switch off (or starts to record to SD Card)</summary>
        NMOffline = 0xDE,

        /// <summary>NM has detected that Module Configuration is different from SD Card configuration</summary>
        ModuleConfigChanged = 0xE0,

        /// <summary>Set Clock</summary>
        SetClock = 0x87,

        /// <summary>Read Clock</summary>
        GetClock = 0x88,

        /// <summary>Neuromaster sends Firmware Version</summary>
        GetFirmwareVersion = 0x89,

        /// <summary>Neuromaster sends SD Card Info</summary>
        GetSDCardInfo = 0x8B,

        /// <summary>Set Module Configuration</summary>
        SetModuleConfig = 0x93,

        /// <summary>Reads Module Configuration</summary>
        GetChannelConfig = 0x94,

        /// <summary>Tells NM that PC is closing the Connection</summary>
        SetConnectionClosed = 0x96,

        /// <summary>Write-Read Command for Modules - only passed through from Neuromaster</summary>
        /// <remarks> +1byte HW_cn, +1byte number of bytes to send, +1byte number of bytes to read</remarks>
        WrRdModuleCommand = 0x97,

        /// <summary>Get configuration of the selected module (all sw channels)</summary>
        GetModuleConfig = 0x98,

        /// <summary>Scans connected Modules</summary>
        ScanModules = 0x99,

        // Module Specific Commands (Used with WrRdModuleCommand)
        /// <summary>Returns Module specific data</summary>
        Module_GetSpecific = 0x55,

        /// <summary>Sets Module specific data</summary>
        Module_SetSpecific = 0x56
    }


    public enum CommunicationState
    {
        Idle,
        SendingCommand,
        WaitingForResponse,
        ProcessingResponse,
        Error
    }
    internal class CCommunicationStateMachine
    {
        private CommunicationState _currentState = CommunicationState.Idle;
        private readonly CRS232Receiver2 _serialPort; // Assumed interface for async communication
        private NeuromasterCommand _currentCommand = NeuromasterCommand.None;

        /// <summary>
        /// CRC8 Algorithm
        /// </summary>
        protected CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);


        public CCommunicationStateMachine(CRS232Receiver2 cRS232Receiver2)
        {
            _serialPort = cRS232Receiver2;
        }

        public async Task ExecuteCommandAsync(NeuromasterCommand command, CancellationToken cancellationToken)
        {
            _currentCommand = command;
            _currentState = CommunicationState.SendingCommand;

            byte [] buf = BuildNMCommand ((byte) command, []);
            await SendCommandAsync(buf, cancellationToken);

            _currentState = CommunicationState.WaitingForResponse;

            // Await response
            var response = await WaitForResponseAsync(cancellationToken);

            // Handle the response based on current command
            ProcessResponse(response);
        }

        private async Task SendCommandAsync(byte[] commandBytes, CancellationToken cancellationToken)
        {
            await _serialPort.SendAsync(commandBytes, 0, commandBytes.Length, cancellationToken);
        }

        private async Task<byte[]> WaitForResponseAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Assume we have a timeout and async read operation
                return await _serialPort..ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _currentState = CommunicationState.Error;
                throw; // Or handle timeout as needed
            }
        }

        private void ProcessResponse(byte[] response)
        {
            switch (_currentCommand)
            {
                case NeuromasterCommand.SetClock:
                    // Process response for SetClock command
                    break;
                case NeuromasterCommand.GetClock:
                    // Process response for GetClock command
                    break;
                case NeuromasterCommand.GetFirmwareVersion:
                    // Process response for GetFirmwareVersion command
                    break;
                case NeuromasterCommand.GetSDCardInfo:
                    // Process response for GetSDCardInfo command
                    break;
                case NeuromasterCommand.SetModuleConfig:
                    // Process response for SetModuleConfig command
                    break;
                case NeuromasterCommand.GetChannelConfig:
                    // Process response for GetChannelConfig command
                    break;
                default:
                    throw new InvalidOperationException("Unexpected command response");
            }

            // Transition back to Idle after processing
            _currentState = CommunicationState.Idle;
            _currentCommand = NeuromasterCommand.None;
        }

        protected byte[] BuildNMCommand(byte C8KanalRecCommandCode, byte[] AdditionalDataToSend)
        {
            // Validate AdditionalDataToSend size early
            if (AdditionalDataToSend.Length > 250)
            {
                throw new ArgumentException("Size of AdditionalDataToSend must be <= 250", nameof(AdditionalDataToSend));
            }

            const int overhead = 4; // CommandCode, Command, Length, CRC
            int bytestosend = overhead + AdditionalDataToSend.Length;
            byte[] buf = new byte[bytestosend];

            // Build the command buffer
            buf[0] = C8KanalReceiverCommandCodes.CommandCode;  // Add the base command code
            buf[1] = C8KanalRecCommandCode;                    // Add the specific command code
            buf[2] = (byte)(AdditionalDataToSend.Length + 1);  // Length byte (+CRC)

            // Copy additional data into the buffer
            Buffer.BlockCopy(AdditionalDataToSend, 0, buf, overhead - 1, AdditionalDataToSend.Length);

            // Calculate and set CRC
            buf[^1] = CRC8.Calc_CRC8(buf, buf.Length - 1);
            return buf;
        }
    }*/
}
