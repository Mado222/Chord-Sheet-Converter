using ComponentsLib_GUI;
using FeedbackDataLib_GUI;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8
{
    partial class frmInsight_Manufacturing5
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            tabMeasurements = new TabControl();
            tabProgNeuroModul = new TabPage();
            cbIgnoreSerialNumberCheck = new CheckBox();
            lblFWVersion = new Label();
            btReadhexFile = new Button();
            btModulNMauslesen = new Button();
            btModulICD3auslesen = new Button();
            lblResult = new Label();
            btnProgNMo = new Button();
            gbModuleSelection = new GroupBox();
            flpModuleSelection = new FlowLayoutPanel();
            tabFY6900 = new TabPage();
            gbPhidget = new GroupBox();
            cbPhi_ICDOn = new CheckBox();
            cbOffsetOn = new CheckBox();
            groupBox2 = new GroupBox();
            rbOffsetLow = new RadioButton();
            rbOffsetHigh = new RadioButton();
            cbEEGOn = new CheckBox();
            cbOffsetPlus = new CheckBox();
            gbFY6900 = new GroupBox();
            btSetAmplitude = new Button();
            btArtifECG_005 = new Button();
            btArtifECG_004 = new Button();
            cToggleButton_COM = new CToggleButton();
            btArtifECG_003 = new Button();
            btSetSinus = new Button();
            btArtifECG_002 = new Button();
            setFrequency = new Button();
            numericUpDownFrequqency = new NumericUpDown();
            numericUpDownAmplitude = new NumericUpDown();
            btShowRS232 = new Button();
            groupBox1 = new GroupBox();
            btGetProgrammers = new Button();
            cbProgrammer = new ComboBox();
            btShowDatabase = new Button();
            lblNoFeedback = new Label();
            btCalcTemp2 = new Button();
            label4 = new Label();
            txtSerialNo = new TextBox();
            dgvMeasurements = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Measurement_Active = new DataGridViewCheckBoxColumn();
            Measurement_SavetoDB = new DataGridViewCheckBoxColumn();
            Measurement_done = new DataGridViewCheckBoxColumn();
            Measurement_Duration = new DataGridViewTextBoxColumn();
            cMeasurementsBindingSource = new BindingSource(components);
            openFileDialog_txt = new OpenFileDialog();
            tabMonitor = new TabControl();
            tabPLog = new TabPage();
            txtStatus = new CStatusText();
            tabPModuleInfo = new TabPage();
            btCalValstoDB = new Button();
            dgv_SWChannelInfo = new DataGridView();
            tmrGui = new System.Windows.Forms.Timer(components);
            cFlowChartDX1 = new FeedbackDataLib_GUI.UcFlowChartDX_NM();
            cbComPortSelector = new ComboBox();
            tabMeasurements.SuspendLayout();
            tabProgNeuroModul.SuspendLayout();
            gbModuleSelection.SuspendLayout();
            tabFY6900.SuspendLayout();
            gbPhidget.SuspendLayout();
            groupBox2.SuspendLayout();
            gbFY6900.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrequqency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownAmplitude).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMeasurements).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cMeasurementsBindingSource).BeginInit();
            tabMonitor.SuspendLayout();
            tabPLog.SuspendLayout();
            tabPModuleInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_SWChannelInfo).BeginInit();
            SuspendLayout();
            // 
            // tabMeasurements
            // 
            tabMeasurements.Controls.Add(tabProgNeuroModul);
            tabMeasurements.Controls.Add(tabFY6900);
            tabMeasurements.Location = new Point(23, 228);
            tabMeasurements.Margin = new Padding(4, 5, 4, 5);
            tabMeasurements.Name = "tabMeasurements";
            tabMeasurements.SelectedIndex = 0;
            tabMeasurements.Size = new Size(781, 426);
            tabMeasurements.TabIndex = 3;
            tabMeasurements.Selecting += tabMeasurements_Selecting;
            // 
            // tabProgNeuroModul
            // 
            tabProgNeuroModul.Controls.Add(cbIgnoreSerialNumberCheck);
            tabProgNeuroModul.Controls.Add(lblFWVersion);
            tabProgNeuroModul.Controls.Add(btReadhexFile);
            tabProgNeuroModul.Controls.Add(btModulNMauslesen);
            tabProgNeuroModul.Controls.Add(btModulICD3auslesen);
            tabProgNeuroModul.Controls.Add(lblResult);
            tabProgNeuroModul.Controls.Add(btnProgNMo);
            tabProgNeuroModul.Controls.Add(gbModuleSelection);
            tabProgNeuroModul.Location = new Point(4, 29);
            tabProgNeuroModul.Margin = new Padding(4, 5, 4, 5);
            tabProgNeuroModul.Name = "tabProgNeuroModul";
            tabProgNeuroModul.Size = new Size(773, 393);
            tabProgNeuroModul.TabIndex = 6;
            tabProgNeuroModul.Text = "Neuro-Device";
            // 
            // cbIgnoreSerialNumberCheck
            // 
            cbIgnoreSerialNumberCheck.AutoSize = true;
            cbIgnoreSerialNumberCheck.Location = new Point(372, 339);
            cbIgnoreSerialNumberCheck.Margin = new Padding(3, 4, 3, 4);
            cbIgnoreSerialNumberCheck.Name = "cbIgnoreSerialNumberCheck";
            cbIgnoreSerialNumberCheck.Size = new Size(211, 24);
            cbIgnoreSerialNumberCheck.TabIndex = 50;
            cbIgnoreSerialNumberCheck.Text = "Ignore Serial number check";
            cbIgnoreSerialNumberCheck.UseVisualStyleBackColor = true;
            // 
            // lblFWVersion
            // 
            lblFWVersion.AutoSize = true;
            lblFWVersion.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFWVersion.ForeColor = Color.Green;
            lblFWVersion.Location = new Point(559, 299);
            lblFWVersion.Margin = new Padding(4, 0, 4, 0);
            lblFWVersion.Name = "lblFWVersion";
            lblFWVersion.Size = new Size(149, 20);
            lblFWVersion.TabIndex = 49;
            lblFWVersion.Text = "HEX-File Version: ";
            // 
            // btReadhexFile
            // 
            btReadhexFile.Location = new Point(372, 289);
            btReadhexFile.Margin = new Padding(4, 5, 4, 5);
            btReadhexFile.Name = "btReadhexFile";
            btReadhexFile.Size = new Size(179, 38);
            btReadhexFile.TabIndex = 48;
            btReadhexFile.Text = "Hexfile lesen";
            btReadhexFile.UseVisualStyleBackColor = true;
            btReadhexFile.Click += btReadhexFile_Click;
            // 
            // btModulNMauslesen
            // 
            btModulNMauslesen.Location = new Point(559, 210);
            btModulNMauslesen.Margin = new Padding(4, 5, 4, 5);
            btModulNMauslesen.Name = "btModulNMauslesen";
            btModulNMauslesen.Size = new Size(179, 60);
            btModulNMauslesen.TabIndex = 47;
            btModulNMauslesen.Text = "Device lesen \n (Neurolink, NModul -> B)";
            btModulNMauslesen.UseVisualStyleBackColor = true;
            btModulNMauslesen.Click += btModulNMauslesen_Click;
            // 
            // btModulICD3auslesen
            // 
            btModulICD3auslesen.Location = new Point(372, 210);
            btModulICD3auslesen.Margin = new Padding(4, 5, 4, 5);
            btModulICD3auslesen.Name = "btModulICD3auslesen";
            btModulICD3auslesen.Size = new Size(179, 60);
            btModulICD3auslesen.TabIndex = 46;
            btModulICD3auslesen.Text = "Device lesen \n (Programmer)";
            btModulICD3auslesen.UseVisualStyleBackColor = true;
            btModulICD3auslesen.Click += btModulICD3auslesen_Click;
            // 
            // lblResult
            // 
            lblResult.BackColor = SystemColors.ControlDark;
            lblResult.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblResult.ForeColor = Color.White;
            lblResult.Location = new Point(372, 120);
            lblResult.Margin = new Padding(4, 0, 4, 0);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(383, 72);
            lblResult.TabIndex = 45;
            lblResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnProgNMo
            // 
            btnProgNMo.Location = new Point(372, 28);
            btnProgNMo.Margin = new Padding(4, 5, 4, 5);
            btnProgNMo.Name = "btnProgNMo";
            btnProgNMo.Size = new Size(383, 74);
            btnProgNMo.TabIndex = 39;
            btnProgNMo.Text = "Device programmieren";
            btnProgNMo.UseVisualStyleBackColor = true;
            btnProgNMo.Click += btnProgNeuroModul_Click;
            // 
            // gbModuleSelection
            // 
            gbModuleSelection.Controls.Add(flpModuleSelection);
            gbModuleSelection.Location = new Point(11, 19);
            gbModuleSelection.Margin = new Padding(4, 5, 4, 5);
            gbModuleSelection.Name = "gbModuleSelection";
            gbModuleSelection.Padding = new Padding(4, 5, 4, 5);
            gbModuleSelection.Size = new Size(353, 346);
            gbModuleSelection.TabIndex = 42;
            gbModuleSelection.TabStop = false;
            gbModuleSelection.Text = "Neuro-Device auswählen";
            // 
            // flpModuleSelection
            // 
            flpModuleSelection.Dock = DockStyle.Fill;
            flpModuleSelection.FlowDirection = FlowDirection.TopDown;
            flpModuleSelection.Location = new Point(4, 25);
            flpModuleSelection.Margin = new Padding(4, 5, 4, 5);
            flpModuleSelection.Name = "flpModuleSelection";
            flpModuleSelection.Size = new Size(345, 316);
            flpModuleSelection.TabIndex = 1;
            // 
            // tabFY6900
            // 
            tabFY6900.Controls.Add(gbPhidget);
            tabFY6900.Controls.Add(gbFY6900);
            tabFY6900.Location = new Point(4, 29);
            tabFY6900.Margin = new Padding(4, 5, 4, 5);
            tabFY6900.Name = "tabFY6900";
            tabFY6900.Padding = new Padding(4, 5, 4, 5);
            tabFY6900.Size = new Size(773, 393);
            tabFY6900.TabIndex = 7;
            tabFY6900.Text = "FY6900 / Phidget";
            tabFY6900.UseVisualStyleBackColor = true;
            // 
            // gbPhidget
            // 
            gbPhidget.BackColor = Color.PapayaWhip;
            gbPhidget.Controls.Add(cbPhi_ICDOn);
            gbPhidget.Controls.Add(cbOffsetOn);
            gbPhidget.Controls.Add(groupBox2);
            gbPhidget.Controls.Add(cbEEGOn);
            gbPhidget.Controls.Add(cbOffsetPlus);
            gbPhidget.Location = new Point(27, 185);
            gbPhidget.Margin = new Padding(4, 5, 4, 5);
            gbPhidget.Name = "gbPhidget";
            gbPhidget.Padding = new Padding(4, 5, 4, 5);
            gbPhidget.Size = new Size(617, 145);
            gbPhidget.TabIndex = 19;
            gbPhidget.TabStop = false;
            gbPhidget.Text = "Phidget / Relais";
            // 
            // cbPhi_ICDOn
            // 
            cbPhi_ICDOn.AutoSize = true;
            cbPhi_ICDOn.Location = new Point(8, 32);
            cbPhi_ICDOn.Margin = new Padding(4, 5, 4, 5);
            cbPhi_ICDOn.Name = "cbPhi_ICDOn";
            cbPhi_ICDOn.Size = new Size(86, 24);
            cbPhi_ICDOn.TabIndex = 8;
            cbPhi_ICDOn.Text = "ICD3 On";
            cbPhi_ICDOn.UseVisualStyleBackColor = true;
            cbPhi_ICDOn.CheckedChanged += cbPhi_ICDON_CheckedChanged;
            // 
            // cbOffsetOn
            // 
            cbOffsetOn.AutoSize = true;
            cbOffsetOn.Location = new Point(8, 62);
            cbOffsetOn.Margin = new Padding(4, 5, 4, 5);
            cbOffsetOn.Name = "cbOffsetOn";
            cbOffsetOn.Size = new Size(94, 24);
            cbOffsetOn.TabIndex = 9;
            cbOffsetOn.Text = "Offset On";
            cbOffsetOn.UseVisualStyleBackColor = true;
            cbOffsetOn.CheckedChanged += cbOffsetOn_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(rbOffsetLow);
            groupBox2.Controls.Add(rbOffsetHigh);
            groupBox2.Location = new Point(252, 19);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(228, 98);
            groupBox2.TabIndex = 13;
            groupBox2.TabStop = false;
            groupBox2.Text = "Offset Voltage [mV]";
            // 
            // rbOffsetLow
            // 
            rbOffsetLow.AutoSize = true;
            rbOffsetLow.Location = new Point(15, 58);
            rbOffsetLow.Margin = new Padding(4, 5, 4, 5);
            rbOffsetLow.Name = "rbOffsetLow";
            rbOffsetLow.Size = new Size(111, 24);
            rbOffsetLow.TabIndex = 1;
            rbOffsetLow.TabStop = true;
            rbOffsetLow.Text = "rbOffsetLow";
            rbOffsetLow.UseVisualStyleBackColor = true;
            // 
            // rbOffsetHigh
            // 
            rbOffsetHigh.AutoSize = true;
            rbOffsetHigh.Location = new Point(15, 22);
            rbOffsetHigh.Margin = new Padding(4, 5, 4, 5);
            rbOffsetHigh.Name = "rbOffsetHigh";
            rbOffsetHigh.Size = new Size(116, 24);
            rbOffsetHigh.TabIndex = 0;
            rbOffsetHigh.TabStop = true;
            rbOffsetHigh.Text = "rbOffsetHigh";
            rbOffsetHigh.UseVisualStyleBackColor = true;
            rbOffsetHigh.CheckedChanged += rbOffsetHigh_CheckedChanged;
            // 
            // cbEEGOn
            // 
            cbEEGOn.AutoSize = true;
            cbEEGOn.Location = new Point(8, 94);
            cbEEGOn.Margin = new Padding(4, 5, 4, 5);
            cbEEGOn.Name = "cbEEGOn";
            cbEEGOn.Size = new Size(80, 24);
            cbEEGOn.TabIndex = 10;
            cbEEGOn.Text = "EEG On";
            cbEEGOn.UseVisualStyleBackColor = true;
            cbEEGOn.CheckedChanged += cbEEGOn_CheckedChanged;
            // 
            // cbOffsetPlus
            // 
            cbOffsetPlus.AutoSize = true;
            cbOffsetPlus.Location = new Point(111, 59);
            cbOffsetPlus.Margin = new Padding(4, 5, 4, 5);
            cbOffsetPlus.Name = "cbOffsetPlus";
            cbOffsetPlus.Size = new Size(138, 24);
            cbOffsetPlus.TabIndex = 11;
            cbOffsetPlus.Text = "Offset Polarity +";
            cbOffsetPlus.UseVisualStyleBackColor = true;
            cbOffsetPlus.CheckedChanged += cbOffsetPlus_CheckedChanged;
            // 
            // gbFY6900
            // 
            gbFY6900.BackColor = Color.WhiteSmoke;
            gbFY6900.Controls.Add(cbComPortSelector);
            gbFY6900.Controls.Add(btSetAmplitude);
            gbFY6900.Controls.Add(btArtifECG_005);
            gbFY6900.Controls.Add(btArtifECG_004);
            gbFY6900.Controls.Add(cToggleButton_COM);
            gbFY6900.Controls.Add(btArtifECG_003);
            gbFY6900.Controls.Add(btSetSinus);
            gbFY6900.Controls.Add(btArtifECG_002);
            gbFY6900.Controls.Add(setFrequency);
            gbFY6900.Controls.Add(numericUpDownFrequqency);
            gbFY6900.Controls.Add(numericUpDownAmplitude);
            gbFY6900.Location = new Point(27, 9);
            gbFY6900.Margin = new Padding(4, 5, 4, 5);
            gbFY6900.Name = "gbFY6900";
            gbFY6900.Padding = new Padding(4, 5, 4, 5);
            gbFY6900.Size = new Size(617, 171);
            gbFY6900.TabIndex = 18;
            gbFY6900.TabStop = false;
            gbFY6900.Text = "FY6900";
            // 
            // btSetAmplitude
            // 
            btSetAmplitude.Location = new Point(20, 118);
            btSetAmplitude.Margin = new Padding(4, 5, 4, 5);
            btSetAmplitude.Name = "btSetAmplitude";
            btSetAmplitude.Size = new Size(140, 35);
            btSetAmplitude.TabIndex = 6;
            btSetAmplitude.Text = "Set Amplitude";
            btSetAmplitude.UseVisualStyleBackColor = true;
            btSetAmplitude.Click += btSetAmplitude_Click;
            // 
            // btArtifECG_005
            // 
            btArtifECG_005.Location = new Point(464, 68);
            btArtifECG_005.Margin = new Padding(4, 5, 4, 5);
            btArtifECG_005.Name = "btArtifECG_005";
            btArtifECG_005.Size = new Size(140, 35);
            btArtifECG_005.TabIndex = 17;
            btArtifECG_005.Text = "Set ArtifECG_005";
            btArtifECG_005.UseVisualStyleBackColor = true;
            btArtifECG_005.Click += btArtifECG_005_Click;
            // 
            // btArtifECG_004
            // 
            btArtifECG_004.Location = new Point(464, 25);
            btArtifECG_004.Margin = new Padding(4, 5, 4, 5);
            btArtifECG_004.Name = "btArtifECG_004";
            btArtifECG_004.Size = new Size(140, 35);
            btArtifECG_004.TabIndex = 16;
            btArtifECG_004.Text = "Set ArtifECG_004";
            btArtifECG_004.UseVisualStyleBackColor = true;
            btArtifECG_004.Click += btArtifECG_004_Click;
            // 
            // cToggleButton_COM
            // 
            cToggleButton_COM.AcceptChange = true;
            cToggleButton_COM.BackColor = Color.FromArgb(192, 255, 192);
            cToggleButton_COM.ColorState1 = Color.FromArgb(192, 255, 192);
            cToggleButton_COM.ColorState2 = Color.FromArgb(255, 192, 192);
            cToggleButton_COM.Location = new Point(297, 118);
            cToggleButton_COM.Margin = new Padding(4, 5, 4, 5);
            cToggleButton_COM.Name = "cToggleButton_COM";
            cToggleButton_COM.Size = new Size(140, 35);
            cToggleButton_COM.TabIndex = 2;
            cToggleButton_COM.Text = "Open COM";
            cToggleButton_COM.TextState1 = "Open COM";
            cToggleButton_COM.TextState2 = "Close COM";
            cToggleButton_COM.UseVisualStyleBackColor = false;
            cToggleButton_COM.ToState2 += CToggleButton_COM_ToState2;
            cToggleButton_COM.ToState1 += cToggleButton_COM_ToState1;
            // 
            // btArtifECG_003
            // 
            btArtifECG_003.Location = new Point(316, 25);
            btArtifECG_003.Margin = new Padding(4, 5, 4, 5);
            btArtifECG_003.Name = "btArtifECG_003";
            btArtifECG_003.Size = new Size(140, 35);
            btArtifECG_003.TabIndex = 15;
            btArtifECG_003.Text = "Set ArtifECG_003";
            btArtifECG_003.UseVisualStyleBackColor = true;
            btArtifECG_003.Click += btArtifECG_003_Click;
            // 
            // btSetSinus
            // 
            btSetSinus.Location = new Point(20, 25);
            btSetSinus.Margin = new Padding(4, 5, 4, 5);
            btSetSinus.Name = "btSetSinus";
            btSetSinus.Size = new Size(140, 35);
            btSetSinus.TabIndex = 3;
            btSetSinus.Text = "Set Sinus";
            btSetSinus.UseVisualStyleBackColor = true;
            btSetSinus.Click += btSetSinus_Click;
            // 
            // btArtifECG_002
            // 
            btArtifECG_002.Location = new Point(168, 25);
            btArtifECG_002.Margin = new Padding(4, 5, 4, 5);
            btArtifECG_002.Name = "btArtifECG_002";
            btArtifECG_002.Size = new Size(140, 35);
            btArtifECG_002.TabIndex = 14;
            btArtifECG_002.Text = "Set ArtifECG_002";
            btArtifECG_002.UseVisualStyleBackColor = true;
            btArtifECG_002.Click += btArtifECG_002_Click;
            // 
            // setFrequency
            // 
            setFrequency.Location = new Point(20, 71);
            setFrequency.Margin = new Padding(4, 5, 4, 5);
            setFrequency.Name = "setFrequency";
            setFrequency.Size = new Size(140, 35);
            setFrequency.TabIndex = 4;
            setFrequency.Text = "Set Frequency";
            setFrequency.UseVisualStyleBackColor = true;
            setFrequency.Click += setFrequency_Click;
            // 
            // numericUpDownFrequqency
            // 
            numericUpDownFrequqency.DecimalPlaces = 1;
            numericUpDownFrequqency.Location = new Point(173, 78);
            numericUpDownFrequqency.Margin = new Padding(4, 5, 4, 5);
            numericUpDownFrequqency.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDownFrequqency.Name = "numericUpDownFrequqency";
            numericUpDownFrequqency.Size = new Size(91, 27);
            numericUpDownFrequqency.TabIndex = 5;
            numericUpDownFrequqency.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // numericUpDownAmplitude
            // 
            numericUpDownAmplitude.DecimalPlaces = 2;
            numericUpDownAmplitude.Location = new Point(173, 121);
            numericUpDownAmplitude.Margin = new Padding(4, 5, 4, 5);
            numericUpDownAmplitude.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownAmplitude.Name = "numericUpDownAmplitude";
            numericUpDownAmplitude.Size = new Size(91, 27);
            numericUpDownAmplitude.TabIndex = 7;
            numericUpDownAmplitude.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // btShowRS232
            // 
            btShowRS232.Location = new Point(238, 138);
            btShowRS232.Margin = new Padding(4, 5, 4, 5);
            btShowRS232.Name = "btShowRS232";
            btShowRS232.Size = new Size(224, 36);
            btShowRS232.TabIndex = 57;
            btShowRS232.Text = "Verbundene RS232 Geräte";
            btShowRS232.UseVisualStyleBackColor = true;
            btShowRS232.Click += btShowRS232_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btShowRS232);
            groupBox1.Controls.Add(btGetProgrammers);
            groupBox1.Controls.Add(cbProgrammer);
            groupBox1.Controls.Add(btShowDatabase);
            groupBox1.Controls.Add(lblNoFeedback);
            groupBox1.Controls.Add(btCalcTemp2);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(txtSerialNo);
            groupBox1.Location = new Point(21, 14);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(813, 204);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Seriennummer";
            // 
            // btGetProgrammers
            // 
            btGetProgrammers.Location = new Point(464, 26);
            btGetProgrammers.Margin = new Padding(4, 5, 4, 5);
            btGetProgrammers.Name = "btGetProgrammers";
            btGetProgrammers.Size = new Size(132, 32);
            btGetProgrammers.TabIndex = 206;
            btGetProgrammers.Text = "Get Programmers";
            btGetProgrammers.UseVisualStyleBackColor = true;
            btGetProgrammers.Visible = false;
            btGetProgrammers.Click += btGetProgrammers_Click;
            // 
            // cbProgrammer
            // 
            cbProgrammer.DisplayMember = "FullInfo";
            cbProgrammer.FormattingEnabled = true;
            cbProgrammer.Location = new Point(604, 28);
            cbProgrammer.Margin = new Padding(4, 5, 4, 5);
            cbProgrammer.Name = "cbProgrammer";
            cbProgrammer.Size = new Size(200, 28);
            cbProgrammer.TabIndex = 205;
            cbProgrammer.ValueMember = "CMDLine_string";
            cbProgrammer.Visible = false;
            // 
            // btShowDatabase
            // 
            btShowDatabase.Location = new Point(16, 138);
            btShowDatabase.Margin = new Padding(4, 5, 4, 5);
            btShowDatabase.Name = "btShowDatabase";
            btShowDatabase.Size = new Size(197, 35);
            btShowDatabase.TabIndex = 52;
            btShowDatabase.Text = "Datenbank anzeigen";
            btShowDatabase.UseVisualStyleBackColor = true;
            btShowDatabase.Click += btShowDatabase_Click;
            // 
            // lblNoFeedback
            // 
            lblNoFeedback.AutoSize = true;
            lblNoFeedback.Location = new Point(221, 75);
            lblNoFeedback.Margin = new Padding(4, 0, 4, 0);
            lblNoFeedback.Name = "lblNoFeedback";
            lblNoFeedback.Size = new Size(58, 20);
            lblNoFeedback.TabIndex = 49;
            lblNoFeedback.Text = "label10";
            // 
            // btCalcTemp2
            // 
            btCalcTemp2.Enabled = false;
            btCalcTemp2.Location = new Point(683, 75);
            btCalcTemp2.Margin = new Padding(4, 5, 4, 5);
            btCalcTemp2.Name = "btCalcTemp2";
            btCalcTemp2.Size = new Size(100, 35);
            btCalcTemp2.TabIndex = 45;
            btCalcTemp2.Text = "CalcTemp2";
            btCalcTemp2.UseVisualStyleBackColor = true;
            btCalcTemp2.Visible = false;
            btCalcTemp2.Click += btCalcTemp2_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 40);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(351, 20);
            label4.TabIndex = 48;
            label4.Text = "Seriennummer für Neuromodule (MAX. 16 Zeichen):";
            // 
            // txtSerialNo
            // 
            txtSerialNo.Location = new Point(16, 65);
            txtSerialNo.Margin = new Padding(4, 5, 4, 5);
            txtSerialNo.MaxLength = 16;
            txtSerialNo.Name = "txtSerialNo";
            txtSerialNo.Size = new Size(196, 27);
            txtSerialNo.TabIndex = 47;
            txtSerialNo.Text = "SNQ000000";
            txtSerialNo.TextChanged += txtSerialNo_TextChanged;
            // 
            // dgvMeasurements
            // 
            dgvMeasurements.AllowUserToAddRows = false;
            dgvMeasurements.AllowUserToDeleteRows = false;
            dgvMeasurements.AutoGenerateColumns = false;
            dgvMeasurements.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMeasurements.Columns.AddRange(new DataGridViewColumn[] { Column1, Measurement_Active, Measurement_SavetoDB, Measurement_done, Measurement_Duration });
            dgvMeasurements.DataSource = cMeasurementsBindingSource;
            dgvMeasurements.Location = new Point(27, 660);
            dgvMeasurements.Margin = new Padding(4, 5, 4, 5);
            dgvMeasurements.Name = "dgvMeasurements";
            dgvMeasurements.RowHeadersVisible = false;
            dgvMeasurements.RowHeadersWidth = 51;
            dgvMeasurements.Size = new Size(776, 365);
            dgvMeasurements.TabIndex = 44;
            dgvMeasurements.DataBindingComplete += dgvMeasurements_DataBindingComplete;
            dgvMeasurements.DataError += dgvMeasurements_DataError;
            // 
            // Column1
            // 
            Column1.DataPropertyName = "Measurement_Name";
            Column1.HeaderText = "Measurement / Process";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.Resizable = DataGridViewTriState.False;
            Column1.Width = 350;
            // 
            // Measurement_Active
            // 
            Measurement_Active.DataPropertyName = "Measurement_Active";
            Measurement_Active.HeaderText = "Active";
            Measurement_Active.MinimumWidth = 6;
            Measurement_Active.Name = "Measurement_Active";
            Measurement_Active.Resizable = DataGridViewTriState.False;
            Measurement_Active.Width = 50;
            // 
            // Measurement_SavetoDB
            // 
            Measurement_SavetoDB.DataPropertyName = "Measurement_SavetoDB";
            Measurement_SavetoDB.HeaderText = "Save";
            Measurement_SavetoDB.MinimumWidth = 6;
            Measurement_SavetoDB.Name = "Measurement_SavetoDB";
            Measurement_SavetoDB.Resizable = DataGridViewTriState.False;
            Measurement_SavetoDB.Width = 50;
            // 
            // Measurement_done
            // 
            Measurement_done.DataPropertyName = "Measurement_done";
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(224, 224, 224);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(224, 224, 224);
            dataGridViewCellStyle1.NullValue = false;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(224, 224, 224);
            Measurement_done.DefaultCellStyle = dataGridViewCellStyle1;
            Measurement_done.HeaderText = "Done";
            Measurement_done.MinimumWidth = 6;
            Measurement_done.Name = "Measurement_done";
            Measurement_done.Resizable = DataGridViewTriState.False;
            Measurement_done.Width = 50;
            // 
            // Measurement_Duration
            // 
            Measurement_Duration.DataPropertyName = "Measurement_Duration";
            dataGridViewCellStyle2.Format = "N2";
            dataGridViewCellStyle2.NullValue = null;
            Measurement_Duration.DefaultCellStyle = dataGridViewCellStyle2;
            Measurement_Duration.HeaderText = "Duration [s]";
            Measurement_Duration.MinimumWidth = 6;
            Measurement_Duration.Name = "Measurement_Duration";
            Measurement_Duration.Resizable = DataGridViewTriState.False;
            Measurement_Duration.Width = 125;
            // 
            // cMeasurementsBindingSource
            // 
            cMeasurementsBindingSource.DataSource = typeof(CMeasurements);
            // 
            // openFileDialog_txt
            // 
            openFileDialog_txt.FileName = "openFileDialog_txt";
            openFileDialog_txt.Filter = "TXT files|*.txt";
            // 
            // tabMonitor
            // 
            tabMonitor.Controls.Add(tabPLog);
            tabMonitor.Controls.Add(tabPModuleInfo);
            tabMonitor.Location = new Point(844, 20);
            tabMonitor.Margin = new Padding(4, 5, 4, 5);
            tabMonitor.Name = "tabMonitor";
            tabMonitor.SelectedIndex = 0;
            tabMonitor.Size = new Size(740, 500);
            tabMonitor.TabIndex = 174;
            // 
            // tabPLog
            // 
            tabPLog.Controls.Add(txtStatus);
            tabPLog.Location = new Point(4, 29);
            tabPLog.Margin = new Padding(4, 5, 4, 5);
            tabPLog.Name = "tabPLog";
            tabPLog.Padding = new Padding(4, 5, 4, 5);
            tabPLog.Size = new Size(732, 467);
            tabPLog.TabIndex = 0;
            tabPLog.Text = "Log";
            tabPLog.UseVisualStyleBackColor = true;
            // 
            // txtStatus
            // 
            txtStatus.AddDate = false;
            txtStatus.AddTime = false;
            txtStatus.ClearLines = 0;
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.Location = new Point(4, 5);
            txtStatus.Margin = new Padding(4, 5, 4, 5);
            txtStatus.MaxLines = 0;
            txtStatus.Name = "txtStatus";
            txtStatus.RedTextAddedd = false;
            txtStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtStatus.Size = new Size(724, 457);
            txtStatus.TabIndex = 0;
            txtStatus.Text = "";
            // 
            // tabPModuleInfo
            // 
            tabPModuleInfo.Controls.Add(btCalValstoDB);
            tabPModuleInfo.Controls.Add(dgv_SWChannelInfo);
            tabPModuleInfo.Location = new Point(4, 29);
            tabPModuleInfo.Margin = new Padding(4, 5, 4, 5);
            tabPModuleInfo.Name = "tabPModuleInfo";
            tabPModuleInfo.Padding = new Padding(4, 5, 4, 5);
            tabPModuleInfo.Size = new Size(732, 467);
            tabPModuleInfo.TabIndex = 1;
            tabPModuleInfo.Text = "Module Info";
            tabPModuleInfo.UseVisualStyleBackColor = true;
            // 
            // btCalValstoDB
            // 
            btCalValstoDB.Location = new Point(8, 406);
            btCalValstoDB.Margin = new Padding(4, 5, 4, 5);
            btCalValstoDB.Name = "btCalValstoDB";
            btCalValstoDB.Size = new Size(148, 35);
            btCalValstoDB.TabIndex = 1;
            btCalValstoDB.Text = "Cal values to DB";
            btCalValstoDB.UseVisualStyleBackColor = true;
            btCalValstoDB.Visible = false;
            // 
            // dgv_SWChannelInfo
            // 
            dgv_SWChannelInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_SWChannelInfo.Dock = DockStyle.Top;
            dgv_SWChannelInfo.Location = new Point(4, 5);
            dgv_SWChannelInfo.Margin = new Padding(4, 5, 4, 5);
            dgv_SWChannelInfo.Name = "dgv_SWChannelInfo";
            dgv_SWChannelInfo.RowHeadersWidth = 51;
            dgv_SWChannelInfo.Size = new Size(724, 365);
            dgv_SWChannelInfo.TabIndex = 0;
            // 
            // tmrGui
            // 
            tmrGui.Enabled = true;
            tmrGui.Interval = 200;
            tmrGui.Tick += tmrGui_Tick;
            // 
            // cFlowChartDX1
            // 
            cFlowChartDX1.BackColor = SystemColors.Control;
            cFlowChartDX1.ChartBackColor = SystemColors.Control;
            cFlowChartDX1.Location = new Point(852, 546);
            cFlowChartDX1.Margin = new Padding(3, 4, 3, 4);
            cFlowChartDX1.Name = "cFlowChartDX1";
            cFlowChartDX1.numberOfCharts = 2;
            cFlowChartDX1.Size = new Size(728, 479);
            cFlowChartDX1.TabIndex = 175;
            // 
            // cbComPortSelector
            // 
            cbComPortSelector.FormattingEnabled = true;
            cbComPortSelector.Location = new Point(297, 78);
            cbComPortSelector.Name = "cbComPortSelector";
            cbComPortSelector.Size = new Size(140, 28);
            cbComPortSelector.TabIndex = 18;
            // 
            // frmInsight_Manufacturing5
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1600, 1042);
            Controls.Add(tabMeasurements);
            Controls.Add(tabMonitor);
            Controls.Add(cFlowChartDX1);
            Controls.Add(dgvMeasurements);
            Controls.Add(groupBox1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmInsight_Manufacturing5";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Insight Manufacturing 6.0.0";
            FormClosing += frmInsight_Manufacturing5_FormClosing;
            Load += Insight_Manufacturing5_Load;
            tabMeasurements.ResumeLayout(false);
            tabProgNeuroModul.ResumeLayout(false);
            tabProgNeuroModul.PerformLayout();
            gbModuleSelection.ResumeLayout(false);
            tabFY6900.ResumeLayout(false);
            gbPhidget.ResumeLayout(false);
            gbPhidget.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            gbFY6900.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrequqency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownAmplitude).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMeasurements).EndInit();
            ((System.ComponentModel.ISupportInitialize)cMeasurementsBindingSource).EndInit();
            tabMonitor.ResumeLayout(false);
            tabPLog.ResumeLayout(false);
            tabPModuleInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgv_SWChannelInfo).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TabControl tabMeasurements;
        private System.Windows.Forms.TabPage tabProgNeuroModul;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblNoFeedback;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSerialNo;
        private System.Windows.Forms.Button btnProgNMo;
        private System.Windows.Forms.GroupBox gbModuleSelection;
        private System.Windows.Forms.FlowLayoutPanel flpModuleSelection;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btModulICD3auslesen;
        private System.Windows.Forms.Button btModulNMauslesen;
        private System.Windows.Forms.DataGridView dgvMeasurements;
        private System.Windows.Forms.Button btReadhexFile;
        private System.Windows.Forms.Button btShowDatabase;
        private System.Windows.Forms.Button btCalcTemp2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Measurement_Active;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Measurement_SavetoDB;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Measurement_done;
        private System.Windows.Forms.DataGridViewTextBoxColumn Measurement_Duration;
        private System.Windows.Forms.TabPage tabFY6900;
        private ComponentsLib_GUI.CToggleButton cToggleButton_COM;
        private FeedbackDataLib_GUI.UcFlowChartDX_NM cFlowChartDX1;
        private System.Windows.Forms.NumericUpDown numericUpDownAmplitude;
        private System.Windows.Forms.Button btSetAmplitude;
        private System.Windows.Forms.NumericUpDown numericUpDownFrequqency;
        private System.Windows.Forms.Button setFrequency;
        private System.Windows.Forms.Button btSetSinus;
        private System.Windows.Forms.CheckBox cbPhi_ICDOn;
        private System.Windows.Forms.CheckBox cbOffsetOn;
        private System.Windows.Forms.CheckBox cbEEGOn;
        private System.Windows.Forms.CheckBox cbOffsetPlus;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbOffsetLow;
        private System.Windows.Forms.RadioButton rbOffsetHigh;
        private System.Windows.Forms.Button btArtifECG_002;
        private System.Windows.Forms.Button btArtifECG_004;
        private System.Windows.Forms.Button btArtifECG_003;
        private System.Windows.Forms.OpenFileDialog openFileDialog_txt;
        private System.Windows.Forms.TabControl tabMonitor;
        private System.Windows.Forms.TabPage tabPLog;
        private System.Windows.Forms.TabPage tabPModuleInfo;
        private System.Windows.Forms.DataGridView dgv_SWChannelInfo;
        private System.Windows.Forms.Button btArtifECG_005;
        private System.Windows.Forms.GroupBox gbPhidget;
        private System.Windows.Forms.GroupBox gbFY6900;
        private System.Windows.Forms.Button btGetProgrammers;
        private System.Windows.Forms.ComboBox cbProgrammer;
        private System.Windows.Forms.Button btShowRS232;
        private ComponentsLib_GUI.CStatusText txtStatus;
        private System.Windows.Forms.Timer tmrGui;
        private System.Windows.Forms.Label lblFWVersion;
        private System.Windows.Forms.Button btCalValstoDB;
        private System.Windows.Forms.CheckBox cbIgnoreSerialNumberCheck;
        private BindingSource cMeasurementsBindingSource;
        private ComboBox cbComPortSelector;
    }
}
