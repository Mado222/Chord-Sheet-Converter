namespace BMTCommunicationLib
{

    //public delegate void DataReadyEventHandler(object sender, List<CDataIn> DataRead);
    //public delegate void StatusChangedEventHandler(object sender);
    //public delegate void CommandModeChangedEventHandler(object sender, bool CommandMode);
    //public delegate void DeviceCommunicationToPCEventHandler(object sender, byte[] buf);

    /// <summary>
    /// Interface for Communication
    /// </summary>
    /// <remarks>
    /// must be implemented by all drivers
    /// 	 * 6.7.2006
    /// 	 * Close implemented
    /// </remarks>
    //public interface ICommunication : IDisposable
    //{
    //    event DataReadyEventHandler DataReadyComm;
    //    event StatusChangedEventHandler StatusChangedComm;


    //    //Properties
    //    int ReceiverTimerInterval { get; }
    //    bool EnableDataReceiving { get; set; }  //Die dieses Interface implementierende Komponente empf�ngt keine Daten!

    //    //Methodes
    //    EnumConnectionStatus GetConnectionStatus();
    //    //int SendByteData(byte[] DataOut, int NumData);      //Erm�licht den direkten Zugriff auf die Kommunikation
    //    int GetByteData(ref byte[] DataIn, int NumData, int Offset);        //R�ckgabe der tats�chlich gelesenen Daten
    //    int GetByteDataTimeOut(ref byte[] DataIn, int NumData, int Offset, uint TimeOut);
    //    void ClearReceiveBuffer();
    //    void ClearTransmitBuffer();
    //    void Connect_via_tryToConnectWorker();
    //    void Close();
    //}

}


