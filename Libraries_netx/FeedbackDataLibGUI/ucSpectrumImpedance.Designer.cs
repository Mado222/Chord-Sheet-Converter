namespace FeedbackDataLibGUI
{
    partial class ucSpectrumImpedance
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
            tableLayoutPanel1 = new TableLayoutPanel();
            tbUa2 = new TextBox();
            tbUa1 = new TextBox();
            label7 = new Label();
            label6 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            mtbxp = new TextBox();
            mtbxn = new TextBox();
            mtbUel = new TextBox();
            lblTitle = new Label();
            cbAutoscale = new CheckBox();
            label4 = new Label();
            nudYmax = new NumericUpDown();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudYmax).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6666679F));
            tableLayoutPanel1.Controls.Add(tbUa2, 5, 2);
            tableLayoutPanel1.Controls.Add(tbUa1, 4, 2);
            tableLayoutPanel1.Controls.Add(label7, 5, 1);
            tableLayoutPanel1.Controls.Add(label6, 4, 1);
            tableLayoutPanel1.Controls.Add(label3, 2, 1);
            tableLayoutPanel1.Controls.Add(label2, 1, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(mtbxp, 0, 2);
            tableLayoutPanel1.Controls.Add(mtbxn, 1, 2);
            tableLayoutPanel1.Controls.Add(mtbUel, 2, 2);
            tableLayoutPanel1.Controls.Add(lblTitle, 0, 0);
            tableLayoutPanel1.Controls.Add(cbAutoscale, 0, 3);
            tableLayoutPanel1.Controls.Add(label4, 2, 3);
            tableLayoutPanel1.Controls.Add(nudYmax, 4, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(616, 492);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tbUa2
            // 
            tbUa2.Dock = DockStyle.Fill;
            tbUa2.Enabled = false;
            tbUa2.Location = new Point(513, 83);
            tbUa2.Name = "tbUa2";
            tbUa2.Size = new Size(100, 27);
            tbUa2.TabIndex = 15;
            tbUa2.TextAlign = HorizontalAlignment.Center;
            // 
            // tbUa1
            // 
            tbUa1.Dock = DockStyle.Fill;
            tbUa1.Enabled = false;
            tbUa1.Location = new Point(411, 83);
            tbUa1.Name = "tbUa1";
            tbUa1.Size = new Size(96, 27);
            tbUa1.TabIndex = 14;
            tbUa1.TextAlign = HorizontalAlignment.Center;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Fill;
            label7.Font = new Font("Segoe UI", 9F);
            label7.Location = new Point(513, 40);
            label7.Name = "label7";
            label7.Size = new Size(100, 40);
            label7.TabIndex = 12;
            label7.Text = "Ua2[µV]";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Font = new Font("Segoe UI", 9F);
            label6.Location = new Point(411, 40);
            label6.Name = "label6";
            label6.Size = new Size(96, 40);
            label6.TabIndex = 11;
            label6.Text = "Ua1[µV]";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label3, 2);
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Segoe UI", 9F);
            label3.Location = new Point(207, 40);
            label3.Name = "label3";
            label3.Size = new Size(198, 40);
            label3.TabIndex = 2;
            label3.Text = "Uelektr. [µV] = Ua0";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(105, 40);
            label2.Name = "label2";
            label2.Size = new Size(96, 40);
            label2.TabIndex = 1;
            label2.Text = "xn [kOhm]";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(3, 40);
            label1.Name = "label1";
            label1.Size = new Size(96, 40);
            label1.TabIndex = 0;
            label1.Text = "xp [kOhm]";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mtbxp
            // 
            mtbxp.Dock = DockStyle.Fill;
            mtbxp.Enabled = false;
            mtbxp.Location = new Point(3, 83);
            mtbxp.Name = "mtbxp";
            mtbxp.Size = new Size(96, 27);
            mtbxp.TabIndex = 3;
            mtbxp.TextAlign = HorizontalAlignment.Center;
            // 
            // mtbxn
            // 
            mtbxn.Dock = DockStyle.Fill;
            mtbxn.Enabled = false;
            mtbxn.Location = new Point(105, 83);
            mtbxn.Name = "mtbxn";
            mtbxn.Size = new Size(96, 27);
            mtbxn.TabIndex = 4;
            mtbxn.TextAlign = HorizontalAlignment.Center;
            // 
            // mtbUel
            // 
            tableLayoutPanel1.SetColumnSpan(mtbUel, 2);
            mtbUel.Dock = DockStyle.Fill;
            mtbUel.Enabled = false;
            mtbUel.Location = new Point(207, 83);
            mtbUel.Name = "mtbUel";
            mtbUel.Size = new Size(198, 27);
            mtbUel.TabIndex = 5;
            mtbUel.TextAlign = HorizontalAlignment.Center;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(lblTitle, 6);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(3, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(610, 40);
            lblTitle.TabIndex = 6;
            lblTitle.Text = "Title";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cbAutoscale
            // 
            cbAutoscale.AutoSize = true;
            cbAutoscale.Checked = true;
            cbAutoscale.CheckState = CheckState.Checked;
            cbAutoscale.Dock = DockStyle.Left;
            cbAutoscale.Location = new Point(3, 123);
            cbAutoscale.Name = "cbAutoscale";
            cbAutoscale.Size = new Size(96, 34);
            cbAutoscale.TabIndex = 7;
            cbAutoscale.Text = "Autoscale";
            cbAutoscale.UseVisualStyleBackColor = true;
            cbAutoscale.CheckedChanged += CbAutoscale_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 2);
            label4.Dock = DockStyle.Right;
            label4.Location = new Point(322, 120);
            label4.Name = "label4";
            label4.Size = new Size(83, 40);
            label4.TabIndex = 8;
            label4.Text = "y max [µV]:";
            label4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudYmax
            // 
            tableLayoutPanel1.SetColumnSpan(nudYmax, 2);
            nudYmax.Dock = DockStyle.Fill;
            nudYmax.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            nudYmax.Location = new Point(411, 123);
            nudYmax.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudYmax.Name = "nudYmax";
            nudYmax.Size = new Size(202, 27);
            nudYmax.TabIndex = 9;
            nudYmax.TextAlign = HorizontalAlignment.Right;
            nudYmax.Value = new decimal(new int[] { 10, 0, 0, 0 });
            nudYmax.ValueChanged += NudYmax_ValueChanged;
            // 
            // uc_Spectrum_Impedance
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "uc_Spectrum_Impedance";
            Size = new Size(616, 492);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudYmax).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label3;
        private Label label2;
        private TextBox mtbxp;
        private TextBox mtbxn;
        private TextBox mtbUel;
        private Label lblTitle;
        private CheckBox cbAutoscale;
        private Label label4;
        private NumericUpDown nudYmax;
        private TextBox tbUa2;
        private TextBox tbUa1;
        private Label label7;
        private Label label6;
    }
}
