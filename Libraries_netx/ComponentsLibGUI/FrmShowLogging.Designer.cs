namespace ComponentsLibGUI
{
    partial class FrmShowLogging
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
            tableLayoutPanel1 = new TableLayoutPanel();
            richTextBoxLogs = new RichTextBox();
            _ucSetLogging = new UCSetLogging();
            panel1 = new Panel();
            BtClearLog = new Button();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195F));
            tableLayoutPanel1.Controls.Add(richTextBoxLogs, 0, 1);
            tableLayoutPanel1.Controls.Add(_ucSetLogging, 0, 0);
            tableLayoutPanel1.Controls.Add(panel1, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 17.059639F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 82.94036F));
            tableLayoutPanel1.Size = new Size(1071, 721);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // richTextBoxLogs
            // 
            tableLayoutPanel1.SetColumnSpan(richTextBoxLogs, 2);
            richTextBoxLogs.Dock = DockStyle.Fill;
            richTextBoxLogs.Location = new Point(2, 125);
            richTextBoxLogs.Margin = new Padding(2);
            richTextBoxLogs.Name = "richTextBoxLogs";
            richTextBoxLogs.Size = new Size(1067, 594);
            richTextBoxLogs.TabIndex = 3;
            richTextBoxLogs.Text = "";
            // 
            // _ucSetLogging
            // 
            _ucSetLogging.BackColor = Color.FromArgb(255, 224, 192);
            _ucSetLogging.Dock = DockStyle.Top;
            _ucSetLogging.Location = new Point(3, 3);
            _ucSetLogging.Name = "_ucSetLogging";
            _ucSetLogging.Size = new Size(870, 117);
            _ucSetLogging.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(BtClearLog);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(879, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(189, 117);
            panel1.TabIndex = 5;
            // 
            // BtClearLog
            // 
            BtClearLog.Location = new Point(15, 75);
            BtClearLog.Name = "BtClearLog";
            BtClearLog.Size = new Size(94, 29);
            BtClearLog.TabIndex = 0;
            BtClearLog.Text = "Clear Log";
            BtClearLog.UseVisualStyleBackColor = true;
            BtClearLog.Click += BtClearLog_Click;
            // 
            // FrmShowLogging
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1071, 721);
            Controls.Add(tableLayoutPanel1);
            Name = "FrmShowLogging";
            Text = "Console Output";
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private RichTextBox richTextBoxLogs;
        private UCSetLogging _ucSetLogging;
        private Panel panel1;
        private Button BtClearLog;
    }
}