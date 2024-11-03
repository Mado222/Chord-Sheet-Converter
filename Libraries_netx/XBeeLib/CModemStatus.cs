namespace XBeeLib
{
    /// <summary>
    /// implements the API Type Modem Status (0x8A)
    /// </summary>
    /// <remarks>base class: CBasicAPIResponse</remarks>
    public class CModemStatus : CBasicAPIResponse
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks>inits the AP-ID</remarks>
        public CModemStatus()
        {
            _APID = CXBAPICommands.ModemStatus;
        }

        private ModemStatus _modemStatus = ModemStatus.HardwareReset;

        /// <summary>
        /// modem status
        /// </summary>
        virtual public ModemStatus modemStatus
        {
            get { return _modemStatus; }
            set { _modemStatus = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">frame data</param>
        public override void initResponse(List<byte> recFrameData)
        {
            modemStatus = (ModemStatus)recFrameData[0];
        }

    }

    /// <summary>
    /// enum for possible modem status
    /// </summary>
    public enum ModemStatus
    {
        HardwareReset,
        WatchdogTimerReset,
        Associated,
        Disassociated,
        SynchronizationLost,
        CoordinatorRealignment,
        CoordinatorStarted
    }
}
