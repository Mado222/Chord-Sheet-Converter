using BMTCommunication;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using FeedbackDataLib_GUI;
using MathNetNuget;
using System.Diagnostics;
using System.Xml.Serialization;
using WindControlLib;


namespace Neuromaster_V5
{
    public partial class NeuromasterV5 : Form
    {
        /// <summary>
        /// Class to connect to Neuromaster
        /// </summary>
        private C8KanalReceiverV2 DataReceiver;

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
        private C8KanalReceiverV2.EnumConnectionResult LastConnectionResult = C8KanalReceiverV2.EnumConnectionResult.No_Active_Neurolink;

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

        private readonly CNeuromaster_Textfile_Importer_Exporter Neuromaster_Textfile_Importer_Exporter = new();

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
            if (LastConnectionResult != C8KanalReceiverV2.EnumConnectionResult.Connected_via_SDCard)
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
                cFlowChartDX1.SetupFlowChart_for_Module_Default(DataReceiver.Connection.Device.ModuleInfos);
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
            if (LastConnectionResult == C8KanalReceiverV2.EnumConnectionResult.Connected_via_SDCard)
            {
                if (SDCardData != null)
                {
                    for (int swcn = 0; swcn < DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].SWChannels.Count; swcn++)
                    {
                        _ = SDCardData.GetChannelData(DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HW_cn, swcn);
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
            if (DataReceiver.Connection.SendCloseConnection())  //4.8.2014
            {
                AddStatusString("Close Connection OK", Color.Green);
            }
            else
            {
                AddStatusString("Close Connection not confirmed", Color.Red);
            }

            GoDisconnected();
            DontReconnectOntbConnect_ToState1 = true;
            tbConnect.Enabled = true;
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
            DataReceiver?.Close_All();
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
        private void StartConnection()
        {
            stopwatch.Reset();
            stopwatch.Start();
            DontReconnectOntbConnect_ToState1 = false;
            C8KanalReceiverV2.EnumConnectionResult conres;

            DataReceiver ??= new C8KanalReceiverV2();

            if (sDCardToolStripMenuItem.Checked)
            {
                //SD Card mode
                _ = DataReceiver.Init_via_SDCard();
                //!!!! Now Connect
                LastConnectionResult = DataReceiver.Connect();
                AddEvents();
            }
            else
            {
                AddStatusString("Searching for Neurolink  ...." + "sw: " + stopwatch.ElapsedMilliseconds.ToString());

                if (d2XXToolStripMenuItem.Checked)
                    conres = DataReceiver.Init_via_D2XX();
                else //if (rbVirtCom.Checked)
                    conres = DataReceiver.Init_via_VirtualCom();

                if ((conres == C8KanalReceiverV2.EnumConnectionResult.Connected_via_USBCable) ||
                    conres == C8KanalReceiverV2.EnumConnectionResult.Connected_via_XBee)
                {

                    AddStatusString("Neurolink: " + DataReceiver.NeurolinkSerialNumber + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Blue);

                    //!!!! Now Connect - can take a while if Neurolink is connected via XBEE for the first time
                    LastConnectionResult = DataReceiver.Connect();

                    switch (LastConnectionResult)
                    {
                        case C8KanalReceiverV2.EnumConnectionResult.Connected_via_XBee:
                            {
                                AddStatusString("XBee Connection found: " + DataReceiver.PortName + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Green);
                                AddEvents();
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.Connected_via_USBCable:
                            {
                                AddStatusString("USB cable connection found: " + DataReceiver.PortName + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Green);
                                AddEvents();
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.Error_during_Port_scan:
                            {
                                AddStatusString("Error during Port scan." + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Red);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.Error_during_USBcable_connection:
                            {
                                AddStatusString("Error_during_USBcable_connection" + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.Error_during_XBee_connection:
                            {
                                AddStatusString("Error_during_XBee_connection" + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.More_than_one_Neurolink_detected:
                            {
                                AddStatusString("Please connect only one Neurolink" + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.No_Active_Neurolink:
                            {
                                AddStatusString("No active Neurolink found." + "  sw: " + stopwatch.ElapsedMilliseconds.ToString(), Color.Orange);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.Error_read_ErrorString:
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
                    switch (conres)
                    {
                        case C8KanalReceiverV2.EnumConnectionResult.Error_during_Port_scan:
                            {
                                AddStatusString("Error during Port scan.", Color.Red);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.More_than_one_Neurolink_detected:
                            {
                                AddStatusString("Please connect only one Neurolink", Color.Orange);
                                break;
                            }
                        case C8KanalReceiverV2.EnumConnectionResult.No_Active_Neurolink:
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
                stopwatch.Stop();
            }
        }


        private void AddEvents()
        {
            //Treiber versucht selbständig die Verbindung wieder herzustellen
            //Entsprechend den Verbindungsversuchen kommt in UpdateStaus Not_Connected, und Connecting

            //Event for Data
            DataReceiver.Connection.DataReady -= DataReceiver_DataReady;
            DataReceiver.Connection.DataReady += new DataReadyEventHandler(DataReceiver_DataReady);

            //Event to inform PC about Battery Status
            DataReceiver.Connection.DeviceToPC_BatteryStatus -= Connection_DeviceToPC_BatteryStatus;
            DataReceiver.Connection.DeviceToPC_BatteryStatus += new C8KanalReceiverV2_CommBase.DeviceToPC_BatteryStatusEventHandler(Connection_DeviceToPC_BatteryStatus);

            //Buffer in Neuromaster is full
            DataReceiver.Connection.DeviceToPC_BufferFull -= Connection_DeviceToPC_BufferFull;
            DataReceiver.Connection.DeviceToPC_BufferFull += new C8KanalReceiverV2_CommBase.DeviceToPC_BufferFullEventHAndler(Connection_DeviceToPC_BufferFull);

            //Error in Neuromodule occured - for future use
            DataReceiver.Connection.DeviceToPC_ModuleError -= Connection_DeviceToPC_ModuleError;
            DataReceiver.Connection.DeviceToPC_ModuleError += new C8KanalReceiverV2_CommBase.DeviceToPC_ModuleErrorEventHandler(Connection_DeviceToPC_ModuleError);

            //Events for USB-Device Connection / Disconnection = USB Cable goes away!!
            DataReceiver.DeviceConnected -= DataReceiver_DeviceConnected;
            DataReceiver.DeviceConnected += new C8KanalReceiverV2.DeviceConnectedEventHandler(DataReceiver_DeviceConnected);

            DataReceiver.DeviceDisconnected -= DataReceiver_DeviceDisconnected;
            DataReceiver.DeviceDisconnected += new C8KanalReceiverV2.DeviceDisconnectedEventHandler(DataReceiver_DeviceDisconnected);

            //Vaso Sensor Updated - see #define VasoCheckerActive in C8KanalReveierV2_Base.cs
            DataReceiver.Connection.Vaso_InfoSpecific_Updated -= Connection_Vaso_InfoSpecific_Updated;
            DataReceiver.Connection.Vaso_InfoSpecific_Updated += Connection_Vaso_InfoSpecific_Updated;

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
            if (InvokeRequired)
            {
                Invoke(new Action(Connection_DeviceToPC_BufferFull));
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
        void Connection_DeviceToPC_BatteryStatus(uint Battery_Voltage_mV, uint percentage, uint Supply_Voltage_mV)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<uint, uint, uint>(Connection_DeviceToPC_BatteryStatus), Battery_Voltage_mV, percentage, Supply_Voltage_mV);
            }
            else
            {
                txtBattery.Text = "";
                AddStatusString(txtBattery, "Battery Voltage: " + Battery_Voltage_mV.ToString() + "mV /t" +
                    "Battery Status: " + percentage.ToString() + "%", Color.Blue);
                AddStatusString(txtBattery, "Supply Voltage: " + Supply_Voltage_mV.ToString() + "mV", Color.Violet);
            }
        }

        #endregion

        private void EnDisComponents(bool EnDis)
        {
            pnControls.Enabled = EnDis;
        }



        #region AddStatusString
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
            CTextCol tc = new(txtStatus, text, Col);
            StatusMessages.Push(tc);
        }

        private class CTextCol(RichTextBox txtBox, string text, Color Col)
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
            if ((DataReceiver != null) && (DataReceiver.Connection != null))
            {
                //Update XBEE signal strength
                int b = DataReceiver.RSSI_percent;
                if (b > pbXBEESignalStrength.Maximum) b = pbXBEESignalStrength.Maximum;
                pbXBEESignalStrength.Value = b;

                if (OldConnection_Status != DataReceiver.ConnectionStatus)
                {
                    OldConnection_Status = DataReceiver.ConnectionStatus;

                    switch (OldConnection_Status)
                    {
                        case EnumConnectionStatus.Connected:
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
                                    DataReceiver.Connection.Device.ModuleInfos = new List<CModuleBase>(BU_ModuleInfo);

                                    idx_SelectedModule = BU_idx_SelectedModule;

                                    DataReceiver.Connection.Device.Calculate_SkalMax_SkalMin();

                                    RestoreOldConfiguration();
                                    SetAllConfig();
                                    DataReceiver.Connection.EnableDataReadyEvent = true;
                                }
                                else
                                {
                                    //Get Config
                                    btGetConfigModules.PerformClick();
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
                                //GoDisconnected();
                                break;
                            }
                        case EnumConnectionStatus.Dis_Connected:
                            {
                                AddStatusString("Dis-Connected", Color.Red);
                                Disconnect();

                                if (DataReceiver.Connection.Device.ModuleInfos != null)
                                {
                                    BU_ModuleInfo = new List<CModuleBase>(DataReceiver.Connection.Device.ModuleInfos);
                                }

                                if (!DontReconnectOntbConnect_ToState1)
                                {
                                    //Neue Verbindung starten
                                    DataReceiver.Connection.Connect_via_tryToConnectWorker();
                                }
                                else
                                {
                                    tbConnect.Enabled = true;
                                }
                                break;
                            }
                        case EnumConnectionStatus.No_Data_Link:
                            {
                                AddStatusString("No Data Link", Color.Red);
                                //cFlowChart1.Stop();
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
        #endregion

        #region DataReady_Event
        void DataReceiver_DataReady(object sender, List<CDataIn> DataChannel)
        {
            try //Just because some issues when closing the app
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object, List<CDataIn>>(DataReceiver_DataReady), sender, DataChannel);
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
                cChannelsControlV2x11.SetChannelActivity(ci.HW_cn);
                CYvsTimeData c = new(1);

                if (relativeToolStripMenuItem.Checked)
                    c.xData = ci.DT_relative;
                else
                    c.xData = ci.DT_absolute;

                if (scaledToolStripMenuItem.Checked)
                    c.yData[0] = DataReceiver.Connection.GetScaledValue(ci);
                else
                    c.yData[0] = ci.Value;


                /////////// ***** /////////////
                // Normal Processing
                /////////// ***** /////////////
                if (LastConnectionResult == C8KanalReceiverV2.EnumConnectionResult.Connected_via_SDCard)
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
                    int idx_Track = cFlowChartDX1.Link_Track_ModuleConfig.Get_idx_Track(ci.HW_cn, ci.SW_cn);

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
            ConfigSetOK = false;
            if (DataReceiver.Connection.ScanModules())
            {
                //Thread.Sleep(1000);  //Wait for Neuromaster to finish scanning
                AddStatusString("ScanModules OK", Color.Green);

                if (DataReceiver.Connection.GetDeviceConfig())
                {
                    if ((DataReceiver.Connection.Device.ModuleInfos != null) && (DataReceiver.Connection.Device.ModuleInfos.Count > 0))
                    {
                        //Backup module configuration
                        CModuleBase[] cmi = new CModuleBase[DataReceiver.Connection.Device.ModuleInfos.Count];
                        DataReceiver.Connection.Device.ModuleInfos.CopyTo(cmi);
                        BU_ModuleInfo = new List<CModuleBase>(cmi);
                    }

                    AddStatusString("DeviceConfig received", Color.Green);
                    DataReceiver.Connection.EnableDataReadyEvent = true;

                    /*
                    SignalFilters.Clear();
                    for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos.Count; i++)
                    {
                        SignalFilters.Add(new CSignalFilter(enumSignalFilterType.BandStop, 2, DataReceiver.Connection.Device.ModuleInfos[i].SWChannels[0].SampleInt, 2));
                    }*/
                    cChannelsControlV2x11.SetModuleInfos(DataReceiver.Connection.Device.GetModuleInfo_Clone());
                    cChannelsControlV2x11.Refresh();
                    SetupFlowChart();
                    Init_Graphs();

                    //Is EEG dabei?
                    for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos.Count; i++)
                    {
                        if (DataReceiver.Connection.Device.ModuleInfos[i].ModuleType == enumModuleType.cModuleEEG)
                            tmrUpdateFFT.Start();
                    }
                }
                else
                {
                    AddStatusString("Error during DeviceConfig", Color.Red);
                }
            }

            else
            {
                AddStatusString("Error during ScanModules", Color.Red);
            }
        }

        private void RestoreOldConfiguration()
        {
            if ((BU_ModuleInfo != null) && (BU_ModuleInfo.Count > 0))
            {
                //Check if possible
                for (int HW_cn = 0; HW_cn < DataReceiver.Connection.Device.ModuleInfos.Count; HW_cn++)
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
            int HW_cn = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HW_cn;
            ConfigSetOK = false;
            //DataReceiver.Connection.Device.ModuleInfos[HW_cn] = cChannelsControlV2x11.GetModuleInfo(HW_cn);

            //Update Modules 
            //var target = DataReceiver.Connection.Device.ModuleInfos[HW_cn];
            var source = cChannelsControlV2x11.GetModuleInfo(HW_cn);
            if (source != null)
                DataReceiver.Connection.Device.ModuleInfos[HW_cn].Update_SWChannels(source.SWChannels);

            if (DataReceiver.Connection.SetConfigModules(HW_cn))
            {
                AddStatusString("Config set: " + HW_cn.ToString(), Color.Green);
                pbXBeeChannelCapacity.Value = DataReceiver.Connection.GetChannelCapcity();
                lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();
                ConfigSetOK = true;
                SetupFlowChart();
            }
            else
                AddStatusString("Config not set" + HW_cn.ToString(), Color.Red);
        }


        private void BtSetAllConfig_Click(object sender, EventArgs e)
        {
            SetAllConfig();
        }


        private void SetAllConfig()
        {
            ConfigSetOK = false;
            if (DataReceiver.Connection.SetConfigChannel())
            {
                AddStatusString("Configuration set: ", Color.Green);
                pbXBeeChannelCapacity.Value = DataReceiver.Connection.GetChannelCapcity();
                lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();
                ConfigSetOK = true;
            }
            else
                AddStatusString("Configuration not set", Color.Red);

            cChannelsControlV2x11.Refresh();
        }


        private void BtGetClock_Click(object sender, EventArgs e)
        {
            DateTime dt = new();
            if (DataReceiver.Connection.GetClock(ref dt))
            {
                if (dt != null)
                {
                    txtTime.Text = dt.ToString();
                    AddStatusString("GetClock OK", Color.Green);
                }
                else
                {
                    AddStatusString("GetClock: Data Packet null", Color.Red);
                }
            }
            else
            {
                AddStatusString("Error during GetClock", Color.Red);
            }
        }

        private void BtSetClock_Click(object sender, EventArgs e)
        {
            if (DataReceiver.Connection.SetClock(DateTime.Now))
            {
                txtTime.Text = DateTime.Now.ToString();
                AddStatusString("SetClock OK", Color.Green);
            }
            else
            {
                txtTime.Text = "Error";
            }
        }

        private void CChannelsControlV2x11_ModuleRowChanged(object sender, CModuleBase ModuleInfo)
        {

            for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos.Count; i++)
            {
                if (DataReceiver.Connection.Device.ModuleInfos[i].HW_cn == ModuleInfo.HW_cn)
                {
                    idx_SelectedModule = i;
                    break;
                }
            }

            SetupFlowChart();
            cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule]);

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
                    DataReceiver.Connection.Device.ModuleInfos);
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
            for (int HW_cn = 0; HW_cn < DataReceiver.Connection.Device.ModuleInfos.Count; HW_cn++)
            {
                if (DataReceiver.Connection.Device.ModuleInfos[HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    if (HW_cn == 1)
                    {
                        //Multisensor
                    }
                    else
                    {
                        if (!DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[0].SendChannel)
                        {
                            DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[0].SendChannel = true;
                        }
                    }
                }
            }
            SetAllConfig();
        }

        private void AllDefaultOn_Click(object sender, EventArgs e)
        {
            UcFlowChartDX_NM.CDefaultChannels Def = new();
            AllChannelsOff();

            for (int HW_cn = 0; HW_cn < DataReceiver.Connection.Device.ModuleInfos.Count; HW_cn++)
            {
                if (DataReceiver.Connection.Device.ModuleInfos[HW_cn].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    foreach (UcFlowChartDX_NM.CDefaultChannels.CDefChannel dc in Def.DefChannel)
                    {
                        if (dc.ModuleType == DataReceiver.Connection.Device.ModuleInfos[HW_cn].ModuleType)
                        {
                            if (!DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[dc.SW_cn].SendChannel)
                            {
                                DataReceiver.Connection.Device.ModuleInfos[HW_cn].SWChannels[dc.SW_cn].SendChannel = true;
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
            for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos.Count; i++)
            {
                for (int j = 0; j < DataReceiver.Connection.Device.ModuleInfos[i].NumSWChannels; j++)
                {
                    DataReceiver.Connection.Device.ModuleInfos[i].SWChannels[j].SendChannel = false;
                    DataReceiver.Connection.Device.ModuleInfos[i].SWChannels[j].SaveChannel = false;
                }
            }

        }

        private void GetFTDIStatus(string Info, Color col)
        {
            byte GetModemStatus = 0;
            BMTCommunication.FTDI.FT_STATUS ftStatus = DataReceiver.FTDI_D2xx.GetModemStatus(ref GetModemStatus);

            AddStatusString(Info, col);
            AddStatusString("ModemStatus: " + GetModemStatus.ToString("X") + " (Status:" + ftStatus.ToString() + ")", Color.Black);
            AddStatusString("Port is open: " + DataReceiver.FTDI_D2xx.IsOpen.ToString(), Color.Black);
        }

        private void BtGetSDCardInfo_Click(object sender, EventArgs e)
        {
            CSDCardInfo SDCardInfo = new();

            if (DataReceiver.Connection.GetSDCardInfo(ref SDCardInfo))
            {
                AddStatusString("SD Card Info:");
                AddStatusString("SDCardSize_bytes: " + SDCardInfo.SDCardSize_bytes.ToString(), Color.DarkOliveGreen);
                AddStatusString("SDCardType: " + SDCardInfo.SDCardType.ToString(), Color.DarkOliveGreen);
                AddStatusString("SDFree_bytes: " + SDCardInfo.SDFree_bytes.ToString(), Color.DarkOliveGreen);
            }
            else
                AddStatusString("Failed to read SD Card!", Color.Red);
        }

        private void BtGetFirmwareVersion_Click(object sender, EventArgs e)
        {
            CNMFirmwareVersion NMFirmwareVersion = new();
            DataReceiver.Connection.GetNMFirmwareVersion(ref NMFirmwareVersion);

            AddStatusString("NM UID: " + NMFirmwareVersion.Uuid, Color.DarkOliveGreen);
            AddStatusString("NM HW Version: " + NMFirmwareVersion.HWVersion_string, Color.DarkOliveGreen);
            AddStatusString("NM SW Version: " + NMFirmwareVersion.SWVersion_string, Color.DarkOliveGreen);
        }


        CSDCardData SDCardData;
        readonly Stopwatch SDStopWatch = new();
        private void ReadBackSDCard()
        {
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
                        cChannelsControlV2x11.SetModuleInfos(DataReceiver.Connection.Device.ModuleInfos);
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
                        ((C8KanalReceiverV2_SDCard)((object)DataReceiver.Connection)).SDCardConnection.StartTime = DateTime.Now - new TimeSpan(1, 1, 1, 0);
                        //Set Sync Interval 
                        ((C8KanalReceiverV2_SDCard)((object)DataReceiver.Connection)).SDCardConnection.SyncInterval = DataReceiver.Connection.SyncInterval;


                        //Event das die Daten liefert aktivieren
                        DataReceiver.Connection.EnableDataReadyEvent = true;

                    }
                    else
                    {
                        AddStatusString("Error during DeviceConfig", Color.Red);
                    }
                }
            }
        }

        FileStream SDFileStream;
        BinaryReader SDBinaryRader;
        //const int bytes_to_read_from_file = 512;
        byte[] SDFiledata;
        const long numSDBytes_toread = 512;
        private void TmrSDDataReader_Tick(object sender, EventArgs e)
        {
            if (SDFiledata.Length > 0)
            {
                //Daten an Library übergeben

                /// <summary>
                /// Adds the sd card values.
                /// </summary>
                /// <returns>NUmber of data added</returns>
                int read = ((C8KanalReceiverV2_SDCard)((object)DataReceiver.Connection)).SDCardConnection.AddSDCardValues(SDFiledata);
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
            }
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
                _SDCardData = new List<CYvsTimeData>[C8KanalReceiverV2_CommBase.max_num_HWChannels][];
                for (int i = 0; i < C8KanalReceiverV2_CommBase.max_num_HWChannels; i++)
                {
                    _SDCardData[i] = new List<CYvsTimeData>[C8KanalReceiverV2_CommBase.max_num_SWChannels];
                    for (int j = 0; j < C8KanalReceiverV2_CommBase.max_num_SWChannels; j++)
                    {
                        _SDCardData[i][j] = [];
                    }
                }
            }

            public void Add(int HW_cn, int SW_cn, CYvsTimeData data)
            {
                MaxTime = data.xData;
                if (MinTime == DateTime.MinValue) MinTime = data.xData;
                _SDCardData[HW_cn][SW_cn].Add(data);
                if (_SDCardData[HW_cn][SW_cn].Count > MaxnumVals_perChannel)
                {
                    _SDCardData[HW_cn][SW_cn].RemoveAt(0);
                    MinTime = _SDCardData[HW_cn][SW_cn][0].xData;
                }
                else if (_SDCardData[HW_cn][SW_cn][0].xData < MinTime)
                {
                    _SDCardData[HW_cn][SW_cn].RemoveAt(0);
                }
            }

            public List<CYvsTimeData> GetChannelData(int HW_cn, int SW_cn)
            {
                List<CYvsTimeData> ret = [];
                if (HW_cn < _SDCardData.Length)
                    if (SW_cn < _SDCardData[HW_cn].Length)
                    {
                        ret = new List<CYvsTimeData>(_SDCardData[HW_cn][SW_cn]);
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
            int HW_cn = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HW_cn;

            if (DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].IsModuleActive())
            {
                if (DataReceiver.Connection.GetModuleInfoSpecific(HW_cn, true) != null)
                {
                    //Ok
                    AddStatusString("Module specific read OK", Color.Green);
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection.Device.ModuleInfos[HW_cn]);
                    cChannelsControlV2x11.Refresh();

                    byte[] buf = DataReceiver.Connection.Device.ModuleInfos[HW_cn].GetModuleSpecific();
                    string s = "Reci: ";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        s += buf[i].ToString("X2") + ", ";
                    }
                    AddStatusString(s, Color.Blue);


                    SetupFlowChart();
                    Init_Graphs();

                }
                else
                { AddStatusString("Module specific read failed", Color.Red); }
            }
            else
            {
                AddStatusString("Module specific read only possible if module is active", Color.Red);
            }
        }

        private void Connection_Vaso_InfoSpecific_Updated()
        {
            if (DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleVaso ||
                DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleVasosensorDig)
            {
                cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule]);
            }
        }


        private void BtSetModuleSpecific_Click(object sender, EventArgs e)
        {
            int HW_cn = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HW_cn;

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
                        s += buf[i].ToString("X2") + ", ";
                    }
                    AddStatusString(s, Color.Orange);

                    Application.DoEvents();
                    cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver.Connection.Device.ModuleInfos[HW_cn]);
                    cChannelsControlV2x11.Refresh();

                    /*
                    buf = DataReceiver.Connection.Device.ModuleInfos[HW_cn].GetModuleSpecific();
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
            ReadBackSDCard();
        }

        private void ResyncTimeBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataReceiver.Connection.Device.Resync();
        }

        private void StatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CVID_PID vp = new CVID_PID();
            //vp.VID_PID = "VID_0403&PID_6001";

            GetFTDIStatus("FTDI Status is: ", Color.Green);
        }


        private void ResetPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataReceiver is not null && DataReceiver.FTDI_D2xx != null)
            {
                _ = DataReceiver.FTDI_D2xx.ResetPort();
                GetFTDIStatus("Reset Ports: ", Color.Green);
            }
        }

        private void ResetDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataReceiver is not null && DataReceiver.FTDI_D2xx != null)
            {
                _ = DataReceiver.FTDI_D2xx.ResetDevice();
                GetFTDIStatus("Reset Device: ", Color.Green);
            }
        }

        private void CyclePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataReceiver is not null && DataReceiver.FTDI_D2xx != null)
            {
                _ = DataReceiver.FTDI_D2xx.CyclePort();
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
                    DataReceiver ??= new C8KanalReceiverV2();

                    //DataReceiver.Connection.Device.ModuleInfos = cm;
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
            CNeuromaster_Textfile_Importer_Exporter.CDatafromFile cdf = Neuromaster_Textfile_Importer_Exporter.GetNextValueFrom_SDCardFile(DateRecorded);
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
            _ = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].HW_cn;
        }

        private void ConvertToTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSDCardReader frmSDCardReader = new();
            frmSDCardReader.ShowDialog();
        }


        int num_raw_EEG_Channels = -1;
        private void TmrUpdateFFT_Tick(object sender, EventArgs e)
        {
            if (DataReceiver is not null)
            {
                if (DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].ModuleType == enumModuleType.cModuleEEG)
                {
                    try
                    {
                        if (num_raw_EEG_Channels < 0)
                        {
                            num_raw_EEG_Channels = DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule].NumRawChannels;

                        }

                        for (int i = 0; i < num_raw_EEG_Channels; i++)
                        {
                            var module = (CModuleExGADS1294_EEG)DataReceiver.Connection.Device.ModuleInfos[idx_SelectedModule];

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
            if (DataReceiver.Connection.Device.ModuleInfos != null)
            {
                BU_ModuleInfo = new List<CModuleBase>(DataReceiver.Connection.Device.ModuleInfos);
            }
            BU_idx_SelectedModule = idx_SelectedModule;
            DataReceiver.Close_Connection();
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

        private void DataReceiver_DeviceConnected(C8KanalReceiverV2.EnumConnectionResult ConnectionResult)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<C8KanalReceiverV2.EnumConnectionResult>(DataReceiver_DeviceConnected), ConnectionResult);
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
#endregion