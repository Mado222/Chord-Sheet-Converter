using BMTCommunicationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackDataLib
{
    public interface IC8Base
    {
        ISerialPort SerialPort { get; }
        //const string DriverSearchName = "";
        int BaudRate_LocalDevice {get; }
        int BaudRate_RemoteDevice { get; }
        string LastErrorString { get; }

        public void Init(ISerialPort SerialPort, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn);
        public void Init(string ComPortName, byte CommandChannelNo, byte[] ConnectSequToSend, byte[] ConnectSequToReturn);
        void Close();

    }
}
