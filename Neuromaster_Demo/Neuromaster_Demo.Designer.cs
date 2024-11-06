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
            btGetConfigModules = new Button();
            btSetConfig = new Button();
            btGetClock = new Button();
            btSetClock = new Button();
            pnControls = new Panel();
            cChannelsControlV2x11 = new FeedbackDataLib_GUI.CChannelsControlV2x1();
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
            pnControls.SuspendLayout();
            gbConnectivitie.SuspendLayout();
            gbClock.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btGetConfigModules
            // 
            btGetConfigModules.Location = new Point(19, 491);
            btGetConfigModules.Margin = new Padding(4, 5, 4, 5);
            btGetConfigModules.Name = "btGetConfigModules";
            btGetConfigModules.Size = new Size(261, 43);
            btGetConfigModules.TabIndex = 129;
            btGetConfigModules.Text = "Get Config Modules";
            btGetConfigModules.UseVisualStyleBackColor = true;
            btGetConfigModules.Click += BtGetConfigModules_Click;
            // 
            // btSetConfig
            // 
            btSetConfig.Location = new Point(303, 488);
            btSetConfig.Margin = new Padding(4, 5, 4, 5);
            btSetConfig.Name = "btSetConfig";
            btSetConfig.Size = new Size(232, 46);
            btSetConfig.TabIndex = 130;
            btSetConfig.Text = "Set Config";
            btSetConfig.UseVisualStyleBackColor = true;
            btSetConfig.Click += BtSetConfig_Click;
            // 
            // btGetClock
            // 
            btGetClock.Location = new Point(8, 69);
            btGetClock.Margin = new Padding(4, 5, 4, 5);
            btGetClock.Name = "btGetClock";
            btGetClock.Size = new Size(208, 35);
            btGetClock.TabIndex = 135;
            btGetClock.Text = "Get Clock";
            btGetClock.UseVisualStyleBackColor = true;
            btGetClock.Click += BtGetClock_Click;
            // 
            // btSetClock
            // 
            btSetClock.Location = new Point(8, 109);
            btSetClock.Margin = new Padding(4, 5, 4, 5);
            btSetClock.Name = "btSetClock";
            btSetClock.Size = new Size(208, 35);
            btSetClock.TabIndex = 136;
            btSetClock.Text = "Set Clock";
            btSetClock.UseVisualStyleBackColor = true;
            btSetClock.Click += BtSetClock_Click;
            // 
            // pnControls
            // 
            pnControls.Controls.Add(cChannelsControlV2x11);
            pnControls.Controls.Add(btSetAllConfig);
            pnControls.Controls.Add(btGetConfigModules);
            pnControls.Controls.Add(btSetConfig);
            pnControls.Location = new Point(887, 15);
            pnControls.Margin = new Padding(4, 5, 4, 5);
            pnControls.Name = "pnControls";
            pnControls.Size = new Size(787, 546);
            pnControls.TabIndex = 143;
            // 
            // cChannelsControlV2x11
            // 
            cChannelsControlV2x11.BackColor = Color.FromArgb(224, 224, 224);
            cChannelsControlV2x11.Location = new Point(15, 0);
            cChannelsControlV2x11.Margin = new Padding(5, 6, 5, 6);
            cChannelsControlV2x11.Name = "cChannelsControlV2x11";
            cChannelsControlV2x11.Size = new Size(772, 477);
            cChannelsControlV2x11.TabIndex = 142;
            cChannelsControlV2x11.ModuleRowChanged += CChannelsControlV2x11_ModuleRowChanged;
            // 
            // btSetAllConfig
            // 
            btSetAllConfig.Location = new Point(543, 488);
            btSetAllConfig.Margin = new Padding(4, 5, 4, 5);
            btSetAllConfig.Name = "btSetAllConfig";
            btSetAllConfig.Size = new Size(233, 46);
            btSetAllConfig.TabIndex = 144;
            btSetAllConfig.Text = "Set whole Configuration";
            btSetAllConfig.UseVisualStyleBackColor = true;
            btSetAllConfig.Click += BtSetAllConfig_Click;
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
            gbConnectivitie.Location = new Point(887, 769);
            gbConnectivitie.Margin = new Padding(4, 5, 4, 5);
            gbConnectivitie.Name = "gbConnectivitie";
            gbConnectivitie.Padding = new Padding(4, 5, 4, 5);
            gbConnectivitie.Size = new Size(785, 292);
            gbConnectivitie.TabIndex = 157;
            gbConnectivitie.TabStop = false;
            gbConnectivitie.Text = "Connectivity";
            // 
            // pbXBEESignalStrength
            // 
            pbXBEESignalStrength.Location = new Point(139, 205);
            pbXBEESignalStrength.Margin = new Padding(4, 5, 4, 5);
            pbXBEESignalStrength.Name = "pbXBEESignalStrength";
            pbXBEESignalStrength.Size = new Size(155, 31);
            pbXBEESignalStrength.TabIndex = 155;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(135, 180);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(148, 20);
            label1.TabIndex = 154;
            label1.Text = "XBee Signal Strength";
            // 
            // lblXBeeCapacity
            // 
            lblXBeeCapacity.AutoSize = true;
            lblXBeeCapacity.Location = new Point(207, 125);
            lblXBeeCapacity.Margin = new Padding(4, 0, 4, 0);
            lblXBeeCapacity.Name = "lblXBeeCapacity";
            lblXBeeCapacity.Size = new Size(17, 20);
            lblXBeeCapacity.TabIndex = 153;
            lblXBeeCapacity.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(135, 98);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(161, 20);
            label2.TabIndex = 152;
            label2.Text = "XBee Channel Capacity";
            // 
            // pbXBeeChannelCapacity
            // 
            pbXBeeChannelCapacity.Location = new Point(139, 125);
            pbXBeeChannelCapacity.Margin = new Padding(4, 5, 4, 5);
            pbXBeeChannelCapacity.Maximum = 2000;
            pbXBeeChannelCapacity.Name = "pbXBeeChannelCapacity";
            pbXBeeChannelCapacity.Size = new Size(155, 31);
            pbXBeeChannelCapacity.TabIndex = 151;
            // 
            // txtStatus
            // 
            txtStatus.Location = new Point(304, 25);
            txtStatus.Margin = new Padding(4, 5, 4, 5);
            txtStatus.Name = "txtStatus";
            txtStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtStatus.Size = new Size(456, 252);
            txtStatus.TabIndex = 146;
            txtStatus.Text = "";
            // 
            // tbConnect
            // 
            tbConnect.AcceptChange = false;
            tbConnect.BackColor = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState1 = Color.FromArgb(255, 128, 128);
            tbConnect.ColorState2 = Color.FromArgb(128, 255, 128);
            tbConnect.Location = new Point(139, 29);
            tbConnect.Margin = new Padding(4, 5, 4, 5);
            tbConnect.Name = "tbConnect";
            tbConnect.Size = new Size(155, 48);
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
            gbClock.Location = new Point(1432, 571);
            gbClock.Margin = new Padding(4, 5, 4, 5);
            gbClock.Name = "gbClock";
            gbClock.Padding = new Padding(4, 5, 4, 5);
            gbClock.Size = new Size(239, 154);
            gbClock.TabIndex = 161;
            gbClock.TabStop = false;
            gbClock.Text = "Clock Neuromaster";
            // 
            // txtTime
            // 
            txtTime.Location = new Point(8, 29);
            txtTime.Margin = new Padding(4, 5, 4, 5);
            txtTime.Name = "txtTime";
            txtTime.Size = new Size(207, 27);
            txtTime.TabIndex = 138;
            // 
            // btResync
            // 
            btResync.Location = new Point(748, 1026);
            btResync.Margin = new Padding(4, 5, 4, 5);
            btResync.Name = "btResync";
            btResync.Size = new Size(131, 35);
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
            radioButton1.Size = new Size(89, 24);
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
            radioButton2.Size = new Size(83, 24);
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
            txtData.Location = new Point(17, 20);
            txtData.Margin = new Padding(4, 5, 4, 5);
            txtData.Multiline = true;
            txtData.Name = "txtData";
            txtData.Size = new Size(855, 795);
            txtData.TabIndex = 168;
            // 
            // btGetModuleSpecific
            // 
            btGetModuleSpecific.Location = new Point(1223, 571);
            btGetModuleSpecific.Margin = new Padding(4, 5, 4, 5);
            btGetModuleSpecific.Name = "btGetModuleSpecific";
            btGetModuleSpecific.Size = new Size(181, 43);
            btGetModuleSpecific.TabIndex = 169;
            btGetModuleSpecific.Text = "Get Module specific";
            btGetModuleSpecific.UseVisualStyleBackColor = true;
            btGetModuleSpecific.Click += BtGetModuleSpecific_Click;
            // 
            // btSetModuleSpecific
            // 
            btSetModuleSpecific.Location = new Point(1223, 626);
            btSetModuleSpecific.Margin = new Padding(4, 5, 4, 5);
            btSetModuleSpecific.Name = "btSetModuleSpecific";
            btSetModuleSpecific.Size = new Size(181, 43);
            btSetModuleSpecific.TabIndex = 170;
            btSetModuleSpecific.Text = "Set Module Specific";
            btSetModuleSpecific.UseVisualStyleBackColor = true;
            btSetModuleSpecific.Click += BtSetModuleSpecific_Click;
            // 
            // btOpenTCP
            // 
            btOpenTCP.Location = new Point(887, 592);
            btOpenTCP.Margin = new Padding(4, 5, 4, 5);
            btOpenTCP.Name = "btOpenTCP";
            btOpenTCP.Size = new Size(181, 43);
            btOpenTCP.TabIndex = 171;
            btOpenTCP.Text = "Open TCP";
            btOpenTCP.UseVisualStyleBackColor = true;
            btOpenTCP.Click += BtOpenTCP_Click;
            // 
            // btNMInfo
            // 
            btNMInfo.Location = new Point(1440, 735);
            btNMInfo.Margin = new Padding(4, 5, 4, 5);
            btNMInfo.Name = "btNMInfo";
            btNMInfo.Size = new Size(207, 43);
            btNMInfo.TabIndex = 172;
            btNMInfo.Text = "Get Neuromaster Info";
            btNMInfo.UseVisualStyleBackColor = true;
            btNMInfo.Click += BtNMInfo_Click;
            // 
            // Neuromaster_Demo_Library
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1704, 1078);
            Controls.Add(btNMInfo);
            Controls.Add(btOpenTCP);
            Controls.Add(btGetModuleSpecific);
            Controls.Add(btSetModuleSpecific);
            Controls.Add(txtData);
            Controls.Add(btResync);
            Controls.Add(gbClock);
            Controls.Add(gbConnectivitie);
            Controls.Add(pnControls);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Neuromaster_Demo_Library";
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

        private System.Windows.Forms.Button btGetConfigModules;
        private System.Windows.Forms.Button btSetConfig;
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
        private FeedbackDataLib_GUI.CChannelsControlV2x1 cChannelsControlV2x11;
        private System.Windows.Forms.Button btGetModuleSpecific;
        private System.Windows.Forms.Button btSetModuleSpecific;
        private Button btOpenTCP;
        private Button btNMInfo;
    }
}
