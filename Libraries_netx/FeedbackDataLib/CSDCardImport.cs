using BMTCommunicationLib;
using FeedbackDataLib.Modules;
using WindControlLib;

namespace FeedbackDataLib
{
    /// <summary>
    /// Imports Data from the SD Card
    /// </summary>
    public class CSDCardImport
    {

        private long _DataFileLength;
        /// <summary>
        /// Gets the length of the data file.
        /// </summary>
        /// <value>
        /// The length of the data file.
        /// </value>
        public long DataFileLength
        {
            get { return _DataFileLength; }
        }

        /// <summary>
        /// Gets how many bytes are already read
        /// </summary>
        /// <value>
        /// =SDBinaryReader.BaseStream.Position
        /// </value>
        public long BytesRead
        {
            get
            {
                if (SDBinaryReader != null)
                    return SDBinaryReader.BaseStream.Position;
                return 0;
            }
        }

        private List<CModuleBase> _ModuleInfo = [];
        public List<CModuleBase> ModuleInfo { get { return _ModuleInfo; } }

        /// <summary>
        /// Event Handler for ImportInfo
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="Info">information as text</param>
        /// <param name="col">Suggested color for display</param>
        public delegate void ImportInfoEventHandler(object sender, string Info, Color col);

        /// <summary>
        /// Occurs when some information are given to the GUI
        /// </summary>
        public event ImportInfoEventHandler? ImportInfo;

        /// <summary>
        /// Called when ImportInfo should be triggered
        /// </summary>
        /// <param name="Info">information as text</param>
        /// <param name="col">Suggested color for display</param>
        protected virtual void OnImportInfo(string Info, Color col)
        {
            ImportInfo?.Invoke(this, Info, col);
        }

        /// <summary>
        /// Class with scaled data
        /// </summary>
        public class CDataIn_Scaled : CDataIn
        {
            public CDataIn_Scaled(CDataIn cd)
            {
                base.Copy(cd);
            }

            /// <summary>
            /// Sampled value, scaled
            /// </summary>
            public double Value_Scaled = 0;

            public void Copy(CDataIn_Scaled DataIn)
            {
                base.Copy(DataIn);
                Value_Scaled = DataIn.Value_Scaled;
            }
        }


        /// <summary>
        /// Finalizes an instance of the <see cref="CSDCardImport"/> class.
        /// </summary>
        ~CSDCardImport()
        {
            StopImport();
        }

        public bool Sync_Packet_has_4_bytes { get; set; } = true;


        /// <summary>
        /// Gaets DateTime from path to NM saved files
        /// </summary>
        /// <param name="path">Path to time directory</param>
        /// <returns>DateTime.MinValue if failed</returns>
        public static DateTime Get_DateTime_FromDirectory(string path)
        {
            DateTime ret;
            try
            {
                //Split Directory
                string[] ss = path.Split(Path.DirectorySeparatorChar);
                string t = ss[^1];   //time
                string d = ss[^2];   //date

                string[] tt = ss[^1].Split(['-']); //time
                string[] dd = ss[^2].Split(['-']); //date


                ret = new DateTime(
                    2000 + Convert.ToInt32(dd[0]),
                    Convert.ToInt32(dd[1]),
                    Convert.ToInt32(dd[2]),
                    Convert.ToInt32(tt[0]),
                    Convert.ToInt32(tt[1]),
                    Convert.ToInt32(tt[2]));
            }
            catch (Exception)
            {
                //OnImportInfo("DateTime from Path failed", Color.Red);
                return DateTime.MinValue;
            }

            return ret;
        }


        /// Class to connect to Neuromaster
        private C8KanalReceiverV2? DataReceiver;
        private readonly WindControlLib.CCRC8 CRC8 = new(CCRC8.CRC8_POLY.CRC8_CCITT);
        private DateTime LastSyncSignal;
        private DateTime Recording_Start_Time_from_File = DateTime.MinValue;
        private TimeSpan SyncInterval = new(0, 0, 0, 0, C8KanalReceiverV2_CommBase.SyncInterval_ms);

        private FileStream? SDFileStream = null;
        private BinaryReader? SDBinaryReader = null;

        /// <summary>
        /// Anzahl der Byte die bei einem SD Zugriff gelesen werden
        /// </summary>
        private const int _RS232BytetoRead = 4;

        /// <summary>
        /// Puffer für Bytes die von der RS232 gelesen werden; Groesse:_AnzReturnBlocks*_RS232BytetoRead
        /// </summary>
        private readonly byte[] RS232inBytes = new byte[_RS232BytetoRead];


        private int bytein = 0;
        private bool isDataByte = true;

        /// <summary>
        /// Initialises the import procedure
        /// </summary>
        /// <param name="SourcePath">Source path</param>
        /// <returns>Configuration of the Modules</returns>
        public List<CModuleBase> StartImport(string SourcePath)
        {
            LastSyncSignal = DateTime.MinValue;
            Recording_Start_Time_from_File = CSDCardImport.Get_DateTime_FromDirectory(SourcePath);
            if (Recording_Start_Time_from_File != DateTime.MinValue)
            {
                //Init
                if (DataReceiver == null)
                {
                    DataReceiver = new C8KanalReceiverV2();
                    DataReceiver.Init_via_SDCard();
                    DataReceiver.Connect();
                }

                if (DataReceiver is not null && DataReceiver.SDCardConnection != null)
                {
                    //Verzeichnisname übergeben
                    DataReceiver.SDCardConnection.PathToConfigFile = SourcePath + "\\CHANCONF.NMC"; //File in dem die Kunfiguration steht

                    //Konfiguration zum Zeipunkt der Aufzeichnung holen
                    if (DataReceiver.Connection is not null && DataReceiver.Connection.GetDeviceConfig())
                    {
                        if (DataReceiver.Connection.Device is not null)
                        {
                            _ModuleInfo = DataReceiver.Connection.Device.ModuleInfos;
                        }
                    }
                    else
                    {
                        OnImportInfo("Error during DeviceConfig", Color.Red);
                    }
                    DataReceiver.Close_Connection();
                }


                try
                {
                    //Datenfile öffnen
                    SDFileStream = new FileStream(SourcePath + "\\DATA.NMD",
                        FileMode.Open,
                        FileAccess.Read);
                    SDBinaryReader = new BinaryReader(SDFileStream);
                    _DataFileLength = SDBinaryReader.BaseStream.Length;
                    //Anfang suchen
                    byte b = 0xFF;
                    do
                    {
                        b = SDBinaryReader.ReadByte();
                    } while ((b & 0x80) != 0 && SDBinaryReader.BaseStream.Length - _RS232BytetoRead > SDBinaryReader.BaseStream.Position);

                    if (SDBinaryReader.BaseStream.Length - _RS232BytetoRead > SDBinaryReader.BaseStream.Position)
                    {
                        SDBinaryReader.BaseStream.Position--;
                    }
                    else
                    {
                        //No useful information found
                        OnImportInfo("No useful information found in data file", Color.Red);
                        _ModuleInfo = [];
                    }
                }
                catch (Exception ee)
                {
                    OnImportInfo("StartImport: " + ee.Message, Color.Red);
                    _ModuleInfo = [];
                }
            }

            return _ModuleInfo;
        }

        /// <summary>
        /// Stops the import, closes everything
        /// </summary>
        public void StopImport()
        {
            SDBinaryReader?.Close();
            SDFileStream?.Close();

            DataReceiver?.Close_All();
        }


        private int cntSyncPackages = 0;                        //Counts the incoming sync packages

        /// <summary>
        /// Gets the next value; Communication vals etc are ignored
        /// </summary>
        /// <returns>null if no more vales in the file</returns>
        public CDataIn_Scaled? GetNextValue()
        {
            if (SDBinaryReader is null) throw new ArgumentNullException(nameof(SDBinaryReader), "SDBinaryReader connection cannot be null."); ;

            CDataIn DataIn = new();
            isDataByte = false;
            int idxRS232inBytes = 0;
            while (!isDataByte)
            {
                //Bytes hereinholen
                bytein = SDBinaryReader.Read(RS232inBytes, idxRS232inBytes, RS232inBytes.Length - idxRS232inBytes);

                bool benoughbytesreceived = false;
                if (bytein == RS232inBytes.Length - idxRS232inBytes)
                    benoughbytesreceived = true;

                //Überprüfen ob eine auswertbare Anzahl von Bytes zur Verfügung steht
                if (benoughbytesreceived)
                {
                    bool bpacketvalid = false;
                    //16 Bit Dekodierung
                    if (CInsightDataEnDecoder.Parse4Byte(RS232inBytes, ref DataIn))
                    {
                        if (DataIn.HW_cn == C8KanalReceiverCommandCodes.cCommandChannelNo)
                        {
                            isDataByte = false;

                            //Kommando kommt herein
                            byte[] buf = new byte[DataIn.Value];        //Array fuer die kommenden bytes inkl CRC

                            //Die fehlenden bytes holen
                            int rs232ReadBytes = SDBinaryReader.Read(buf, 0, buf.Length);
                            if ((buf.Length == rs232ReadBytes) && (rs232ReadBytes > 0))
                            {
                                //CRC check
                                if (CRC8.Check_CRC8(ref buf, 0, true, true))    //Last Byte is CRC, remove CRC
                                {
                                    //Check for commands that must be processed here
                                    switch (buf[0])
                                    {
                                        case C8KanalReceiverCommandCodes.cChannelSync:
                                            {
                                                if (LastSyncSignal == DateTime.MinValue)
                                                {
                                                    LastSyncSignal = Recording_Start_Time_from_File;
                                                    cntSyncPackages = 0;
                                                }
                                                else
                                                {
                                                    LastSyncSignal += SyncInterval;
                                                    cntSyncPackages++;
                                                }
                                                break;
                                            }
                                        case C8KanalReceiverCommandCodes.cDeviceAlive:
                                            {
                                                OnImportInfo("Alive packet detected", Color.Black);
                                                break;
                                            }
                                        case C8KanalReceiverCommandCodes.cNeuromasterToPC:
                                            {
                                                OnImportInfo("cNeuromasterToPC", Color.Black);
                                                break;
                                            }
                                        default:
                                            {
                                                OnImportInfo("default", Color.Black);
                                                break;
                                            }
                                    }
                                    bpacketvalid = true;
                                } // (CRC8.Check_CRC8(ref buf, 0, true, true))    //Last Byte is CRC, remove CRC
                                else
                                {
                                    //CRC failed
                                    OnImportInfo("CRC failed", Color.Black);
                                }
                            } //if ((buf.Length == rs232ReadBytes) && (rs232ReadBytes > 0))
                        }// if ((DataIn.HWChannelNumber == C8KanalReceiverCommandCodes.cCommandChannelNo))

                        else
                        {
                            //Überprüfen ob Daten plausibel
                            if (DataIn.HW_cn < ModuleInfo.Count)
                            {
                                if (ModuleInfo[DataIn.HW_cn].IsModuleActive())
                                {
                                    if (DataIn.SW_cn < ModuleInfo[DataIn.HW_cn].NumSWChannels)
                                    {
                                        if (ModuleInfo[DataIn.HW_cn].SWChannels[DataIn.SW_cn].SaveChannel)
                                        {
                                            bpacketvalid = true;
                                            isDataByte = true;
                                            //Sync Sequence requires 5th byte to be received
                                            if (DataIn.SyncFlag == 1)
                                            {
                                                byte[] sync = new byte[1];
                                                if (!Sync_Packet_has_4_bytes)
                                                    SDBinaryReader.Read(sync, 0, 1);

                                                sync[0] = (byte)(sync[0] & 0x7f);
                                                if (DataIn.Sync7 == 1)
                                                    sync[0] += 128;
                                                DataIn.SyncVal = sync[0];


                                                if (DataReceiver != null && DataReceiver.Connection != null && DataReceiver.Connection.Device != null)
                                                {
                                                    DataReceiver.Connection.Device.ModuleInfos[DataIn.HW_cn].SWChannels[DataIn.SW_cn].SWChan_Started = Recording_Start_Time_from_File;
                                                }

                                                ModuleInfo[DataIn.HW_cn].SWChannels[DataIn.SW_cn].SynPackagesreceived = cntSyncPackages;
                                            }

                                            if (DataIn.EP == 1)
                                            {
                                                //24 or 32 bit value, receive at least 2 more bytes
                                                byte[] bt2 = new byte[2];
                                                if (SDBinaryReader.Read(bt2, 0, 2) == 2)
                                                {
                                                    if ((bt2[0] & 0x60) != 0) //0b0110 0000
                                                    {
                                                        //Todo ... 32bit or more transmission
                                                    }
                                                    else
                                                    {
                                                        int hb = (bt2[0] & 0x7F) << 6;
                                                        int temp = (bt2[1] >> 1) & 0x3F;   //0b0011 1111
                                                        hb |= temp;
                                                        hb <<= 16;
                                                        DataIn.Value = hb | DataIn.Value;
                                                    }
                                                }
                                                else
                                                {
                                                    //intDataArray[DataAdded].Value += 8;
                                                    //Data not valid
                                                }
                                            }


                                            //Daten akzeptieren
                                            if (isDataByte)
                                            {
                                                DataIn.LastSync = LastSyncSignal;
                                                try
                                                {
                                                    //Daten sind gültig

                                                    if (DataReceiver != null && DataReceiver.Connection != null && DataReceiver.Connection.Device != null)
                                                        DataReceiver.Connection.Device.ModuleInfos[DataIn.HW_cn].SWChannels[DataIn.SW_cn].UpdateTime(ref DataIn);
                                                    if (DataIn.DT_relative == DateTime.MinValue)
                                                    {
                                                        //Ausblenden der ersten Daten bis Sync kommt
                                                        isDataByte = false;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    isDataByte = false;
                                                }
                                            }

                                            idxRS232inBytes = 0;
                                        }
                                    }
                                }
                            }
                        }
                    } //if (CDecodeBytes.Decode4Byte(bh2, bh1, bh0, bl, ref DataIn))

                    if (!bpacketvalid)
                    {
                        //Decode failed, Ein byte weiter schieben
                        idxRS232inBytes = 3;

                        OnImportInfo("Byte not valid" + CMyConvert.ByteArrayto_HexString(RS232inBytes, false), Color.Black);

                        //Verbliebene Daten an den Anfang des puffers verschieben
                        Array.Copy(RS232inBytes, 1, RS232inBytes, 0, idxRS232inBytes);

                    }
                } //if (benoughbytesreceived)
                else
                {
                    //End of file
                    StopImport();
                    return null;
                }
            }//while (!isDataByte)

            double v = 0;
            if (DataReceiver != null && DataReceiver.Connection != null)
                v = DataReceiver.Connection.GetScaledValue(DataIn);

            CDataIn_Scaled cds = new(DataIn)
            {
                Value_Scaled = v
            };

            return cds;
        }

    }
}
