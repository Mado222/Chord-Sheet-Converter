using FeedbackDataLib;
using FeedbackDataLib.Modules;
using Neuromaster_Demo_Library_Reduced__netx;
using Serilog.Events;
using WindControlLib;

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
        private CNMaster? cNMaster;

        /// <summary>
        /// Locking property
        /// </summary>
        private bool dontReconnectOntbConnect_ToState1 = false;

        /// <summary>
        /// Saves recent Connection status
        /// </summary>
        private EnConnectionStatus oldConnection_Status = EnConnectionStatus.NoConnection;

        /// <summary>
        /// Ringpuffer for Status Messages (used from tmrStatusMessages)
        /// </summary>
        private readonly CFifoBuffer<ColoredText> statusMessages = new();

        /// <summary>
        /// Buckup of the Index of the currently selected module
        /// </summary>
        private int buIdxSelectedModule = -1;

        /// <summary>
        /// Flag for USB disconnection / reconnection
        /// </summary>
        private bool uSB_Reconnected = false;

        /// <summary>TCP Interface to access data</summary>
        private CTCPInterface? tCPInterface;

        /// <summary>
        /// Index of the currently selected module
        /// </summary>
        private int idxSelectedModule;

        /// <summary>
        /// Currently selected HWcn
        /// </summary>
        /// <value>The h WCN selected.</value>
        private int HWcnSelected { get => cNMaster!.ModuleInfos[idxSelectedModule].HWcn; }


        /// <summary>Logging device for all modules - see code in Program.cs</summary>
        public readonly LoggingSettings _loggingSettings = new();

        /// <summary>
        /// Dumb helper field
        /// </summary>
        bool lockCbLogLevel = false;


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
            //Init GUI for selecting logging - Demo
            CbLogging.Checked = _loggingSettings.IsLoggingEnabled;
            LblLogFilePath.Text = _loggingSettings.LogFilePath;
            lockCbLogLevel = true;
            CbLogLevel.DataSource = Enum.GetValues(typeof(LogEventLevel));
            CbLogLevel.SelectedItem = _loggingSettings.LogLevel;
            lockCbLogLevel = false;

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
            dontReconnectOntbConnect_ToState1 = true;
            AddStatusString("Searching for Neurolink ....");

            // Perform initialization and connection in the background thread
            await Task.Run(() =>
            {
                // Create Receiver if it does not exist
                cNMaster ??= new CNMaster();
                
                EnConnectionStatus? conres = cNMaster.Connect();

                if (conres == EnConnectionStatus.Connected_via_RS232 ||
                    conres == EnConnectionStatus.Connected_via_XBee)
                {
                    // Neurolink is detected and initialized
                    Invoke(() => AddStatusString("Neurolink: " + cNMaster.NMReceiver.NeurolinkSerialNumber, Color.Blue));
                }
                // Update GUI based on connection results
                // Possible errors if no Neurolink is detected
                Invoke(() =>
                {
                    switch (conres)
                    {
                        case EnConnectionStatus.Connected_via_XBee:
                            AddStatusString("XBee Connection found: " + cNMaster.NMReceiver.PortName, Color.Green);
                            AddEvents(); // Attach events
                            break;
                        case EnConnectionStatus.Connected_via_RS232:
                            AddStatusString("RS232 connection (cable) found: " + cNMaster.NMReceiver.PortName, Color.Green);
                            AddEvents(); // Attach events
                            break;

                        // Possible errors if no Neurolink is detected
                        case EnConnectionStatus.Error_during_Port_scan:
                            AddStatusString("Error during Port scan.", Color.Red);
                            break;
                        case EnConnectionStatus.More_than_one_Neurolink_detected:
                            AddStatusString("Please connect only one Neurolink", Color.Orange);
                            break;
                        case EnConnectionStatus.No_Active_Neurolink:
                            AddStatusString("No active Neurolink found.", Color.Orange);
                            break;
                        default:
                            AddStatusString("Unknown error occurred.", Color.Orange);
                            break;
                    }
                });

            });
        }


        #region Events_for_async_communication_with_Neuromaster
        /// <summary>
        /// Required ecents for async data provessing
        /// </summary>
        private void AddEvents()
        {
            // Event for Data
            if (cNMaster?.NMReceiver is not null)
            {
                cNMaster.DataReadyResponse -= CNMaster_DataReadyResponse; // Ensure unsubscribing
                cNMaster.DataReadyResponse += CNMaster_DataReadyResponse;

                // Event to inform PC about Battery Status
                cNMaster.DeviceToPC_BatteryStatus -= CNMaster_DeviceToPC_BatteryStatus;
                cNMaster.DeviceToPC_BatteryStatus += CNMaster_DeviceToPC_BatteryStatus;

                // Buffer in Neuromaster is full
                cNMaster.DeviceToPC_BufferFull -= CNMaster_DeviceToPC_BufferFull;
                cNMaster.DeviceToPC_BufferFull += CNMaster_DeviceToPC_BufferFull;

                //Events for USB-Device Connection / Disconnection = USB Cable goes away!!
                cNMaster.NMReceiver.DeviceConnected -= NMReceiver_DeviceConnected;
                cNMaster.NMReceiver.DeviceConnected += NMReceiver_DeviceConnected;

                cNMaster.NMReceiver.DeviceDisconnected -= NMReceiver_DeviceDisconnected;
                cNMaster.NMReceiver.DeviceDisconnected += NMReceiver_DeviceDisconnected;

                // Responses to Commands sent to Neuromaster
                cNMaster.GetClockResponse -= CNMaster_GetClockResponse;
                cNMaster.GetClockResponse += CNMaster_GetClockResponse;

                cNMaster.SetClockResponse -= CNMaster_SetClockResponse;
                cNMaster.SetClockResponse += CNMaster_SetClockResponse;

                cNMaster.ScanModulesResponse -= CNMaster_ScanModulesResponse;
                cNMaster.ScanModulesResponse += CNMaster_ScanModulesResponse;

                cNMaster.SetModuleConfigResponse -= CNMaster_SetModuleConfigResponse;
                cNMaster.SetModuleConfigResponse += CNMaster_SetModuleConfigResponse;

                cNMaster.SetConnectionClosedResponse -= CNMaster_SetConnectionClosedResponse;
                cNMaster.SetConnectionClosedResponse += CNMaster_SetConnectionClosedResponse;

                cNMaster.GetFirmwareVersionResponse -= CNMaster_GetFirmwareVersionResponse;
                cNMaster.GetFirmwareVersionResponse += CNMaster_GetFirmwareVersionResponse;

                cNMaster.GetDeviceConfigResponse -= CNMaster_GetDeviceConfigResponse;
                cNMaster.GetDeviceConfigResponse += CNMaster_GetDeviceConfigResponse;

                cNMaster.SetDeviceConfigResponse -= CNMaster_SetDeviceConfigResponse;
                cNMaster.SetDeviceConfigResponse += CNMaster_SetDeviceConfigResponse;

                cNMaster.GetModuleSpecificResponse -= CNMaster_GetModuleSpecificResponse;
                cNMaster.GetModuleSpecificResponse += CNMaster_GetModuleSpecificResponse;

                cNMaster.SetModuleSpecificResponse -= CNMaster_SetModuleSpecificResponse;
                cNMaster.SetModuleSpecificResponse += CNMaster_SetModuleSpecificResponse;


                
            }
        }

        private void CNMaster_SetModuleSpecificResponse(object? sender, (bool isSuccess, byte HWcn, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
                if (e.isSuccess)
                {
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(cNMaster!.ModuleInfos[e.HWcn]);
                    cChannelsControlV2x11.Refresh();
                }
            });
        }

        private void CNMaster_GetModuleSpecificResponse(object? sender, (bool isSuccess, byte HWcn, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);

            });
        }

        private void CNMaster_SetDeviceConfigResponse(object? sender, (bool success, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);

            });
        }

        private void CNMaster_SetModuleConfigResponse(object? sender, (bool success, byte HWcn, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
                if (e.success)
                {
                    AddStatusString("Config set @ Channel " + e.HWcn.ToString(), Color.Green);
                    pbXBeeChannelCapacity.Value = cNMaster?.GetChannelCapcity() ?? 0;  //Calculate channel capacity with the new setting
                    lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();  //Display
                }
                else
                    AddStatusString("Config not set", Color.Red);    //Error
            });
        }

        private void CNMaster_ScanModulesResponse(object? sender, (bool isSuccessful, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
            });
        }

        private void CNMaster_GetFirmwareVersionResponse(object? sender, (CNMFirmwareVersion? FWVersion, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                if (e.FWVersion != null)
                    AddStatusString(e.FWVersion.GetFWVersionString(), Color.Violet);
                AddStatusString(e.msg.Text, e.msg.Color);
            });
        }

        private void CNMaster_GetDeviceConfigResponse(object? sender, (List<CModuleBase>? cmb, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
                if (cNMaster != null)
                    cChannelsControlV2x11.SetModuleInfos(cNMaster.ModuleInfos);
            });
        }

        private void CNMaster_SetConnectionClosedResponse(object? sender, (bool isClosed, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
            });
        }

        private void CNMaster_SetClockResponse(object? sender, (bool success, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
            });
        }

        private void CNMaster_GetClockResponse(object? sender, (DateTime? clock, ColoredText msg) e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.msg);
                txtTime.Text = e.clock?.ToString() ?? "Failed";
            });
        }

        /// <summary>
        /// Buffer is full in Neuromaster
        /// </summary>
        /// <remarks>
        /// Neuromaster stops sampling - must be reconfigured
        /// </remarks>
        private void CNMaster_DeviceToPC_BufferFull(object? sender, EventArgs e)
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
        private void CNMaster_DeviceToPC_BatteryStatus(object? sender, (uint BatteryVoltageMV, uint Percentage, uint SupplyVoltageMV) e)
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
        private void CNMaster_DataReadyResponse(object? sender, List<CDataIn> DataChannel)
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

                        c.yData[0] = cNMaster!.GetScaledValue(ci);

                        //Display value ... just that something happens in the GUI
                        string s = c.xData.ToString() + "\t" + c.yData[0].ToString() + "\t" + ci.HWcn.ToString() + "\t" + ci.SWcn.ToString() + Environment.NewLine;
                        txtData.AppendText(s);
                        tCPInterface?.Write(ci);
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
            dontReconnectOntbConnect_ToState1 = true;
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
            cNMaster?.Close();

        }

        /// <summary>
        /// Handles the Tick event of the tmrStatusMessages control.
        /// </summary>
        private void TmrStatusMessages_Tick(object sender, EventArgs e)
        {
            while (statusMessages?.Count > 0)
            {
                //Output status Messages to Screen
                ColoredText? tc = statusMessages?.Pop();
                if (tc != null)
                {
                    txtStatus.AppendText(Environment.NewLine);
                    txtStatus.SelectionColor = tc.Color;
                    txtStatus.AppendText(tc.Text);
                }
            }
            if (cNMaster is not null && cNMaster.NMReceiver.ConnectionStatus != EnConnectionStatus.NoConnection)
                CheckConnectionStatus();
        }

        #endregion

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
                int b = cNMaster?.NMReceiver.RSSI_percent ?? 0;

                if (b > pbXBEESignalStrength.Maximum) b = pbXBEESignalStrength.Maximum;
                pbXBEESignalStrength.Value = b;

                if (cNMaster is not null && oldConnection_Status != cNMaster.NMReceiver.ConnectionStatus)   //Status changed?
                {
                    oldConnection_Status = cNMaster.NMReceiver.ConnectionStatus;

                    switch (oldConnection_Status)
                    {
                        case EnConnectionStatus.Connected_via_RS232:
                        case EnConnectionStatus.Connected_via_XBee:
                        case EnConnectionStatus.Connected_via_SDCard:
                            {
                                //Connection has (re-) appeard
                                AddStatusString("Connected", Color.Green);
                                GoConnected();

                                //If USB connection crashed - restore configuration on the fly (see Disconnect())
                                if (uSB_Reconnected)
                                {
                                    uSB_Reconnected = false;

                                    //Reload old configuration
                                    cNMaster.ModuleInfos = new List<CModuleBase>(BU_ModuleInfo);
                                    idxSelectedModule = buIdxSelectedModule;

                                    RestoreOldConfiguration();
                                    btSetAllConfig.PerformClick();  //Send entire configuration to Neuromaster
                                }
                                break;
                            }
                        case EnConnectionStatus.Connecting:
                            {
                                AddStatusString("Connecting", Color.Green);
                                break;
                            }
                        case EnConnectionStatus.NoConnection:
                            {
                                AddStatusString("Not Connected", Color.Red);
                                GoDisconnected();
                                break;
                            }
                        case EnConnectionStatus.Dis_Connected:
                            {
                                AddStatusString("Dis-Connected", Color.Red);
                                GoDisconnected();

                                if (!dontReconnectOntbConnect_ToState1)
                                {
                                    //Start new connection
                                    //Debug
                                    //c8Receiver!.Connection!.Connect_via_tryToConnectWorker();   //Starts own thread for Connection retries 
                                }
                                break;
                            }
                        case EnConnectionStatus.No_Data_Link:
                            {
                                AddStatusString("No Data Link", Color.Red);
                                break;
                            }
                        case EnConnectionStatus.PortError:
                            {
                                AddStatusString("Cannot open COM - restart", Color.Red);
                                break;
                            }
                        case EnConnectionStatus.USB_disconnected:
                            {
                                break;
                            }
                        case EnConnectionStatus.USB_reconnected:
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
        private async void BtGetDeviceConfig_Click(object sender, EventArgs e)
        {
            if (cNMaster is not null)
            {
                cNMaster!.ScanModules();
                await cNMaster!.GetDeviceConfigAsync();
            }
        }


        /// <summary>
        /// Handles the Click event of the btSetConfig control.
        /// </summary>
        private void BtSetModuleConfig_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;

            var mi = cChannelsControlV2x11.GetModuleInfo(HWcnSelected);
            if (mi is not null)
                cNMaster!.ModuleInfos[idxSelectedModule] = mi;

            cNMaster?.SetModuleConfig(HWcnSelected); //Sets Configuration of all Channels
        }

        /// <summary>
        /// Handles the Click event of the btSetAllConfig control.
        /// </summary>
        private async void BtSetDeviceConfig_Click(object sender, EventArgs e)
        {
            if (cNMaster != null)
            {
                //Update all configs
                for (int HWcn = 0; HWcn < cNMaster!.ModuleInfos.Count; HWcn++)
                {
                    var mi = cChannelsControlV2x11.GetModuleInfo(HWcn);
                    if (mi != null)
                        cNMaster!.ModuleInfos[HWcn] = mi;
                }

                await cNMaster!.SetDeviceConfigAsync();
            }
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
            cNMaster?.GetClock();
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
            cNMaster?.SetClock(DateTime.Now); //Set clock to PC time
        }

        private void BtNMInfo_Click(object sender, EventArgs e)
        {
            cNMaster?.GetNMFirmwareVersion();
        }

        /// <summary>
        /// In case relative time differs from the absolute time
        /// </summary>
        private void BtResync_Click(object sender, EventArgs e)
        {
            cNMaster?.Resync();
        }

        private void BtGetModuleSpecific_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;

            if (cNMaster!.ModuleInfos[idxSelectedModule].IsModuleActive())
            {

                cNMaster.GetModuleSpecific(HWcnSelected, true);
            }
            else
            {
                AddStatusString("Module specific read only possible if module is active", Color.Red);
            }
        }

        private void BtSetModuleSpecific_Click(object sender, EventArgs e)
        {
            if (!IsDeviceAvailable()) return;

            if (cNMaster!.ModuleInfos[idxSelectedModule].IsModuleActive())
            {
                byte[] buf = cChannelsControlV2x11.GetModuleSpecific(HWcnSelected);
                cNMaster.ModuleInfos[idxSelectedModule].SetModuleSpecific(buf);

                var HWcn = HWcnSelected;
                cNMaster.SetModuleSpecific(HWcn);
            }
            else
            {
                AddStatusString("Module specific write only possible if module is active", Color.Red);
            }
        }


        #endregion

        #region USB_Connect_Disconnect

        /// <summary>
        /// Neurolinks USB cable is suddenly disconnected
        /// </summary>
        /// <remarks>
        /// USB port is monitored in the background if cable is plugged in again
        /// </remarks>
        /// <param name="PID_VID">ProductID & VendorID of the disconnected device</param>
        private void NMReceiver_DeviceDisconnected(object? sender, CVidPid e)
        {
            RunOnUiThread(() =>
            {

                AddStatusString("USB Device " + e.VID_PID + " disconnected", Color.Red);
                //Only close Connection, USB Monitoring Stays On
                Disconnect();
            });
        }

        /// <summary>
        /// Neuromaster_s the connection_ device connected.
        /// </summary>
        /// <param name="ConnectionResult">The connection result.</param>
        private void NMReceiver_DeviceConnected(object? sender, EnConnectionStatus e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString("USB Device: " + e, Color.Green);
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
            if (IsDeviceAvailable() && cNMaster!.ModuleInfos != null)
            {
                BU_ModuleInfo = new List<CModuleBase>(cNMaster!.ModuleInfos);
            }
            buIdxSelectedModule = idxSelectedModule;
            //cNMaster?.NMReceiver.Close_Connection(); //Close Background threads responsible for receiving
            cNMaster?.Close();
        }

        /// <summary>
        /// Things to do when device reappears
        /// </summary>
        private void Reconnect()
        {
            StartConnection();          //Reestablish connection - restore backup
            tbConnect.Enabled = true;   //Just to make GUI save
            uSB_Reconnected = true;
        }


        #endregion

        #region TCPInterfacing
        private void BtOpenTCP_Click(object sender, EventArgs e)
        {
            tCPInterface ??= new CTCPInterface();
            tCPInterface.StatusMessage += TCPInterface_StatusMessage;
            tCPInterface.Init();
        }

        private void TCPInterface_StatusMessage(object? sender, (string data, Color color) e)
        {
            AddStatusString(e.data, e.color);
        }
        #endregion

        #region Logging

        private void CbLogging_CheckedChanged(object sender, EventArgs e)
        {
            _loggingSettings.IsLoggingEnabled = CbLogging.Checked;
            //_loggingSettings.LogFilePath = "logs/app.log";

            // Apply updated settings to AppLogger
            AppLogger.UpdateLoggingSettings(_loggingSettings);
        }

        private void CbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CbLogLevel.SelectedItem is null || lockCbLogLevel) return;
            _loggingSettings.LogLevel = (LogEventLevel)CbLogLevel.SelectedItem;
            AppLogger.UpdateLoggingSettings(_loggingSettings); // Reconfigure logger
        }
        #endregion

        #region Helper_Functions_only_of_value_for_this_Application

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
            statusMessages.Push(tc);
        }

        /// <summary>
        ///   <para>
        /// Helper to switch Threads to GUI Thread</para>
        /// </summary>
        /// <param name="action">The action.</param>
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
            if (cNMaster is null) return false;
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
                for (int HW_cn = 0; HW_cn < cNMaster!.ModuleInfos.Count; HW_cn++)
                {
                    if ((cNMaster!.ModuleInfos[HW_cn].ModuleType == BU_ModuleInfo[HW_cn].ModuleType) &&
                        (cNMaster!.ModuleInfos[HW_cn].ModuleType != EnModuleType.cModuleTypeEmpty))
                    {
                        //Only if Module Types are equal
                        for (int SW_cn = 0; SW_cn < cNMaster.ModuleInfos[HW_cn].SWChannels.Count; SW_cn++)
                        {
                            cNMaster.ModuleInfos[HW_cn].SWChannels[SW_cn].SampleInt =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SampleInt;

                            cNMaster.ModuleInfos[HW_cn].SWChannels[SW_cn].SaveChannel =
                                BU_ModuleInfo[HW_cn].SWChannels[SW_cn].SaveChannel;

                            cNMaster.ModuleInfos[HW_cn].SWChannels[SW_cn].SendChannel =
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
                for (int i = 0; i < cNMaster!.ModuleInfos.Count; i++)
                {
                    //Find index of the recently selected Module
                    if (cNMaster.ModuleInfos[i].HWcn == ModuleInfo.HWcn)
                    {
                        idxSelectedModule = i;
                        break;
                    }
                }
        }
        #endregion
    }
}
