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
            button1 = new Button();
            button2 = new Button();
            label3 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            richTextBoxLogs = new RichTextBox();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // CbLogginOnOff
            // 
            CbLogginOnOff.AutoSize = true;
            CbLogginOnOff.Location = new Point(4, 4);
            CbLogginOnOff.Margin = new Padding(4);
            CbLogginOnOff.Name = "CbLogginOnOff";
            CbLogginOnOff.Size = new Size(162, 29);
            CbLogginOnOff.TabIndex = 0;
            CbLogginOnOff.Text = "Logging On/Off";
            CbLogginOnOff.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 37);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(116, 25);
            label1.TabIndex = 1;
            label1.Text = "Log File Path:";
            // 
            // TxTLogPath
            // 
            TxTLogPath.Location = new Point(128, 31);
            TxTLogPath.Margin = new Padding(4);
            TxTLogPath.Name = "TxTLogPath";
            TxTLogPath.Size = new Size(639, 31);
            TxTLogPath.TabIndex = 2;
            TxTLogPath.MouseClick += TxTLogPath_MouseClick;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 87);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(86, 25);
            label2.TabIndex = 3;
            label2.Text = "Log Level";
            // 
            // CbLogLevel
            // 
            CbLogLevel.FormattingEnabled = true;
            CbLogLevel.Location = new Point(128, 84);
            CbLogLevel.Margin = new Padding(4);
            CbLogLevel.Name = "CbLogLevel";
            CbLogLevel.Size = new Size(286, 33);
            CbLogLevel.TabIndex = 4;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(192, 255, 192);
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(421, 78);
            button1.Name = "button1";
            button1.Size = new Size(143, 43);
            button1.TabIndex = 5;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(255, 192, 192);
            button2.DialogResult = DialogResult.Cancel;
            button2.Location = new Point(579, 78);
            button2.Name = "button2";
            button2.Size = new Size(188, 43);
            button2.TabIndex = 6;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(635, 8);
            label3.Name = "label3";
            label3.Size = new Size(132, 25);
            label3.TabIndex = 7;
            label3.Text = "Click to change";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(richTextBoxLogs, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 28.4810123F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 71.51899F));
            tableLayoutPanel1.Size = new Size(836, 632);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.Controls.Add(CbLogginOnOff);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(CbLogLevel);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(TxTLogPath);
            panel1.Controls.Add(label2);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(830, 174);
            panel1.TabIndex = 0;
            // 
            // richTextBoxLogs
            // 
            richTextBoxLogs.Dock = DockStyle.Fill;
            richTextBoxLogs.Location = new Point(3, 183);
            richTextBoxLogs.Name = "richTextBoxLogs";
            richTextBoxLogs.Size = new Size(830, 446);
            richTextBoxLogs.TabIndex = 1;
            richTextBoxLogs.Text = "";
            // 
            // FrmSetLogging
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(836, 632);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4);
            Name = "FrmSetLogging";
            Text = "Set Logging Parameters";
            Load += FrmSetLogging_Load;
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckBox CbLogginOnOff;
        private Label label1;
        private TextBox TxTLogPath;
        private Label label2;
        private ComboBox CbLogLevel;
        private SaveFileDialog saveFileDialog1;
        private Button button1;
        private Button button2;
        private Label label3;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private RichTextBox richTextBoxLogs;
    }
}