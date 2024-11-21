namespace ComponentsLibGUI
{
    partial class UCSetLogging
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CbLogginOnOff = new CheckBox();
            LblLogPath = new Label();
            label1 = new Label();
            TxTLogPath = new TextBox();
            BtOK = new Button();
            BtCancel = new Button();
            CbLogLevel = new ComboBox();
            label2 = new Label();
            PnDisplay = new Panel();
            SuspendLayout();
            // 
            // CbLogginOnOff
            // 
            CbLogginOnOff.AutoSize = true;
            CbLogginOnOff.Location = new Point(3, 3);
            CbLogginOnOff.Name = "CbLogginOnOff";
            CbLogginOnOff.Size = new Size(136, 24);
            CbLogginOnOff.TabIndex = 1;
            CbLogginOnOff.Text = "Logging On/Off";
            CbLogginOnOff.UseVisualStyleBackColor = true;
            // 
            // LblLogPath
            // 
            LblLogPath.AutoSize = true;
            LblLogPath.Location = new Point(446, 10);
            LblLogPath.Margin = new Padding(2, 0, 2, 0);
            LblLogPath.Name = "LblLogPath";
            LblLogPath.Size = new Size(110, 20);
            LblLogPath.TabIndex = 10;
            LblLogPath.Text = "Click to change";
            LblLogPath.Click += LblLogPath_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1, 38);
            label1.Name = "label1";
            label1.Size = new Size(96, 20);
            label1.TabIndex = 8;
            label1.Text = "Log File Path:";
            // 
            // TxTLogPath
            // 
            TxTLogPath.Location = new Point(100, 33);
            TxTLogPath.Name = "TxTLogPath";
            TxTLogPath.Size = new Size(456, 27);
            TxTLogPath.TabIndex = 9;
            // 
            // BtOK
            // 
            BtOK.BackColor = Color.FromArgb(192, 255, 192);
            BtOK.DialogResult = DialogResult.OK;
            BtOK.Location = new Point(335, 75);
            BtOK.Margin = new Padding(2);
            BtOK.Name = "BtOK";
            BtOK.Size = new Size(95, 34);
            BtOK.TabIndex = 13;
            BtOK.Text = "OK";
            BtOK.UseVisualStyleBackColor = false;
            // 
            // BtCancel
            // 
            BtCancel.BackColor = Color.FromArgb(255, 192, 192);
            BtCancel.DialogResult = DialogResult.Cancel;
            BtCancel.Location = new Point(461, 75);
            BtCancel.Margin = new Padding(2);
            BtCancel.Name = "BtCancel";
            BtCancel.Size = new Size(95, 34);
            BtCancel.TabIndex = 14;
            BtCancel.Text = "Cancel";
            BtCancel.UseVisualStyleBackColor = false;
            // 
            // CbLogLevel
            // 
            CbLogLevel.FormattingEnabled = true;
            CbLogLevel.Location = new Point(100, 80);
            CbLogLevel.Name = "CbLogLevel";
            CbLogLevel.Size = new Size(230, 28);
            CbLogLevel.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1, 83);
            label2.Name = "label2";
            label2.Size = new Size(72, 20);
            label2.TabIndex = 11;
            label2.Text = "Log Level";
            // 
            // GbDisplay
            // 
            PnDisplay.Dock = DockStyle.Right;
            PnDisplay.Location = new Point(610, 0);
            PnDisplay.Name = "GbDisplay";
            PnDisplay.Size = new Size(225, 127);
            PnDisplay.TabIndex = 15;
            PnDisplay.TabStop = false;
            // 
            // UCSetLogging
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 224, 192);
            Controls.Add(PnDisplay);
            Controls.Add(BtOK);
            Controls.Add(BtCancel);
            Controls.Add(CbLogLevel);
            Controls.Add(label2);
            Controls.Add(LblLogPath);
            Controls.Add(label1);
            Controls.Add(TxTLogPath);
            Controls.Add(CbLogginOnOff);
            Name = "UCSetLogging";
            Size = new Size(835, 127);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox CbLogginOnOff;
        private Label LblLogPath;
        private Label label1;
        private TextBox TxTLogPath;
        private Button BtOK;
        private Button BtCancel;
        private ComboBox CbLogLevel;
        private Label label2;
        private Panel PnDisplay;
    }
}
