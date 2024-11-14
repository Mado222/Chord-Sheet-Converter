namespace Neuromaster_Demo
{
    partial class Neuromaster_Demo
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
            btGetDevice = new Button();
            btSetModuleConfig = new Button();
            btGetClock = new Button();
            btSetClock = new Button();
            pnControls = new Panel();
            cChannelsControlV2x11 = new FeedbackDataLibGUI.CChannelsControlV2x1();
            btSetAllConfig = new Button();
            gbConnectivitie = new GroupBox();
            pbXBEESignalStrength = new ProgressBar();
            label1 = new Label();
            lblXBeeCapacity = new Label();
            label2 = new Label();
            pbXBeeChannelCapacity = new ProgressBar();
            txtStatus = new RichTextBox();
            tbConnect = new Neuromaster_Demo_Library_Reduced__netx.CToggleButton();
            gbClock = new GroupBox();
            txtTime = new TextBox();
            btResync = new Button();
            groupBox2 = new GroupBox();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            tmrStatusMessages = new System.Windows.Forms.Timer(components);
            txtData = new TextBox();
            btGetModuleSpecific = new Button();
            btSetModuleSpecific = new Button();
            btOpenTCP = new Button();
            btNMInfo = new Button();
            checkBox1 = new CheckBox();
            CbLogging = new CheckBox();
            pnControls.SuspendLayout();
            gbConnectivitie.SuspendLayout();
            gbClock.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btGetDevice
            // 
            btGetDevice.Location = new Point(24, 614);
            btGetDevice.Margin = new Padding(5, 6, 5, 6);
            btGetDevice.Name = "btGetDevice";
            btGetDevice.Size = new Size(326, 54);
            btGetDevice.TabIndex = 129;
            btGetDevice.Text = "Get Config Modules";
            btGetDevice.UseVisualStyleBackColor = true;
            btGetDevice.Click += BtGetDeviceConfig_Click;
            // 
            // btSetModuleConfig
            // 
            btSetModuleConfig.Location = new Point(379, 610);
            btSetModuleConfig.Margin = new Padding(5, 6, 5, 6);
            btSetModuleConfig.Name = "btSetModuleConfig";
            btSetModuleConfig.Size = new Size(290, 58);
            btSetModuleConfig.TabIndex = 130;
            btSetModuleConfig.Text = "Set Config";
            btSetModuleConfig.UseVisualStyleBackColor = true;
            btSetModuleConfig.Click += BtSetModuleConfig_Click;
            // 
            // btGetClock
            // 
            btGetClock.Location = new Point(10, 86);
            btGetClock.Margin = new Padding(5, 6, 5, 6);
            btGetClock.Name = "btGetClock";
            btGetClock.Size = new Size(260, 44);
            btGetClock.TabIndex = 135;
            btGetClock.Text = "Get Clock";
            btGetClock.UseVisualStyleBackColor = true;
            btGetClock.Click += BtGetClock_Click;
            // 
            // btSetClock
            // 
            btSetClock.Location = new Point(10, 136);
            btSetClock.Margin = new Padding(5, 6, 5, 6);
            btSetClock.Name = "btSetClock";
            btSetClock.Size = new Size(260, 44);
            btSetClock.TabIndex = 136;
            btSetClock.Text = "Set Clock";
            btSetClock.UseVisualStyleBackColor = true;
            btSetClock.Click += BtSetClock_Click;
            // 
            // pnControls
            // 
            pnControls.Controls.Add(cChannelsControlV2x11);
            pnControls.Controls.Add(btSetAllConfig);
            pnControls.Controls.Add(btGetDevice);
            pnControls.Controls.Add(btSetModuleConfig);
            pnControls.Location = new Point(1109, 19);
            pnControls.Margin = new Padding(5, 6, 5, 6);
            pnControls.Name = "pnControls";
            pnControls.Size = new Size(984, 682);
            pnControls.TabIndex = 143;
            // 
            // cChannelsControlV2x11
            // 
            cChannelsControlV2x11.BackColor = Color.FromArgb(224, 224, 224);
            cChannelsControlV2x11.Location = new Point(19, 0);
            cChannelsControlV2x11.Margin = new Padding(6, 8, 6, 8);
            cChannelsControlV2x11.Name = "cChannelsControlV2x11";
            cChannelsControlV2x11.Size = new Size(965, 596);
            cChannelsControlV2x11.TabIndex = 142;
            cChannelsControlV2x11.ModuleRowChanged += CChannelsControlV2x11_ModuleRowChanged;
            // 
            // btSetAllConfig
            // 
            btSetAllConfig.Location = new Point(679, 610);
            btSetAllConfig.Margin = new Padding(5, 6, 5, 6);
            btSetAllConfig.Name = "btSetAllConfig";
            btSetAllConfig.Size = new Size(291, 58);
            btSetAllConfig.TabIndex = 144;
            btSetAllConfig.Text = "Set whole Configuration";
            btSetAllConfig.UseVisualStyleBackColor = true;
            btSetAllConfig.Click += BtSetDeviceConfig_Click;
            // 
            // gbConnectivitie
            // 
            gbConnectivitie.Controls.Add(pbXBEESignalStrength);
            gbConnectivitie.Controls.Add(label1);
            gbConnectivitie.Controls.Add(lblXBeeCapacity);
            gbConnectivitie.Controls.Add(label2);
            gbConnectivitie.Controls.Add(pbXBeeChannelCapacity);
            gbConnectivitie.Controls.Add(txtStatus);
            gbConnectivitie.Controls.Add(tbConnect);
            gbConnectivitie.Location = new Point(1109, 961);
            gbConnectivitie.Margin = new Padding(5, 6, 5, 6);
            gbConnectivitie.Name = "gbConnectivitie";
            gbConnectivitie.Padding = new Padding(5, 6, 5, 6);
            gbConnectivitie.Size = new Size(981, 365);
            gbConnectivitie.TabIndex = 157;
            gbConnectivitie.TabStop = false;
            gbConnectivitie.Text = "Connectivity";
            // 
            // pbXBEESignalStrength
            // 
            pbXBEESignalStrength.Location = new Point(174, 256);
            pbXBEESignalStrength.Margin = new Padding(5, 6, 5, 6);
            pbXBEESignalStrength.Name = "pbXBEESignalStrength";
            pbXBEESignalStrength.Size = new Size(194, 39);
            pbXBEESignalStrength.TabIndex = 155;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(169, 225);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new Size(176, 25);
            label1.TabIndex = 154;
            label1.Text = "XBee Signal Strength";
            // 
            // lblXBeeCapacity
            // 
            lblXBeeCapacity.AutoSize = true;
            lblXBeeCapacity.Location = new Point(259, 156);
            lblXBeeCapacity.Margin = new Padding(5, 0, 5, 0);
            lblXBeeCapacity.Name = "lblXBeeCapacity";
            lblXBeeCapacity.Size = new Size(22, 25);
            lblXBeeCapacity.TabIndex = 153;
            lblXBeeCapacity.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(169, 122);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new Size(191, 25);
            label2.TabIndex = 152;
            label2.Text = "XBee Channel Capacity";
            // 
            // pbXBeeChannelCapacity
            // 
            pbXBeeChannelCapacity.Location = new Point(174, 156);
            pbXBeeChannelCapacity.Margin = new Padding(5, 6, 5, 6);
            pbXBeeChannelCapacity.Maximum = 2000;
            pbXBeeChannelCapacity.Name = "pbXBeeChannelCapacity";
            pbXBeeChannelCapacity.Size = new Size(194, 39);
            pbXBeeChannelCapacity.TabIndex = 151;
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(380, 31);
            txtStatus.Margin = new Padding(5, 6, 5, 6);
            txtStatus.Name = "txtStatus";
            txtStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtStatus.Size = new Size(569, 314);
            txtStatus.TabIndex = 146;
            txtStatus.Text = "";
            // 
            // tbConnect
            // 
            tbConnect.AcceptChange = false;
            tbConnect.BackColor = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState1 = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState2 = Color.FromArgb(128, 255, 128);
            tbConnect.Location = new Point(174, 36);
            tbConnect.Margin = new Padding(5, 6, 5, 6);
            tbConnect.Name = "tbConnect";
            tbConnect.Size = new Size(194, 60);
            tbConnect.TabIndex = 145;
            tbConnect.Text = "Connect";
            tbConnect.TextState1 = "Connect";
            tbConnect.TextState2 = "Disconnect";
            tbConnect.UseVisualStyleBackColor = false;
            tbConnect.ToState2 += TbConnect_ToState2;
            tbConnect.ToState1 += TbConnect_ToState1;
            // 
            // gbClock
            // 
            gbClock.Controls.Add(txtTime);
            gbClock.Controls.Add(btGetClock);
            gbClock.Controls.Add(btSetClock);
            gbClock.Location = new Point(1790, 714);
            gbClock.Margin = new Padding(5, 6, 5, 6);
            gbClock.Name = "gbClock";
            gbClock.Padding = new Padding(5, 6, 5, 6);
            gbClock.Size = new Size(299, 192);
            gbClock.TabIndex = 161;
            gbClock.TabStop = false;
            gbClock.Text = "Clock Neuromaster";
            // 
            // txtTime
            // 
            txtTime.Location = new Point(10, 36);
            txtTime.Margin = new Padding(5, 6, 5, 6);
            txtTime.Name = "txtTime";
            txtTime.Size = new Size(258, 31);
            txtTime.TabIndex = 138;
            // 
            // btResync
            // 
            btResync.Location = new Point(935, 1282);
            btResync.Margin = new Padding(5, 6, 5, 6);
            btResync.Name = "btResync";
            btResync.Size = new Size(164, 44);
            btResync.TabIndex = 156;
            btResync.Text = "Resync";
            btResync.UseVisualStyleBackColor = true;
            btResync.Click += BtResync_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(radioButton1);
            groupBox2.Controls.Add(radioButton2);
            groupBox2.Location = new Point(402, 501);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(119, 75);
            groupBox2.TabIndex = 167;
            groupBox2.TabStop = false;
            groupBox2.Text = "Time Base";
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(7, 45);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(104, 29);
            radioButton1.TabIndex = 159;
            radioButton1.Text = "Absolute";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Checked = true;
            radioButton2.Location = new Point(7, 23);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(93, 29);
            radioButton2.TabIndex = 158;
            radioButton2.TabStop = true;
            radioButton2.Text = "Relative";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // tmrStatusMessages
            // 
            tmrStatusMessages.Enabled = true;
            tmrStatusMessages.Interval = 50;
            tmrStatusMessages.Tick += TmrStatusMessages_Tick;
            // 
            // txtData
            // 
            txtData.Location = new Point(21, 25);
            txtData.Margin = new Padding(5, 6, 5, 6);
            txtData.Multiline = true;
            txtData.Name = "txtData";
            txtData.Size = new Size(1068, 993);
            txtData.TabIndex = 168;
            // 
            // btGetModuleSpecific
            // 
            btGetModuleSpecific.Location = new Point(1529, 714);
            btGetModuleSpecific.Margin = new Padding(5, 6, 5, 6);
            btGetModuleSpecific.Name = "btGetModuleSpecific";
            btGetModuleSpecific.Size = new Size(226, 54);
            btGetModuleSpecific.TabIndex = 169;
            btGetModuleSpecific.Text = "Get Module specific";
            btGetModuleSpecific.UseVisualStyleBackColor = true;
            btGetModuleSpecific.Click += BtGetModuleSpecific_Click;
            // 
            // btSetModuleSpecific
            // 
            btSetModuleSpecific.Location = new Point(1529, 782);
            btSetModuleSpecific.Margin = new Padding(5, 6, 5, 6);
            btSetModuleSpecific.Name = "btSetModuleSpecific";
            btSetModuleSpecific.Size = new Size(226, 54);
            btSetModuleSpecific.TabIndex = 170;
            btSetModuleSpecific.Text = "Set Module Specific";
            btSetModuleSpecific.UseVisualStyleBackColor = true;
            btSetModuleSpecific.Click += BtSetModuleSpecific_Click;
            // 
            // btOpenTCP
            // 
            btOpenTCP.Location = new Point(1109, 740);
            btOpenTCP.Margin = new Padding(5, 6, 5, 6);
            btOpenTCP.Name = "btOpenTCP";
            btOpenTCP.Size = new Size(226, 54);
            btOpenTCP.TabIndex = 171;
            btOpenTCP.Text = "Open TCP";
            btOpenTCP.UseVisualStyleBackColor = true;
            btOpenTCP.Click += BtOpenTCP_Click;
            // 
            // btNMInfo
            // 
            btNMInfo.Location = new Point(1800, 919);
            btNMInfo.Margin = new Padding(5, 6, 5, 6);
            btNMInfo.Name = "btNMInfo";
            btNMInfo.Size = new Size(259, 54);
            btNMInfo.TabIndex = 172;
            btNMInfo.Text = "Get Neuromaster Info";
            btNMInfo.UseVisualStyleBackColor = true;
            btNMInfo.Click += BtNMInfo_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(0, 0);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(117, 29);
            checkBox1.TabIndex = 173;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // CbLogging
            // 
            CbLogging.AutoSize = true;
            CbLogging.Location = new Point(23, 1061);
            CbLogging.Name = "CbLogging";
            CbLogging.Size = new Size(100, 29);
            CbLogging.TabIndex = 174;
            CbLogging.Text = "Logging";
            CbLogging.UseVisualStyleBackColor = true;
            CbLogging.CheckedChanged += CbLogging_CheckedChanged;
            // 
            // Neuromaster_Demo
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2130, 1348);
            Controls.Add(CbLogging);
            Controls.Add(checkBox1);
            Controls.Add(btNMInfo);
            Controls.Add(btOpenTCP);
            Controls.Add(btGetModuleSpecific);
            Controls.Add(btSetModuleSpecific);
            Controls.Add(txtData);
            Controls.Add(btResync);
            Controls.Add(gbClock);
            Controls.Add(gbConnectivitie);
            Controls.Add(pnControls);
            Margin = new Padding(5, 6, 5, 6);
            Name = "Neuromaster_Demo";
            Text = "Neuromaster / Insight Instruments";
            FormClosing += NeuromasterV2_FormClosing;
            Load += NeuromasterV2_Load;
            pnControls.ResumeLayout(false);
            gbConnectivitie.ResumeLayout(false);
            gbConnectivitie.PerformLayout();
            gbClock.ResumeLayout(false);
            gbClock.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btGetDevice;
        private System.Windows.Forms.Button btSetModuleConfig;
        private System.Windows.Forms.Button btGetClock;
        private System.Windows.Forms.Button btSetClock;
        private System.Windows.Forms.Panel pnControls;
        private System.Windows.Forms.GroupBox gbConnectivitie;
        private System.Windows.Forms.RichTextBox txtStatus;
        private Neuromaster_Demo_Library_Reduced__netx.CToggleButton tbConnect;
        private System.Windows.Forms.GroupBox gbClock;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.ProgressBar pbXBeeChannelCapacity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btSetAllConfig;
        private System.Windows.Forms.Label lblXBeeCapacity;
        private System.Windows.Forms.Button btResync;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Timer tmrStatusMessages;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.ProgressBar pbXBEESignalStrength;
        private System.Windows.Forms.Label label1;
        private FeedbackDataLibGUI.CChannelsControlV2x1 cChannelsControlV2x11;
        private System.Windows.Forms.Button btGetModuleSpecific;
        private System.Windows.Forms.Button btSetModuleSpecific;
        private Button btOpenTCP;
        private Button btNMInfo;
        private CheckBox checkBox1;
        private CheckBox CbLogging;
    }
}
