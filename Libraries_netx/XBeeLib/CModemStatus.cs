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

        private EnModemStatus _modemStatus = EnModemStatus.HardwareReset;

        /// <summary>
        /// modem status
        /// </summary>
        virtual public EnModemStatus ModemStatus
        {
            get { return _modemStatus; }
            set { _modemStatus = value; }
        }

        /// <summary>
        /// inits the object with the receiving frame
        /// </summary>
        /// <param name="recFrameData">frame data</param>
        public override void InitResponse(List<byte> recFrameData)
        {
            ModemStatus = (EnModemStatus)recFrameData[0];
        }

    }

    /// <summary>
    /// enum for possible modem status
    /// </summary>
    public enum EnModemStatus
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
