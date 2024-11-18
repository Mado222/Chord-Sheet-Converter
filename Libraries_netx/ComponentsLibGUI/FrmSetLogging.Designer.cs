namespace ComponentsLibGUI
{
    partial class FrmSetLogging
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
            CbLogginOnOff = new CheckBox();
            label1 = new Label();
            TxTLogPath = new TextBox();
            label2 = new Label();
            CbLogLevel = new ComboBox();
            saveFileDialog1 = new SaveFileDialog();
            SuspendLayout();
            // 
            // CbLogginOnOff
            // 
            CbLogginOnOff.AutoSize = true;
            CbLogginOnOff.Location = new Point(21, 32);
            CbLogginOnOff.Name = "CbLogginOnOff";
            CbLogginOnOff.Size = new Size(136, 24);
            CbLogginOnOff.TabIndex = 0;
            CbLogginOnOff.Text = "Logging On/Off";
            CbLogginOnOff.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(21, 85);
            label1.Name = "label1";
            label1.Size = new Size(96, 20);
            label1.TabIndex = 1;
            label1.Text = "Log File Path:";
            // 
            // TxTLogPath
            // 
            TxTLogPath.Location = new Point(139, 82);
            TxTLogPath.Name = "TxTLogPath";
            TxTLogPath.Size = new Size(512, 27);
            TxTLogPath.TabIndex = 2;
            TxTLogPath.MouseClick += TxTLogPath_MouseClick;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(21, 137);
            label2.Name = "label2";
            label2.Size = new Size(72, 20);
            label2.TabIndex = 3;
            label2.Text = "Log Level";
            // 
            // CbLogLevel
            // 
            CbLogLevel.FormattingEnabled = true;
            CbLogLevel.Location = new Point(139, 134);
            CbLogLevel.Name = "CbLogLevel";
            CbLogLevel.Size = new Size(230, 28);
            CbLogLevel.TabIndex = 4;
            // 
            // FrmSetLogging
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(729, 205);
            Controls.Add(CbLogLevel);
            Controls.Add(label2);
            Controls.Add(TxTLogPath);
            Controls.Add(label1);
            Controls.Add(CbLogginOnOff);
            Name = "FrmSetLogging";
            Text = "Set Logging Parameters";
            FormClosing += FrmSetLogging_FormClosing;
            Load += FrmSetLogging_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox CbLogginOnOff;
        private Label label1;
        private TextBox TxTLogPath;
        private Label label2;
        private ComboBox CbLogLevel;
        private SaveFileDialog saveFileDialog1;
    }
}