using FeedbackDataLibGUI;

namespace Neuromaster_V5
{
    partial class NeuromasterV5
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuromasterV5));
            System.Text.ASCIIEncoding asciiEncodingSealed2 = new System.Text.ASCIIEncoding();
            System.Text.DecoderReplacementFallback decoderReplacementFallback2 = new System.Text.DecoderReplacementFallback();
            System.Text.EncoderReplacementFallback encoderReplacementFallback2 = new System.Text.EncoderReplacementFallback();
            btGetConfigModules = new Button();
            btSetConfig = new Button();
            btGetClock = new Button();
            btSetClock = new Button();
            pnControls = new Panel();
            btGetElectrodeInfo = new Button();
            cChannelsControlV2x11 = new CChannelsControlV2x1();
            btGetModuleSpecific = new Button();
            btSetModuleSpecific = new Button();
            btGetFirmwareVersion = new Button();
            AllDefaultOn = new Button();
            btGetSDCardInfo = new Button();
            btAllCnhan0On = new Button();
            gbClock = new GroupBox();
            txtTime = new TextBox();
            btLoadConfig = new Button();
            btSaveConfig = new Button();
            btRestoreLastConfig = new Button();
            btSetAllConfig = new Button();
            gbConnectivitie = new GroupBox();
            txtBattery = new RichTextBox();
            pbXBEESignalStrength = new ProgressBar();
            label1 = new Label();
            lblXBeeCapacity = new Label();
            label2 = new Label();
            pbXBeeChannelCapacity = new ProgressBar();
            txtStatus = new RichTextBox();
            tbConnect = new ComponentsLib_GUI.CToggleButton();
            groupBox2 = new GroupBox();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            saveFileDialog_xml = new SaveFileDialog();
            openFileDialog_xml = new OpenFileDialog();
            serialPort1 = new System.IO.Ports.SerialPort(components);
            tmrStatusMessages = new System.Windows.Forms.Timer(components);
            tmrSDDataReader = new System.Windows.Forms.Timer(components);
            folderBrowserDialog1 = new FolderBrowserDialog();
            menuStrip1 = new MenuStrip();
            connectionToolStripMenuItem = new ToolStripMenuItem();
            d2XXToolStripMenuItem = new ToolStripMenuItem();
            virtualComToolStripMenuItem = new ToolStripMenuItem();
            sDCardToolStripMenuItem = new ToolStripMenuItem();
            displayedChannelsToolStripMenuItem = new ToolStripMenuItem();
            selectedModuleToolStripMenuItem = new ToolStripMenuItem();
            rawChannelsToolStripMenuItem = new ToolStripMenuItem();
            predefinedChannelsToolStripMenuItem = new ToolStripMenuItem();
            scaleValuesToolStripMenuItem = new ToolStripMenuItem();
            scaledToolStripMenuItem = new ToolStripMenuItem();
            unscaledToolStripMenuItem = new ToolStripMenuItem();
            timebaseToolStripMenuItem = new ToolStripMenuItem();
            absolutToolStripMenuItem = new ToolStripMenuItem();
            relativeToolStripMenuItem = new ToolStripMenuItem();
            resyncTimeBaseToolStripMenuItem = new ToolStripMenuItem();
            fTDIToolStripMenuItem = new ToolStripMenuItem();
            statusToolStripMenuItem = new ToolStripMenuItem();
            resetDeviceToolStripMenuItem = new ToolStripMenuItem();
            resetPortToolStripMenuItem = new ToolStripMenuItem();
            cyclePortToolStripMenuItem = new ToolStripMenuItem();
            sDCardToolStripMenuItemTop = new ToolStripMenuItem();
            readBackToolStripMenuItem = new ToolStripMenuItem();
            convertToTxtToolStripMenuItem = new ToolStripMenuItem();
            saveDataToFileToolStripMenuItem = new ToolStripMenuItem();
            saveDataToFileToolStripMenuItem1 = new ToolStripMenuItem();
            readDataFromFileToolStripMenuItem = new ToolStripMenuItem();
            openFileDialog_cfg = new OpenFileDialog();
            tmrFileDataReader = new System.Windows.Forms.Timer(components);
            tlpMeasure = new TableLayoutPanel();
            ucSignalAnalyser1 = new FeedbackDataLib_GUI.UcSignalAnalyser();
            openFileDialog_nmc = new OpenFileDialog();
            panel1 = new Panel();
            cFlowChartDX1 = new UcFlowChartDX_NM();
            pnControls.SuspendLayout();
            gbClock.SuspendLayout();
            gbConnectivitie.SuspendLayout();
            groupBox2.SuspendLayout();
            menuStrip1.SuspendLayout();
            tlpMeasure.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btGetConfigModules
            // 
            resources.ApplyResources(btGetConfigModules, "btGetConfigModules");
            btGetConfigModules.Name = "btGetConfigModules";
            btGetConfigModules.UseVisualStyleBackColor = true;
            btGetConfigModules.Click += BtGetConfigModules_Click;
            // 
            // btSetConfig
            // 
            resources.ApplyResources(btSetConfig, "btSetConfig");
            btSetConfig.Name = "btSetConfig";
            btSetConfig.UseVisualStyleBackColor = true;
            btSetConfig.Click += BtSetConfig_Click;
            // 
            // btGetClock
            // 
            resources.ApplyResources(btGetClock, "btGetClock");
            btGetClock.Name = "btGetClock";
            btGetClock.UseVisualStyleBackColor = true;
            btGetClock.Click += BtGetClock_Click;
            // 
            // btSetClock
            // 
            resources.ApplyResources(btSetClock, "btSetClock");
            btSetClock.Name = "btSetClock";
            btSetClock.UseVisualStyleBackColor = true;
            btSetClock.Click += BtSetClock_Click;
            // 
            // pnControls
            // 
            pnControls.Controls.Add(btGetElectrodeInfo);
            pnControls.Controls.Add(cChannelsControlV2x11);
            pnControls.Controls.Add(btGetModuleSpecific);
            pnControls.Controls.Add(btSetModuleSpecific);
            pnControls.Controls.Add(btGetFirmwareVersion);
            pnControls.Controls.Add(AllDefaultOn);
            pnControls.Controls.Add(btGetSDCardInfo);
            pnControls.Controls.Add(btAllCnhan0On);
            pnControls.Controls.Add(gbClock);
            pnControls.Controls.Add(btLoadConfig);
            pnControls.Controls.Add(btSaveConfig);
            pnControls.Controls.Add(btRestoreLastConfig);
            pnControls.Controls.Add(btSetAllConfig);
            pnControls.Controls.Add(btGetConfigModules);
            pnControls.Controls.Add(btSetConfig);
            resources.ApplyResources(pnControls, "pnControls");
            pnControls.Name = "pnControls";
            // 
            // btGetElectrodeInfo
            // 
            resources.ApplyResources(btGetElectrodeInfo, "btGetElectrodeInfo");
            btGetElectrodeInfo.Name = "btGetElectrodeInfo";
            btGetElectrodeInfo.UseVisualStyleBackColor = true;
            btGetElectrodeInfo.Click += BtGetElectrodeInfo_Click;
            // 
            // cChannelsControlV2x11
            // 
            cChannelsControlV2x11.BackColor = Color.FromArgb(224, 224, 224);
            resources.ApplyResources(cChannelsControlV2x11, "cChannelsControlV2x11");
            cChannelsControlV2x11.Name = "cChannelsControlV2x11";
            cChannelsControlV2x11.ModuleRowChanged += CChannelsControlV2x11_ModuleRowChanged;
            cChannelsControlV2x11.SWChannelRowChanged += CChannelsControlV2x11_SWChannelRowChanged;
            // 
            // btGetModuleSpecific
            // 
            resources.ApplyResources(btGetModuleSpecific, "btGetModuleSpecific");
            btGetModuleSpecific.Name = "btGetModuleSpecific";
            btGetModuleSpecific.UseVisualStyleBackColor = true;
            btGetModuleSpecific.Click += BtGetModuleSpecific_Click;
            // 
            // btSetModuleSpecific
            // 
            resources.ApplyResources(btSetModuleSpecific, "btSetModuleSpecific");
            btSetModuleSpecific.Name = "btSetModuleSpecific";
            btSetModuleSpecific.UseVisualStyleBackColor = true;
            btSetModuleSpecific.Click += BtSetModuleSpecific_Click;
            // 
            // btGetFirmwareVersion
            // 
            resources.ApplyResources(btGetFirmwareVersion, "btGetFirmwareVersion");
            btGetFirmwareVersion.Name = "btGetFirmwareVersion";
            btGetFirmwareVersion.UseVisualStyleBackColor = true;
            btGetFirmwareVersion.Click += BtGetFirmwareVersion_Click;
            // 
            // AllDefaultOn
            // 
            resources.ApplyResources(AllDefaultOn, "AllDefaultOn");
            AllDefaultOn.Name = "AllDefaultOn";
            AllDefaultOn.UseVisualStyleBackColor = true;
            AllDefaultOn.Click += AllDefaultOn_Click;
            // 
            // btGetSDCardInfo
            // 
            resources.ApplyResources(btGetSDCardInfo, "btGetSDCardInfo");
            btGetSDCardInfo.Name = "btGetSDCardInfo";
            btGetSDCardInfo.UseVisualStyleBackColor = true;
            btGetSDCardInfo.Click += BtGetSDCardInfo_Click;
            // 
            // btAllCnhan0On
            // 
            resources.ApplyResources(btAllCnhan0On, "btAllCnhan0On");
            btAllCnhan0On.Name = "btAllCnhan0On";
            btAllCnhan0On.UseVisualStyleBackColor = true;
            btAllCnhan0On.Click += BtAllCnhan0On_Click;
            // 
            // gbClock
            // 
            gbClock.Controls.Add(txtTime);
            gbClock.Controls.Add(btGetClock);
            gbClock.Controls.Add(btSetClock);
            resources.ApplyResources(gbClock, "gbClock");
            gbClock.Name = "gbClock";
            gbClock.TabStop = false;
            // 
            // txtTime
            // 
            resources.ApplyResources(txtTime, "txtTime");
            txtTime.Name = "txtTime";
            // 
            // btLoadConfig
            // 
            resources.ApplyResources(btLoadConfig, "btLoadConfig");
            btLoadConfig.Name = "btLoadConfig";
            btLoadConfig.UseVisualStyleBackColor = true;
            btLoadConfig.Click += BtLoadConfig_Click;
            // 
            // btSaveConfig
            // 
            resources.ApplyResources(btSaveConfig, "btSaveConfig");
            btSaveConfig.Name = "btSaveConfig";
            btSaveConfig.UseVisualStyleBackColor = true;
            btSaveConfig.Click += BtSaveConfig_Click;
            // 
            // btRestoreLastConfig
            // 
            resources.ApplyResources(btRestoreLastConfig, "btRestoreLastConfig");
            btRestoreLastConfig.Name = "btRestoreLastConfig";
            btRestoreLastConfig.UseVisualStyleBackColor = true;
            btRestoreLastConfig.Click += BtRestoreLastConfig_Click;
            // 
            // btSetAllConfig
            // 
            resources.ApplyResources(btSetAllConfig, "btSetAllConfig");
            btSetAllConfig.Name = "btSetAllConfig";
            btSetAllConfig.UseVisualStyleBackColor = true;
            btSetAllConfig.Click += BtSetAllConfig_Click;
            // 
            // gbConnectivitie
            // 
            gbConnectivitie.Controls.Add(txtBattery);
            gbConnectivitie.Controls.Add(pbXBEESignalStrength);
            gbConnectivitie.Controls.Add(label1);
            gbConnectivitie.Controls.Add(lblXBeeCapacity);
            gbConnectivitie.Controls.Add(label2);
            gbConnectivitie.Controls.Add(pbXBeeChannelCapacity);
            gbConnectivitie.Controls.Add(txtStatus);
            gbConnectivitie.Controls.Add(tbConnect);
            resources.ApplyResources(gbConnectivitie, "gbConnectivitie");
            gbConnectivitie.Name = "gbConnectivitie";
            gbConnectivitie.TabStop = false;
            // 
            // txtBattery
            // 
            resources.ApplyResources(txtBattery, "txtBattery");
            txtBattery.Name = "txtBattery";
            // 
            // pbXBEESignalStrength
            // 
            resources.ApplyResources(pbXBEESignalStrength, "pbXBEESignalStrength");
            pbXBEESignalStrength.Name = "pbXBEESignalStrength";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // lblXBeeCapacity
            // 
            resources.ApplyResources(lblXBeeCapacity, "lblXBeeCapacity");
            lblXBeeCapacity.Name = "lblXBeeCapacity";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // pbXBeeChannelCapacity
            // 
            resources.ApplyResources(pbXBeeChannelCapacity, "pbXBeeChannelCapacity");
            pbXBeeChannelCapacity.Maximum = 2000;
            pbXBeeChannelCapacity.Name = "pbXBeeChannelCapacity";
            // 
            // txtStatus
            // 
            resources.ApplyResources(txtStatus, "txtStatus");
            txtStatus.Name = "txtStatus";
            // 
            // tbConnect
            // 
            tbConnect.AcceptChange = false;
            tbConnect.BackColor = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState1 = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState2 = Color.FromArgb(128, 255, 128);
            resources.ApplyResources(tbConnect, "tbConnect");
            tbConnect.Name = "tbConnect";
            tbConnect.TextState1 = "Connect";
            tbConnect.TextState2 = "Disconnect";
            tbConnect.UseVisualStyleBackColor = false;
            tbConnect.ToState2 += TbConnect_ToState2;
            tbConnect.ToState1 += TbConnect_ToState1;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(radioButton1);
            groupBox2.Controls.Add(radioButton2);
            resources.ApplyResources(groupBox2, "groupBox2");
            groupBox2.Name = "groupBox2";
            groupBox2.TabStop = false;
            // 
            // radioButton1
            // 
            resources.ApplyResources(radioButton1, "radioButton1");
            radioButton1.Name = "radioButton1";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            resources.ApplyResources(radioButton2, "radioButton2");
            radioButton2.Checked = true;
            radioButton2.Name = "radioButton2";
            radioButton2.TabStop = true;
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog_xml
            // 
            resources.ApplyResources(saveFileDialog_xml, "saveFileDialog_xml");
            saveFileDialog_xml.InitialDirectory = "c:\\";
            // 
            // openFileDialog_xml
            // 
            openFileDialog_xml.FileName = "config";
            resources.ApplyResources(openFileDialog_xml, "openFileDialog_xml");
            openFileDialog_xml.InitialDirectory = "c:\\";
            // 
            // serialPort1
            // 
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.DiscardNull = false;
            serialPort1.DtrEnable = false;
            serialPort1.Handshake = System.IO.Ports.Handshake.None;
            serialPort1.NewLine = "\n";
            serialPort1.Parity = System.IO.Ports.Parity.None;
            serialPort1.ParityReplace = 63;
            serialPort1.PortName = "COM1";
            serialPort1.ReadBufferSize = 4096;
            serialPort1.ReadTimeout = -1;
            serialPort1.ReceivedBytesThreshold = 1;
            serialPort1.RtsEnable = false;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.WriteBufferSize = 2048;
            serialPort1.WriteTimeout = -1;
            // 
            // tmrStatusMessages
            // 
            tmrStatusMessages.Enabled = true;
            tmrStatusMessages.Interval = 150;
            tmrStatusMessages.Tick += TmrStatusMessages_Tick;
            // 
            // tmrSDDataReader
            // 
            tmrSDDataReader.Interval = 5;
            tmrSDDataReader.Tick += TmrSDDataReader_Tick;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { connectionToolStripMenuItem, displayedChannelsToolStripMenuItem, scaleValuesToolStripMenuItem, timebaseToolStripMenuItem, resyncTimeBaseToolStripMenuItem, fTDIToolStripMenuItem, sDCardToolStripMenuItemTop, saveDataToFileToolStripMenuItem });
            resources.ApplyResources(menuStrip1, "menuStrip1");
            menuStrip1.Name = "menuStrip1";
            // 
            // connectionToolStripMenuItem
            // 
            connectionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { d2XXToolStripMenuItem, virtualComToolStripMenuItem, sDCardToolStripMenuItem });
            connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            resources.ApplyResources(connectionToolStripMenuItem, "connectionToolStripMenuItem");
            // 
            // d2XXToolStripMenuItem
            // 
            d2XXToolStripMenuItem.Checked = true;
            d2XXToolStripMenuItem.CheckState = CheckState.Checked;
            d2XXToolStripMenuItem.Name = "d2XXToolStripMenuItem";
            resources.ApplyResources(d2XXToolStripMenuItem, "d2XXToolStripMenuItem");
            d2XXToolStripMenuItem.Click += D2XXToolStripMenuItem_Click;
            // 
            // virtualComToolStripMenuItem
            // 
            virtualComToolStripMenuItem.Name = "virtualComToolStripMenuItem";
            resources.ApplyResources(virtualComToolStripMenuItem, "virtualComToolStripMenuItem");
            virtualComToolStripMenuItem.Click += D2XXToolStripMenuItem_Click;
            // 
            // sDCardToolStripMenuItem
            // 
            sDCardToolStripMenuItem.Name = "sDCardToolStripMenuItem";
            resources.ApplyResources(sDCardToolStripMenuItem, "sDCardToolStripMenuItem");
            sDCardToolStripMenuItem.Click += D2XXToolStripMenuItem_Click;
            // 
            // displayedChannelsToolStripMenuItem
            // 
            displayedChannelsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectedModuleToolStripMenuItem, rawChannelsToolStripMenuItem, predefinedChannelsToolStripMenuItem });
            displayedChannelsToolStripMenuItem.Name = "displayedChannelsToolStripMenuItem";
            resources.ApplyResources(displayedChannelsToolStripMenuItem, "displayedChannelsToolStripMenuItem");
            // 
            // selectedModuleToolStripMenuItem
            // 
            selectedModuleToolStripMenuItem.Checked = true;
            selectedModuleToolStripMenuItem.CheckState = CheckState.Checked;
            selectedModuleToolStripMenuItem.Name = "selectedModuleToolStripMenuItem";
            resources.ApplyResources(selectedModuleToolStripMenuItem, "selectedModuleToolStripMenuItem");
            selectedModuleToolStripMenuItem.Click += SelectedModuleToolStripMenuItem_Click;
            // 
            // rawChannelsToolStripMenuItem
            // 
            rawChannelsToolStripMenuItem.Name = "rawChannelsToolStripMenuItem";
            resources.ApplyResources(rawChannelsToolStripMenuItem, "rawChannelsToolStripMenuItem");
            rawChannelsToolStripMenuItem.Click += SelectedModuleToolStripMenuItem_Click;
            // 
            // predefinedChannelsToolStripMenuItem
            // 
            predefinedChannelsToolStripMenuItem.Name = "predefinedChannelsToolStripMenuItem";
            resources.ApplyResources(predefinedChannelsToolStripMenuItem, "predefinedChannelsToolStripMenuItem");
            predefinedChannelsToolStripMenuItem.Click += SelectedModuleToolStripMenuItem_Click;
            // 
            // scaleValuesToolStripMenuItem
            // 
            scaleValuesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { scaledToolStripMenuItem, unscaledToolStripMenuItem });
            scaleValuesToolStripMenuItem.Name = "scaleValuesToolStripMenuItem";
            resources.ApplyResources(scaleValuesToolStripMenuItem, "scaleValuesToolStripMenuItem");
            // 
            // scaledToolStripMenuItem
            // 
            scaledToolStripMenuItem.Checked = true;
            scaledToolStripMenuItem.CheckState = CheckState.Checked;
            scaledToolStripMenuItem.Name = "scaledToolStripMenuItem";
            resources.ApplyResources(scaledToolStripMenuItem, "scaledToolStripMenuItem");
            scaledToolStripMenuItem.Click += ScaledToolStripMenuItem_Click;
            // 
            // unscaledToolStripMenuItem
            // 
            unscaledToolStripMenuItem.Name = "unscaledToolStripMenuItem";
            resources.ApplyResources(unscaledToolStripMenuItem, "unscaledToolStripMenuItem");
            unscaledToolStripMenuItem.Click += ScaledToolStripMenuItem_Click;
            // 
            // timebaseToolStripMenuItem
            // 
            timebaseToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { absolutToolStripMenuItem, relativeToolStripMenuItem });
            timebaseToolStripMenuItem.Name = "timebaseToolStripMenuItem";
            resources.ApplyResources(timebaseToolStripMenuItem, "timebaseToolStripMenuItem");
            // 
            // absolutToolStripMenuItem
            // 
            absolutToolStripMenuItem.Checked = true;
            absolutToolStripMenuItem.CheckState = CheckState.Checked;
            absolutToolStripMenuItem.Name = "absolutToolStripMenuItem";
            resources.ApplyResources(absolutToolStripMenuItem, "absolutToolStripMenuItem");
            absolutToolStripMenuItem.Click += AbsolutToolStripMenuItem_Click;
            // 
            // relativeToolStripMenuItem
            // 
            relativeToolStripMenuItem.Name = "relativeToolStripMenuItem";
            resources.ApplyResources(relativeToolStripMenuItem, "relativeToolStripMenuItem");
            relativeToolStripMenuItem.Click += AbsolutToolStripMenuItem_Click;
            // 
            // resyncTimeBaseToolStripMenuItem
            // 
            resyncTimeBaseToolStripMenuItem.Name = "resyncTimeBaseToolStripMenuItem";
            resources.ApplyResources(resyncTimeBaseToolStripMenuItem, "resyncTimeBaseToolStripMenuItem");
            resyncTimeBaseToolStripMenuItem.Click += ResyncTimeBaseToolStripMenuItem_Click;
            // 
            // fTDIToolStripMenuItem
            // 
            fTDIToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { statusToolStripMenuItem, resetDeviceToolStripMenuItem, resetPortToolStripMenuItem, cyclePortToolStripMenuItem });
            fTDIToolStripMenuItem.Name = "fTDIToolStripMenuItem";
            resources.ApplyResources(fTDIToolStripMenuItem, "fTDIToolStripMenuItem");
            // 
            // statusToolStripMenuItem
            // 
            statusToolStripMenuItem.Name = "statusToolStripMenuItem";
            resources.ApplyResources(statusToolStripMenuItem, "statusToolStripMenuItem");
            statusToolStripMenuItem.Click += StatusToolStripMenuItem_Click;
            // 
            // resetDeviceToolStripMenuItem
            // 
            resetDeviceToolStripMenuItem.Name = "resetDeviceToolStripMenuItem";
            resources.ApplyResources(resetDeviceToolStripMenuItem, "resetDeviceToolStripMenuItem");
            resetDeviceToolStripMenuItem.Click += ResetDeviceToolStripMenuItem_Click;
            // 
            // resetPortToolStripMenuItem
            // 
            resetPortToolStripMenuItem.Name = "resetPortToolStripMenuItem";
            resources.ApplyResources(resetPortToolStripMenuItem, "resetPortToolStripMenuItem");
            resetPortToolStripMenuItem.Click += ResetPortToolStripMenuItem_Click;
            // 
            // cyclePortToolStripMenuItem
            // 
            cyclePortToolStripMenuItem.Name = "cyclePortToolStripMenuItem";
            resources.ApplyResources(cyclePortToolStripMenuItem, "cyclePortToolStripMenuItem");
            cyclePortToolStripMenuItem.Click += CyclePortToolStripMenuItem_Click;
            // 
            // sDCardToolStripMenuItemTop
            // 
            sDCardToolStripMenuItemTop.DropDownItems.AddRange(new ToolStripItem[] { readBackToolStripMenuItem, convertToTxtToolStripMenuItem });
            sDCardToolStripMenuItemTop.Name = "sDCardToolStripMenuItemTop";
            resources.ApplyResources(sDCardToolStripMenuItemTop, "sDCardToolStripMenuItemTop");
            // 
            // readBackToolStripMenuItem
            // 
            resources.ApplyResources(readBackToolStripMenuItem, "readBackToolStripMenuItem");
            readBackToolStripMenuItem.Name = "readBackToolStripMenuItem";
            readBackToolStripMenuItem.Click += ReadBackToolStripMenuItem_Click;
            // 
            // convertToTxtToolStripMenuItem
            // 
            convertToTxtToolStripMenuItem.Name = "convertToTxtToolStripMenuItem";
            resources.ApplyResources(convertToTxtToolStripMenuItem, "convertToTxtToolStripMenuItem");
            convertToTxtToolStripMenuItem.Click += ConvertToTxtToolStripMenuItem_Click;
            // 
            // saveDataToFileToolStripMenuItem
            // 
            saveDataToFileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveDataToFileToolStripMenuItem1, readDataFromFileToolStripMenuItem });
            saveDataToFileToolStripMenuItem.Name = "saveDataToFileToolStripMenuItem";
            resources.ApplyResources(saveDataToFileToolStripMenuItem, "saveDataToFileToolStripMenuItem");
            // 
            // saveDataToFileToolStripMenuItem1
            // 
            saveDataToFileToolStripMenuItem1.Name = "saveDataToFileToolStripMenuItem1";
            resources.ApplyResources(saveDataToFileToolStripMenuItem1, "saveDataToFileToolStripMenuItem1");
            saveDataToFileToolStripMenuItem1.Click += SaveDataToFileToolStripMenuItem_Click;
            // 
            // readDataFromFileToolStripMenuItem
            // 
            readDataFromFileToolStripMenuItem.Name = "readDataFromFileToolStripMenuItem";
            resources.ApplyResources(readDataFromFileToolStripMenuItem, "readDataFromFileToolStripMenuItem");
            readDataFromFileToolStripMenuItem.Click += ReadDataFromFileToolStripMenuItem_Click;
            // 
            // openFileDialog_cfg
            // 
            openFileDialog_cfg.FileName = "config";
            resources.ApplyResources(openFileDialog_cfg, "openFileDialog_cfg");
            openFileDialog_cfg.InitialDirectory = "c:\\";
            // 
            // tmrFileDataReader
            // 
            tmrFileDataReader.Interval = 5;
            tmrFileDataReader.Tick += TmrFileDataReader_Tick;
            // 
            // tlpMeasure
            // 
            resources.ApplyResources(tlpMeasure, "tlpMeasure");
            tlpMeasure.Controls.Add(ucSignalAnalyser1, 0, 0);
            tlpMeasure.Name = "tlpMeasure";
            // 
            // ucSignalAnalyser1
            // 
            ucSignalAnalyser1.HeaderText = "xx";
            resources.ApplyResources(ucSignalAnalyser1, "ucSignalAnalyser1");
            ucSignalAnalyser1.Name = "ucSignalAnalyser1";
            // 
            // openFileDialog_nmc
            // 
            openFileDialog_nmc.FileName = "config";
            resources.ApplyResources(openFileDialog_nmc, "openFileDialog_nmc");
            openFileDialog_nmc.InitialDirectory = "c:\\";
            // 
            // panel1
            // 
            panel1.Controls.Add(cFlowChartDX1);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            // 
            // cFlowChartDX1
            // 
            resources.ApplyResources(cFlowChartDX1, "cFlowChartDX1");
            cFlowChartDX1.Name = "cFlowChartDX1";
            cFlowChartDX1.numberOfCharts = 2;
            // 
            // NeuromasterV5
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(tlpMeasure);
            Controls.Add(gbConnectivitie);
            Controls.Add(pnControls);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "NeuromasterV5";
            FormClosing += NeuromasterV2_FormClosing;
            Load += NeuromasterV2_Load;
            pnControls.ResumeLayout(false);
            gbClock.ResumeLayout(false);
            gbClock.PerformLayout();
            gbConnectivitie.ResumeLayout(false);
            gbConnectivitie.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tlpMeasure.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btGetConfigModules;
        private System.Windows.Forms.Button btSetConfig;
        private System.Windows.Forms.Button btGetClock;
        private System.Windows.Forms.Button btSetClock;
        private System.Windows.Forms.Panel pnControls;
        private System.Windows.Forms.GroupBox gbConnectivitie;
        private ComponentsLib_GUI.CToggleButton tbConnect;
        private System.Windows.Forms.GroupBox gbClock;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.ProgressBar pbXBeeChannelCapacity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btSetAllConfig;
        private System.Windows.Forms.Button btRestoreLastConfig;
        private System.Windows.Forms.Label lblXBeeCapacity;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog_xml;
        private System.Windows.Forms.OpenFileDialog openFileDialog_xml;
        private System.Windows.Forms.Button btLoadConfig;
        private System.Windows.Forms.Button btSaveConfig;

        private System.Windows.Forms.Button btAllCnhan0On;
        private System.Windows.Forms.Button AllDefaultOn;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Timer tmrStatusMessages;
        private System.Windows.Forms.ProgressBar pbXBEESignalStrength;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btGetSDCardInfo;
        private System.Windows.Forms.Button btGetFirmwareVersion;
        private System.Windows.Forms.Timer tmrSDDataReader;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private FeedbackDataLib_GUI.UcSignalAnalyser ucSignalAnalyser1;
        private System.Windows.Forms.Button btGetModuleSpecific;
        private System.Windows.Forms.Button btSetModuleSpecific;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.RichTextBox txtBattery;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem displayedChannelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedModuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawChannelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem predefinedChannelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scaleValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scaledToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unscaledToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timebaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem absolutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem relativeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sDCardToolStripMenuItemTop;
        private System.Windows.Forms.ToolStripMenuItem readBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resyncTimeBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fTDIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetDeviceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cyclePortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem d2XXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem virtualComToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sDCardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveDataToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveDataToFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem readDataFromFileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog_cfg;
        private FeedbackDataLib_GUI.CChannelsControlV2x1 cChannelsControlV2x11;
        private System.Windows.Forms.Timer tmrFileDataReader;
        private System.Windows.Forms.TableLayoutPanel tlpMeasure;
        private System.Windows.Forms.OpenFileDialog openFileDialog_nmc;
        private System.Windows.Forms.Button btGetElectrodeInfo;
        private System.Windows.Forms.ToolStripMenuItem convertToTxtToolStripMenuItem;
        private Panel panel1;
        private UcFlowChartDX_NM cFlowChartDX1;
    }
}

