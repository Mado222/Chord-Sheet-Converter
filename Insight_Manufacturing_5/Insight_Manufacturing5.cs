#pragma warning disable 1591
using BMTCommunication;
using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.tests_measurements;
using Insight_Manufacturing5_net8.dataSources;
using System.Media;
using WindControlLib;
using System.Runtime.Versioning;


namespace Insight_Manufacturing5_net8
{
    [SupportedOSPlatform("windows")]
    public partial class frmInsight_Manufacturing5 : Form
    {
        //Basic Paths
        public const string Dir_Basic = @"C:\Insight Manufacturing\";
        public const string Dir_CombinedHex = Dir_Basic + @"Combined Hex\";
        public const string Dir_Results_Base = Dir_Basic + @"Results\";
        public const string Dir_FTDI = Dir_Basic + @"FTDI\";


        bool measurement_finished = true;
        /// <summary>
        /// Zähler f d einzelnen Measurements
        /// </summary>
        int cnt_scan = 0;
        /// <summary>
        /// Timer der die einzelnen Tasks der Reihe nach abarbeitet .. flashen, messen, ... 
        /// </summary>
        System.Windows.Forms.Timer? tmrMeasurementManager = null;
        DateTime dtSingleMeasurementStarted = DateTime.Now;

        /// <summary>
        /// Alle möglichen Measurements
        /// </summary>
        public CMeasurements measurements = new CMeasurements();

        /// <summary>
        /// Function Generator - used to avoid turning it on and off during serial Measurements
        /// </summary>
        public CFY6900 FY6900;

        /// <summary>
        /// Test Board with Phidgets
        /// </summary>
        public CInsightModuleTesterV1? InsightModuleTestBoardV1 = null;

        /// <summary>
        /// Gerade vom tmrMeasurementManager ausgeführtes Item
        /// </summary>
        CMeasurementItem Current_CMeasurementItem;
        public DateTime? dtAllMeasurementsStarted = null;

        public CMicrochip_Programmer programmer_info;

        /// <summary>
        /// Wird in die Datenbank eingetragen, wenn das Neurodevice das 1. Mal geflashed wird
        /// </summary>
        public string Firmware_Version = "";

        public string lblFWVersionBasicText = "";


        /// <summary>
        /// Database Access
        /// </summary>
        dsManufacturing _dsManufacturing = new dsManufacturing();
        dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter;
        dataSources.dsManufacturingTableAdapters.TableAdapterManager tableAdapterManager;
        BindingSource neuromodule_KalibrierdatenBindingSource;
        dataSources.dsManufacturingTableAdapters.Neuromodule_KalibrierdatenTableAdapter neuromodule_KalibrierdatenTableAdapter;
        BindingSource neuromodule_DatenBindingSource;
        dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter;


        enum enNeurodeviceInDb
        {
            InvalidDeviceNo,
            NotInDB,
            ExistsInDB
        }

        enNeurodeviceInDb NeuroDevice_in_DB = enNeurodeviceInDb.NotInDB;

        public frmInsight_Manufacturing5()
        {
            InitializeComponent();
            btGetProgrammers_Click(null, null);
            FY6900 = new CFY6900();
            lblFWVersionBasicText = lblFWVersion.Text;
            FY6900.Open();
            cbComPortSelector.Items.Clear();
            List<string>? p = FY6900.GetRelatedComPorts();
            if (p is not null)
            {
                cbComPortSelector.Items.AddRange(p.ToArray());
                if (cbComPortSelector.Items.Count >0)
                    cbComPortSelector.SelectedIndex = 0;
            }
            else
                txtStatus.AddStatusString("No COM Ports found");

            rbOffsetHigh.Text = CInsightModuleTesterV1.Uoff_High_mV.ToString();
            rbOffsetLow.Text = CInsightModuleTesterV1.Uoff_Low_mV.ToString();
        }
        
        private void Insight_Manufacturing5_Load(object sender, EventArgs e)
        {
            neurodevicesTableAdapter = new dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();
            neurodevicesTableAdapter.ClearBeforeFill = true;
            neurodevicesTableAdapter.Fill(_dsManufacturing.Neurodevices);
            if (tabMeasurements.SelectedTab != tabMeasurements.TabPages["tabProgNeuroModul"])
                tabMeasurements.SelectedTab = tabMeasurements.TabPages["tabProgNeuroModul"];
            else
                Proc_tabMeasurements();
        }

        private void frmInsight_Manufacturing5_FormClosing(object sender, FormClosingEventArgs e)
        {
            cFlowChartDX1.Stop();
            if (FY6900 != null)
            {
                FY6900.Close();
            }
        }

        public bool Open_InsightTestBoard()
        {
            bool ret = false;
            if (InsightModuleTestBoardV1 == null)
            {
                InsightModuleTestBoardV1 = new CInsightModuleTesterV1();
                InsightModuleTestBoardV1.PhidgetConnected += InsightModuleTestBoardV1_PhidgetConnected;
                InsightModuleTestBoardV1.ReportMeasurementProgress += InsightModuleTestBoardV1_ReportMeasurementProgress;
            }

            if (!InsightModuleTestBoardV1.PhidgetIsConnected)
            {
                if (!InsightModuleTestBoardV1.Open())
                    txtStatus.AddStatusString("Test Board / Phidget NOT found - autosearch started", Color.Red);
            }
            else
                bShow_Battery_Check = true;

            return ret;
        }

        private void InsightModuleTestBoardV1_ReportMeasurementProgress(object sender, string text, Color col)
        {
            txtStatus.AddStatusString(text, col);
        }

        private void InsightModuleTestBoardV1_PhidgetConnected(object sender)
        {
            txtStatus.AddStatusString("Test Board / Phidget found", Color.Green);
            bShow_Battery_Check = true;
        }


        delegate void AddTextThreadSafeDelegate(Control control, string txt);
        void AddTextThreadSafe(Control control, string txt)
        {
            if (txtStatus.InvokeRequired)
            {
                Invoke(new AddTextThreadSafeDelegate(AddTextThreadSafe), new object[] { control, txt });
            }
            else
            {
                control.Text = txt;
            }
        }

        /// <summary>
        /// start measurements more detailed comments in the function
        /// </summary>
        private void start_scan()
        {
            txtStatus.AddStatusString("Started: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString(), Color.Black);
            En_Dis_Gui(true);
            lblResult.BackColor = Color.Orange;
            lblResult.Text = "";
            Application.DoEvents();

            //Delete Time Column and Done Column
            foreach (CMeasurementItem cm in measurements.Measurements_Items)
            {
                cm.Measurement_done = false;
                cm.Measurement_Duration = "";
            }
            dgvMeasurements.Refresh();

            if (Save_FirstTime_to_DB())     //Does something only if module / device is new
            {
                if (tmrMeasurementManager == null)
                {
                    tmrMeasurementManager = new System.Windows.Forms.Timer();
                    tmrMeasurementManager.Tick += TmrMeasurementManager_Tick;
                    tmrMeasurementManager.Interval = 200;
                }

                cnt_scan = -1;
                measurement_finished = true;
                tmrMeasurementManager.Enabled = true;
                dtAllMeasurementsStarted = DateTime.Now;
            }
        }

        public void En_Dis_Gui(bool Lock)
        {
            tabMeasurements.Enabled = !Lock;
        }

        private void btnProgNeuroModul_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            lblResult.Text = "";
            lblResult.BackColor = Color.Orange;

            if (tabMeasurements.SelectedTab == tabMeasurements.TabPages["tabProgNeuroModul"])
            {
                dgvMeasurements_ResetColor();

                //Update Paths
                if (flashNeuromodul != null)
                {
                    //flashNeuromodul.HexFilePath_Base -> already set
                    flashNeuromodul.HexFilePath_Target = HexFilePath_with_SN;
                }

                for (int i = 0; i < uccheck.Count; i++)
                {
                    uccheck[i].SerialNumber = txtSerialNo.Text;
                }

                if (flashNeuromodul_Calibrated != null)
                {
                    string dir = Path.GetDirectoryName(HexFilePath_with_SN);
                    string fn = Path.GetFileNameWithoutExtension(HexFilePath_with_SN);
                    string HexFilePath_Target = dir + "\\" + fn;
                    HexFilePath += "_calib";
                    flashNeuromodul_Calibrated.HexFilePath_Base = HexFilePath_with_SN;
                    flashNeuromodul_Calibrated.HexFilePath_Target = HexFilePath_Target + ".hex";
                }
                
                if (frmAgain != null)
                {
                    frmAgain.Close();
                    frmAgain = null;
                }
                if (frmAgain == null)
                    frmAgain = new frmAmplitudeGain();
            }
            start_scan();
        }

        private void btModulICD3auslesen_Click(object sender, EventArgs e)
        {
                //Read Information from Module connected to ICD
                InsightModuleTestBoardV1.ICD_Connect();
                CIPE_Neuromodul_PIC24 nhx = new CIPE_Neuromodul_PIC24(programmer_info);
                //CIPE_Neuromodul_PIC24 nhx = programmer_info;
                nhx.ReportMeasurementProgress += Nhx_ReportMeasurementProgress;
                string SerialNumber = "";
                string SWVersion = "";
                string HWVersion = "";
                string tempPath = "";
                txtStatus.Text = "";
                nhx.Get_Serial_Number_from_connected_Module(ref tempPath, ref SerialNumber, ref SWVersion, ref HWVersion, Selected_Module);
                txtSerialNo.Text = SerialNumber;

                CIPE_Neuromodul_PIC24 cNeuromodule_Handle_Hexfile = new CIPE_Neuromodul_PIC24(programmer_info);
                dgv_SWChannelInfo.DataSource = cNeuromodule_Handle_Hexfile.Get_ChannelInfo_from_Combined_hex_file(tempPath, Selected_Module);
                dgv_SWChannelInfo.Refresh();
            
        }

        private void btModulNMauslesen_Click(object sender, EventArgs e)
        {
            if (Selected_Module == enumModuleType.cNeuromaster)
            {
                MessageBox.Show("Neuromaster mit Neurolink verbinden und einschalten.", "Nächster Schritt:", MessageBoxButtons.OK);
                CRead_Neuromaster uc = new CRead_Neuromaster(FY6900);
                uc.ReportMeasurementProgress += Measurement_Object_ReportMeasurementProgress;

                txtStatus.Clear();

                try
                {
                    if (uc.Connect_DataReceiver(false) == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                    {
                        CNMFirmwareVersion NMFirmwareVersion = new CNMFirmwareVersion();
                        uc.DataReceiver.Connection.GetNMFirmwareVersion(ref NMFirmwareVersion);

                        txtStatus.AddStatusString("Neuromaster: " + NMFirmwareVersion.uuid.ToString() + " connected", Color.Blue);
                        txtStatus.AddStatusString("Neuromaster HW-Version: " + NMFirmwareVersion.HWVersion_string, Color.Blue);
                        txtStatus.AddStatusString("Neuromaster SW-Version: " + NMFirmwareVersion.SWVersion_string, Color.Blue);

                        if (NMFirmwareVersion.uuid != "")
                        {
                            txtSerialNo.Text = NMFirmwareVersion.uuid;
                        }
                    }
                }
                finally
                {
                    uc.ReportMeasurementProgress -= Measurement_Object_ReportMeasurementProgress;
                    uc.DataReceiver.Close_All();
                }

            }
            else if (Selected_Module == enumModuleType.cNeurolink)
            {
                C8KanalReceiverV2 Datareceiver_NL = new C8KanalReceiverV2(); ;

                txtStatus.Clear();
                txtStatus.AddStatusString("Searching for Neurolink  ....", Color.Green);
                C8KanalReceiverV2.enumConnectionResult? conres = Datareceiver_NL.Init_via_D2XX(new List<string> { "FT232R" });
                if ((conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable) || conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_XBee)
                {
                    txtStatus.AddStatusString("Neurolink: " + Datareceiver_NL.NeurolinkSerialNumber + " connected", Color.Green);
                }

                if (Datareceiver_NL.NeurolinkSerialNumber != "")
                    txtSerialNo.Text = Datareceiver_NL.NeurolinkSerialNumber;

                Datareceiver_NL.Close_All();
            }
            else
                {
                    CRead_Neuromaster readNM = new CRead_Neuromaster(FY6900);
                    readNM.ReportMeasurementProgress += Measurement_Object_ReportMeasurementProgress;
                    try
                    {
                        InsightModuleTestBoardV1.ICD_DisConnect();
                    }
                    catch (Exception ee)
                    {
                        txtStatus.AddStatusString("TestBoardV1 Exception: " + ee.Message);
                    }

                    int channo = 0;
                    if (Selected_Module == enumModuleType.cModuleMultisensor)
                    {
                        channo = 1;
                        MessageBox.Show("Multisensor in Neuromaster einsetzen", "Nächster Schritt:", MessageBoxButtons.OK);
                    }
                    else
                    {
                        //MessageBox.Show("Neuromodul Neuromaster verbinden (Kanal B)", "Nächster Schritt:", MessageBoxButtons.OK);
                    }

                    try
                    {
                        if (readNM.Connect_DataReceiver(false) == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                        {
                            readNM.DataReceiver.Connection.GetDeviceConfig();

                            if ((readNM.DataReceiver.Connection.Device.ModuleInfos != null) && !readNM.DataReceiver.Connection.Device.ModuleInfos[0].ModuleBootloaderError)
                            {
                                txtStatus.AddStatusString("Serial: " + readNM.DataReceiver.Connection.Device.ModuleInfos[channo].UUID, Color.Blue);
                                txtStatus.AddStatusString("Firmware Version: " + readNM.DataReceiver.Connection.Device.ModuleInfos[channo].SWRevision_string, Color.Blue);
                                txtStatus.AddStatusString("Module Type: " + readNM.DataReceiver.Connection.Device.ModuleInfos[channo].ModuleType_string, Color.Blue);
                                txtSerialNo.Text = readNM.DataReceiver.Connection.Device.ModuleInfos[channo].UUID;
                                dgv_SWChannelInfo.DataSource = readNM.DataReceiver.Connection.Device.ModuleInfos[channo].SWChannels;
                                dgv_SWChannelInfo.Refresh();
                            }
                        }
                    }
                    finally
                    {
                        readNM.ReportMeasurementProgress -= Measurement_Object_ReportMeasurementProgress;
                        readNM.DataReceiver.Close_All();
                    }
                }
            }

        private void btReadhexFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"c:\Insight Manufacturing\Combined Hex",
                Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtStatus.Clear();
                CIPE_Neuromodul_PIC24 nm = new CIPE_Neuromodul_PIC24(programmer_info);
                string tempPath = openFileDialog1.FileName;

                CIPE_Neuromodul_PIC24.CModuleInformation cmi = nm.Get_FullInfo(tempPath);

                txtStatus.AddStatusString("Neuromodule Type: " + cmi.ModuleType, Color.Violet);
                txtStatus.AddStatusString("Serial: " + cmi.SerialNumber, Color.Blue);
                //SerialNofromFile = cmi.SerialNumber;
                //txtStatus.AddStatusString("HWVersion: " + cmi.HWVersion + " ( 0x"+ Convert.ToInt16(cmi.HWVersion).ToString("X") +" )", Color.Blue);
                txtStatus.AddStatusString("HWVersion: " + cmi.HWVersion, Color.Blue);
                txtStatus.AddStatusString("SW Version: " + cmi.SWVersion, Color.Blue);
                txtStatus.AddStatusString("Bootloader Version: " + cmi.BLVersion, Color.Blue);

                CIPE_Neuromodul_PIC24 cNeuromodule_Handle_Hexfile = new CIPE_Neuromodul_PIC24(programmer_info);
                dgv_SWChannelInfo.DataSource = cNeuromodule_Handle_Hexfile.Get_ChannelInfo_from_Combined_hex_file(tempPath, Selected_Module);
                dgv_SWChannelInfo.Refresh();
            }
        }


        private void tabMeasurements_Selecting(object sender, TabControlCancelEventArgs e)
        {
            Proc_tabMeasurements();
        }

        private void Proc_tabMeasurements()
        {

            if (tabMeasurements.SelectedTab.Name == tabProgNeuroModul.Name)
            {
                Selected_Module = enumModuleType.cModuleTypeEmpty;  //19.12.2021
                if (rbbModuleSelection.Count == 0)
                    Init_Neuromodul_Buttons();
                rbbModuleSelection[1].Checked = true;
                rbbModuleSelection[2].Checked = true;
                rbbModuleSelection[1].Checked = true;
                dgvMeasurements.Visible = true;
                Open_InsightTestBoard();
            }
            else if (tabMeasurements.SelectedTab.Name == tabFY6900.Name)
            {
                Open_InsightTestBoard();
                dgvMeasurements.Visible = false;
            }
            dgvMeasurements_ResetColor();
        }

        delegate enNeurodeviceInDb CheckDBforNeurodeviceDelegate();
        /// <summary>
        /// Checks the d bfor neurodevice.
        /// </summary>
        /// <returns></returns>
        private enNeurodeviceInDb CheckDBforNeurodevice()
        {
            NeuroDevice_in_DB = enNeurodeviceInDb.InvalidDeviceNo;

            if (txtSerialNo.InvokeRequired)
            {
                Invoke(new CheckDBforNeurodeviceDelegate(CheckDBforNeurodevice));
            }
            else
            {
                //Check DB
                if (txtSerialNo.Text.Length > txtSerialNo.MaxLength)
                {
                    txtSerialNo.ForeColor = Color.Red;
                    lblNoFeedback.ForeColor = System.Drawing.Color.Black;
                    lblNoFeedback.Text = "Seriennummer muss " + txtSerialNo.MaxLength + " Stellen haben";
                    NeuroDevice_in_DB = enNeurodeviceInDb.InvalidDeviceNo;
                }
                else
                {
                    neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, txtSerialNo.Text);

                    if (_dsManufacturing.Neurodevices.Count == 0)
                    {
                        txtSerialNo.ForeColor = Color.Green;
                        lblNoFeedback.Text = "New Device";
                        NeuroDevice_in_DB = enNeurodeviceInDb.NotInDB;
                    }
                    else
                    {
                        lblNoFeedback.Text = "Device exists";
                        lblNoFeedback.ForeColor = Color.Blue;
                        txtSerialNo.ForeColor = Color.Blue;
                        txtSerialNo.Text = _dsManufacturing.Neurodevices[0].SerialNumber;
                        NeuroDevice_in_DB = enNeurodeviceInDb.ExistsInDB;

                        int noofElemnts = Enum.GetNames(typeof(enumModuleType)).Length;


                        Array moduleType_array = Enum.GetValues(typeof(enumModuleType));
                        foreach (enumModuleType mt in moduleType_array)
                        {
                            if (_dsManufacturing.Neurodevices[0].Typ == mt.ToString())
                            {
                                //is Neuromodul, select Radio Button accordingly
                                if (tabMeasurements.SelectedTab != tabMeasurements.TabPages["tabProgNeuroModul"])
                                {
                                    tabMeasurements.SelectedTab = tabMeasurements.TabPages["tabProgNeuroModul"];
                                    Application.DoEvents();
                                }
                                foreach (RadioButton rb in rbbModuleSelection)
                                {
                                    if (rb.Text == mt.ToString())
                                    {
                                        rb.Checked = true; break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    Application.DoEvents();
                }
            }
            return NeuroDevice_in_DB;
        }

        private void txtSerialNo_TextChanged(object sender, EventArgs e)
        {
            CheckDBforNeurodevice();
        }

        bool Save_FirstTime_to_DB()
        {
            bool failed = false;

            if (txtSerialNo.Text != "")
            {
                if (CheckDBforNeurodevice() == enNeurodeviceInDb.NotInDB)
                {
                    //Save to DB
                    try
                    {
                        dsManufacturing.NeurodevicesRow nrow = _dsManufacturing.Neurodevices.NewNeurodevicesRow();
                        DateTime dt_created = DateTime.Now;
                        nrow.Hex_File_geflashed = dt_created.ToLongDateString();
                        nrow.SerialNumber = txtSerialNo.Text;
                        nrow.Programmierdatum = DateTime.Now;
                        nrow.Version = Firmware_Version;

                        if (Selected_Module != enumModuleType.cModuleTypeEmpty)
                        {
                            nrow.Typ = Selected_Module.ToString();
                        }

                        _dsManufacturing.Neurodevices.Rows.Add(nrow);
                        neurodevicesTableAdapter.Update(_dsManufacturing.Neurodevices);
                    }
                    catch (Exception ee)
                    {
                        txtStatus.AddStatusString("dsManufacturing.NeurodevicesRow Adding Row failed: " + ee.Message, Color.Red);
                        failed = true;
                    }

                    if (!failed)
                        txtStatus.AddStatusString("Wrote to Database: ", Color.Green);
                    CheckDBforNeurodevice();    //Wozu??

                }
                else
                    txtStatus.AddStatusString("Device exist in DB", Color.Black);
            }
            else
            {
                txtStatus.AddStatusString("Seriennummer darf nicht leer sein", Color.Red);
            }

            return !failed;
        }

        /// <summary>
        /// Bernhards Part from here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        // Neurolink
        private void btnProgNL_Click(object sender, EventArgs e)
        {

        }
        // Neuromaster

        private void btShowDatabase_Click(object sender, EventArgs e)
        {
            frmDatabase frm = new frmDatabase();
            frm.ShowDialog();

            //frmDatabase2 frm2 = new();
            //frm2.ShowDialog();

        }

        private void btCalcTemp2_Click(object sender, EventArgs e)
        {
            //Get All Multisensors
            neurodevicesTableAdapter.FillBy_Typ(_dsManufacturing.Neurodevices, enumModuleType.cModuleMultisensor.ToString());
            if (_dsManufacturing.Neurodevices.Count != 0)
            {
                foreach (dsManufacturing.NeurodevicesRow nrow in _dsManufacturing.Neurodevices)
                {
                    //Get related calibration data
                    neuromodule_KalibrierdatenTableAdapter.FillBy_SerialNo(_dsManufacturing.Neuromodule_Kalibrierdaten, nrow.SerialNumber);
                    dsManufacturing.Neuromodule_KalibrierdatenRow krow = (dsManufacturing.Neuromodule_KalibrierdatenRow)_dsManufacturing.Neuromodule_Kalibrierdaten.Rows[0];

                    //get related data
                    neuromodule_DatenTableAdapter.FillBy_id_neuromodule_kalibrierdaten(_dsManufacturing.Neuromodule_Daten, krow.id_neuromodule_kalibrierdaten);
                    dsManufacturing.Neuromodule_DatenRow drow = (dsManufacturing.Neuromodule_DatenRow)_dsManufacturing.Neuromodule_Daten.Rows[0];

                    double hex1 = (drow.Temp1 - krow.Offset_d_1) / krow.SkalValue_k_1 + krow.Offset_hex_1;  //Hex value from Temp1

                    double Multi_Temp1_soll = CMulti_Read_NM_base.Multi_Temp1_soll();
                    double Multi_Temp2_soll = CMulti_Read_NM_base.Multi_Temp2_soll();

                    //SkalValue_k = (Multi_Temp1_soll() - Multi_Temp2_soll()) / (hex1 - hex2);
                    double hex2 = -((Multi_Temp1_soll - Multi_Temp2_soll) / krow.SkalValue_k_1 - hex1);

                    //ScaledValue [V, °,...]= (HexValue-Offset_hex)*SkalValue_k+ Offset_d
                    drow.Temp2 = (hex2 - krow.Offset_hex_1) * krow.SkalValue_k_1 + krow.Offset_d_1;
                }
            }
        }

        private void dgvMeasurements_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (tabMeasurements.SelectedTab == tabMeasurements.TabPages["tabProgNeuroModul"] ||
                tabMeasurements.SelectedTab == tabMeasurements.TabPages["tabProgNeuroLink"])
            {
                for (int i = 0; i < measurements.Measurements_Items.Count; i++)
                {
                    Color bgcol = Color.White;
                    switch (measurements.Measurements_Items[i].ModuleTestResult)
                    {
                        case CBase_tests_measurements.enModuleTestResult.notChecked:
                            //bgcol = Color.White;
                            break;
                        case CBase_tests_measurements.enModuleTestResult.OK:
                            bgcol = Color.Green;
                            break;
                        case CBase_tests_measurements.enModuleTestResult.Fail:
                            bgcol = Color.Red;
                            break;
                        case CBase_tests_measurements.enModuleTestResult.Partially_Failed:
                            bgcol = Color.Orange;
                            break;
                        case CBase_tests_measurements.enModuleTestResult.Partially_OK:
                            bgcol = Color.Orange;
                            break;
                        case CBase_tests_measurements.enModuleTestResult.Fail_no_further_processing:
                            bgcol = Color.Red;
                            break;
                    }
                    dgvMeasurements.Rows[i].Cells[3].Style.BackColor = bgcol;
                }
            }
        }

        void dgvMeasurements_ResetColor()
        {
            for (int i = 0; i < measurements.Measurements_Items.Count; i++)
            {
                measurements.Measurements_Items[i].Reset();
            }

            for (int i = 0; i < dgvMeasurements.Rows.Count; i++)
            {
                for (int j = 0; j < dgvMeasurements.Rows[i].Cells.Count; j++)
                {
                    dgvMeasurements.Rows[i].Cells[j].Style.BackColor = Color.White;
                }
            }
        }

        private void btGetProgrammers_Click(object? sender, EventArgs? e)
        {
            CIPE_Base cIPE_Base = new();
            List<CMicrochip_Programmer> mp = cIPE_Base.Get_Available_Programmers();
            if (mp is not null && cbProgrammer is not null)
            {
                cbProgrammer.DataSource = mp;
                programmer_info = (CMicrochip_Programmer)cbProgrammer.SelectedItem;
            }
            else
            {
                txtStatus.AddStatusString("Could not find: ipecmd.exe", Color.Red);
            }
        }

        private void btShowRS232_Click(object sender, EventArgs e)
        {
            CFTDI_D2xx FTDI_D2xx = new ();
            int numDevices = FTDI_D2xx.CheckForConnectedDevices();  //fast

            txtStatus.Clear();
            if (numDevices != 0)
            {
                
                for (int i = 0; i < numDevices; i++)
                    txtStatus.AddStatusString(FTDI_D2xx.Description(i));
            }
            else
                txtStatus.AddStatusString("Keine RS232 Geräte gefunden");

        }

        private void dgvMeasurements_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e is not null && e.Exception is not null)
            { 
                txtStatus.AddStatusString("DataGrid Error: " + e.Exception.Message);
            }
        }

        bool bShow_Battery_Check = false;
        private void tmrGui_Tick(object sender, EventArgs e)
        {
            if (bShow_Battery_Check)
            {
                bShow_Battery_Check = false;
                Show_Battery_Check();
            }
        }

        private void Show_Battery_Check()
        {
            Application.DoEvents();
            if (tabMeasurements.SelectedTab == tabMeasurements.TabPages["tabProgNeuroModul"])
            {
                if (InsightModuleTestBoardV1 is not null && InsightModuleTestBoardV1.PhidgetIsConnected)
                {
                    CInsightModuleTester_Settings def = CInsightModuleTesterV1.Get_Default_Setting();
                    def.Uoff = CInsightModuleTester_Settings.enUoff.Uoff_On;
                    def.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_High;

                    if (!InsightModuleTestBoardV1.Init(def))
                        txtStatus.AddStatusString("Test-Board setting failed", Color.Red);

#if !DEBUG1
                    Update_from_Testboard(def);
                    frm_image_text fit = new frm_image_text(
                        "Offsetspannung überprüfen:",
                        "Offset_300mV_600.jpg",
                        "Bitte Batterien / Offsetspannung überprüfen (295mV... 305mV)");

                    fit.ShowDialog();

                    def.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low;
                    Update_from_Testboard(def);

                    fit = new frm_image_text(
                        "Offsetspannung überprüfen:",
                        "Offset_30mV_600.jpg",
                        "Bitte Batterien / Offsetspannung überprüfen (28mV... 32mV)");

                    fit.ShowDialog();
#endif
                    def = CInsightModuleTesterV1.Get_Default_Setting();
                    InsightModuleTestBoardV1.Init(def);
                    Update_from_Testboard(def);
                }
                else
                    txtStatus.AddStatusString("Batterie-Kontrollfenster nicht angezeigt da Phidget nicht verbunden", Color.Red);
            }
        }

        public string HexFilePath_with_SN
        {
            get
            {
                string ret = Dir_Results_Base + txtSerialNo.Text + @"\";   //12.5.2022
                if (!Directory.Exists(ret))
                    Directory.CreateDirectory(ret);
                ret += @"Combinedhex_w_SN.hex";
                return ret;
            }
        }


        /*
         * test measurement modules 
         * könnten in Process_Neuromodul auch lokal definiert werden
         * global nur wegen der Datenübergabe uc_Read_Neuromaster -> uc_Build_Calibrated_hex_flash in do_measurement_specific_stuff
         */
        CFlashNeuromodul flashNeuromodul;
        CFlashNeurolink flashNeurolink;
        CFlashNeuromodul_Calibrated flashNeuromodul_Calibrated;

        CRead_Neuromaster ucread;
        readonly List<CRead_Neuromaster> uccheck = new List<CRead_Neuromaster>();
        CRead_Neuromaster? ucagain = null;

        frmAmplitudeGain frmAgain;

        /// <summary>
        /// Path to original hex file
        /// </summary>
        public string HexFilePath = "";

        #region Neuromodule_Radio_Buttons

        private readonly List<myRadioButton> rbbModuleSelection = new List<myRadioButton>();
        private enumModuleType Selected_Module = enumModuleType.cModuleMultisensor;

        /// <summary>
        /// RadioButton with ModuleType Property
        /// </summary>
        /// <seealso cref="System.Windows.Forms.RadioButton" />
        private class myRadioButton : RadioButton
        {
            private enumModuleType _ModuleType;
            public enumModuleType ModuleType
            {
                get { return _ModuleType; }
                set { _ModuleType = value; }
            }
        }

        enumModuleType prevSelectedModule = enumModuleType.cModuleTypeEmpty;

        /// <summary>
        /// Handles the CheckedChanged event of the rbbModuleSelection
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void rb_CheckedChanged(object sender, EventArgs e)
        {
            Selected_Module = ((myRadioButton)sender).ModuleType;
            if (Selected_Module != prevSelectedModule)
            {
                prevSelectedModule = Selected_Module;
                Process_Neuromodul();
            }
        }

        public void Init_Neuromodul_Buttons()
        {
            //Setup Radio Buttons for Modules
            enumModuleType[] enVals = CManufacturing5_Config.get_enumModuleTypes_toEvaluate();          //(enumModuleType[])Enum.GetValues(typeof(enumModuleType));
            for (int i = 0; i < enVals.Length; i++)
            {
                enumModuleType mt = enVals[i];

                myRadioButton rb = new myRadioButton
                {
                    Text = mt.ToString().Replace("cModule", ""),
                    ModuleType = mt,
                    AutoSize = true
                };

                if (mt == enumModuleType.cNeuromaster)
                {
                    rb.Text = "NeuroMaster";
                    rb.BackColor = Color.LightBlue;
                }
                if (mt == enumModuleType.cNeurolink)
                {
                    rb.Text = "NeuroLink";
                    rb.BackColor = Color.LightGreen;
                }

                rb.CheckedChanged += new EventHandler(rb_CheckedChanged);
                rbbModuleSelection.Add(rb);
                flpModuleSelection.Controls.Add(rb);

            }
        }
        #endregion

        public void Process_Neuromodul()
        {
            //Check / Start Function Generator
            bool FY6900_is_OK = true;
            lblFWVersion.Text = lblFWVersionBasicText + "??";

            if (FY6900_is_OK)
            {
                //Setup List of Measurements
                measurements.Measurements_Items.Clear();

                //////////////////////////////////////
                //First we flash the combined hex file
                //////////////////////////////////////

                string s = Selected_Module.ToString().Replace("cModule", ""); //- Get the Path of the combined hex file

                frm_image_text fit = null;
                s = s.ToUpper();
                if (Selected_Module == enumModuleType.cNeuromaster)
                {
                    //"c:\Insight Manufacturing\Combined Hex\PIC24F_NEUROMASTER.hex" 
                    s = "Neuromaster";
                    fit = new frm_image_text(
                        "Nächster Schritt:",
                        "Prepare_NM.jpg",
                        "Programmierstecker und Neurolink mit Neuromasterplatine verbinden, Einschaltknopf während des Programmiervorgangs gedrueckt halten");

                }
                else
                {
                    //"c:\Insight Manufacturing\Combined Hex\PIC24F_ATEM_V40.hex"
                    if (s == "ATEM") s = "ATEM_";
                }

                string[] filePaths;
                if (Selected_Module == enumModuleType.cNeurolink)
                {
                    s = Selected_Module.ToString().Replace("c", "");
                    s = s.ToUpper();
                    filePaths = Directory.GetFiles(Dir_FTDI, "*" + s + "*.xml");
                }
                else
                {
                    filePaths = Directory.GetFiles(Dir_CombinedHex, "*" + s + "*.hex");
                }

                if (filePaths.Length == 1)  //only 1 file found?
                {
                    HexFilePath = filePaths[0];

                    CIPE_Neuromodul_PIC24 ICD3 = new CIPE_Neuromodul_PIC24(programmer_info);
                    Firmware_Version = ICD3.Get_SWVersion_from_hexFile(HexFilePath, Selected_Module);
                    lblFWVersion.Text = lblFWVersionBasicText + Firmware_Version;

                    if (Selected_Module == enumModuleType.cNeurolink)
                    {
                        if (flashNeurolink == null) flashNeurolink = new CFlashNeurolink();
                        measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                            "", true, true, flashNeurolink));
                    }
                    else
                    {
                        if (flashNeuromodul == null) flashNeuromodul = new CFlashNeuromodul(programmer_info);
                        flashNeuromodul.ConnectedModuleType = Selected_Module;
                        flashNeuromodul.Pre_Job_Message = "";
                        flashNeuromodul.Pre_Job_MessageBox = fit;
                        flashNeuromodul.HexFilePath_Base = HexFilePath;
                        //flashNeuromodul.HexFilePath_Target = HexFilePath_with_SN;

                        string nametoDisplay = "Flash default hex: " + Path.GetFileNameWithoutExtension(HexFilePath);

                        measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                            nametoDisplay, true, true, flashNeuromodul));
                    }


                    //////////////////////////////////////
                    //Now we measure the values via Neuromaster
                    //////////////////////////////////////
                    uccheck.Clear();

                    switch (Selected_Module)
                    {
                        case enumModuleType.cNeurolink:
                            {
                                flashNeuromodul_Calibrated = null;
                                ucread = null;
                                uccheck.Add(new CNL_Test_Cable_Connection(FY6900));
                                uccheck.Add(new CNL_Test_XBEE_Connection(FY6900));
                                break;
                            }
                        case enumModuleType.cNeuromaster:
                            {
                                flashNeuromodul_Calibrated = null;
                                ucread = null;
                                //ucread = new CNM_Test_Cable_Connection_pre(FY6900);
                                uccheck.Add(new CNM_Test_Cable_Connection(FY6900));
                                uccheck.Add(new CNM_Test_XBEE_Connection(FY6900));
                                uccheck.Add(new CNM_Test_All_HW_Channels(FY6900));
                                break;
                            }

                        case enumModuleType.cModuleECG:
                            {
                                ucread = new CECG_Read_NM_pre(FY6900);
                                uccheck.Add(new CECG_Read_NM_final(FY6900));
                                uccheck.Add(new CECG_Read_NM_Offset_Plus(FY6900));
                                uccheck.Add(new CECG_Read_NM_Offset_Minus(FY6900));
                                uccheck.Add(new CECG_AGain_Read_NM(FY6900));
                                break;
                            }
                        case enumModuleType.cModuleEMG:
                            {
                                ucread = new CEMG_Read_NM_pre(FY6900);
                                uccheck.Add(new CEMG_Read_NM_final(FY6900));
                                uccheck.Add(new CEMG_Read_NM_Offset_Plus(FY6900));
                                uccheck.Add(new CEMG_Read_NM_Offset_Minus(FY6900));
                                uccheck.Add(new CEMG_AGain_Read_NM(FY6900));
                                break;
                            }
                        case enumModuleType.cModuleEEG:
                            {
                                ucread = new CEEG_Read_NM_pre(FY6900);
                                uccheck.Add(new CEEG_Read_NM_final(FY6900));
                                uccheck.Add(new CEEG_Read_NM_Offset_Plus(FY6900));
                                uccheck.Add(new CEEG_Read_NM_Offset_Minus(FY6900));
                                break;
                            }
                        case enumModuleType.cModuleAtem:
                            {
                                ucread = new CAtem_Read_NM_pre(FY6900);
                                uccheck.Add(new CAtem_Read_NM_final(FY6900));
                                break;
                            }
                        case enumModuleType.cModuleAtemIRDig:
                            {
                                ucread = null;
                                break;
                            }
                        case enumModuleType.cModuleVasosensorDig:
                            {
                                ucread = null;
                                break;
                            }
                        case enumModuleType.cModuleMultisensor:
                            {
                                flashNeuromodul.Pre_Job_Message = "Multisensor mit Programmer verbinden";
                                ucread = new CMulti_SCL1_2_Temp1_2_Read_NM(FY6900);
                                uccheck.Add(new CMulti_SCL1_2_Temp1_2_Read_NM(FY6900, "Multi_final"));
                                uccheck.Add(new CMulti_Pulse(FY6900));
                                uccheck.Add(new CMulti_SCL_Sensor(FY6900)); //Check mit 1MOhm Widerstand
                                uccheck.Add(new CMulti_Temp(FY6900));
                                break;
                            }
                    }


                    //////////////////////////////////////
                    //First read
                    //////////////////////////////////////

                    if (ucread != null)
                    {
                        ucread.ModuleInfoAvailable -= Ucread_ModuleInfoAvailable;
                        ucread.ModuleInfoAvailable += Ucread_ModuleInfoAvailable;

                        ucread.Udpate_id_neuromodule_kalibrierdaten -= Ucread_Udpate_id_neuromodule_kalibrierdaten;
                        ucread.Udpate_id_neuromodule_kalibrierdaten += Ucread_Udpate_id_neuromodule_kalibrierdaten;

                        ucread.DataReady_xy -= Ucread_DataReady_xy;
                        ucread.DataReady_xy += Ucread_DataReady_xy;

                        ucread.ConnectedModuleType = Selected_Module;
                        ucread.SerialNumber = txtSerialNo.Text;
                        ucread.my_name = "read";
                        ucread.Path_to_save_xml = HexFilePath;   //To save calibration xml

                        measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                            "Check Neuromodule", true,
                            true, ucread));

                        //Generate combined hex file and flash it
                        switch (Selected_Module)
                        {
                            case enumModuleType.cModuleEEG:
                            case enumModuleType.cModuleEMG:
                            case enumModuleType.cModuleECG:
                            case enumModuleType.cModuleAtem:
                            case enumModuleType.cModuleMultisensor:
                                {
                                    if (flashNeuromodul_Calibrated == null) flashNeuromodul_Calibrated = new CFlashNeuromodul_Calibrated(programmer_info);
                                    flashNeuromodul_Calibrated.ConnectedModuleType = Selected_Module;
                                    flashNeuromodul_Calibrated.SerialNumber = txtSerialNo.Text;

                                    flashNeuromodul_Calibrated.Pre_Job_Message = flashNeuromodul.Pre_Job_Message;

                                    flashNeuromodul_Calibrated.HexFilePath_Base = HexFilePath;

                                    measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                                        "Build hex and flash", true,
                                        true, flashNeuromodul_Calibrated));
                                    break;
                                }
                        }
                    }

                    //////////////////////////////////////
                    //Check final flashed module
                    //////////////////////////////////////
                    if (uccheck != null)
                    {
                        for (int i = 0; i < uccheck.Count; i++)
                        {
                            uccheck[i].ModuleInfoAvailable -= Ucread_ModuleInfoAvailable;
                            uccheck[i].ModuleInfoAvailable += Ucread_ModuleInfoAvailable;

                            uccheck[i].Udpate_id_neuromodule_kalibrierdaten -= Ucread_Udpate_id_neuromodule_kalibrierdaten;
                            uccheck[i].Udpate_id_neuromodule_kalibrierdaten += Ucread_Udpate_id_neuromodule_kalibrierdaten;

                            uccheck[i].DataReady_xy -= Ucread_DataReady_xy;
                            uccheck[i].DataReady_xy += Ucread_DataReady_xy;

                            uccheck[i].ConnectedModuleType = Selected_Module;
                            uccheck[i].SerialNumber = txtSerialNo.Text;
                            if (uccheck[i].my_name == "")
                                uccheck[i].my_name = "check";

                            uccheck[i].Dir_Results_Base = Dir_Results_Base;

                            if (uccheck[i].GetType() == typeof(CECG_AGain_Read_NM) ||
                                uccheck[i].GetType() == typeof(CEEG_AGain_Read_NM) ||
                                uccheck[i].GetType() == typeof(CEMG_AGain_Read_NM))
                            {
                                uccheck[i].ModulePortNo = 0;
                                uccheck[i].Pre_Job_Message = "";
                                uccheck[i].UdpateAmplitudeGain -= Ucagain_UdpateAmplitudeGain;
                                uccheck[i].UdpateAmplitudeGain += Ucagain_UdpateAmplitudeGain;

                                measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                                    "Amplitudengain", true, true, uccheck[i]));
                            }
                            else
                            {
                                string display_txt = "Check final flashed module";
                                if (Selected_Module == enumModuleType.cNeuromaster)
                                {
                                    display_txt = "Check NeuroMaster";
                                }
                                else if (Selected_Module == enumModuleType.cNeurolink)
                                {
                                    display_txt = "Check NeuroLink";
                                }
                                measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                                display_txt, true, true, uccheck[i]));
                            }
                        }
                    }

                    //////////////////////////////////////
                    //Amplitudengain
                    //////////////////////////////////////

                    if (ucagain != null)
                    {
                        //We can perform Amplitude Gain

                        ucagain.ConnectedModuleType = Selected_Module;

                        ucagain.ModuleInfoAvailable -= Ucread_ModuleInfoAvailable;
                        ucagain.ModuleInfoAvailable += Ucread_ModuleInfoAvailable;

                        ucagain.DataReady_xy -= Ucread_DataReady_xy;
                        ucagain.DataReady_xy += Ucread_DataReady_xy;

                        ucagain.ConnectedModuleType = Selected_Module;
                        ucagain.SerialNumber = txtSerialNo.Text;
                        ucagain.my_name = "Amplitudegain";
                        ucagain.ModulePortNo = 0;
                        ucagain.Pre_Job_Message = "";

                        ucagain.UdpateAmplitudeGain -= Ucagain_UdpateAmplitudeGain;
                        ucagain.UdpateAmplitudeGain += Ucagain_UdpateAmplitudeGain;

                        ucagain.HexFilePath_Target = Dir_Results_Base + txtSerialNo.Text;

                        measurements.Measurements_Items.Add(new CMeasurementItem(measurements.Measurements_Items.Count,
                            "Amplitudengain", true,
                            true, ucagain));
                    }
                    Update_dgvMeasurments();
                }
                else if (filePaths.Length == 0)
                {
                    txtStatus.AddStatusString("No combined hex file " + s);
                }
                else
                {

                    txtStatus.AddStatusString("Too many combined hex files found: File containing " + s);
                }
            }   //FY6900 failed
            else
            {
                txtStatus.AddStatusString("Verbindung zum FY6900 (Funktionsgenerator) verloren", Color.Red);
            }
        }

        private void Ucread_ModuleInfoAvailable2(object sender, CModuleBase ModuleInfo)
        {
            throw new NotImplementedException();
        }

        private void Ucread_ModuleInfoAvailable1(object sender, CModuleBase ModuleInfo)
        {
            throw new NotImplementedException();
        }

        /* Scanning über timer gelöst, da Main Thread in Idle Mode gehen muss damit die geschlossenen Threads
* auch wirklich dicht machen
*/
        /// <summary>
        /// This timer event call measurement object after object
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TmrMeasurementManager_Tick(object sender, EventArgs e)
        {
            CBase_tests_measurements.enModuleTestResult check_result = CBase_tests_measurements.enModuleTestResult.notChecked;
            if (measurement_finished)
            {
                cnt_scan++;
                if (cnt_scan >= measurements.Measurements_Items.Count)
                {
                    //measurement_finished = true;
                    tmrMeasurementManager.Enabled = false;

                    //Check Final result
                    bool module_OK = true;
                    bool all_checks_performed = false;
                    int num_not_checked = 0;
                    int num_OK = 0;
                    int num_Fail = 0;
                    //Falls nur geflasht wird
                    CBase_tests_measurements.enModuleTestResult ModuleTestResult = Current_CMeasurementItem.Measurement_Object.ModuleTestResult;
                    if (ucread != null) //Module die nur geflasht werden haben kein ucread
                    {
                        ModuleTestResult = ucread.ModuleTestResult;

                        if (ModuleTestResult != CBase_tests_measurements.enModuleTestResult.Fail_no_further_processing)
                        {
                            for (int i = 0; i < uccheck.Count; i++)
                            {
                                num_OK++;
                                if (uccheck[i].ModuleTestResult != CBase_tests_measurements.enModuleTestResult.OK)
                                {
                                    num_OK--;
                                    num_Fail++;
                                    module_OK = false;
                                    if (uccheck[i].ModuleTestResult == CBase_tests_measurements.enModuleTestResult.notChecked)
                                    {
                                        num_Fail--;
                                        num_not_checked++;
                                    }
                                }
                            }

                            int num_checked = uccheck.Count - num_not_checked;
                            if (num_checked == num_OK + num_Fail)
                                all_checks_performed = true;       //17.7.2023 -> only checked tests are evaluated

                            if (num_checked == num_OK)
                            {
                                //alle aktivierten tests waren OK 
                                module_OK = true;
                            }

                            if (num_not_checked == uccheck.Count)
                            {
                                //No checks at all
                                ModuleTestResult = CBase_tests_measurements.enModuleTestResult.Fail;
                                module_OK = false;
                                txtStatus.AddStatusString("No final checks done!", Color.Red);
                            }
                            else if (all_checks_performed)
                            {
                                if (module_OK)
                                    ModuleTestResult = CBase_tests_measurements.enModuleTestResult.OK;
                                else
                                {
                                    ModuleTestResult = CBase_tests_measurements.enModuleTestResult.Partially_Failed;
                                    txtStatus.AddStatusString(num_OK.ToString() + " Checks OK, " + num_Fail.ToString() + " Checks failed", Color.Red);
                                }
                            }
                            else
                            {
                                ModuleTestResult = CBase_tests_measurements.enModuleTestResult.Partially_Failed;
                                if (module_OK)
                                    ModuleTestResult = CBase_tests_measurements.enModuleTestResult.Partially_OK;
                                txtStatus.AddStatusString("Only " + (num_checked - num_OK).ToString() + " out of " + num_checked.ToString() + " Tests done", Color.Red);
                            }
                        }
                    }
                    else
                    {
                        //ucread not available
                        module_OK = false;
                        if (check_result == CBase_tests_measurements.enModuleTestResult.OK)
                            module_OK = true;
                    }

                    Save_OKState_to_DB(ModuleTestResult, txtSerialNo.Text);

                    Set_lblResult(ModuleTestResult, true);
                    En_Dis_Gui(false);

                    //Play sound
                    SoundPlayer simpleSound = null;
#if !DEBUG1
                    string sndfile = @"\fail.wav";
                    if (ModuleTestResult == CBase_tests_measurements.enModuleTestResult.OK)
                    {
                        sndfile = @"\fanfare10.wav";
                    }
                    simpleSound = new SoundPlayer(Path.GetDirectoryName(
                        System.Reflection.Assembly.GetEntryAssembly().Location) + sndfile);
                    simpleSound.Play();
#endif


                    //Save Status Window
                    try
                    {
                        string p = Dir_Results_Base + txtSerialNo.Text + @"\";
                        string f = p + @"log.rtf";
                        if (!Directory.Exists(p))
                            Directory.CreateDirectory(p);

                        RichTextBox rt = new RichTextBox();
                        rt.Clear();
                        if (File.Exists(f))
                        {
                            //Load old file
                            rt.LoadFile(f);
                        }
                        txtStatus.SelectAll();
                        txtStatus.Copy();
                        if (rt.Text.Length > 0)
                        {
                            rt.Select(rt.Text.Length - 1, 0);   //Set Cursor to end
                        }
                        rt.AppendText("\r\n");
                        rt.Paste();
                        rt.SaveFile(f, RichTextBoxStreamType.RichText);
                    }
                    catch (Exception ee)
                    {
                        txtStatus.AddStatusString(ee.Message, Color.Red);
                    }
                }
                else
                {
                    ////////////////////////////////////////////////
                    /// here we do the measuring job
                    ////////////////////////////////////////////////

                    if (measurements.Measurements_Items[cnt_scan].Measurement_Active == true)
                    {
                        //Do the job
                        measurement_finished = false;
                        Current_CMeasurementItem = measurements.Measurements_Items[cnt_scan];


                        if (Current_CMeasurementItem.Measurement_Object.InsightModuleTester_Settings != null)
                        {
                            //Set Testboard
                            InsightModuleTestBoardV1.Init(Current_CMeasurementItem.Measurement_Object.InsightModuleTester_Settings);
                            Current_CMeasurementItem.Measurement_Object.InsightModuleTestBoardV1 = InsightModuleTestBoardV1;
                        }

                        if (Current_CMeasurementItem.Measurement_Object.Pre_Job_Message != "")
                        {
                            MessageBox.Show(Current_CMeasurementItem.Measurement_Object.Pre_Job_Message, "Nächster Schritt:", MessageBoxButtons.OK);
                        }

                        Current_CMeasurementItem.Measurement_Object.Pre_Job_MessageBox?.ShowDialog();

                        txtStatus.AddStatusString(Current_CMeasurementItem.Measurement_Object.Job_Message + ": started", Color.Blue);

                        Current_CMeasurementItem.Measurement_Object.ReportMeasurementProgress -= Measurement_Object_ReportMeasurementProgress;  //just to make sure there is no double subscription
                        Current_CMeasurementItem.Measurement_Object.ReportMeasurementProgress += Measurement_Object_ReportMeasurementProgress;
                        Current_CMeasurementItem.Measurement_Object.SerialNumber = txtSerialNo.Text;

                        //Do whatever initialisation is necessary

                        Current_CMeasurementItem.Measurement_Object.MeasurementStarted = dtAllMeasurementsStarted; //if == null DateTime.Now will be used
                        Current_CMeasurementItem.Measurement_Object.SerialNumber = txtSerialNo.Text;

                        //Perform Measurement
                        dtSingleMeasurementStarted = DateTime.Now;
                        bool meas_result = Current_CMeasurementItem.Measurement_Object.Perform_Measurement(cbIgnoreSerialNumberCheck.Checked);

                        if (!meas_result)
                        {
                            //There was an error, stop entire Measurement
                            cnt_scan = measurements.Measurements_Items.Count - 1;
                            measurement_finished = true;
                        }
                        else
                        {
                            //Messung hat prinzipiell funktioniert ... aber stimmen auch die Messdaten?
                            check_result = Measurement_Object_MeasurementFinished(Current_CMeasurementItem.Measurement_Object); //calls also Check_if_Neuromodul_OK()

                            if (check_result == CBase_tests_measurements.enModuleTestResult.Fail_no_further_processing)
                            {
                                //Major error -> stop measurement
                                cnt_scan = measurements.Measurements_Items.Count - 1;
                                measurement_finished = true;
                                txtStatus.AddStatusString("Pre-check failed, cannot process further", Color.Red);
                            }
                        }
                    }
                }
            }
        }



        private void Ucread_Udpate_id_neuromodule_kalibrierdaten(object sender, Guid id_neuromodule_kalibrierdaten)
        {
            if (uccheck != null)
            {
                for (int i = 0; i < uccheck.Count; i++)
                {
                    uccheck[i].last_id_neuromodule_kalibrierdaten = id_neuromodule_kalibrierdaten;
                }
            }
            if (ucagain != null)
            {
                ucagain.last_id_neuromodule_kalibrierdaten = id_neuromodule_kalibrierdaten;
            }
        }

        delegate void Ucagain_UdpateAmplitudeDelegate(object sender, double f, double v_db, double ueff_out);

        private void Ucagain_UdpateAmplitudeGain(object sender, double f, double v_db, double ueff_out)
        {
            UdpateAmplitudeGain(f, v_db, ueff_out);
        }
        delegate void UdpateAmplitudeDelegate(double f, double v_db, double ueff_out);


        private void UdpateAmplitudeGain(double f, double v_db, double ueff_out)
        {
            if (InvokeRequired)
            {
                Invoke(new UdpateAmplitudeDelegate(UdpateAmplitudeGain), new object[] { f, v_db, ueff_out });
            }
            else
            {
                //Update Chart
                if (frmAgain != null)
                {
                    frmAgain.Show();
                    frmAgain.UdpateAmplitudeGain(f, v_db);
                }
            }
        }


        /// <summary>
        /// Module Info available, Set Chart
        /// </summary>
        private void Ucread_ModuleInfoAvailable(object sender, CModuleBase ModuleInfo)
        {
            Setup_NeuromoduleGraph(sender, ModuleInfo);
        }

        delegate void Setup_NeuromoduleGraphDelegate(object sender, CModuleBase ModuleInfo);
        void Setup_NeuromoduleGraph(object sender, CModuleBase ModuleInfo)
        {
            CRead_Neuromaster crnm = (CRead_Neuromaster)sender;
            if (cFlowChartDX1.InvokeRequired)
            {
                Invoke(new Setup_NeuromoduleGraphDelegate(Setup_NeuromoduleGraph), new object[] { sender, ModuleInfo });
            }
            else
            {
                //FlowChart
                //cFlowChartDX1.SetupFlowChart_for_Module(ModuleInfo);
                //cFlowChartDX1.InitChart();
                //cFlowChartDX1.XFormatString = "hh:mm:ss";
                //cFlowChartDX1.Visible = true;
                //cFlowChartDX1.Enabled = true;
                //cFlowChartDX1.Start(CFlowChartDX.enumAutoRenderer.Start_Auto_Renderer, DateTime.Now);
            }
        }

        private void Ucread_DataReady_xy(object sender, double y_calibrated, WindControlLib.CDataIn DataIn)
        {
            Update_NeuromoduleGraph(sender, y_calibrated, DataIn);
        }

        delegate void Update_NeuromoduleGraphDelegate(object sender, double y_calibrated, WindControlLib.CDataIn DataIn);
        void Update_NeuromoduleGraph(object sender, double y_calibrated, WindControlLib.CDataIn DataIn)
        {

            if (cFlowChartDX1.InvokeRequired)
            {
                Invoke(new Update_NeuromoduleGraphDelegate(Update_NeuromoduleGraph), new object[] { sender, y_calibrated, DataIn });
            }
            else
            {
                try
                {
                    CYvsTimeData c = new CYvsTimeData(1)
                    {
                        xData = DataIn.DT_absolute
                    };
                    if (((CRead_Neuromaster)(Current_CMeasurementItem.Measurement_Object)).DataReceiver != null)
                    {
                        c.yData[0] = ((CRead_Neuromaster)(Current_CMeasurementItem.Measurement_Object)).DataReceiver.Connection.GetScaledValue(DataIn);
                        //cFlowChartDX1.AddValue(DataIn.SWChannelNumber, c);
                        cFlowChartDX1.AddValue(c, DataIn);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Posts the process neuromodul.
        /// </summary>
        /// <returns>Module was processes</returns>
        public CBase_tests_measurements.enModuleTestResult Post_Process_Neuromodul(CBase_tests_measurements Current_Measurement_object)
        {
            CBase_tests_measurements.enModuleTestResult ret = CBase_tests_measurements.enModuleTestResult.notChecked;
            if (Current_Measurement_object is CRead_Neuromaster cRead_Neuromaster)
            {
                ret = cRead_Neuromaster.isModule_OK();
                if (measurements.Measurements_Items[cnt_scan].Measurement_SavetoDB)
                {
                    cRead_Neuromaster.Save_Results_to_DB(); //Also saves calibration data; Sets Was_Checked -> true 
                    cRead_Neuromaster.Add_TestDetails_toDB(cRead_Neuromaster.Test_Details);
                }

                Set_lblResult(ret, false);
                //                txtStatus.AddStatusString(Current_CMeasurementItem.Measurement_Object.Test_Details.Replace(";", Environment.NewLine), Color.DarkViolet);
                AddStatusString_OK_Fail(Current_CMeasurementItem.Measurement_Object.Test_Details, Color.DarkViolet);

                if (cRead_Neuromaster.DataConnection_Required)
                {
                    if (flashNeuromodul_Calibrated != null)
                    {
                        //Provide SWChannel Info - with new calibrationon values
                        flashNeuromodul_Calibrated.NewCalibrationValues = cRead_Neuromaster.Get_Calibrated_SWChannelInfo();
                    }
                }
            }
            else if (Current_Measurement_object is CFlashNeuromodul_Calibrated)
            {
            }
            else if (Current_Measurement_object is CFlashNeuromodul)
            {
            }

            Application.DoEvents(); //Damit letzte werte in Data_Ready abgearbeitet weden
            Application.DoEvents();
            return ret;
        }

        void Set_lblResult(CBase_tests_measurements.enModuleTestResult ModuleTestResult, bool isFinal)
        {
            string text = CBase_tests_measurements.Get_TestString(ModuleTestResult);
            switch (ModuleTestResult)
            {
                case CBase_tests_measurements.enModuleTestResult.OK:
                    lblResult.BackColor = Color.Green;
                    break;
                case CBase_tests_measurements.enModuleTestResult.Fail:
                    lblResult.BackColor = Color.Red;
                    break;
                case CBase_tests_measurements.enModuleTestResult.Partially_OK:
                    lblResult.BackColor = Color.Orange;
                    break;
                case CBase_tests_measurements.enModuleTestResult.Partially_Failed:
                    lblResult.BackColor = Color.Red;
                    break;
                case CBase_tests_measurements.enModuleTestResult.notChecked:
                    lblResult.BackColor = Color.Red;
                    break;
                case CBase_tests_measurements.enModuleTestResult.Fail_no_further_processing:
                    lblResult.BackColor = Color.Red;
                    break;
            }

            if (isFinal)
            {
                lblResult.Text = "Device: " + text;
            }
            else
            {
                if (lblResult.BackColor == Color.Green) lblResult.BackColor = Color.LightGreen;
                lblResult.Text = measurements.Measurements_Items[cnt_scan].Measurement_Name + ": " + text;
            }
            Update_dgvMeasurments();
        }

        private void Nhx_ReportMeasurementProgress(object sender, string text, Color col)
        {
            txtStatus.AddStatusString(text, col);
        }


        public void Save_OKState_to_DB(CBase_tests_measurements.enModuleTestResult ModuleTestResult, string SerialNumber)
        {
            try
            {
                dsManufacturing _dsManufacturing = new dsManufacturing();
                dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();

                neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
                if (_dsManufacturing.Neurodevices.Count == 1)
                {
                    _dsManufacturing.Neurodevices[0].TestOK = CBase_tests_measurements.Get_TestString(ModuleTestResult);
                    _dsManufacturing.Neurodevices[0].Testdatum = DateTime.Now;
                    neurodevicesTableAdapter.Update(_dsManufacturing.Neurodevices);
                }
            }
            catch (Exception ee)
            {
                txtStatus.AddStatusString("Failed to save final State in DB: " + ee.Message, Color.Red);
            }
        }

        /// <summary>
        /// Measurements object has finished
        /// </summary>
        /// <param name="sender">The sender.</param>
        private CBase_tests_measurements.enModuleTestResult Measurement_Object_MeasurementFinished(object sender)
        {
            CBase_tests_measurements.enModuleTestResult ret = do_measurement_specific_stuff((CBase_tests_measurements)sender);
            measurement_finished = true;
            double d = (DateTime.Now - dtSingleMeasurementStarted).TotalMilliseconds;
            d /= 1000;
            Current_CMeasurementItem.Measurement_Duration = string.Format("{0:0.00}", d);
            Current_CMeasurementItem.Measurement_done = true;
            Update_dgvMeasurments();
            return ret;
        }


        /// <summary>
        /// Only place where we do specific stuff
        /// </summary>
        CBase_tests_measurements.enModuleTestResult do_measurement_specific_stuff(CBase_tests_measurements Current_Measurement_object)
        {
            CBase_tests_measurements.enModuleTestResult ret = CBase_tests_measurements.enModuleTestResult.notChecked;
            if (txtSerialNo.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    do_measurement_specific_stuff(Current_Measurement_object);
                }));

            }
            else
            {
                if (Selected_Module.ToString().Contains("cModule"))
                {
                    ret = Post_Process_Neuromodul(Current_Measurement_object);
                    if (ret != CBase_tests_measurements.enModuleTestResult.OK)
                    {
                    }
                }
                else if (Selected_Module == enumModuleType.cNeurolink)
                {
                    ret = Current_Measurement_object.Post_Process();
                }
                else if (Selected_Module == enumModuleType.cNeuromaster)
                {
                   ret = Current_Measurement_object.Post_Process();
                }
                else
                {
                    //kA
                }
            }
            return ret;
        }
        private void Measurement_Object_ReportMeasurementProgress(object sender, string text, Color col)
        {
            AddStatusString_OK_Fail(text, col);
        }

        public void AddStatusString_OK_Fail(string text, Color col)
        {
            string[] str = text.Split(';');
            foreach (string s in str)
            {
                if (s.Contains(CBase_tests_measurements.Test_success))
                    txtStatus.AddStatusString(s, Color.Green);
                else if (s.Contains(CBase_tests_measurements.Test_failed))
                    txtStatus.AddStatusString(s, Color.Red);
                else
                    txtStatus.AddStatusString(s, col);
            }
        }

        #region FY6900_Routines
        private void CToggleButton_COM_ToState2(object sender, EventArgs e)
        {
            //OpenCOM
            FY6900.Open(cbComPortSelector.Text);
            if (!FY6900.IsOpen)
            {
                ((CToggleButton)sender).AcceptChange = false;
                txtStatus.AddStatusString("Failed to open "+ cbComPortSelector.Text, Color.Red);
            }
            txtStatus.AddStatusString(cbComPortSelector.Text + " is open", Color.Green);
        }

        private void cToggleButton_COM_ToState1(object sender, EventArgs e)
        {
            FY6900.Close();
            txtStatus.AddStatusString(FY6900.ComPort + " closed." , Color.Green);
        }

        private void btSetSinus_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: Sinus ";
            if (FY6900.SetSinus(true))
            {
                int i = FY6900.GetWaveform();
                if (i == 0)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_002_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_002 ";
            if (FY6900.SetArbitrary(1, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 36)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_003_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_003 ";
            if (FY6900.SetArbitrary(2, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 37)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_004_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_004 ";
            if (FY6900.SetArbitrary(3, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 38)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_005_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_005 ";
            if (FY6900.SetArbitrary(4, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 39)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void setFrequency_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: f ";
            if (FY6900.SetFrequency((double)numericUpDownFrequqency.Value, true))
            {
                double i = FY6900.GetFrequency();
                if (i > -1)
                    stat += "OK " + i.ToString();
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);

        }

        private void btSetAmplitude_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: Vss ";
            if (FY6900.SetVss((double)numericUpDownAmplitude.Value, true))
            {
                double i = FY6900.GetVss();
                if (i > -1)
                    stat += "OK " + i.ToString();
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        public CInsightModuleTester_Settings Settings_To_Testboard()
        {
            CInsightModuleTester_Settings ret = new();
            if (cbPhi_ICDOn.Checked)
            {
                //ICD on
                ret.ICD_State = CInsightModuleTester_Settings.enICD.ICD_Connected;
            }
            else
            {
                //ICD off
                ret.ICD_State = CInsightModuleTester_Settings.enICD.ICD_DisConnected;
            }

            if (cbOffsetOn.Checked)
            {
                //Offset on
                ret.Uoff = CInsightModuleTester_Settings.enUoff.Uoff_On;
            }
            else
            {
                //Offset off
                ret.Uoff = CInsightModuleTester_Settings.enUoff.Uoff_Off;
            }

            if (cbOffsetPlus.Checked)
            {
                //Offset Plus
                ret.UoffPolarity = CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus;
            }
            else
            {
                //Offset Minus
                ret.UoffPolarity = CInsightModuleTester_Settings.enUoffPolarity.Polarity_Minus;
            }

            if (cbEEGOn.Checked)
            {
                //EEG on
                ret.EEG = CInsightModuleTester_Settings.enEEG.EEG_On;
            }
            else
            {
                //EEG off
                ret.EEG = CInsightModuleTester_Settings.enEEG.EEG_Off;
            }

            if (rbOffsetLow.Checked)
            {
                ret.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low;
            }

            if (rbOffsetHigh.Checked)
            {
                ret.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_High;
            }

            _ = InsightModuleTestBoardV1.Init(ret);

            return ret;
        }

        private delegate void Update_from_TestboardDelegate(CInsightModuleTester_Settings settings);
        public void Update_from_Testboard(CInsightModuleTester_Settings settings)
        {
            if (cbPhi_ICDOn.InvokeRequired)
            {
                Invoke(
                    new Update_from_TestboardDelegate(Update_from_Testboard),
                    [settings]);
            }
            else
            {
                UpdateBoard = false;
                if (settings.ICD_State == CInsightModuleTester_Settings.enICD.ICD_Connected)
                    cbPhi_ICDOn.Checked = true;
                else
                    cbPhi_ICDOn.Checked = false;

                if (settings.Uoff == CInsightModuleTester_Settings.enUoff.Uoff_On)
                    cbOffsetOn.Checked = true;
                else
                    cbOffsetOn.Checked = false;

                if (settings.UoffPolarity == CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus)
                    cbOffsetPlus.Checked = true;
                else
                    cbOffsetPlus.Checked = false;

                if (settings.EEG == CInsightModuleTester_Settings.enEEG.EEG_On)
                    cbEEGOn.Checked = true;
                else
                    cbEEGOn.Checked = false;

                if (settings.UoffLevel == CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low)
                    rbOffsetLow.Checked = true;
                else
                    rbOffsetHigh.Checked = true;

                UpdateBoard = true;
            }
        }

        bool UpdateBoard = true;

        private void cbPhi_ICDON_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbOffsetOn_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbOffsetPlus_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbEEGOn_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void rbOffsetLow_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void rbOffsetHigh_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        #endregion
    }
}



