using BMTCommunicationLib;

namespace FeedbackDataLib
{
    public interface IC8Base
    {
        ISerialPort SerialPort { get; }
        //const string DriverSearchName = "";
        int BaudRate_LocalDevice { get; }
        int BaudRate_RemoteDevice { get; }
        string LastErrorString { get; }

        EnumConnectionStatus ConnectionStatus { get; }
        public void Init(ISerialPort SerialPort, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn);
        public void Init(string ComPortName, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn);
        void Close();

    }
}
