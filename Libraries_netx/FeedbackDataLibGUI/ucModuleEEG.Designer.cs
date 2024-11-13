namespace FeedbackDataLibGUI
{
    partial class ucModuleEEG
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
            tableLayoutPanel6 = new TableLayoutPanel();
            label1 = new Label();
            label7 = new Label();
            txtSR0 = new TextBox();
            tableLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 3;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel6.Controls.Add(label1, 0, 1);
            tableLayoutPanel6.Controls.Add(label7, 1, 0);
            tableLayoutPanel6.Controls.Add(txtSR0, 1, 1);
            tableLayoutPanel6.Location = new Point(0, 0);
            tableLayoutPanel6.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 2;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel6.Size = new Size(454, 101);
            tableLayoutPanel6.TabIndex = 24;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 50);
            label1.Name = "label1";
            label1.Size = new Size(142, 20);
            label1.TabIndex = 38;
            label1.Text = "FFT sample_int [ms]:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(203, 0);
            label7.Name = "label7";
            label7.Size = new Size(54, 16);
            label7.TabIndex = 21;
            label7.Text = "Chan0:";
            // 
            // txtSR0
            // 
            txtSR0.Dock = DockStyle.Fill;
            txtSR0.Location = new Point(203, 54);
            txtSR0.Margin = new Padding(3, 4, 3, 4);
            txtSR0.Name = "txtSR0";
            txtSR0.Size = new Size(124, 27);
            txtSR0.TabIndex = 39;
            txtSR0.Text = "200";
            // 
            // ucModuleEEG
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel6);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ucModuleEEG";
            Size = new Size(464, 111);
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSR0;
        private System.Windows.Forms.Label label7;
    }
}
