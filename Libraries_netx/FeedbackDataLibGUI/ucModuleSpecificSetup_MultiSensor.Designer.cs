namespace FeedbackDataLibGUI
{
    partial class ucModuleSpecificSetup_MultiSensor
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
            this.gbPulse = new System.Windows.Forms.GroupBox();
            this.cbVPulseHiLo = new System.Windows.Forms.CheckBox();
            this.cbClearPulse = new System.Windows.Forms.CheckBox();
            this.gbSCL = new System.Windows.Forms.GroupBox();
            this.cbResHiLow = new System.Windows.Forms.CheckBox();
            this.cbTestMultisensor = new System.Windows.Forms.CheckBox();
            this.gbPulse.SuspendLayout();
            this.gbSCL.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPulse
            // 
            this.gbPulse.Controls.Add(this.cbVPulseHiLo);
            this.gbPulse.Controls.Add(this.cbClearPulse);
            this.gbPulse.Location = new System.Drawing.Point(11, 3);
            this.gbPulse.Name = "gbPulse";
            this.gbPulse.Size = new System.Drawing.Size(125, 60);
            this.gbPulse.TabIndex = 165;
            this.gbPulse.TabStop = false;
            this.gbPulse.Text = "Pulse:";
            this.gbPulse.Visible = false;
            // 
            // cbVPulseHiLo
            // 
            this.cbVPulseHiLo.AutoSize = true;
            this.cbVPulseHiLo.Location = new System.Drawing.Point(10, 36);
            this.cbVPulseHiLo.Name = "cbVPulseHiLo";
            this.cbVPulseHiLo.Size = new System.Drawing.Size(63, 17);
            this.cbVPulseHiLo.TabIndex = 159;
            this.cbVPulseHiLo.Text = "v= High";
            this.cbVPulseHiLo.UseVisualStyleBackColor = true;
            // 
            // cbClearPulse
            // 
            this.cbClearPulse.AutoSize = true;
            this.cbClearPulse.Location = new System.Drawing.Point(10, 17);
            this.cbClearPulse.Name = "cbClearPulse";
            this.cbClearPulse.Size = new System.Drawing.Size(46, 17);
            this.cbClearPulse.TabIndex = 158;
            this.cbClearPulse.Text = "Run";
            this.cbClearPulse.UseVisualStyleBackColor = true;
            // 
            // gbSCL
            // 
            this.gbSCL.Controls.Add(this.cbResHiLow);
            this.gbSCL.Controls.Add(this.cbTestMultisensor);
            this.gbSCL.Location = new System.Drawing.Point(142, 3);
            this.gbSCL.Name = "gbSCL";
            this.gbSCL.Size = new System.Drawing.Size(177, 60);
            this.gbSCL.TabIndex = 164;
            this.gbSCL.TabStop = false;
            this.gbSCL.Text = "SCL";
            this.gbSCL.Visible = false;
            // 
            // cbResHiLow
            // 
            this.cbResHiLow.AutoSize = true;
            this.cbResHiLow.Location = new System.Drawing.Point(6, 36);
            this.cbResHiLow.Name = "cbResHiLow";
            this.cbResHiLow.Size = new System.Drawing.Size(168, 17);
            this.cbResHiLow.TabIndex = 155;
            this.cbResHiLow.Text = "Res = 0.303µS (3.03µS sonst)";
            this.cbResHiLow.UseVisualStyleBackColor = true;
            // 
            // cbTestMultisensor
            // 
            this.cbTestMultisensor.AutoSize = true;
            this.cbTestMultisensor.Location = new System.Drawing.Point(6, 17);
            this.cbTestMultisensor.Name = "cbTestMultisensor";
            this.cbTestMultisensor.Size = new System.Drawing.Size(70, 17);
            this.cbTestMultisensor.TabIndex = 154;
            this.cbTestMultisensor.Text = "Test SCL";
            this.cbTestMultisensor.UseVisualStyleBackColor = true;
            // 
            // ucModuleSpecificSetup_MultiSensor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbPulse);
            this.Controls.Add(this.gbSCL);
            this.Name = "ucModuleSpecificSetup_MultiSensor";
            this.Size = new System.Drawing.Size(326, 69);
            this.gbPulse.ResumeLayout(false);
            this.gbPulse.PerformLayout();
            this.gbSCL.ResumeLayout(false);
            this.gbSCL.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbPulse;
        private System.Windows.Forms.CheckBox cbVPulseHiLo;
        private System.Windows.Forms.CheckBox cbClearPulse;
        private System.Windows.Forms.GroupBox gbSCL;
        private System.Windows.Forms.CheckBox cbResHiLow;
        private System.Windows.Forms.CheckBox cbTestMultisensor;
    }
}
