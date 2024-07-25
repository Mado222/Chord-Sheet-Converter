using FeedbackDataLib.Modules;
using FeedbackDataLib.Modules.CADS1292x;

namespace FeedbackDataLib_GUI
{
    partial class ucModuleExGADS_Impedance
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
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblxpV2 = new System.Windows.Forms.Label();
            this.lblxpV1 = new System.Windows.Forms.Label();
            this.lblxnV2 = new System.Windows.Forms.Label();
            this.lblxnV1 = new System.Windows.Forms.Label();
            this.lblxpR2 = new System.Windows.Forms.Label();
            this.lblxpR1 = new System.Windows.Forms.Label();
            this.lblxnR2 = new System.Windows.Forms.Label();
            this.lblxnR1 = new System.Windows.Forms.Label();
            this.uElektrode_xp_mVLabel = new System.Windows.Forms.Label();
            this.uElektrode_xn_mVLabel = new System.Windows.Forms.Label();
            this.electrode_imp_xpLabel = new System.Windows.Forms.Label();
            this.electrode_imp_xnLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSR0 = new System.Windows.Forms.TextBox();
            this.txtSR1 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 3;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel6.Controls.Add(this.label1, 0, 5);
            this.tableLayoutPanel6.Controls.Add(this.lblxpV2, 2, 4);
            this.tableLayoutPanel6.Controls.Add(this.lblxpV1, 1, 4);
            this.tableLayoutPanel6.Controls.Add(this.lblxnV2, 2, 3);
            this.tableLayoutPanel6.Controls.Add(this.lblxnV1, 1, 3);
            this.tableLayoutPanel6.Controls.Add(this.lblxpR2, 2, 2);
            this.tableLayoutPanel6.Controls.Add(this.lblxpR1, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.lblxnR2, 2, 1);
            this.tableLayoutPanel6.Controls.Add(this.lblxnR1, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.uElektrode_xp_mVLabel, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.uElektrode_xn_mVLabel, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.electrode_imp_xpLabel, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.electrode_imp_xnLabel, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.label6, 2, 0);
            this.tableLayoutPanel6.Controls.Add(this.label7, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.txtSR0, 1, 5);
            this.tableLayoutPanel6.Controls.Add(this.txtSR1, 2, 5);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 6;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(454, 136);
            this.tableLayoutPanel6.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 17);
            this.label1.TabIndex = 38;
            this.label1.Text = "FFT sample_int [ms]:";
            // 
            // lblxpV2
            // 
            this.lblxpV2.AutoSize = true;
            this.lblxpV2.Location = new System.Drawing.Point(333, 84);
            this.lblxpV2.Name = "lblxpV2";
            this.lblxpV2.Size = new System.Drawing.Size(26, 17);
            this.lblxpV2.TabIndex = 37;
            this.lblxpV2.Text = "xxx";
            // 
            // lblxpV1
            // 
            this.lblxpV1.AutoSize = true;
            this.lblxpV1.Location = new System.Drawing.Point(203, 84);
            this.lblxpV1.Name = "lblxpV1";
            this.lblxpV1.Size = new System.Drawing.Size(26, 17);
            this.lblxpV1.TabIndex = 36;
            this.lblxpV1.Text = "xxx";
            // 
            // lblxnV2
            // 
            this.lblxnV2.AutoSize = true;
            this.lblxnV2.Location = new System.Drawing.Point(333, 63);
            this.lblxnV2.Name = "lblxnV2";
            this.lblxnV2.Size = new System.Drawing.Size(26, 17);
            this.lblxnV2.TabIndex = 35;
            this.lblxnV2.Text = "xxx";
            // 
            // lblxnV1
            // 
            this.lblxnV1.AutoSize = true;
            this.lblxnV1.Location = new System.Drawing.Point(203, 63);
            this.lblxnV1.Name = "lblxnV1";
            this.lblxnV1.Size = new System.Drawing.Size(26, 17);
            this.lblxnV1.TabIndex = 34;
            this.lblxnV1.Text = "xxx";
            // 
            // lblxpR2
            // 
            this.lblxpR2.AutoSize = true;
            this.lblxpR2.Location = new System.Drawing.Point(333, 42);
            this.lblxpR2.Name = "lblxpR2";
            this.lblxpR2.Size = new System.Drawing.Size(26, 17);
            this.lblxpR2.TabIndex = 33;
            this.lblxpR2.Text = "xxx";
            // 
            // lblxpR1
            // 
            this.lblxpR1.AutoSize = true;
            this.lblxpR1.Location = new System.Drawing.Point(203, 42);
            this.lblxpR1.Name = "lblxpR1";
            this.lblxpR1.Size = new System.Drawing.Size(26, 17);
            this.lblxpR1.TabIndex = 31;
            this.lblxpR1.Text = "xxx";
            // 
            // lblxnR2
            // 
            this.lblxnR2.AutoSize = true;
            this.lblxnR2.Location = new System.Drawing.Point(333, 21);
            this.lblxnR2.Name = "lblxnR2";
            this.lblxnR2.Size = new System.Drawing.Size(26, 17);
            this.lblxnR2.TabIndex = 30;
            this.lblxnR2.Text = "xxx";
            // 
            // lblxnR1
            // 
            this.lblxnR1.AutoSize = true;
            this.lblxnR1.Location = new System.Drawing.Point(203, 21);
            this.lblxnR1.Name = "lblxnR1";
            this.lblxnR1.Size = new System.Drawing.Size(26, 17);
            this.lblxnR1.TabIndex = 29;
            this.lblxnR1.Text = "xxx";
            // 
            // uElektrode_xp_mVLabel
            // 
            this.uElektrode_xp_mVLabel.AutoSize = true;
            this.uElektrode_xp_mVLabel.Location = new System.Drawing.Point(3, 84);
            this.uElektrode_xp_mVLabel.Name = "uElektrode_xp_mVLabel";
            this.uElektrode_xp_mVLabel.Size = new System.Drawing.Size(129, 17);
            this.uElektrode_xp_mVLabel.TabIndex = 23;
            this.uElektrode_xp_mVLabel.Text = "UElektrode xp [µV]:";
            // 
            // uElektrode_xn_mVLabel
            // 
            this.uElektrode_xn_mVLabel.AutoSize = true;
            this.uElektrode_xn_mVLabel.Location = new System.Drawing.Point(3, 63);
            this.uElektrode_xn_mVLabel.Name = "uElektrode_xn_mVLabel";
            this.uElektrode_xn_mVLabel.Size = new System.Drawing.Size(129, 17);
            this.uElektrode_xn_mVLabel.TabIndex = 23;
            this.uElektrode_xn_mVLabel.Text = "UElektrode xn [µV]:";
            // 
            // electrode_imp_xpLabel
            // 
            this.electrode_imp_xpLabel.AutoSize = true;
            this.electrode_imp_xpLabel.Location = new System.Drawing.Point(3, 42);
            this.electrode_imp_xpLabel.Name = "electrode_imp_xpLabel";
            this.electrode_imp_xpLabel.Size = new System.Drawing.Size(165, 17);
            this.electrode_imp_xpLabel.TabIndex = 23;
            this.electrode_imp_xpLabel.Text = "Electrode imp xp [kOhm]:";
            // 
            // electrode_imp_xnLabel
            // 
            this.electrode_imp_xnLabel.AutoSize = true;
            this.electrode_imp_xnLabel.Location = new System.Drawing.Point(3, 21);
            this.electrode_imp_xnLabel.Name = "electrode_imp_xnLabel";
            this.electrode_imp_xnLabel.Size = new System.Drawing.Size(165, 17);
            this.electrode_imp_xnLabel.TabIndex = 23;
            this.electrode_imp_xnLabel.Text = "Electrode imp xn [kOhm]:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(333, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 17);
            this.label6.TabIndex = 20;
            this.label6.Text = "Chan1:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(203, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 17);
            this.label7.TabIndex = 21;
            this.label7.Text = "Chan0:";
            // 
            // txtSR0
            // 
            this.txtSR0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSR0.Location = new System.Drawing.Point(203, 108);
            this.txtSR0.Name = "txtSR0";
            this.txtSR0.Size = new System.Drawing.Size(124, 22);
            this.txtSR0.TabIndex = 39;
            this.txtSR0.Text = "200";
            // 
            // txtSR1
            // 
            this.txtSR1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSR1.Location = new System.Drawing.Point(333, 108);
            this.txtSR1.Name = "txtSR1";
            this.txtSR1.Size = new System.Drawing.Size(124, 22);
            this.txtSR1.TabIndex = 41;
            this.txtSR1.Text = "200";
            // 
            // ucModuleExGADS_Impedance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel6);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ucModuleExGADS_Impedance";
            this.Size = new System.Drawing.Size(464, 145);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label lblxpV2;
        private System.Windows.Forms.Label lblxpV1;
        private System.Windows.Forms.Label lblxnV2;
        private System.Windows.Forms.Label lblxnV1;
        private System.Windows.Forms.Label lblxpR2;
        private System.Windows.Forms.Label lblxpR1;
        private System.Windows.Forms.Label lblxnR2;
        private System.Windows.Forms.Label lblxnR1;
        private System.Windows.Forms.Label uElektrode_xp_mVLabel;
        private System.Windows.Forms.Label uElektrode_xn_mVLabel;
        private System.Windows.Forms.Label electrode_imp_xpLabel;
        private System.Windows.Forms.Label electrode_imp_xnLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSR0;
        private System.Windows.Forms.TextBox txtSR1;
    }
}
