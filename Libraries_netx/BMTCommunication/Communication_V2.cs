using WindControlLib;

namespace BMTCommunication
{
    public enum EnumConnectionStatus : int
    {
        Not_Connected,
        Connected,
        Dis_Connected,
        Wrong_Device,
        PortError,
        Connecting,
        No_Data_Link,
        USB_disconnected,
        USB_reconnected
    }

    public enum EnumTimQueryStatus : int
    {
        no_Special,
        isSync
    }

    public delegate void DataReadyEventHandler(object sender, List<CDataIn> DataRead);
    public delegate void StatusChangedEventHandler(object sender);
    public delegate void CommandModeChangedEventHandler(object sender, bool CommandMode);
    public delegate void DeviceCommunicationToPCEventHandler(object sender, byte[] buf);

    /// <summary>
    /// Interface for Communication
    /// </summary>
    /// <remarks>
    /// must be implemented by all drivers
    /// 	 * 6.7.2006
    /// 	 * Close implemented
    /// </remarks>
    public interface ICommunication : IDisposable
    {
        event DataReadyEventHandler DataReadyComm;
        event StatusChangedEventHandler StatusChangedComm;
        //event LogErrorEventHandler LogError;

        void InitReceiverBuffer(int ReceiverTimerInterval, int AnzReturnBlocks, int BytetoRead, int ReadValues);
        //AnzReturnBlocks: Anzahl der cUSBBytetoRead Gruppen die bei einem DataReady Event zurückgegeben werden
        //BytetoRead: 	   Anzahl der Byte die bei einem Zugriff gelesen werden
        //ReadValues:	   Anzahl der CDataIn die aus BytetoRead hervorgehen

        //Properties
        int ReceiverTimerInterval { get; }
        bool EnableDataReadyEvent { get; set; }
        bool EnableDataReceiving { get; set; }  //Die dieses Interface implementierende Komponente empfängt keine Daten!
        byte[] GetCommand { get; }

        //Methodes
        void GetData(ref List<CDataIn> Data);
        EnumConnectionStatus GetConnectionStatus();
        int SendByteData(byte[] DataOut, int NumData);      //Ermölicht den direkten Zugriff auf die Kommunikation
        int GetByteData(ref byte[] DataIn, int NumData, int Offset);        //Rückgabe der tatsächlich gelesenen Daten
        int GetByteDataTimeOut(ref byte[] DataIn, int NumData, int Offset, uint TimeOut);
        void ClearReceiveBuffer();
        void ClearTransmitBuffer();
        void Connect_via_tryToConnectWorker();
        void Close();
    }

}


