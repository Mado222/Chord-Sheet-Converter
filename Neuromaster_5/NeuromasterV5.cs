using BMTCommunicationLib;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using FeedbackDataLibGUI;
using MathNetNuget;
using System.Diagnostics;
using System.Xml.Serialization;
using WindControlLib;


namespace Neuromaster_V5
{
#pragma warning disable // Disables all warnings for this file
    public partial class NeuromasterV5 : Form
    {
        /// <summary>
        /// Class to connect to Neuromaster
        /// </summary>
        public CNMaster cNMaster;

        /// <summary>
        /// Locking property
        /// </summary>
        private bool DontReconnectOntbConnect_ToState1 = false;

        /// <summary>
        /// Saves recent Connection status
        /// </summary>
        private EnConnectionStatus OldConnection_Status = EnConnectionStatus.NoConnection;

        /// <summary>
        /// Result of the last attempt to connect to Neuromaster
        /// </summary>
        private EnConnectionStatus LastConnectionResult = EnConnectionStatus.No_Active_Neurolink;

        /// <summary>
        /// Ringpuffer for Status Messages (used from tmrStatusMessages)
        /// </summary>
        private readonly CRingpuffer StatusMessages = new(100);

        /// <summary>
        /// Index of the currently selected module
        /// </summary>
        private int idx_SelectedModule;

        /// <summary>
        /// Buckup of the Index of the currently selected module
        /// </summary>
        private int BU_idx_SelectedModule = -1;

        private readonly CNeuromasterTextfileImporterExporter Neuromaster_Textfile_Importer_Exporter = new();

        //private readonly CFFT_MathNet FFT_MathNet = new();

        private bool ConfigSetOK = false;

        private readonly List<UcSignalAnalyser> ucSignalAnalysers = [];

        private readonly System.Windows.Forms.Timer tmrUpdateFFT;
        private frmSpectrum? FrmSpectrum = null;

        public NeuromasterV5()
        {
            InitializeComponent();
            SuspendLayout();


            Init_Graphs();
            ucSignalAnalysers.Clear();
            ucSignalAnalysers.Add(ucSignalAnalyser1);
            tlpMeasure.Controls.Add(ucSignalAnalyser1, 0, 0);

            tmrUpdateFFT = new System.Windows.Forms.Timer
            {
                Interval = 1000,
                Enabled = true
            };
            tmrUpdateFFT.Tick += TmrUpdateFFT_Tick;
        }

        private void Init_Graphs()
        {
            if (LastConnectionResult != EnConnectionStatus.Connected_via_SDCard)
            {
                cFlowChartDX1.Dock = DockStyle.Fill;
                cFlowChartDX1.Visible = true;
                cFlowChartDX1.Enabled = true;
            }
            else
            {
                cFlowChartDX1.Visible = false;
                cFlowChartDX1.Enabled = false;

                //Color[] colors = [Color.White, Color.White, Color.White, Color.White];

            }
            Setup_tlpMeasure();
        }


        enum EnDisplayState
        {
            None,
            Module,
            Raw,
            Default
        }
        private EnDisplayState DisplayState = EnDisplayState.None;


        private void SetupFlowChart()
        {
            if (rawChannelsToolStripMenuItem.Checked && (DisplayState != EnDisplayState.Raw))
            {
                Init_Graphs();
                DisplayState = EnDisplayState.Raw;
            }

            else if (predefinedChannelsToolStripMenuItem.Checked && (DisplayState != EnDisplayState.Default))
            {
                cFlowChartDX1.SetupFlowChart_for_Module_Default(cNMaster.ModuleInfos);
                Init_Graphs();
                DisplayState = EnDisplayState.Default;
            }
            else if (selectedModuleToolStripMenuItem.Checked)
            {
                cFlowChartDX1.SetupFlowChart_for_Module(cChannelsControlV2x11.GetModuleInfo(idx_SelectedModule));
                Init_Graphs();
                DisplayState = EnDisplayState.Module;
            }

            //In SD Card Mode show cashed data
            if (LastConnectionResult == EnConnectionStatus.Connected_via_SDCard)
            {
                if (SDCardData != null)
                {
                    for (int swcn = 0; swcn < cNMaster.ModuleInfos[idx_SelectedModule].SWChannels.Count; swcn++)
                    {
                        _ = SDCardData.GetChannelData(cNMaster.ModuleInfos[idx_SelectedModule].HWcn, swcn);
                    }
                }
            }
        }

        private void Setup_tlpMeasure()
        {
            //tlpMeasure
            if (cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count > 0)
            {
                if (cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count != tlpMeasure.RowCount)
                {
                    //Neu aufbauen
                    if (cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count > tlpMeasure.RowCount)
                    {
                        //Add ucSignalAnalysers
                        int rc = tlpMeasure.RowCount;
                        tlpMeasure.RowCount = cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count;
                        for (int i = rc; i <= cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count - 1; i++)
                        {
                            UcSignalAnalyser uc = new();
                            tlpMeasure.Controls.Add(uc, 0, i);
                            ucSignalAnalysers.Add(uc);
                        }

                    }
                    else
                    {
                        //Remove
                        for (int i = tlpMeasure.RowCount - 1; i >= cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count; i--)
                        {
                            tlpMeasure.Controls.RemoveAt(tlpMeasure.Controls.Count - 1);
                            ucSignalAnalysers.RemoveAt(ucSignalAnalysers.Count - 1);
                        }
                        tlpMeasure.RowCount = cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues.Count;
                    }

                    while (tlpMeasure.RowStyles.Count <= tlpMeasure.RowCount)
                    {
                        tlpMeasure.RowStyles.Add(new RowStyle(SizeType.Percent));
                    }

                    double d = 100 / tlpMeasure.RowCount;

                    for (int i = 0; i < tlpMeasure.RowStyles.Count; i++)
                    {
                        tlpMeasure.RowStyles[i].SizeType = SizeType.Percent;
                        tlpMeasure.RowStyles[i].Height = (int)d;
                    }
                }
                for (int i = 0; i < ucSignalAnalysers.Count; i++)
                {
                    ucSignalAnalysers[i].HeaderText = cFlowChartDX1.Link_Track_ModuleConfig.LinkedValues[i].SWChannelName;
                    ucSignalAnalysers[i].Autoupdate(1000);
                }
            }
        }

        private void NeuromasterV2_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrStatusMessages.Enabled = false;
            cFlowChartDX1.Stop();
            GoDisconnected();

            Neuromaster_Textfile_Importer_Exporter.CloseFile();

            if (_frmSaveData != null)
            {
                Neuromaster_Textfile_Importer_Exporter?.CloseFile();

                _frmSaveData.Close();
            }
        }

        #region Organise_GUI


        /// <summary>
        /// Disconnect from Neuromaster
        /// </summary>
        private void TbConnect_ToState1(object sender, EventArgs e)
        {
            cNMaster.Connection.SendCloseConnection();
        }


        private void GoConnected()
        {
            EnDisComponents(true);
            tbConnect.GoToState2(false);    //Just set Button to Green
            btSetClock.PerformClick();
        }

        private void GoDisconnected()
        {
            EnDisComponents(false);
            tbConnect.GoToState1(false);    //Just set Button to Red
            cNMaster?.Close_All();
        }


        /// <summary>
        /// Connect to Neuromaster
        /// </summary>
        private void TbConnect_ToState2(object sender, EventArgs e)
        {
            tbConnect.AcceptChange = false;
            StartConnection();
        }

        readonly Stopwatch stopwatch = new();
        private async void StartConnection()
        {
            DontReconnectOntbConnect_ToState1 = false;
            EnConnectionStatus conres;

            cNMaster ??= new CNMaster();

            if (sDCardToolStripMenuItem.Checked)
            {
                // SD Card mode
                await Task.Run(() =>
                {
                    LastConnectionResult = cNMaster.Connect();
                });

                Invoke(() => AddEvents());
            }
            else
            {
                AddStatusString("Searching for Neurolink  ...." + "sw: " + stopwatch.ElapsedMilliseconds.ToString());

                // Run the connection logic on a background task
                await Task.Run(() =>
                {
                    // Choose connection initialization method
                    conres = d2XXToolStripMenuItem.Checked
                        ? cNMaster.Init_via_D2XX()
                        : cNMaster.Init_via_VirtualCom();

                    if (conres == EnConnectionStatus.Connected_via_USBCable ||
                        conres == EnConnectionStatus.Connected_via_XBee)
                    {
                        // Successfully connected Neurolink
                        Invoke(() =>
                            AddStatusString("Neurolink: " + cNMaster.NeurolinkSerialNumber + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Blue)
                        );

                        LastConnectionResult = cNMaster.Connect();

                        // Use Invoke for UI updates in each case
                        Invoke(() =>
                        {
                            switch (LastConnectionResult)
                            {
                                case EnConnectionStatus.Connected_via_XBee:
                                    AddStatusString("XBee Connection found: " + cNMaster.PortName + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Green);
                                    AddEvents();
                                    break;
                                case EnConnectionStatus.Connected_via_USBCable:
                                    AddStatusString("USB cable connection found: " + cNMaster.PortName + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Green);
                                    AddEvents();
                                    break;
                                case EnConnectionStatus.Error_during_Port_scan:
                                    AddStatusString("Error during Port scan.  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Red);
                                    break;
                                case EnConnectionStatus.Error_during_USBcable_connection:
                                    AddStatusString("Error_during_USBcable_connection  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                    break;
                                case EnConnectionStatus.Error_during_XBee_connection:
                                    AddStatusString("Error_during_XBee_connection  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                    break;
                                case EnConnectionStatus.More_than_one_Neurolink_detected:
                                    AddStatusString("Please connect only one Neurolink  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                    break;
                                case EnConnectionStatus.No_Active_Neurolink:
                                    AddStatusString("No active Neurolink found.  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                    break;
                                default:
                                    break;
                            }
                        });
                    }
                    else
                    {
                        // Handle error cases if no Neurolink is detected
                        Invoke(() =>
                        {
                            switch (conres)
                            {
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
                                    break;
                            }
                        });
                    }
                });
            }
        }



        private void AddEvents()
        {
            //Treiber versucht selbständig die Verbindung wieder herzustellen
            //Entsprechend den Verbindungsversuchen kommt in UpdateStaus NoConnection, und Connecting

            //Event for Data
            cNMaster.DataReadyResponse -= CNMaster_DataReadyResponse;
            cNMaster.DataReadyResponse += CNMaster_DataReadyResponse;

            //Event to inform PC about Battery Status
            cNMaster.DeviceToPC_BatteryStatus -= CNMaster_DeviceToPC_BatteryStatus;
            cNMaster.DeviceToPC_BatteryStatus += CNMaster_DeviceToPC_BatteryStatus;//+= new EventHandler(Connection_DeviceToPC_BatteryStatus);

            //Buffer in Neuromaster is full
            cNMaster.DeviceToPC_BufferFull -= CNMaster_DeviceToPC_BufferFull;
            cNMaster.DeviceToPC_BufferFull += CNMaster_DeviceToPC_BufferFull;

            //Error in Neuromodule occured - for future use
            cNMasterToPC_ModuleError -= Connection_DeviceToPC_ModuleError;
            cNMasterToPC_ModuleError += Connection_DeviceToPC_ModuleError;

            //Messages from Command processing
            cNMaster.Connection.CommandProcessedResponse += Connection_CommandProcessedResponse;

            //Events for USB-Device Connection / Disconnection = USB Cable goes away!!
            cNMaster.DeviceConnected -= DataReceiver_DeviceConnected;
            cNMaster.DeviceConnected += new CNMaster.DeviceConnectedEventHandler(DataReceiver_DeviceConnected);

            cNMaster.DeviceDisconnected -= DataReceiver_DeviceDisconnected;
            cNMaster.DeviceDisconnected += new CNMaster.DeviceDisconnectedEventHandler(DataReceiver_DeviceDisconnected);

            //Vaso Sensor Updated - see #define VasoCheckerActive in C8KanalReveierV2_Base.cs
            cNMaster.Connection.Vaso_InfoSpecific_Updated -= Connection_Vaso_InfoSpecific_Updated;
            cNMaster.Connection.Vaso_InfoSpecific_Updated += Connection_Vaso_InfoSpecific_Updated;

        }


        #region DeviceCommunicationToPC_Event

        /// <summary>
        /// An Error occured in on Module
        /// </summary>
        /// <remarks>
        /// Currently not implemented
        /// </remarks>
        /// <param name="HWcn">Hardware Channel number of the Module</param>

        private void Connection_DeviceToPC_ModuleError(object? sender, byte HWcn)
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
        private void CNMaster_DeviceToPC_BufferFull(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => CNMaster_DeviceToPC_BufferFull(sender, e)));
            }
            else
            {
                AddStatusString("Neuromaster: Transmit Buffer is full", Color.Blue);
            }
        }


        /// <summary>
        /// Neuromaster Battery Satus
        /// </summary>
        /// <param name="Battery_Voltage_mV">The battery_ voltage_m v.</param>
        /// <param name="percentage">Percentage of Battery Capacity</param>
        /// <param name="Supply_Voltage_mV">The supply_ voltage_m v.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void CNMaster_DeviceToPC_BatteryStatus(object? sender, (uint BatteryVoltageMV, uint Percentage, uint SupplyVoltageMV) e)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => CNMaster_DeviceToPC_BatteryStatus(sender, e)));
            }
            else
            {
                txtBattery.Text = "";
                AddStatusString(txtBattery, $"Battery Voltage: {e.BatteryVoltageMV}mV\tBattery Status: {e.Percentage}%", Color.Blue);
                AddStatusString(txtBattery, $"Supply Voltage: {e.SupplyVoltageMV}mV", Color.Violet);
            }
        }

        #endregion

        private void EnDisComponents(bool EnDis)
        {
            pnControls.Enabled = EnDis;
        }



        #region AddStatusString
        public void AddStatusString(string text)
        {
            AddStatusString(text, Color.Black);
        }

        public void AddStatusString(string text, Color Col)
        {
            AddStatusString(txtStatus, text, Col);
        }

        public void AddStatusString(RichTextBox txtStatus, string text, Color Col)
        {
            CTextCol tc = new(txtStatus, text, Col);
            StatusMessages.Push(tc);
        }

        public class CTextCol(RichTextBox txtBox, string text, Color Col)
        {
            public string text = text;
            public Color col = Col;
            public RichTextBox txtBox = txtBox;
        }

        private void TmrStatusMessages_Tick(object sender, EventArgs e)
        {
            while (StatusMessages.StoredObjects > 0)
            {
                /*
                if (txtStatus.Lines.Length == 12)
                    txtStatus.Clear();*/
                CTextCol tc = (CTextCol)StatusMessages.Pop();

                if (tc.txtBox.Lines.Length > 40)
                    tc.txtBox.Text = "";
                tc.txtBox.SelectionColor = tc.col;
                tc.txtBox.AppendText(tc.text);
                tc.txtBox.AppendText(Environment.NewLine);
            }
            CheckConnectionStatus();
        }
        #endregion


        #region UpdtaeStatus_Event
        bool FlowChartisStarted = false;

        private void CheckConnectionStatus()
        {
            //if (!(sDCardToolStripMenuItem.Checked))
            if ((cNMaster != null) && (cNMaster.Connection != null))
            {
                //Update XBEE signal strength
                int b = cNMaster.RSSI_percent;
                if (b > pbXBEESignalStrength.Maximum) b = pbXBEESignalStrength.Maximum;
                pbXBEESignalStrength.Value = b;

                if (OldConnection_Status != cNMaster.ConnectionStatus)
                {
                    OldConnection_Status = cNMaster.ConnectionStatus;

                    switch (OldConnection_Status)
                    {
                        case EnConnectionStatus.Connected:
                            {
                                AddStatusString("Connected", Color.Green);
                                GoConnected();
                                if (!FlowChartisStarted)
                                {
                                    FlowChartisStarted = true;
                                }
                                //USB_Reconnected = false;

                                if (ConfigSetOK)
                                {
                                    //Reload old configuration 
                                    tbConnect.Enabled = true;
                                    cNMaster.ModuleInfos = new List<CModuleBase>(BU_ModuleInfo);

                                    idx_SelectedModule = BU_idx_SelectedModule;

                                    cNMaster.Calculate_SkalMax_SkalMin();

                                    RestoreOldConfiguration();
                                    SetAllConfig();
                                    cNMaster.Connection.EnableDataReadyEvent = true;
                                }
                                else
                                {
                                    //Get Config
                                    btGetConfigModules.PerformClick();
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
                                //GoDisconnected();
                                break;
                            }
                        case EnConnectionStatus.Dis_Connected:
                            {
                                AddStatusString("Dis-Connected", Color.Red);
                                Disconnect();

                                if (cNMaster.ModuleInfos != null)
                                {
                                    BU_ModuleInfo = new List<CModuleBase>(cNMaster.ModuleInfos);
                                }

                                if (!DontReconnectOntbConnect_ToState1)
                                {
                                    //Neue Verbindung starten
                                    cNMaster.Connection.Connect_via_tryToConnectWorker();
                                }
                                else
                                {
                                    tbConnect.Enabled = true;
                                }
                                break;
                            }
                        case EnConnectionStatus.No_Data_Link:
                            {
                                AddStatusString("No Data Link", Color.Red);
                                //cFlowChart1.Stop();
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
        #endregion

        #region DataReady_Event
        void CNMaster_DataReadyResponse(object sender, List<CDataIn> DataChannel)
        {
            try //Just because some issues when closing the app
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object, List<CDataIn>>(CNMaster_DataReadyResponse), sender, DataChannel);
                }
                else
                {
                    Test8Kanal_DataReady(DataChannel);
                }
            }
            catch (Exception ee)
            {
#if DEBUG
                AddStatusString("DataReceiver_DataReady: " + ee.Message);
#endif
            }
        }

        //readonly CVasoProcessor VasoProcessor = new CVasoProcessor();


        /// <summary>
        /// Data is coming in
        /// </summary>
        /// <param name="DataChannel">There can be more than one Datapoint in the list</param>
        void Test8Kanal_DataReady(List<CDataIn> DataChannel)
        {

            //AddStatusString(DataChannel.Count.ToString());
            foreach (CDataIn ci in DataChannel)
            {
                cChannelsControlV2x11.SetChannelActivity(ci.HWcn);
                CYvsTimeData c = new(1);

                if (relativeToolStripMenuItem.Checked)
                    c.xData = ci.DTRelative;
                else
                    c.xData = ci.DTAbsolute;

                if (scaledToolStripMenuItem.Checked)
                    c.yData[0] = cNMaster.Connection.GetScaledValue(ci);
                else
                    c.yData[0] = ci.Value;


                /////////// ***** /////////////
                // Normal Processing
                /////////// ***** /////////////
                if (LastConnectionResult == EnConnectionStatus.Connected_via_SDCard)
                {
                    //Daten zwischenspeichern
                    if (SDCardData != null)
                    {
                        //SDCardData.Add(ci.HWChannelNumber, ci.SWChannelNumber, c);
                    }
                }
                else
                {
                    //Echtzeitanzeige
                    //Daten akzeptieren?
                    int idx_Track = cFlowChartDX1.Link_Track_ModuleConfig.Get_idx_Track(ci.HWcn, ci.SWcn);

                    if (idx_Track >= 0)
                    {
                        cFlowChartDX1.AddPoint(idx_Track, c);
                        ucSignalAnalysers[idx_Track].Add(c.yData[0]);
                    }
                }

                //Im File speichern
                Neuromaster_Textfile_Importer_Exporter.SaveValue(ci, c.yData[0]);
            }
        }
        #endregion


        /// <summary>
        /// Holds Backup of Module-configuration
        /// </summary>
        List<CModuleBase> BU_ModuleInfo = [];

        /// <summary>
        /// Read the current Module-configuration (which modules are connected) from Neuromaster
        /// </summary>
        private void BtGetConfigModules_Click(object sender, EventArgs e)
        {
            //Put both into async execution queu, results via Connection_CommandProcessedResponse
            cNMaster?.Connection?.ScanModules();
            cNMaster?.Connection?.GetDeviceConfig();
        }

        private void RestoreOldConfiguration()
        {
            if ((BU_ModuleInfo != null) && (BU_ModuleInfo.Count > 0))
            {
                //Check if possible
                for (int HWcn = 0; HWcn < cNMaster.ModuleInfos.Count; HWcn++)
                {
                    if ((cNMaster.ModuleInfos[HWcn].ModuleType == BU_ModuleInfo[HWcn].ModuleType) &&
                        (cNMaster.ModuleInfos[HWcn].ModuleType != enumModuleType.cModuleTypeEmpty))
                    {
                        //Only if Module Types are equal
                        for (int SWcn = 0; SWcn < cNMaster.ModuleInfos[HWcn].SWChannels.Count; SWcn++)
                        {
                            cNMaster.ModuleInfos[HWcn].SWChannels[SWcn].SampleInt =
                                BU_ModuleInfo[HWcn].SWChannels[SWcn].SampleInt;

                            cNMaster.ModuleInfos[HWcn].SWChannels[SWcn].SaveChannel =
                                BU_ModuleInfo[HWcn].SWChannels[SWcn].SaveChannel;

                            cNMaster.ModuleInfos[HWcn].SWChannels[SWcn].SendChannel =
                                BU_ModuleInfo[HWcn].SWChannels[SWcn].SendChannel;
                        }
                        AddStatusString("HW channel " + HWcn.ToString() + " updated", Color.Green);
                    }
                }
                cChannelsControlV2x11.Refresh();
                SetupFlowChart();

                //SetScales();
                //Init_Graphs();
            }
        }

        private void BtRestoreLastConfig_Click(object sender, EventArgs e)
        {
            //RestoreOldConfiguration();
            if (LastSaveLoadFilename != "")
            {
                Load_Config_file(LastSaveLoadFilename);
                btSetConfig.PerformClick();
            }
        }


        private void BtSetConfig_Click(object sender, EventArgs e)
        {
            int HWcn = cNMaster.ModuleInfos[idx_SelectedModule].HWcn;
            ConfigSetOK = false;
            //DataReceiver.ModuleInfos[HWcn] = cChannelsControlV2x11.GetModuleInfo(HWcn);

            //Update Modules 
            //var target = DataReceiver.ModuleInfos[HWcn];
            var source = cChannelsControlV2x11.GetModuleInfo(HWcn);
            if (source != null)
                cNMaster.ModuleInfos[HWcn].Update_SWChannels(source.SWChannels);

            cNMaster.Connection.SetModuleConfig(HWcn);
        }


        private void BtSetAllConfig_Click(object sender, EventArgs e)
        {
            SetAllConfig();
        }


        private void SetAllConfig()
        {
            ConfigSetOK = false;
            cNMaster.Connection.SetConfigAllModules();
        }


        private void BtGetClock_Click(object sender, EventArgs e)
        {
            cNMaster.Connection.GetClock();
        }

        private void BtSetClock_Click(object sender, EventArgs e)
        {
            cNMaster.Connection.SetClock(DateTime.Now);
        }

        private void CChannelsControlV2x11_ModuleRowChanged(object sender, CModuleBase ModuleInfo)
        {

            for (int i = 0; i < cNMaster.ModuleInfos.Count; i++)
            {
                if (cNMaster.ModuleInfos[i].HWcn == ModuleInfo.HWcn)
                {
                    idx_SelectedModule = i;
                    break;
                }
            }

            SetupFlowChart();
            cChannelsControlV2x11.UpdateModuleSpecificInfo(cNMaster.ModuleInfos[idx_SelectedModule]);

            //Display FFT in case of EEG
            if (FrmSpectrum is not null)
            {
                if (ModuleInfo.ModuleType == enumModuleType.cModuleEEG || ModuleInfo.ModuleType == enumModuleType.cModuleExGADS94)

                {
                    FrmSpectrum.Visible = true;
                    tmrUpdateFFT.Enabled = true;
                }
                else
                {
                    tmrUpdateFFT.Enabled = false;
                    FrmSpectrum.Visible = false;

                }
            }
        }

        private void CChannelsControlV2x11_SWChannelRowChanged(object sender, CSWChannel SelectedSWChannel)
        {
            //Set Axis
            SetupFlowChart();
        }


        private void OpenFile()
        {
            if (_frmSaveData != null)
            {
                Neuromaster_Textfile_Importer_Exporter.OpenFile_for_writing(
                    _frmSaveData.txtPath.Text,
                    _frmSaveData.txtComment.Text,
                    cNMaster.ModuleInfos);
            }
            /*
            sw = new StreamWriter(txtPath.Text, false);
            DateTime dt = DateTime.Now;
            FileOpen = true;
            sw.Write("Hautleitwert SCL " + dt.ToString() + Environment.NewLine);

            sw.WriteLine("-------------");
            sw.WriteLine(txtComment.Text);
            sw.WriteLine("-------------");

            sw.WriteLine();

            sw.WriteLine("Time" + "\t" +
                "Time_ms" + "\t" +
                "Value" + "\t" +
                "HW CHannel number" + "\t" +
                "SW CHannel number" + "\t" +
                "Resync");
             */
        }

        //CheckBox[] cbChannelActive;
        //List<CSignalFilter_OnlineFilter> SignalFilters = new List<CSignalFilter_OnlineFilter>();

        private static readonly Version version = new(Application.ProductVersion);
        public static Version Version
        {
            get
            {
                //3540FQW-D26-B212A3F79N 
                return version;
            }
        }
        private void NeuromasterV2_Load(object sender, EventArgs e)
        {
            try
            {
                /*
                this.Text = "Neuromaster / Insight Instruments / " +
                    ApplicationDeployment.CurrentDeployment.CurrentVersion.Major.ToString() +
                    "." + ApplicationDeployment.CurrentDeployment.CurrentVersion.Minor.ToString() +
                    "." + ApplicationDeployment.CurrentDeployment.CurrentVersion.Build.ToString();
                */
            }
            catch (Exception ee)
            {
#if DEBUG
                AddStatusString("NeuromasterV2_Load: " + ee.Message);
#endif
            }
            //this.Text += " / DLL: " + Version.ToString(); //String.Format("Version {0}.{1}", Version.Major.ToString(), Version.Minor.ToString());
        }

        string LastSaveLoadFilename = "";

        private void BtSaveConfig_Click(object sender, EventArgs e)
        {
            //Save Config
            if (saveFileDialog_xml.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new(saveFileDialog_xml.FileName, FileMode.Create);
                TextWriter writer = new StreamWriter(fs);

                XmlSerializer ser = new(typeof(CSWConfigValues[][]));
                //Simply save all
                ser.Serialize(writer, cChannelsControlV2x11.GetUserChangeableData());
                writer.Close();
                LastSaveLoadFilename = saveFileDialog_xml.FileName;
                btRestoreLastConfig.Enabled = true;
            }
        }

        private void BtLoadConfig_Click(object sender, EventArgs e)
        {
            //Load Config
            if (openFileDialog_xml.ShowDialog() == DialogResult.OK)
            {
                Load_Config_file(openFileDialog_xml.FileName);
                LastSaveLoadFilename = openFileDialog_xml.FileName;
                btRestoreLastConfig.Enabled = true;
            }
        }

        private void Load_Config_file(string path_xml_file)
        {
            if (File.Exists(path_xml_file))
            {
                FileStream fs = new(path_xml_file, FileMode.Open);
                TextReader reader = new StreamReader(fs);
                XmlSerializer ser = new(typeof(CSWConfigValues[][]));
                CSWConfigValues[][]? ModuleInfo = (CSWConfigValues[][])ser.Deserialize(reader);
                reader.Close();
                //Only user changeable data into fields
                if (ModuleInfo is not null)
                {
                    cChannelsControlV2x11.SetUserChangeableData(ModuleInfo);
                    SetupFlowChart();
                }
            }
        }


        private void BtAllCnhan0On_Click(object sender, EventArgs e)
        {
            //bool changed = false;
            AllChannelsOff();
            for (int HWcn = 0; HWcn < cNMaster.ModuleInfos.Count; HWcn++)
            {
                if (cNMaster.ModuleInfos[HWcn].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    if (HWcn == 1)
                    {
                        //Multisensor
                    }
                    else
                    {
                        if (!cNMaster.ModuleInfos[HWcn].SWChannels[0].SendChannel)
                        {
                            cNMaster.ModuleInfos[HWcn].SWChannels[0].SendChannel = true;
                        }
                    }
                }
            }
            SetAllConfig();
        }

        private void AllDefaultOn_Click(object sender, EventArgs e)
        {
            ucFlowChartDX_NM.CDefaultChannels Def = new();
            AllChannelsOff();

            for (int HWcn = 0; HWcn < cNMaster.ModuleInfos.Count; HWcn++)
            {
                if (cNMaster.ModuleInfos[HWcn].ModuleType != EnModuleType.cModuleTypeEmpty)
                {
                    foreach (UcFlowChartDX_NM.CDefaultChannels.CDefChannel dc in Def.DefChannel)
                    {
                        if (dc.ModuleType == cNMaster.ModuleInfos[HWcn].ModuleType)
                        {
                            if (!cNMaster.ModuleInfos[HWcn].SWChannels[dc.SWcn].SendChannel)
                            {
                                cNMaster.ModuleInfos[HWcn].SWChannels[dc.SWcn].SendChannel = true;
                                break;
                            }
                        }
                    }
                }
            }
            SetAllConfig();
        }

        private void AllChannelsOff()
        {
            for (int i = 0; i < cNMaster.ModuleInfos.Count; i++)
            {
                for (int j = 0; j < cNMaster.ModuleInfos[i].NumSWChannels; j++)
                {
                    cNMaster.ModuleInfos[i].SWChannels[j].SendChannel = false;
                    cNMaster.ModuleInfos[i].SWChannels[j].SaveChannel = false;
                }
            }

        }

        private void GetFTDIStatus(string Info, Color col)
        {
            byte GetModemStatus = 0;
            BMTCommunicationLib.FTDI.FT_STATUS ftStatus = cNMaster.FTDI_D2xx.GetModemStatus(ref GetModemStatus);

            AddStatusString(Info, col);
            AddStatusString("ModemStatus: " + GetModemStatus.ToString("X") + " (Status:" + ftStatus.ToString() + ")", Color.Black);
            AddStatusString("Port is open: " + cNMaster.FTDI_D2xx.IsOpen.ToString(), Color.Black);
        }

        private void BtGetSDCardInfo_Click(object sender, EventArgs e)
        {
            //CSDCardInfo SDCardInfo = new();

            //if (DataReceiver.Connection.GetSDCardInfo(ref SDCardInfo))
            //{
            //    AddStatusString("SD Card Info:");
            //    AddStatusString("SDCardSize_bytes: " + SDCardInfo.SDCardSize_bytes.ToString(), Color.DarkOliveGreen);
            //    AddStatusString("SDCardType: " + SDCardInfo.SDCardType.ToString(), Color.DarkOliveGreen);
            //    AddStatusString("SDFree_bytes: " + SDCardInfo.SDFree_bytes.ToString(), Color.DarkOliveGreen);
            //}
            //else
            //    AddStatusString("Failed to read SD Card!", Color.Red);
        }

        private void BtGetFirmwareVersion_Click(object sender, EventArgs e)
        {
            cNMaster.Connection.GetNMFirmwareVersion();
        }


        CSDCardData SDCardData;
        readonly Stopwatch SDStopWatch = new();
        
        private void ReadBackSDCard()
        {
            /*
            if (openFileDialog_nmc.ShowDialog() == DialogResult.OK)
            {
                //DirectoryInfo dir = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
                string path_nmc = openFileDialog_nmc.FileName;
                SDCardData = new CSDCardData();

                //DataReceiver

                if (DataReceiver.SDCardConnection != null)
                {
                    //Verzeichnisname übergeben
                    DataReceiver.SDCardConnection.PathToConfigFile = path_nmc; //dir.FullName + "\\CHANCONF.NMC"; //File in dem die Kunfiguration steht

                    //Konfiguration zum Zeipunkt der Aufzeichnung holen
                    if (DataReceiver.Connection.GetDeviceConfig())
                    {
                        AddStatusString("DeviceConfig received", Color.Green);  //Meldung anzeigen

                        //Kunfiguartion an die Anzeige übergeben
                        cChannelsControlV2x11.SetModuleInfos(DataReceiver.ModuleInfos);
                        cChannelsControlV2x11.Refresh();

                        //Daten-Anzeige initialisieren
                        SetupFlowChart();
                        Init_Graphs();

                        //Datenfile öffnen
                        string path_nmd = Path.GetDirectoryName(path_nmc) + "\\DATA.NMD";

                        SDFileStream = new FileStream(path_nmd, //dir.FullName + "\\DATA.NMD",
                            FileMode.Open,
                            FileAccess.Read);
                        SDBinaryRader = new BinaryReader(SDFileStream);

                        SDFiledata = SDBinaryRader.ReadBytes((int)numSDBytes_toread);

                        SDStopWatch.Reset();
                        SDStopWatch.Start();

                        tmrSDDataReader.Enabled = true; //Timer, der die Daten stück für stück reinholt initialisieren


                        //Set begin time of recording - should be extracted from directory names of SD Card
                        ((CNMaster_SDCard)((object)DataReceiver.Connection)).SDCardConnection.StartTime = DateTime.Now - new TimeSpan(1, 1, 1, 0);
                        //Set Sync Interval 
                        ((CNMaster_SDCard)((object)DataReceiver.Connection)).SDCardConnection.SyncInterval = DataReceiver.Connection.SyncInterval;


                        //Event das die Daten liefert aktivieren
                        DataReceiver.Connection.EnableDataReadyEvent = true;

                    }
                    else
                    {
                        AddStatusString("Error during DeviceConfig", Color.Red);
                    }
                }
            }*/
        }

        FileStream SDFileStream;
        BinaryReader SDBinaryRader;
        //const int bytes_to_read_from_file = 512;
        byte[] SDFiledata;
        const long numSDBytes_toread = 512;
        private void TmrSDDataReader_Tick(object sender, EventArgs e)
        {
            /*
            if (SDFiledata.Length > 0)
            {
                //Daten an Library übergeben

                /// <summary>
                /// Adds the sd card values.
                /// </summary>
                /// <returns>NUmber of data added</returns>
                int read = ((CNMaster_SDCard)((object)DataReceiver.Connection)).SDCardConnection.AddSDCardValues(SDFiledata);
                //Application.DoEvents();

                //Wurden alle Daten angenommen?
                if (read != SDFiledata.Length)
                {
                    //Angenommenen Daten vorne wegnehmen = nicht angenommene Daten nach vorne kopieren
                    List<byte> TempData = new(SDFiledata);
                    TempData.RemoveRange(0, read);
                    SDFiledata = [.. TempData];
                    //Buffer.BlockCopy(SDFiledata, read, SDFiledata, 0, SDFiledata.Length - read);
                }
                else
                {
                    //Read next
                    SDFiledata = SDBinaryRader.ReadBytes((int)numSDBytes_toread);
                }
                AddStatusString("Read " + read.ToString() + " bytes", Color.Blue);
            }
            else
            {
                tmrSDDataReader.Enabled = false;
                SDStopWatch.Stop();
                AddStatusString("Time for reading back " + SDBinaryRader.BaseStream.Length.ToString() + " bytes: " + SDStopWatch.Elapsed.TotalSeconds.ToString() + "s", Color.Brown);
                SDBinaryRader.Close();
                SDFileStream.Close();
                SetupFlowChart();
            }*/
        }


        /// <summary>
        /// Zwischenspeicher für Zeitdaten
        /// Speichert hereinkommende Daten in den einzelnen Kanälen bis zu einer Anzahl von maxnumVals_perChannel
        /// Dieser Kanal bestimmt minTime, maxTime
        /// Nur Daten in diesem Zeitintervall werden akzeptiert
        /// </summary>
        class CSDCardData
        {
            private readonly List<CYvsTimeData>[][] _SDCardData;

            public DateTime MaxTime { get; set; } = DateTime.MinValue;

            public DateTime MinTime { get; set; } = DateTime.MinValue;
            public int MaxnumVals_perChannel { get; set; } = 10000;

            public CSDCardData()
            {
                _SDCardData = new List<CYvsTimeData>[CNMaster_CommBase.max_num_HWChannels][];
                for (int i = 0; i < CNMaster_CommBase.max_num_HWChannels; i++)
                {
                    _SDCardData[i] = new List<CYvsTimeData>[CNMaster_CommBase.max_num_SWChannels];
                    for (int j = 0; j < CNMaster_CommBase.max_num_SWChannels; j++)
                    {
                        _SDCardData[i][j] = [];
                    }
                }
            }

            public void Add(int HWcn, int SWcn, CYvsTimeData data)
            {
                MaxTime = data.xData;
                if (MinTime == DateTime.MinValue) MinTime = data.xData;
                _SDCardData[HWcn][SWcn].Add(data);
                if (_SDCardData[HWcn][SWcn].Count > MaxnumVals_perChannel)
                {
                    _SDCardData[HWcn][SWcn].RemoveAt(0);
                    MinTime = _SDCardData[HWcn][SWcn][0].xData;
                }
                else if (_SDCardData[HWcn][SWcn][0].xData < MinTime)
                {
                    _SDCardData[HWcn][SWcn].RemoveAt(0);
                }
            }

            public List<CYvsTimeData> GetChannelData(int HWcn, int SWcn)
            {
                List<CYvsTimeData> ret = [];
                if (HWcn < _SDCardData.Length)
                    if (SWcn < _SDCardData[HWcn].Length)
                    {
                        ret = new List<CYvsTimeData>(_SDCardData[HWcn][SWcn]);
                    }
                return ret;
            }
        }

        private void BtGetModuleSpecific_Click(object sender, EventArgs e)
        {
            GetModuleSpecific();
        }

        private void GetModuleSpecific()
        {
            int HWcn = cNMaster.ModuleInfos[idx_SelectedModule].HWcn;

            if (cNMaster.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {
                cNMaster.Connection.GetModuleInfoSpecific(HWcn, true);
            }
        }

        private void Connection_Vaso_InfoSpecific_Updated(object? sender, EventArgs e)
        {
            if (cNMaster.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleVaso ||
                cNMaster.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleVasosensorDig)
            {
                cChannelsControlV2x11.UpdateModuleSpecificInfo(cNMaster.ModuleInfos[idx_SelectedModule]);
            }
        }


        private void BtSetModuleSpecific_Click(object sender, EventArgs e)
        {
            int HWcn = cNMaster.ModuleInfos[idx_SelectedModule].HWcn;

            if (cNMaster.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {
                byte[] buf = cChannelsControlV2x11.GetModuleSpecific(HWcn);
                cNMaster.ModuleInfos[idx_SelectedModule].SetModuleSpecific(buf);

                if (cNMaster.Connection.SetModuleInfoSpecific(HWcn))
                {
                    AddStatusString("Module specific write OK", Color.Green);
                    string s = "Sent: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString("X2") + ", ";
                    }
                    AddStatusString(s, Color.Orange);

                    Application.DoEvents();
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(cNMaster.ModuleInfos[HWcn]);
                    cChannelsControlV2x11.Refresh();

                    /*
                    buf = DataReceiver.ModuleInfos[HWcn].GetModuleSpecific();
                    s = "Reci: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString("X2") + ", ";
                    }
                    AddStatusString(s, Color.Blue);
                    */

                }
                else
                { AddStatusString("Module specific write failed", Color.Red); }
            }
            else
            {
                AddStatusString("Module specific write only possible if module is active", Color.Red);
            }
        }

        private void SelectedModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == selectedModuleToolStripMenuItem)
            {
                selectedModuleToolStripMenuItem.Checked = true;
                rawChannelsToolStripMenuItem.Checked = false;
                predefinedChannelsToolStripMenuItem.Checked = false;
            }
            if (sender == rawChannelsToolStripMenuItem)
            {
                rawChannelsToolStripMenuItem.Checked = true;
                selectedModuleToolStripMenuItem.Checked = false;
                predefinedChannelsToolStripMenuItem.Checked = false;
            }
            if (sender == predefinedChannelsToolStripMenuItem)
            {
                predefinedChannelsToolStripMenuItem.Checked = true;
                rawChannelsToolStripMenuItem.Checked = false;
                selectedModuleToolStripMenuItem.Checked = false;
            }
            SetupFlowChart();
        }

        private void ScaledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == scaledToolStripMenuItem)
            {
                scaledToolStripMenuItem.Checked = true;
                unscaledToolStripMenuItem.Checked = false;
            }
            if (sender == unscaledToolStripMenuItem)
            {
                unscaledToolStripMenuItem.Checked = true;
                scaledToolStripMenuItem.Checked = false;
            }
        }

        private void AbsolutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == absolutToolStripMenuItem)
            {
                absolutToolStripMenuItem.Checked = true;
                relativeToolStripMenuItem.Checked = false;
            }
            if (sender == relativeToolStripMenuItem)
            {
                absolutToolStripMenuItem.Checked = false;
                relativeToolStripMenuItem.Checked = true;
            }
        }

        private void D2XXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == d2XXToolStripMenuItem)
            {
                d2XXToolStripMenuItem.Checked = true;
                virtualComToolStripMenuItem.Checked = false;
                sDCardToolStripMenuItem.Checked = false;
            }
            if (sender == virtualComToolStripMenuItem)
            {
                virtualComToolStripMenuItem.Checked = true;
                d2XXToolStripMenuItem.Checked = false;
                sDCardToolStripMenuItem.Checked = false;
            }
            if (sender == sDCardToolStripMenuItem)
            {
                sDCardToolStripMenuItem.Checked = true;
                virtualComToolStripMenuItem.Checked = false;
                d2XXToolStripMenuItem.Checked = false;
            }
        }


        private void ReadBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ReadBackSDCard();
        }

        private void ResyncTimeBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cNMaster.Resync();
        }

        private void StatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CVID_PID vp = new CVID_PID();
            //vp.VID_PID = "VID_0403&PID_6001";

            GetFTDIStatus("FTDI Status is: ", Color.Green);
        }


        private void ResetPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cNMaster is not null && cNMaster.FTDI_D2xx != null)
            {
                _ = cNMaster.FTDI_D2xx.ResetPort();
                GetFTDIStatus("Reset Ports: ", Color.Green);
            }
        }

        private void ResetDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cNMaster is not null && cNMaster.FTDI_D2xx != null)
            {
                _ = cNMaster.FTDI_D2xx.ResetDevice();
                GetFTDIStatus("Reset Device: ", Color.Green);
            }
        }

        private void CyclePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cNMaster is not null && cNMaster.FTDI_D2xx != null)
            {
                _ = cNMaster.FTDI_D2xx.CyclePort();
                GetFTDIStatus("Cycle Port: ", Color.Green);
            }
        }

        frmSaveData _frmSaveData;
        //ComponentsLib.CToggleButton ctbSaving = new ComponentsLib.CToggleButton();
        private void SaveDataToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_frmSaveData == null)
            {
                _frmSaveData = new frmSaveData();
                _frmSaveData.StopSaving += FrmSaveData_StopSaving;
                _frmSaveData.StartSaving += FrmSaveData_StartSaving;
            }
            _frmSaveData.Show();
        }

        //--> Noch nicht fertig programmiert Liest nur cfg zurück aber noch keine Daten
        private void ReadDataFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog_cfg.ShowDialog() == DialogResult.OK)
            {
                string cfgPath = openFileDialog_cfg.FileName;
                DateTime recorded = DateTime.Now;
                string Comment = "";
                List<CModuleBase> cm = []; ;
                if (Neuromaster_Textfile_Importer_Exporter.Open_File_for_reading(cfgPath, ref recorded, ref Comment, ref cm))
                {
                    cNMaster ??= new CNMaster();

                    //DataReceiver.ModuleInfos = cm;
                    cChannelsControlV2x11.SetModuleInfos(cm);
                    cChannelsControlV2x11.Refresh();
                    EnDisComponents(true);
                    AddStatusString("Data File opened", Color.Green);
                    DateRecorded = recorded.ToShortDateString();
                    tsFileTimeDiv = DateTime.Now - recorded;
                    SetupFlowChart();
                    Init_Graphs();
                    tmrFileDataReader.Enabled = true;
                }
                else
                {
                    AddStatusString("Data File failed", Color.Red);
                }
            }
        }

        TimeSpan tsFileTimeDiv;
        string DateRecorded = "";

        private void TmrFileDataReader_Tick(object sender, EventArgs e)
        {
            CNeuromasterTextfileImporterExporter.CDatafromFile cdf = Neuromaster_Textfile_Importer_Exporter.GetNextValueFrom_SDCardFile(DateRecorded);
            cChannelsControlV2x11.SetChannelActivity(cdf.hwcn);
            CYvsTimeData c = new(1)
            {
                xData = cdf.dt_absolute + tsFileTimeDiv
            };

            if (scaledToolStripMenuItem.Checked)
                c.yData[0] = cdf.y_scaled;
            else
                c.yData[0] = cdf.y_unscaled;


            //Echtzeitanzeige
            //Daten akzeptieren?
            int idx_Track = cFlowChartDX1.Link_Track_ModuleConfig.Get_idx_Track(cdf.hwcn, cdf.swcn);

            if (idx_Track >= 0)
            {
                cFlowChartDX1.AddPoint(idx_Track, c);
                ucSignalAnalysers[cdf.swcn].Add(c.yData[0]);
            }
        }

        private void FrmSaveData_StartSaving()
        {
            //Start saving
            try
            {
                OpenFile();
                //RecordingJustStarted = true;
            }
            catch (Exception ee)
            {
                if (_frmSaveData != null)
                    _frmSaveData.ctbSaving.AcceptChange = false;

                AddStatusString("Saving failed: " + ee.Message, Color.Red);
            }
        }

        private void FrmSaveData_StopSaving()
        {
            //Stop saving
            Neuromaster_Textfile_Importer_Exporter.CloseFile();
        }

        private void BtGetElectrodeInfo_Click(object sender, EventArgs e)
        {
            _ = cNMaster.ModuleInfos[idx_SelectedModule].HWcn;
        }

        private void ConvertToTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmSDCardReader frmSDCardReader = new();
            //frmSDCardReader.ShowDialog();
        }


        int num_raw_EEG_Channels = -1;
        private void TmrUpdateFFT_Tick(object sender, EventArgs e)
        {
            if (cNMaster is not null)
            {
                if (cNMaster.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleEEG)
                {
                    try
                    {
                        if (num_raw_EEG_Channels < 0)
                        {
                            num_raw_EEG_Channels = cNMaster.ModuleInfos[idx_SelectedModule].NumRawChannels;

                        }

                        for (int i = 0; i < num_raw_EEG_Channels; i++)
                        {
                            var module = (CModuleExGADS1294_EEG)cNMaster.ModuleInfos[idx_SelectedModule];

                            module.Set_EEGBands_SampleInt_ms(i, cChannelsControlV2x11.GetSR_ms(i));

                            if (module.SWChannels[i].SendChannel)
                            {
                                double[]? ret = module.GetEEGSpectrum_1Hz_Steps(i);

                                if (ret is not null)
                                {
                                    if (FrmSpectrum is null)
                                    {
                                        FrmSpectrum = new frmSpectrum(num_raw_EEG_Channels);
                                        string[] cat = new string[ret.Length];
                                        for (int j = 0; j < ret.Length; j++)
                                            cat[j] = j.ToString();

                                        for (int j = 0; j < num_raw_EEG_Channels; j++)
                                            FrmSpectrum.UpdateXAxisCategories(j, [.. cat]);
                                        FrmSpectrum.Show();
                                    }

                                    FrmSpectrum.UpdateChartValues(i, ret, module.ElectrodeDatas[i]);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void Reconnect()
        {
            StartConnection();
            tbConnect.Enabled = true;
        }

        private void Disconnect()
        {
            EnDisComponents(false);
            tbConnect.Enabled = false;
            if (cNMaster.ModuleInfos != null)
            {
                BU_ModuleInfo = new List<CModuleBase>(cNMaster.ModuleInfos);
            }
            BU_idx_SelectedModule = idx_SelectedModule;
            cNMaster.Close_Connection();
        }


        #region USB_Connect_Disconnect
        private void DataReceiver_DeviceDisconnected(string PID_VID)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(DataReceiver_DeviceDisconnected), PID_VID);
            }
            else
            {
                AddStatusString("USB Device " + PID_VID + " disconnected", Color.Red);
                //Only close Connection, USB Monitoring Stays On
                Disconnect();
            }
        }

        private void DataReceiver_DeviceConnected(EnConnectionStatus ConnectionResult)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<EnConnectionStatus>(DataReceiver_DeviceConnected), ConnectionResult);
            }
            else
            {
                AddStatusString("USB Device Reconnected: " + ConnectionResult, Color.Red);
                //DataReceiver.StopUSBMonitoring();
                Reconnect();    //Starts USB Monitoring as soon as Device is detected
            }
        }
        #endregion
    }
}
#pragma warning restore // Disables all warnings for this file
#endregion