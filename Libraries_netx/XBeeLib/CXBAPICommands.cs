namespace XBeeLib
{

    /// <summary>
    /// Provides or genertes API Commands
    /// </summary>
    public static class CXBAPICommands
    {
        public const byte ModemStatus = 0x8A;
        public const byte ATCommand = 0x08;
        public const byte ATCommandQueueParameterValue = 0x09;
        public const byte ATCommandResponse = 0x88;
        public const byte RemoteATCommandRequest = 0x17;
        public const byte RemoteCommandResponse = 0x97;
        public const byte TXRequest64bitAddress = 0x00;
        public const byte TXRequest16bitAaddress = 0x01;
        public const byte TXStatus = 0x89;
        public const byte RXPacket64bit = 0x80;
        public const byte RXPacket16bit = 0x81;

        public const byte StartDelimiter = 0x7E;
        public const ushort Default16BitAddress = 0xFFFE;

    }

}

