namespace FeedbackDataLib_GUI
{
    partial class ucModuleSpecificSetup_AtemIR
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
            this.components = new System.ComponentModel.Container();
            this.txtt_calc_new_scaling_ms = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtt_max_overload_time_ms = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtt_inOverload_time_ms = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtpost_shift_value = new System.Windows.Forms.TextBox();
            this.txtMovingAVG_Buffersize_asPowerof2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMovingAVG_Buffersize_overload_asPowerof2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.txtILED = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtt_calc_new_scaling_ms
            // 
            this.txtt_calc_new_scaling_ms.ForeColor = System.Drawing.Color.Black;
            this.txtt_calc_new_scaling_ms.Location = new System.Drawing.Point(142, 0);
            this.txtt_calc_new_scaling_ms.Name = "txtt_calc_new_scaling_ms";
            this.txtt_calc_new_scaling_ms.Size = new System.Drawing.Size(44, 20);
            this.txtt_calc_new_scaling_ms.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "t scaling [ms] ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "t satur.->overl mode [ms] ";
            // 
            // txtt_max_overload_time_ms
            // 
            this.txtt_max_overload_time_ms.ForeColor = System.Drawing.Color.Black;
            this.txtt_max_overload_time_ms.Location = new System.Drawing.Point(142, 23);
            this.txtt_max_overload_time_ms.Name = "txtt_max_overload_time_ms";
            this.txtt_max_overload_time_ms.Size = new System.Drawing.Size(44, 20);
            this.txtt_max_overload_time_ms.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "t overl mode->normal [ms] ";
            // 
            // txtt_inOverload_time_ms
            // 
            this.txtt_inOverload_time_ms.ForeColor = System.Drawing.Color.Black;
            this.txtt_inOverload_time_ms.Location = new System.Drawing.Point(142, 46);
            this.txtt_inOverload_time_ms.Name = "txtt_inOverload_time_ms";
            this.txtt_inOverload_time_ms.Size = new System.Drawing.Size(44, 20);
            this.txtt_inOverload_time_ms.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(192, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Amplification (2^x, -x ..+x)";
            // 
            // txtpost_shift_value
            // 
            this.txtpost_shift_value.ForeColor = System.Drawing.Color.Black;
            this.txtpost_shift_value.Location = new System.Drawing.Point(334, 0);
            this.txtpost_shift_value.Name = "txtpost_shift_value";
            this.txtpost_shift_value.Size = new System.Drawing.Size(44, 20);
            this.txtpost_shift_value.TabIndex = 8;
            // 
            // txtMovingAVG_Buffersize_asPowerof2
            // 
            this.txtMovingAVG_Buffersize_asPowerof2.ForeColor = System.Drawing.Color.Black;
            this.txtMovingAVG_Buffersize_asPowerof2.Location = new System.Drawing.Point(334, 46);
            this.txtMovingAVG_Buffersize_asPowerof2.Name = "txtMovingAVG_Buffersize_asPowerof2";
            this.txtMovingAVG_Buffersize_asPowerof2.Size = new System.Drawing.Size(44, 20);
            this.txtMovingAVG_Buffersize_asPowerof2.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(192, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "AVG Buffer size (2^x)";
            // 
            // txtMovingAVG_Buffersize_overload_asPowerof2
            // 
            this.txtMovingAVG_Buffersize_overload_asPowerof2.ForeColor = System.Drawing.Color.Black;
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Location = new System.Drawing.Point(334, 69);
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Name = "txtMovingAVG_Buffersize_overload_asPowerof2";
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Size = new System.Drawing.Size(44, 20);
            this.txtMovingAVG_Buffersize_overload_asPowerof2.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(192, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "AVG overload Buf size (2^x)";
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Title";
            // 
            // txtILED
            // 
            this.txtILED.ForeColor = System.Drawing.Color.Black;
            this.txtILED.Location = new System.Drawing.Point(334, 23);
            this.txtILED.Name = "txtILED";
            this.txtILED.Size = new System.Drawing.Size(44, 20);
            this.txtILED.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "ILED [10mA]";
            // 
            // ucModuleSpecificSetup_AtemIR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.txtILED);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtMovingAVG_Buffersize_overload_asPowerof2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtMovingAVG_Buffersize_asPowerof2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtpost_shift_value);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtt_inOverload_time_ms);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtt_max_overload_time_ms);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtt_calc_new_scaling_ms);
            this.Name = "ucModuleSpecificSetup_AtemIR";
            this.Size = new System.Drawing.Size(383, 99);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtt_calc_new_scaling_ms;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtt_max_overload_time_ms;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtt_inOverload_time_ms;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtpost_shift_value;
        private System.Windows.Forms.TextBox txtMovingAVG_Buffersize_asPowerof2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMovingAVG_Buffersize_overload_asPowerof2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtILED;
        private System.Windows.Forms.Label label7;
    }
}
