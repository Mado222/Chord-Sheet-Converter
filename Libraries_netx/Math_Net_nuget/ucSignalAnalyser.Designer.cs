namespace MathNetNuget
{
    partial class ucSignalAnalyser
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
            this.lblValeff = new System.Windows.Forms.Label();
            this.lblpeakplusValue = new System.Windows.Forms.Label();
            this.lblpeakminusValue = new System.Windows.Forms.Label();
            this.tlpMeasure = new System.Windows.Forms.TableLayoutPanel();
            this.lblValmean = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.tlpMeasure.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblValeff
            // 
            this.lblValeff.AutoSize = true;
            this.tlpMeasure.SetColumnSpan(this.lblValeff, 2);
            this.lblValeff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblValeff.Location = new System.Drawing.Point(3, 16);
            this.lblValeff.Name = "lblValeff";
            this.lblValeff.Size = new System.Drawing.Size(82, 16);
            this.lblValeff.TabIndex = 1;
            this.lblValeff.Text = "0";
            // 
            // lblpeakplusValue
            // 
            this.lblpeakplusValue.AutoSize = true;
            this.tlpMeasure.SetColumnSpan(this.lblpeakplusValue, 2);
            this.lblpeakplusValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblpeakplusValue.Location = new System.Drawing.Point(3, 48);
            this.lblpeakplusValue.Name = "lblpeakplusValue";
            this.lblpeakplusValue.Size = new System.Drawing.Size(82, 16);
            this.lblpeakplusValue.TabIndex = 3;
            this.lblpeakplusValue.Text = "0";
            // 
            // lblpeakminusValue
            // 
            this.lblpeakminusValue.AutoSize = true;
            this.tlpMeasure.SetColumnSpan(this.lblpeakminusValue, 2);
            this.lblpeakminusValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblpeakminusValue.Location = new System.Drawing.Point(3, 64);
            this.lblpeakminusValue.Name = "lblpeakminusValue";
            this.lblpeakminusValue.Size = new System.Drawing.Size(82, 19);
            this.lblpeakminusValue.TabIndex = 5;
            this.lblpeakminusValue.Text = "0";
            // 
            // tlpMeasure
            // 
            this.tlpMeasure.ColumnCount = 2;
            this.tlpMeasure.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMeasure.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMeasure.Controls.Add(this.lblValmean, 0, 2);
            this.tlpMeasure.Controls.Add(this.lblpeakminusValue, 0, 4);
            this.tlpMeasure.Controls.Add(this.lblpeakplusValue, 0, 3);
            this.tlpMeasure.Controls.Add(this.lblHeader, 0, 0);
            this.tlpMeasure.Controls.Add(this.lblValeff, 0, 1);
            this.tlpMeasure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMeasure.Location = new System.Drawing.Point(0, 0);
            this.tlpMeasure.Name = "tlpMeasure";
            this.tlpMeasure.RowCount = 5;
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMeasure.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMeasure.Size = new System.Drawing.Size(88, 83);
            this.tlpMeasure.TabIndex = 6;
            // 
            // lblValmean
            // 
            this.lblValmean.AutoSize = true;
            this.tlpMeasure.SetColumnSpan(this.lblValmean, 2);
            this.lblValmean.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblValmean.Location = new System.Drawing.Point(3, 32);
            this.lblValmean.Name = "lblValmean";
            this.lblValmean.Size = new System.Drawing.Size(82, 16);
            this.lblValmean.TabIndex = 7;
            this.lblValmean.Text = "0";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.tlpMeasure.SetColumnSpan(this.lblHeader, 2);
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(3, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(41, 13);
            this.lblHeader.TabIndex = 6;
            this.lblHeader.Text = "label1";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucSignalAnalyser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMeasure);
            this.Name = "ucSignalAnalyser";
            this.Size = new System.Drawing.Size(88, 83);
            this.tlpMeasure.ResumeLayout(false);
            this.tlpMeasure.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblValeff;
        private System.Windows.Forms.Label lblpeakplusValue;
        private System.Windows.Forms.Label lblpeakminusValue;
        private System.Windows.Forms.TableLayoutPanel tlpMeasure;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblValmean;
    }
}
