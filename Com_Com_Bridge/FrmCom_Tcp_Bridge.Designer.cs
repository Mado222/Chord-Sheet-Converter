namespace Com_Com_Bridge
{
    partial class FrmCom_Tcp_Bridge
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cToggleButton1 = new ComponentsLib_GUI.CToggleButton();
            ucComPortSelectorIn = new ComponentsLib_GUI.UCComPortSelector();
            tableLayoutPanel1 = new TableLayoutPanel();
            txtStatus = new ComponentsLib_GUI.CStatusText();
            ucComPortSelectorOut = new ComponentsLib_GUI.UCComPortSelector();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // cToggleButton1
            // 
            cToggleButton1.AcceptChange = true;
            cToggleButton1.BackColor = Color.FromArgb(192, 255, 192);
            cToggleButton1.ColorState1 = Color.FromArgb(192, 255, 192);
            cToggleButton1.ColorState2 = Color.FromArgb(255, 192, 192);
            cToggleButton1.Dock = DockStyle.Left;
            cToggleButton1.Location = new Point(541, 48);
            cToggleButton1.Name = "cToggleButton1";
            cToggleButton1.Size = new Size(124, 71);
            cToggleButton1.TabIndex = 1;
            cToggleButton1.Text = "Open COM";
            cToggleButton1.TextState1 = "Open COM";
            cToggleButton1.TextState2 = "Close COM";
            cToggleButton1.UseVisualStyleBackColor = false;
            cToggleButton1.ToState2 += cToggleButton1_ToState2;
            cToggleButton1.ToState1 += cToggleButton1_ToState1;
            // 
            // ucComPortSelectorIn
            // 
            ucComPortSelectorIn.Dock = DockStyle.Top;
            ucComPortSelectorIn.FormattingEnabled = true;
            ucComPortSelectorIn.Location = new Point(411, 48);
            ucComPortSelectorIn.Name = "ucComPortSelectorIn";
            ucComPortSelectorIn.Size = new Size(124, 28);
            ucComPortSelectorIn.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51.02041F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.32653F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.32653F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.32653F));
            tableLayoutPanel1.Controls.Add(txtStatus, 0, 0);
            tableLayoutPanel1.Controls.Add(cToggleButton1, 2, 1);
            tableLayoutPanel1.Controls.Add(ucComPortSelectorOut, 3, 1);
            tableLayoutPanel1.Controls.Add(label1, 1, 0);
            tableLayoutPanel1.Controls.Add(label2, 3, 0);
            tableLayoutPanel1.Controls.Add(ucComPortSelectorIn, 1, 1);
            tableLayoutPanel1.Controls.Add(textBox1, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 37.2881355F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 62.7118645F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 327F));
            tableLayoutPanel1.Size = new Size(800, 450);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // txtStatus
            // 
            txtStatus.AddDate = false;
            txtStatus.AddTime = false;
            txtStatus.ClearLines = 0;
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.Location = new Point(3, 3);
            txtStatus.MaxLines = 0;
            txtStatus.Name = "txtStatus";
            txtStatus.RedTextAddedd = false;
            tableLayoutPanel1.SetRowSpan(txtStatus, 3);
            txtStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtStatus.Size = new Size(402, 444);
            txtStatus.TabIndex = 1;
            txtStatus.Text = "";
            // 
            // ucComPortSelectorOut
            // 
            ucComPortSelectorOut.Dock = DockStyle.Top;
            ucComPortSelectorOut.FormattingEnabled = true;
            ucComPortSelectorOut.Location = new Point(671, 48);
            ucComPortSelectorOut.Name = "ucComPortSelectorOut";
            ucComPortSelectorOut.Size = new Size(126, 28);
            ucComPortSelectorOut.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Bottom;
            label1.Location = new Point(411, 25);
            label1.Name = "label1";
            label1.Size = new Size(124, 20);
            label1.TabIndex = 5;
            label1.Text = "COM in";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Bottom;
            label2.Location = new Point(671, 25);
            label2.Name = "label2";
            label2.Size = new Size(126, 20);
            label2.TabIndex = 6;
            label2.Text = "COM out";
            // 
            // textBox1
            // 
            tableLayoutPanel1.SetColumnSpan(textBox1, 3);
            textBox1.Dock = DockStyle.Bottom;
            textBox1.Enabled = false;
            textBox1.Location = new Point(411, 367);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(386, 80);
            textBox1.TabIndex = 7;
            textBox1.Text = "Works in combination with 2 virtual Ports. https://sourceforge.net/projects/com0com/";
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // FrmCom_Tcp_Bridge
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "FrmCom_Tcp_Bridge";
            Text = "Form1";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ComponentsLib_GUI.CToggleButton cToggleButton1;
        private ComponentsLib_GUI.UCComPortSelector ucComPortSelectorIn;
        private TableLayoutPanel tableLayoutPanel1;
        private ComponentsLib_GUI.CStatusText txtStatus;
        private ComponentsLib_GUI.UCComPortSelector ucComPortSelectorOut;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
    }
}
