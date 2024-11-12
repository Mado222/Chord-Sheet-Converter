using BMTCommunicationLib;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using MathNetNuget;
using Neuromaster_Demo_Library_Reduced__netx;
using WindControlLib;
using static FeedbackDataLib.C8Receiver;

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
        private C8Receiver? c8Receiver;

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
        private readonly CFifoBuffer<ColoredText> StatusMessages = new ();

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

        private CTCPInterface? TCP_Interface;



        /// <summary>
        /// Initializes a new instance of the <see cref="Neuromaster_Demo"/> class.
        /// </summary>
        public Neuromaster_Demo()
        {
            InitializeComponent();
            SuspendLayout();
        }

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
        private async void StartConnection()
        {
            DontReconnectOntbConnect_ToState1 = true;
            AddStatusString("Searching for Neurolink ....");

            await Task.Run(() =>
            {
                // Create Receiver if it does not exist
                c8Receiver ??= new C8Receiver();

                EnumConnectionResult? conres; // Connection Result

                // Perform initialization and connection in the background thread
                conres = c8Receiver.Init_via_D2XX(); // Init via FTDI D2XX Driver (faster)

                if (conres == EnumConnectionResult.Connected_via_USBCable ||
                    conres == EnumConnectionResult.Connected_via_XBee)
                {
                    // Neurolink is detected and initialized
                    Invoke(() => AddStatusString("Neurolink: " + c8Receiver.NeurolinkSerialNumber, Color.Blue));
                }
                // Update GUI based on connection results
                // Possible errors if no Neurolink is detected
                Invoke(() =>
                {
                    switch (conres)
                    {
                        case EnumConnectionResult.Connected_via_XBee:
                            AddStatusString("XBee Connection found: " + c8Receiver.PortName, Color.Green);
                            AddEvents(); // Attach events
                            break;
                        case EnumConnectionResult.Connected_via_USBCable:
                            AddStatusString("USB cable connection found: " + c8Receiver.PortName, Color.Green);
                            AddEvents(); // Attach events
                            break;

                        // Possible errors if no Neurolink is detected
                        case EnumConnectionResult.Error_during_Port_scan:
                            AddStatusString("Error during Port scan.", Color.Red);
                            break;
                        case EnumConnectionResult.More_than_one_Neurolink_detected:
                            AddStatusString("Please connect only one Neurolink", Color.Orange);
                            break;
                        case EnumConnectionResult.No_Active_Neurolink:
                            AddStatusString("No active Neurolink found.", Color.Orange);
                            break;
                        default:
                            AddStatusString("Unknown error occurred.", Color.Orange);
                            break;
                    }
                });

            });
        }


        /// <summary>
        /// Attaches events 
        /// </summary>
        private void AddEvents()
        {
            //Event for Data
            if (c8Receiver?.Connection is not null)
            {
                c8Receiver.Connection.DataReadyResponse -= Connection_DataReadyResponse;
                c8Receiver.Connection.DataReadyResponse += Connection_DataReadyResponse; ;

                //Event to inform PC about Battery Status
                c8Receiver.Connection.DeviceToPC_BatteryStatus -= Connection_DeviceToPC_BatteryStatus;
                c8Receiver.Connection.DeviceToPC_BatteryStatus += Connection_DeviceToPC_BatteryStatus; ;

                //Buffer in Neuromaster is full
                c8Receiver.Connection.DeviceToPC_BufferFull -= Connection_DeviceToPC_BufferFull;
                c8Receiver.Connection.DeviceToPC_BufferFull += Connection_DeviceToPC_BufferFull;

                //Error in Neuromodule occured - for future use
                //DataReceiver.Connection.DeviceToPC_ModuleError -= Connection_DeviceToPC_ModuleError;
                //DataReceiver.Connection.DeviceToPC_ModuleError += Connection_DeviceToPC_ModuleError;

                c8Receiver.Connection.GetClockResponse += Connection_GetClockResponse;
                c8Receiver.Connection.SetConnectionClosedResponse += Connection_SetConnectionClosedResponse;
                c8Receiver.Connection.GetDeviceConfigResponse += Connection_GetDeviceConfigResponse;
                c8Receiver.Connection.GetFirmwareVersionResponse += Connection_GetFirmwareVersionResponse;

                //Events for Device Connection / Disconnection
                c8Receiver.DeviceConnected -= DataReceiver_DeviceConnected;
                c8Receiver.DeviceConnected += DataReceiver_DeviceConnected;

                c8Receiver.DeviceDisconnected -= DataReceiver_DeviceDisconnected;
                c8Receiver.DeviceDisconnected += DataReceiver_DeviceDisconnected; ;
            }
            
        }

        private void Connection_GetFirmwareVersionResponse(object? sender, (CNMFirmwareVersion? FWVersion, ColoredText msg) e)
        {
            if (e.FWVersion != null)
                AddStatusString(e.FWVersion.GetFWVersionString(), Color.Blue);
            AddStatusString(e.msg.Text, e.msg.Color);
        }

        private void Connection_GetDeviceConfigResponse(object? sender, (List<CModuleBase>? cmb, ColoredText msg) e)
        {
            throw new NotImplementedException();
        }

        private void Connection_SetConnectionClosedResponse(object? sender, (bool isClosed, ColoredText msg) e)
        {
            throw new NotImplementedException();
        }

        private void Connection_GetClockResponse(object? sender, (DateTime? clock, ColoredText msg) e)
        {
            throw new NotImplementedException();
        }

        #region DeviceCommunicationToPC_Event

        /// <summary>
        /// An Error occured in on Module
        /// </summary>
        /// <remarks>
        /// Currently not implemented
        /// </remarks>
        /// <param name="HW_cn">Hardware Channel number of the Module</param>
        private void Connection_DeviceToPC_ModuleError(object? sender, byte e)
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
        private void Connection_DeviceToPC_BufferFull(object? sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString("Neuromaster: Transmit Buffer is full", Color.Blue);
            });
        }

        /// <summary>
        /// Neuromaster Battery Satus
        /// </summary>
        /// <param name="Voltage_mV">Battery Voltage [mV]</param>
        /// <param name="percentage">Percentage of Battery Capacity</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Connection_DeviceToPC_BatteryStatus(object? sender, (uint BatteryVoltageMV, uint Percentage, uint SupplyVoltageMV) e)
        {
            // Implemented in last hardware version, not yet tested
            RunOnUiThread(() => AddStatusString($"Battery Status: {e.Percentage}%", Color.Blue));
        }


    #endregion

    #region DataReady_Event
    /// <summary>
    /// Data is comming in
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="DataChannel">There can be more than one Datapoint in the list</param>
    private void Connection_DataReadyResponse(object? sender, List<CDataIn> DataChannel)
    {
            if (!IsDeviceAvailable()) return;
            try //Because of some issues when closing the app
            {
                RunOnUiThread(() =>
                {
                    foreach (CDataIn ci in DataChannel)
                    {
                        CYvsTimeData c = new(1)
                        {
                            xData = ci.DTAbsolute
                        };   //Just another data structure

                        c.yData[0] = c8Receiver!.Connection!.GetScaledValue(ci);

                        //Display value ... just that something happens in the GUI
                        string s = c.xData.ToString() + "\t" + c.yData[0].ToString() + "\t" + ci.HWcn.ToString() + "\t" + ci.SWcn.ToString() + Environment.NewLine;
                        txtData.AppendText(s);
                        TCP_Interface?.Write(ci);
                    }
                });
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
        private void TbConnect_ToState1(object sender, EventArgs e)
        {
            GoDisconnected();
            DontReconnectOntbConnect_ToState1 = true;
        }

        /// <summary>
        /// Connect to Device 
        /// </summary>
        private void TbConnect_ToState2(object sender, EventArgs e)
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
            c8Receiver?.Close_All();
        }

        #endregion

        /// <summary>
        /// Handles the Tick event of the tmrStatusMessages control.
        /// </summary>
        private void TmrStatusMessages_Tick(object sender, EventArgs e)
        {
            while (StatusMessages?.Count > 0)
            {
                //Output status Messages to Screen
                ColoredText? tc = StatusMessages?.Pop();
                if (tc != null)
                {
                    txtStatus.AppendText(Environment.NewLine);
                    txtStatus.SelectionColor = tc.Color;
                    txtStatus.AppendText(tc.Text);
                }
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
            if (IsDeviceAvailable())
            {
                //Update XBEE signal strength
                int b = c8Receiver!.RSSI_percent;

                if (b > pbXBEESignalStrength.Maximum) b = pbXBEESignalStrength.Maximum;
                pbXBEESignalStrength.Value = b;

                if (OldConnection_Status != c8Receiver.ConnectionStatus)   //Status changed?
                {
                    OldConnection_Status = c8Receiver.ConnectionStatus;

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
                                    c8Receiver.Connection.ModuleInfos = new List<CModuleBase>(BU_ModuleInfo);
                                    idx_SelectedModule = BU_idx_SelectedModule;

                                    c8Receiver.Connection.EnableDataReadyEvent = true;

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
                                    //Debug
                                    //c8Receiver!.Connection!.Connect_via_tryToConnectWorker();   //Starts own thread for Connection retries 
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
        List<CModuleBase> BU_ModuleInfo = [];

        /// <summary>
        /// Read the current Module-configuration (which modules are connected) from Neuromaster
        /// </summary>
        private async void BtGetConfigModules_Click(object sender, EventArgs e)
        {
            if (c8Receiver is not null && c8Receiver.Connection is not null)
            {
                c8Receiver!.Connection!.ScanModules();
                await c8Receiver!.Connection!.GetDeviceConfigAsync();
            }
        }


        /// <summary>
        /// Handles the Click event of the btSetConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BtSetConfig_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;
            //Get the Hardware Channel number of the currently selected Module
            int HW_cn = c8Receiver!.Connection!.ModuleInfos[idx_SelectedModule].HWcn;
            var mi = cChannelsControlV2x11.GetModuleInfo(HW_cn);
            
            if (mi != null)
            c8Receiver!.Connection!.ModuleInfos[HW_cn] = mi;

            if (c8Receiver.Connection.SetModuleConfig(HW_cn)) //Set the configuration
            {
                AddStatusString("Config set: " + HW_cn.ToString(), Color.Green);
                pbXBeeChannelCapacity.Value = c8Receiver.Connection.GetChannelCapcity();  //Calculate channel capacity with the new setting
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
            c8Receiver?.Connection?.SetConfigAllModules(); //Sets Configuration of all Channels
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
            c8Receiver?.Connection?.GetClock();
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
            c8Receiver?.Connection?.SetClock(DateTime.Now); //Set clock to PC time
        }

        /*******************************************************************************/
        /************************Feedback from the Communication ***********************/
        /*******************************************************************************/


        #endregion


        /// <summary>
        /// In case relative time differs from the absolute time
        /// </summary>
        private void BtResync_Click(object sender, EventArgs e)
        {
            c8Receiver?.Connection?.Resync();
        }


        #region USB_Connect_Disconnect

        /// <summary>
        /// Neurolinks USB cable is suddenly disconnected
        /// </summary>
        /// <remarks>
        /// USB port is monitored in the background if cable is plugged in again
        /// </remarks>
        /// <param name="PID_VID">ProductID & VendorID of the disconnected device</param>
        private void DataReceiver_DeviceDisconnected(string PID_VID)
        {
            RunOnUiThread(() =>
            {

                AddStatusString("USB Device " + PID_VID + " disconnected", Color.Red);
                //Only close Connection, USB Monitoring Stays On
                Disconnect();
            });
        }

        /// <summary>
        /// Neuromaster_s the connection_ device connected.
        /// </summary>
        /// <param name="ConnectionResult">The connection result.</param>
        private void DataReceiver_DeviceConnected(EnumConnectionResult ConnectionResult)
        {
            RunOnUiThread(() =>
            {
                AddStatusString("USB Device: " + ConnectionResult, Color.Green);
                Reconnect();
            });
        }

        /// <summary>
        /// Things to do when device is disconnected
        /// </summary>
        void Disconnect()
        {
            tbConnect.Enabled = false;  //Just to make GUI save

            //Backup current configuration
            if (IsDeviceAvailable() && c8Receiver!.Connection!.ModuleInfos != null)
            {
                BU_ModuleInfo = new List<CModuleBase>(c8Receiver!.Connection!.ModuleInfos);
            }
            BU_idx_SelectedModule = idx_SelectedModule;
            c8Receiver?.Close_Connection(); //Close Background threads responsible for receiving
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

        private void AddStatusString(ColoredText ct)
        {
            AddStatusString(ct.Text, ct.Color);
        }

        private void AddStatusString(string text, Color Col)
        {
            ColoredText tc = new(text, Col);
            StatusMessages.Push(tc);
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
            if (c8Receiver is null) return false;
            if (c8Receiver.Connection is null) return false;
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
                for (int HW_cn = 0; HW_cn < c8Receiver!.Connection!.ModuleInfos.Count; HW_cn++)
                {
                    if ((c8Receiver!.Connection!.ModuleInfos[HW_cn].ModuleType == BU_ModuleInfo[HW_cn].ModuleType) &&
                        (c8Receiver!.Connection!.ModuleInfos[HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty))
                    {
                        //Only if Module Types are equal
                        for (int SW_cn = 0; SW_cn < c8Receiver.Connection.ModuleInfos[HW_cn].SWChannels.Count; SW_cn++)
                        {
                            c8Receiver.Connection.ModuleInfos[HW_cn].SWChannels[SW_cn].SampleInt =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SampleInt;

                            c8Receiver.Connection.ModuleInfos[HW_cn].SWChannels[SW_cn].SaveChannel =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SaveChannel;

                            c8Receiver.Connection.ModuleInfos[HW_cn].SWChannels[SW_cn].SendChannel =
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
        private void CChannelsControlV2x11_ModuleRowChanged(object sender, CModuleBase ModuleInfo)
        {
            if (IsDeviceAvailable())
                for (int i = 0; i < c8Receiver!.Connection!.ModuleInfos.Count; i++)
                {
                    //Find index of the recently selected Module
                    if (c8Receiver.Connection.ModuleInfos[i].HWcn == ModuleInfo.HWcn)
                    {
                        idx_SelectedModule = i;
                        break;
                    }
                }
        }
        #endregion

        private void BtGetModuleSpecific_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;
            
            int HWcn = c8Receiver!.Connection!.ModuleInfos[idx_SelectedModule].HWcn;

            if (c8Receiver!.Connection.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {

                c8Receiver.Connection.GetModuleInfoSpecific(HWcn, true);
            }
            else
            {
                AddStatusString("Module specific read only possible if module is active", Color.Red);
            }
        }

        private void BtSetModuleSpecific_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;
            
            int HW_cn = c8Receiver!.Connection!.ModuleInfos[idx_SelectedModule].HWcn;

            if (c8Receiver.Connection.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {
                byte[] buf = cChannelsControlV2x11.GetModuleSpecific(HW_cn);
                c8Receiver.Connection.ModuleInfos[idx_SelectedModule].SetModuleSpecific(buf);

                if (c8Receiver.Connection.SetModuleInfoSpecific(HW_cn))
                {
                    AddStatusString("Module specific write OK", Color.Green);
                    string s = "Sent: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString() + ", ";
                    }
                    AddStatusString(s, Color.Orange);

                    Application.DoEvents();
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(c8Receiver.Connection.ModuleInfos[HW_cn]);
                    cChannelsControlV2x11.Refresh();

                    buf = c8Receiver.Connection.ModuleInfos[HW_cn].GetModuleSpecific();
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

        
        private void BtOpenTCP_Click(object sender, EventArgs e)
        {
            TCP_Interface ??= new CTCPInterface();
            TCP_Interface.StatusMessage += TCP_Interface_StatusMessage;
            TCP_Interface.Init();
        }

        private void TCP_Interface_StatusMessage(string data, Color color)
        {
            AddStatusString(data, color);
        }

        private void BtNMInfo_Click(object sender, EventArgs e)
        {
            c8Receiver?.Connection?.GetNMFirmwareVersion();
        }
    }
}
