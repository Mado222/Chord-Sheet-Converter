namespace FeedbackDataLib_GUI
{
    partial class uc_Spectrum_Impedance
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
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            mtbxp = new MaskedTextBox();
            mtbxn = new MaskedTextBox();
            mtbUel = new MaskedTextBox();
            lblTitle = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.Controls.Add(label3, 2, 1);
            tableLayoutPanel1.Controls.Add(label2, 1, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(mtbxp, 0, 2);
            tableLayoutPanel1.Controls.Add(mtbxn, 1, 2);
            tableLayoutPanel1.Controls.Add(mtbUel, 2, 2);
            tableLayoutPanel1.Controls.Add(lblTitle, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            tableLayoutPanel1.Size = new Size(373, 296);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Segoe UI", 9F);
            label3.Location = new Point(251, 29);
            label3.Name = "label3";
            label3.Size = new Size(119, 29);
            label3.TabIndex = 2;
            label3.Text = "Uelektr. [mV]";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(127, 29);
            label2.Name = "label2";
            label2.Size = new Size(118, 29);
            label2.TabIndex = 1;
            label2.Text = "xn [kOhm]";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(3, 29);
            label1.Name = "label1";
            label1.Size = new Size(118, 29);
            label1.TabIndex = 0;
            label1.Text = "xp [kOhm]";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mtbxp
            // 
            mtbxp.Dock = DockStyle.Fill;
            mtbxp.Enabled = false;
            mtbxp.Location = new Point(3, 61);
            mtbxp.Name = "mtbxp";
            mtbxp.Size = new Size(118, 27);
            mtbxp.TabIndex = 3;
            mtbxp.TextAlign = HorizontalAlignment.Center;
            // 
            // mtbxn
            // 
            mtbxn.Dock = DockStyle.Fill;
            mtbxn.Enabled = false;
            mtbxn.Location = new Point(127, 61);
            mtbxn.Name = "mtbxn";
            mtbxn.Size = new Size(118, 27);
            mtbxn.TabIndex = 4;
            mtbxn.TextAlign = HorizontalAlignment.Center;
            // 
            // mtbUel
            // 
            mtbUel.Dock = DockStyle.Fill;
            mtbUel.Enabled = false;
            mtbUel.Location = new Point(251, 61);
            mtbUel.Name = "mtbUel";
            mtbUel.Size = new Size(119, 27);
            mtbUel.TabIndex = 5;
            mtbUel.TextAlign = HorizontalAlignment.Center;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(lblTitle, 3);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(3, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(367, 29);
            lblTitle.TabIndex = 6;
            lblTitle.Text = "Title";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // uc_Spectrum_Impedance
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "uc_Spectrum_Impedance";
            Size = new Size(373, 296);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label3;
        private Label label2;
        private MaskedTextBox mtbxp;
        private MaskedTextBox mtbxn;
        private MaskedTextBox mtbUel;
        private Label lblTitle;
    }
}
