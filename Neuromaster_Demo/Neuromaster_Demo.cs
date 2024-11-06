using BMTCommunicationLib;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using MathNetNuget;
using Neuromaster_Demo_Library_Reduced__netx;
using SkiaSharp;
using WindControlLib;
using static FeedbackDataLib.C8KanalReceiverV2;

namespace Neuromaster_Demo
{
    /// <summary>
    /// Neuromaster Demo Class
    /// </summary>
    /// <remarks>
    /// tmrStatusMessages is used to bring Status Messages on screen - to avoid locking of GUI in some cases and to avoid llegalCrossThreadCalls exceptions
    /// Additionally tmrStatusMessages checks the state of the connection to Neuromaster - mainly implemented due to TÜV requirements
    /// </remarks>
    public partial class Neuromaster_Demo : Form
    {

        /// <summary>
        /// Class to connect to Neuromaster
        /// </summary>
        private FeedbackDataLib.C8KanalReceiverV2 DataReceiver;

        /// <summary>
        /// Locking property
        /// </summary>
        private bool DontReconnectOntbConnect_ToState1 = false;

        /// <summary>
        /// Saves recent Connection status
        /// </summary>
        private EnumConnectionStatus OldConnection_Status = EnumConnectionStatus.Not_Connected;

        /// <summary>
        /// Result of the last attempt to connect to Neuromaster
        /// </summary>
        private EnumConnectionResult LastConnectionResult = EnumConnectionResult.No_Active_Neurolink;

        /// <summary>
        /// Ringpuffer for Status Messages (used from tmrStatusMessages)
        /// </summary>
        private CRingpuffer StatusMessages = new CRingpuffer(100);

        /// <summary>
        /// Index of the currently selected module
        /// </summary>
        private int idx_SelectedModule;

        /// <summary>
        /// Buckup of the Index of the currently selected module
        /// </summary>
        private int BU_idx_SelectedModule = -1;

        /// <summary>
        /// Flag for USB disconnection / reconnection
        /// </summary>
        private bool USB_Reconnected = false;

        //just because it might bee needed
        private CSDCardInfo SDCardInfo = new CSDCardInfo();



        /// <summary>
        /// Initializes a new instance of the <see cref="Neuromaster_Demo"/> class.
        /// </summary>
        public Neuromaster_Demo()
        {
            InitializeComponent();
            this.SuspendLayout();
        }

        CHRV HRV = new ();

        /// <summary>
        /// Handles the Load event of the NeuromasterV2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NeuromasterV2_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the FormClosing event of the NeuromasterV2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void NeuromasterV2_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrStatusMessages.Enabled = false;
            GoDisconnected();
        }

        /// <summary>
        /// Initiates Connection to Neuromaster either via USB or XBEE connection
        /// </summary>
        private void StartConnection()
        {
            DontReconnectOntbConnect_ToState1 = true;
            AddStatusString("Searching for Neurolink  ....");

            // Create Receiver if it does not exist
            DataReceiver ??= new C8KanalReceiverV2();

            EnumConnectionResult? conres;  //Connection Result

            conres = DataReceiver.Init_via_D2XX();      //Init via FTDI D2XX Driver (faster); checks for Neurolink but not yet for NEUROMASTER!!
            //conres = Neuromaster_Connection.Init_via_VirtualCom();  //Init via RS232 emulation(slower)

            if ((conres == EnumConnectionResult.Connected_via_USBCable) ||
                conres == EnumConnectionResult.Connected_via_XBee)
            {

                //Neurolink is detected and initialised 
                AddStatusString("Neurolink: " + DataReceiver.NeurolinkSerialNumber, Color.Blue);  //Display Neurolink serial number

                //!!!! Now Connect - can take a while if Neurolink is connected via XBEE for the first time
                LastConnectionResult = DataReceiver.Connect();

                switch (LastConnectionResult)
                {
                    case EnumConnectionResult.Connected_via_XBee:
                        {
                            AddStatusString("XBee Connection found: " + DataReceiver.PortName, Color.Green);
                            AddEvents();    //Attach events
                            break;
                        }
                    case EnumConnectionResult.Connected_via_USBCable:
                        {
                            AddStatusString("USB cable connection found: " + DataReceiver.PortName, Color.Green);
                            AddEvents();    //Attach events
                            break;
                        }
                    case EnumConnectionResult.Error_during_Port_scan:
                        {
                            AddStatusString("Error during Port scan.", Color.Red);
                            break;
                        }
                    case EnumConnectionResult.Error_during_USBcable_connection:
                        {
                            AddStatusString("Error_during_USBcable_connection", Color.Orange);
                            break;
                        }
                    case EnumConnectionResult.Error_during_XBee_connection:
                        {
                            AddStatusString("Error_during_XBee_connection", Color.Orange);
                            break;
                        }
                    case EnumConnectionResult.More_than_one_Neurolink_detected:
                        {
                            AddStatusString("Please connect only one Neurolink", Color.Orange);
                            break;
                        }
                    case EnumConnectionResult.No_Active_Neurolink:
                        {
                            AddStatusString("No active Neurolink found.", Color.Orange);
                            break;
                        }
                    case EnumConnectionResult.Error_read_ErrorString:
                        {
                            AddStatusString(DataReceiver.LastErrorString, Color.Orange);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            else
            {
                //Possible Errors if no Neurolink is detected
                switch (conres)
                {
                    case EnumConnectionResult.Error_during_Port_scan:
                        {
                            AddStatusString("Error during Port scan.", Color.Red);
                            break;
                        }
                    case EnumConnectionResult.More_than_one_Neurolink_detected:
                        {
                            AddStatusString("Please connect only one Neurolink", Color.Orange);
                            break;
                        }
                    case EnumConnectionResult.No_Active_Neurolink:
                        {
                            AddStatusString("No active Neurolink found.", Color.Orange);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Attaches events 
        /// </summary>
        private void AddEvents()
        {
            //Event for Data
            if (DataReceiver?.Connection is not null)
            {
                DataReceiver.Connection.DataReady -= DataReceiver_Connection_DataReady;
                DataReceiver.Connection.DataReady += new BMTCommunicationLib.DataReadyEventHandler(DataReceiver_Connection_DataReady);

                //Event to inform PC about Battery Status
                DataReceiver.Connection.DeviceToPC_BatteryStatus -= Connection_DeviceToPC_BatteryStatus;
                DataReceiver.Connection.DeviceToPC_BatteryStatus += new C8KanalReceiverV2_CommBase.DeviceToPC_BatteryStatusEventHandler(Connection_DeviceToPC_BatteryStatus);

                //Buffer in Neuromaster is full
                DataReceiver.Connection.DeviceToPC_BufferFull -= Connection_DeviceToPC_BufferFull;
                DataReceiver.Connection.DeviceToPC_BufferFull += new C8KanalReceiverV2_CommBase.DeviceToPC_BufferFullEventHAndler(Connection_DeviceToPC_BufferFull);

                //Error in Neuromodule occured - for future use
                DataReceiver.Connection.DeviceToPC_ModuleError -= Connection_DeviceToPC_ModuleError;
                DataReceiver.Connection.DeviceToPC_ModuleError += new C8KanalReceiverV2_CommBase.DeviceToPC_ModuleErrorEventHandler(Connection_DeviceToPC_ModuleError);

                //Messages from Command processing
                DataReceiver.Connection.CommandProcessedResponse += Connection_CommandProcessedResponse;

                //Events for Device Connection / Disconnection
                DataReceiver.DeviceConnected -= DataReceiver_Connection_DeviceConnected;
                DataReceiver.DeviceConnected += new C8KanalReceiverV2.DeviceConnectedEventHandler(DataReceiver_Connection_DeviceConnected);

                DataReceiver.DeviceDisconnected -= DataReceiver_Connection_DeviceDisconnected;
                DataReceiver.DeviceDisconnected += new C8KanalReceiverV2.DeviceDisconnectedEventHandler(DataReceiver_Connection_DeviceDisconnected);
            }
            
        }

        #region DeviceCommunicationToPC_Event

        /// <summary>
        /// An Error occured in on Module
        /// </summary>
        /// <remarks>
        /// Currently not implemented
        /// </remarks>
        /// <param name="HW_cn">Hardware Channel number of the Module</param>
        void Connection_DeviceToPC_ModuleError(byte HW_cn)
        {
            //For future use
            throw new NotImplementedException();
        }

        /// <summary>
        /// Buffer is full in Neuromaster
        /// </summary>
        /// <remarks>
        /// Neuromaster stops sampling - must be reconfigured
        /// </remarks>
        void Connection_DeviceToPC_BufferFull()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(Connection_DeviceToPC_BufferFull));
            }
            else
            {
                AddStatusString("Neuromaster: Transmit Buffer is full", Color.Blue);
            }
        }

        /// <summary>
        /// Neuromaster Battery Satus
        /// </summary>
        /// <param name="Voltage_mV">Battery Voltage [mV]</param>
        /// <param name="percentage">Percentage of Battery Capacity</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void Connection_DeviceToPC_BatteryStatus(uint Battery_Voltage_mV, uint percentage, uint Supply_Voltage_mV)
        {
            //Implemented in last hardware Version ... not yet tested
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<uint, uint, uint>(Connection_DeviceToPC_BatteryStatus), Battery_Voltage_mV, percentage, Supply_Voltage_mV);
            }
            else
            {
                AddStatusString("Battery Status: " + percentage.ToString() + "%", Color.Blue);
            }

        }

        #endregion

        #region DataReady_Event
        /// <summary>
        /// Data is comming in
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="DataChannel">There can be more than one Datapoint in the list</param>
        void DataReceiver_Connection_DataReady(object sender, List<WindControlLib.CDataIn> DataChannel)
        {
            try //Because of some issues when closing the app
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<object, List<WindControlLib.CDataIn>>(DataReceiver_Connection_DataReady), sender, DataChannel);
                }
                else
                {
                    foreach (CDataIn ci in DataChannel)
                    {
                        CYvsTimeData c = new CYvsTimeData(1);   //Just another data structure

                        c.xData = ci.DTAbsolute;

                        c.yData[0] = DataReceiver.Connection.GetScaledValue(ci);

                        //Display value ... just that something happens in the GUI
                        string s = c.xData.ToString() + "\t" + c.yData[0].ToString() + "\t" + ci.HWcn.ToString() + "\t" + ci.SWcn.ToString() + Environment.NewLine;
                        txtData.AppendText(s);
                        if (TCP_Interface != null)
                            TCP_Interface.Write(ci);
                    }
                }
            }
            catch
            {
            }
        }


        #endregion


        #region Organise_GUI

        /// <summary>
        /// Disconnect from Neuromaster
        /// </summary>
        private void tbConnect_ToState1(object sender, EventArgs e)
        {
            GoDisconnected();
            DontReconnectOntbConnect_ToState1 = true;
        }

        /// <summary>
        /// Connect to Device 
        /// </summary>
        private void tbConnect_ToState2(object sender, EventArgs e)
        {
            tbConnect.AcceptChange = false;
            StartConnection();  //Lets start .... 
        }

        /// <summary>
        /// What to do if connection is established
        /// </summary>
        private void GoConnected()
        {
            tbConnect.GoToState2(false);    //Just set Button to Green
        }

        /// <summary>
        /// What to do if connection is disconnected
        /// </summary>
        private void GoDisconnected()
        {
            tbConnect.GoToState1(false);    //Just set Button to Red
            DataReceiver?.Close_All();
        }

        #endregion

        /// <summary>
        /// Handles the Tick event of the tmrStatusMessages control.
        /// </summary>
        private void tmrStatusMessages_Tick(object sender, EventArgs e)
        {
            while (StatusMessages.StoredObjects > 0)
            {
                //Output status Messages to Screen
                CTextCol tc = (CTextCol)StatusMessages.Pop();
                txtStatus.AppendText(Environment.NewLine);
                txtStatus.SelectionColor = tc.col;
                txtStatus.AppendText(tc.text);
            }

            CheckConnectionStatus();
        }

        #region Communication_with_Neuromaster
        /// <summary>
        /// Checks the connection status.
        /// Was implemented this way due to TÜV testing ... can be simplified?
        /// </summary>
        private void CheckConnectionStatus()
        {
            if ((DataReceiver != null) && (DataReceiver.Connection != null))
            {
                //Update XBEE signal strength
                int b = DataReceiver.RSSI_percent;

                if (b > pbXBEESignalStrength.Maximum) b = pbXBEESignalStrength.Maximum;
                pbXBEESignalStrength.Value = b;

                if (OldConnection_Status != DataReceiver.ConnectionStatus)   //Status changed?
                {
                    OldConnection_Status = DataReceiver.ConnectionStatus;

                    switch (OldConnection_Status)
                    {
                        case EnumConnectionStatus.Connected:
                            {
                                //Connection has (re-) appeard
                                AddStatusString("Connected", Color.Green);
                                GoConnected();

                                //If USB connection crashed - restore configuration on the fly (see Disconnect())
                                if (USB_Reconnected)
                                {
                                    USB_Reconnected = false;

                                    //Reload old configuration
                                    DataReceiver.Connection.Device.ModuleInfos = new List<CModuleBase>(BU_ModuleInfo);
                                    idx_SelectedModule = BU_idx_SelectedModule;

                                    DataReceiver.Connection.EnableDataReadyEvent = true;

                                    RestoreOldConfiguration();
                                    btSetAllConfig.PerformClick();  //Send entire configuration to Neuromaster
                                }
                                break;
                            }
                        case EnumConnectionStatus.Connecting:
                            {
                                AddStatusString("Connecting", Color.Green);
                                break;
                            }
                        case EnumConnectionStatus.Not_Connected:
                            {
                                AddStatusString("Not Connected", Color.Red);
                                GoDisconnected();
                                break;
                            }
                        case EnumConnectionStatus.Dis_Connected:
                            {
                                AddStatusString("Dis-Connected", Color.Red);
                                GoDisconnected();

                                if (!DontReconnectOntbConnect_ToState1)
                                {
                                    //Start new connection
                                    DataReceiver.Connection.Connect_via_tryToConnectWorker();   //Starts own thread for Connection retries 
                                }
                                break;
                            }
                        case EnumConnectionStatus.No_Data_Link:
                            {
                                AddStatusString("No Data Link", Color.Red);
                                break;
                            }
                        case EnumConnectionStatus.PortError:
                            {
                                AddStatusString("Cannot open COM - restart", Color.Red);
                                break;
                            }
                        case EnumConnectionStatus.USB_disconnected:
                            {
                                break;
                            }
                        case EnumConnectionStatus.USB_reconnected:
                            {
                                break;
                            }
                    }
                }
            }
        }



        /// <summary>
        /// Holds Backup of Module-configuration
        /// </summary>
        List<CModuleBase> BU_ModuleInfo = new List<CModuleBase>();

        /// <summary>
        /// Read the current Module-configuration (which modules are connected) from Neuromaster
        /// </summary>
        private void btGetConfigModules_Click(object sender, EventArgs e)
        {
            //Put both into async execution queu, results via Connection_CommandProcessedResponse
            DataReceiver?.Connection?.ScanModules();
            DataReceiver?.Connection?.GetDeviceConfig();
        }


        /// <summary>
        /// Handles the Click event of the btSetConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btSetConfig_Click(object sender, EventArgs e)
        {
            //Get the Hardware Channel number of the currently selected Module
            int HW_cn = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HWcn;
            DataReceiver.Connection.Device.ModuleInfos[HW_cn] = cChannelsControlV2x11.GetModuleInfo(HW_cn);

            if (DataReceiver.Connection.SetConfigModule(HW_cn)) //Set the configuration
            {
                AddStatusString("Config set: " + HW_cn.ToString(), Color.Green);
                pbXBeeChannelCapacity.Value = DataReceiver.Connection.GetChannelCapcity();  //Calculate channel capacity with the new setting
                lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();  //Display
            }
            else
                AddStatusString("Config not set" + HW_cn.ToString(), Color.Red);    //Error
        }


        /// <summary>
        /// Handles the Click event of the btSetAllConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtSetAllConfig_Click(object sender, EventArgs e)
        {
            DataReceiver?.Connection?.SetConfigAllModules(); //Sets Configuration of all Channels
        }


        /// <summary>
        /// Handles the Click event of the btGetClock control.
        /// </summary>
        /// <remarks>
        /// Reads the Clock in Neuromaster
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtGetClock_Click(object sender, EventArgs e)
        {
            DataReceiver?.Connection?.GetClock();
        }

        /// <summary>
        /// Handles the Click event of the btSetClock control.
        /// </summary>
        /// <remarks>
        /// Sets clock in Neuromaster
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtSetClock_Click(object sender, EventArgs e)
        {
            DataReceiver?.Connection?.SetClock(DateTime.Now); //Set clock to PC time
        }

        /*******************************************************************************/
        /************************Feedback from the Communication ***********************/
        /*******************************************************************************/
        private void Connection_CommandProcessedResponse(object? sender, C8KanalReceiverV2_CommBase.CommandProcessedResponseEventArgs e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.Command.ToString() + ": " + e.Message, e.MessageColor);
                if (IsDeviceAvailable())
                {
                    switch (e.Command)
                    {
                        case CRS232Receiver2.EnNeuromasterCommand.GetFirmwareVersion:
                            CNMFirmwareVersion NMFirmwareVersion = new();
                            NMFirmwareVersion.UpdateFrom_ByteArray(e.ResponseData, 0);
                            AddStatusString("NM UID: " + NMFirmwareVersion.Uuid, Color.DarkOliveGreen);
                            AddStatusString("NM HW Version: " + NMFirmwareVersion.HWVersionString, Color.DarkOliveGreen);
                            AddStatusString("NM SW Version: " + NMFirmwareVersion.SWVersionString, Color.DarkOliveGreen);
                            break;

                        case CRS232Receiver2.EnNeuromasterCommand.GetSDCardInfo:
                            SDCardInfo.UpdateFrom_ByteArray(e.ResponseData, 0);
                            break;
                        case CRS232Receiver2.EnNeuromasterCommand.ScanModules:
                            break;
                        case CRS232Receiver2.EnNeuromasterCommand.GetDeviceConfig:
                            if (e.Success)
                                {
                                    DataReceiver!.Connection!.EnableDataReadyEvent = true;

                                    /* Display information about Modules in cChannelsControlV2x11
                                     * cChannelsControlV2x11 directly changes values in DataReceiver.Connection.Device.ModuleInfos
                                     */
                                    cChannelsControlV2x11.SetModuleInfos(DataReceiver!.Connection!.Device!.ModuleInfos);
                                    cChannelsControlV2x11.Refresh();
                                }
                            break;
                        case CRS232Receiver2.EnNeuromasterCommand.SetConfigAllModules:
                            if (e.Success)
                                {
                                    pbXBeeChannelCapacity.Value = DataReceiver?.Connection?.GetChannelCapcity() ?? 0;  //Calculate channel capacity with the new setting
                                    lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();  //Display
                                }
                            break;
                        case CRS232Receiver2.EnNeuromasterCommand.GetModuleInfoSpecific:
                            {
                                //Ok
                                AddStatusString("Module specific read OK", Color.Green);
                                if (IsDeviceAvailable())
                                {
                                    cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection!.Device!.ModuleInfos[e.HWcn]);
                                    cChannelsControlV2x11.Refresh();


                                    byte[] buf = DataReceiver.Connection.Device.ModuleInfos[e.HWcn].GetModuleSpecific();
                                    string s = "Reci: ";
                                    for (int i = 0; i < buf.Length; i++)
                                    {
                                        s += buf[i].ToString() + ", ";
                                    }
                                    AddStatusString(s, Color.Blue);
                                }
                            }
                            break;
                        case CRS232Receiver2.EnNeuromasterCommand.GetClock:
                            txtTime.Text = DataReceiver!.Connection!.DeviceClock.Dt.ToString();
                            break;
                    }
                }
            });
        }

        #endregion


        /// <summary>
        /// In case relative time differs from the absolute time
        /// </summary>
        private void btResync_Click(object sender, EventArgs e)
        {
            DataReceiver?.Connection?.Device?.Resync();
        }


        #region USB_Connect_Disconnect

        /// <summary>
        /// Neurolinks USB cable is suddenly disconnected
        /// </summary>
        /// <remarks>
        /// USB port is monitored in the background if cable is plugged in again
        /// </remarks>
        /// <param name="PID_VID">ProductID & VendorID of the disconnected device</param>
        private void DataReceiver_Connection_DeviceDisconnected(string PID_VID)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(DataReceiver_Connection_DeviceDisconnected), PID_VID);
            }
            else
            {
                AddStatusString("USB Device " + PID_VID + " disconnected", Color.Red);
                //Only close Connection, USB Monitoring Stays On
                Disconnect();
            }
        }

        /// <summary>
        /// Neuromaster_s the connection_ device connected.
        /// </summary>
        /// <param name="ConnectionResult">The connection result.</param>
        private void DataReceiver_Connection_DeviceConnected(EnumConnectionResult ConnectionResult)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<EnumConnectionResult>(DataReceiver_Connection_DeviceConnected), ConnectionResult);
            }
            else
            {
                AddStatusString("USB Device: " + ConnectionResult, Color.Green);
                Reconnect();
            }
        }

        /// <summary>
        /// Things to do when device is disconnected
        /// </summary>
        void Disconnect()
        {
            tbConnect.Enabled = false;  //Just to make GUI save

            //Backup current configuration
            if (DataReceiver.Connection.Device.ModuleInfos != null)
            {
                BU_ModuleInfo = new List<CModuleBase>(DataReceiver.Connection.Device.ModuleInfos);
            }
            BU_idx_SelectedModule = idx_SelectedModule;
            DataReceiver.Close_Connection(); //Close Background threads responsible for receiving
        }

        /// <summary>
        /// Things to do when device reappears
        /// </summary>
        private void Reconnect()
        {
            StartConnection();          //Reestablish connection - restore backup
            tbConnect.Enabled = true;   //Just to make GUI save
            USB_Reconnected = true;
        }


        #endregion

        #region AddStatusString
        /*
         * Routines to display status messages
         */
        private void AddStatusString(string text)
        {
            AddStatusString(text, Color.Black);
        }

        private void AddStatusString(string text, Color Col)
        {
            AddStatusString(txtStatus, text, Col);
        }

        private void AddStatusString(RichTextBox txtStatus, string text, Color Col)
        {
            CTextCol tc = new CTextCol(text, Col);
            StatusMessages.Push(tc);
        }

        private class CTextCol
        {
            public string text = "";
            public Color col = Color.Black;

            public CTextCol(string text, Color Col)
            {
                this.text = text;
                this.col = Col;
            }
        }

        #endregion

        #region Helper_Functions_only_of_value_for_this_Application
        private void RunOnUiThread(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        private bool IsDeviceAvailable()
        {
            if (DataReceiver is null) return false;
            if (DataReceiver.Connection is null) return false;
            if (DataReceiver.Connection.Device is null) return false;
            return true;
        }


        /// <summary>
        /// Restores previous configuration from BU_ModuleInfo
        /// </summary>
        /// <remarks>
        /// Restores only Neuromodule types are equal
        /// </remarks>
        private void RestoreOldConfiguration()
        {
            if ((BU_ModuleInfo != null) && (BU_ModuleInfo.Count > 0) && IsDeviceAvailable())
            {
                //Check if possible
                for (int HW_cn = 0; HW_cn < DataReceiver.Connection!.Device!.ModuleInfos.Count; HW_cn++)
                {
                    if ((DataReceiver.Connection.Device.ModuleInfos[HW_cn].ModuleType == BU_ModuleInfo[HW_cn].ModuleType) &&
                        (DataReceiver.Connection.Device.ModuleInfos[HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty))
                    {
                        //Only if Module Types are equal
                        for (int SW_cn = 0; SW_cn < DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels.Count; SW_cn++)
                        {
                            DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SampleInt =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SampleInt;

                            DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SaveChannel =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SaveChannel;

                            DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[SW_cn].SendChannel =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SendChannel;
                        }
                        AddStatusString("HW channel " + HW_cn.ToString() + " updated", Color.Green);
                    }
                }
            }
        }

        /// <summary>
        /// Another Neuromodule was selected in cChannelsControlV2x11
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="ModuleInfo">The module information.</param>
        private void cChannelsControlV2x11_ModuleRowChanged(object sender, CModuleBase ModuleInfo)
        {
            if (IsDeviceAvailable())
                for (int i = 0; i < DataReceiver!.Connection!.Device!.ModuleInfos.Count; i++)
                {
                    //Find index of the recently selected Module
                    if (DataReceiver.Connection.Device.ModuleInfos[i].HWcn == ModuleInfo.HWcn)
                    {
                        idx_SelectedModule = i;
                        break;
                    }
                }
        }
        #endregion

        private void btGetModuleSpecific_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;
            
            int HWcn = DataReceiver!.Connection!.Device!.ModuleInfos[idx_SelectedModule].HWcn;

            if (DataReceiver!.Connection.Device.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {

                DataReceiver.Connection.GetModuleInfoSpecific(HWcn, true);


            }
            else
            {
                AddStatusString("Module specific read only possible if module is active", Color.Red);
            }
        }

        private void btSetModuleSpecific_Click(object sender, EventArgs e)
        {
            int HW_cn = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HWcn;

            if (DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {
                byte[] buf = cChannelsControlV2x11.GetModuleSpecific(HW_cn);
                DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].SetModuleSpecific(buf);

                if (DataReceiver.Connection.SetModuleInfoSpecific(HW_cn))
                {
                    AddStatusString("Module specific write OK", Color.Green);
                    string s = "Sent: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString() + ", ";
                    }
                    AddStatusString(s, Color.Orange);

                    Application.DoEvents();
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection.Device.ModuleInfos[HW_cn]);
                    cChannelsControlV2x11.Refresh();

                    buf = DataReceiver.Connection.Device.ModuleInfos[HW_cn].GetModuleSpecific();
                    s = "Reci: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString() + ", ";
                    }
                    AddStatusString(s, Color.Blue);

                }
                else
                { AddStatusString("Module specific write failed", Color.Red); }
            }
            else
            {
                AddStatusString("Module specific write only possible if module is active", Color.Red);
            }
        }

        cTCP_Interface TCP_Interface;
        private void btOpenTCP_Click(object sender, EventArgs e)
        {
            TCP_Interface ??= new cTCP_Interface();
            TCP_Interface.StatusMessage += TCP_Interface_StatusMessage;
            TCP_Interface.Init();
        }

        private void TCP_Interface_StatusMessage(string data, Color color)
        {
            AddStatusString(data, color);
        }

        private void btNMInfo_Click(object sender, EventArgs e)
        {
            DataReceiver?.Connection?.GetNMFirmwareVersion();
        }
    }
}
