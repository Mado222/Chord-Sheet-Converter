using System;
using System.IO;
using WindControlLib;


/* 
 * Introduced 8.5.2014
*/

namespace FeedbackDataLib
{
    //***************************************************************
    //
    //              C8KanalReceiverV2_SDCard
    //
    //***************************************************************/
    /// <summary>
    /// Basic Component for Insight Instruments "Neuromaster" with SDCard Connection
    /// </summary>
    /// <remarks></remarks>
    public class C8KanalReceiverV2_SDCard : C8KanalReceiverV2_CommBase, IDisposable
    {

        /// <summary>
        /// SDCard Connection
        /// </summary>
        public CSDCardConnection SDCardConnection;

        public string LastXBeeErrorString { get; } = "";

        public void AddSDCardValues(byte[] SDData)
        {
            SDCardConnection.AddSDCardValues(SDData);
        }

        public override bool GetDeviceConfig()
        {
            bool ret = false;
            if (Device == null) Device = new C8KanalDevice2();

            if (File.Exists(SDCardConnection.PathToConfigFile))
            {
                try
                {
                    FileStream fs = new(SDCardConnection.PathToConfigFile,
                                      FileMode.Open,
                                      FileAccess.Read);
                    BinaryReader br = new(fs);
                    long numBytes = new FileInfo(SDCardConnection.PathToConfigFile).Length;
                    byte[] AllData = br.ReadBytes((int)numBytes);
                    br.Close();
                    fs.Close();

                    //Check CRC
                    if (CRC8.Check_CRC8(ref AllData, null, true, false))
                    {
                        //ein sinnloses byte (vorletztes) und ein CRC byte am ende wegnehmen
                        Array.Resize(ref AllData, AllData.Length - 2);

                        Device.UpdateModuleInfoFrom_ByteArray(AllData);
                        Device.Calculate_SkalMax_SkalMin(); //Calculate max and mins
                        ret = true;
                    }
                }
                catch
                {
                    ret = false;
                }
            }
            return ret;
        }

        //private CByteRingpuffer InBufferSD = new CByteRingpuffer(1024);
        public static void ProcessData()
        {

        }


        /// <summary>
        /// Base Constructor
        /// </summary>
        public C8KanalReceiverV2_SDCard()
        {
            //Base constructor must be empty that the derived class does not call 
        }

        /// <summary>
        /// Path to the Configuration file on the SD Card
        /// </summary>
        /// <value>
        /// The path to configuration file.
        /// </value>
        /*
        public string PathToConfigFile
        {
            get
            {
                string ret = "";
                if (SDCardConnection != null)
                    return SDCardConnection.PathToConfigFile;
                return ret;
            }
            set
            {
                if (SDCardConnection != null)
                    SDCardConnection.PathToConfigFile = value;
            }
        }*/



        /// <summary>
        /// Initializes a new instance of the <see cref="C8KanalReceiverV2_XBee" /> class.
        /// </summary>
        /// <param name="SerialPort">Serial Port - can be null</param>
        public C8KanalReceiverV2_SDCard(CSDCardConnection SerialPort)
        {
            if (SerialPort == null)
            {
                SDCardConnection = new CSDCardConnection();
            }
            else
            {
                SDCardConnection = SerialPort;
            }
            Initialise_C8KanalReceiverV2_SDCard();
        }

        /// <summary>
        /// Function for Constructor
        /// </summary>
        private void Initialise_C8KanalReceiverV2_SDCard()
        {
            RS232Receiver = new CRS232Receiver2(C8KanalReceiverCommandCodes.cCommandChannelNo, SDCardConnection);
            base.C8KanalReceiverV2_Construct(); //calls CDataReceiver2_Construct();
            RS232Receiver.AliveSequToReturn = C8KanalReceiverCommandCodes.AliveSequToReturn();
            RS232Receiver.AliveSequToSend = C8KanalReceiverCommandCodes.AliveSequToSend();
            RS232Receiver.ConnectSequToReturn = C8KanalReceiverCommandCodes.ConnectSequToReturn();
            RS232Receiver.ConnectSequToSend = C8KanalReceiverCommandCodes.ConnectSequToSend();
            RS232Receiver.CRC8 = CRC8;
            RS232Receiver.SendKeepAlive = false;        //That we do not have Timeout issues
        }

        /// <summary>
        /// Opens Com and looks for device
        /// </summary>
        public bool CheckConnection_Start_trytoConnectWorker()
        {
            //Jetzt Verbinding herstellen
            Connect_via_tryToConnectWorker();
            return true;
        }

        public override bool SetClock(DateTime dt)
        {
            return true;
        }

        public override bool ScanModules()
        {
            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }

}

