namespace FeedbackDataLib_GUI
{
    partial class ucModuleSpecificSetup_VasoIR
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.txtt_max_overload_time_ms = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtt_calc_new_scaling_ms = new System.Windows.Forms.TextBox();
            this.txtMovingAVG_Buffersize_overload_asPowerof2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtMovingAVG_Buffersize_asPowerof2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAmpl_max = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAmpl_min = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtAmpl_curr = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtLEDCurrent = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Title";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(383, 78);
            this.tabControl1.TabIndex = 13;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtMovingAVG_Buffersize_overload_asPowerof2);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.txtMovingAVG_Buffersize_asPowerof2);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.txtt_max_overload_time_ms);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtt_calc_new_scaling_ms);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(375, 52);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Timing";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.txtLEDCurrent);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.txtAmpl_curr);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.txtAmpl_max);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.txtAmpl_min);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(375, 52);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Amplification";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "t satur.->overl mode [ms] ";
            // 
            // txtt_max_overload_time_ms
            // 
            this.txtt_max_overload_time_ms.ForeColor = System.Drawing.Color.Black;
            this.txtt_max_overload_time_ms.Location = new System.Drawing.Point(129, 23);
            this.txtt_max_overload_time_ms.Name = "txtt_max_overload_time_ms";
            this.txtt_max_overload_time_ms.Size = new System.Drawing.Size(44, 20);
            this.txtt_max_overload_time_ms.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "t scaling [ms] ";
            // 
            // txtt_calc_new_scaling_ms
            // 
            this.txtt_calc_new_scaling_ms.ForeColor = System.Drawing.Color.Black;
            this.txtt_calc_new_scaling_ms.Location = new System.Drawing.Point(129, 0);
            this.txtt_calc_new_scaling_ms.Name = "txtt_calc_new_scaling_ms";
            this.txtt_calc_new_scaling_ms.Size = new System.Drawing.Size(44, 20);
            this.txtt_calc_new_scaling_ms.TabIndex = 4;
            // 
            // txtMovingAVG_Buffersize_overload_asPowerof2
            // 
            this.txtMovingAVG_Buffersize_overload_asPowerof2.ForeColor = System.Drawing.Color.Black;
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Location = new System.Drawing.Point(323, 23);
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Name = "txtMovingAVG_Buffersize_overload_asPowerof2";
            this.txtMovingAVG_Buffersize_overload_asPowerof2.Size = new System.Drawing.Size(44, 20);
            this.txtMovingAVG_Buffersize_overload_asPowerof2.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(181, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "AVG overload Buf size (2^x)";
            // 
            // txtMovingAVG_Buffersize_asPowerof2
            // 
            this.txtMovingAVG_Buffersize_asPowerof2.ForeColor = System.Drawing.Color.Black;
            this.txtMovingAVG_Buffersize_asPowerof2.Location = new System.Drawing.Point(323, 0);
            this.txtMovingAVG_Buffersize_asPowerof2.Name = "txtMovingAVG_Buffersize_asPowerof2";
            this.txtMovingAVG_Buffersize_asPowerof2.Size = new System.Drawing.Size(44, 20);
            this.txtMovingAVG_Buffersize_asPowerof2.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(181, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "AVG Buffer size (2^x)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Ampl max (2 ^x)";
            // 
            // txtAmpl_max
            // 
            this.txtAmpl_max.ForeColor = System.Drawing.Color.Black;
            this.txtAmpl_max.Location = new System.Drawing.Point(93, 23);
            this.txtAmpl_max.Name = "txtAmpl_max";
            this.txtAmpl_max.Size = new System.Drawing.Size(44, 20);
            this.txtAmpl_max.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Ampl min (2 ^x)";
            // 
            // txtAmpl_min
            // 
            this.txtAmpl_min.ForeColor = System.Drawing.Color.Black;
            this.txtAmpl_min.Location = new System.Drawing.Point(93, 0);
            this.txtAmpl_min.Name = "txtAmpl_min";
            this.txtAmpl_min.Size = new System.Drawing.Size(44, 20);
            this.txtAmpl_min.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(161, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Ampl curr (2 ^x)";
            // 
            // txtAmpl_curr
            // 
            this.txtAmpl_curr.ForeColor = System.Drawing.Color.Black;
            this.txtAmpl_curr.Location = new System.Drawing.Point(244, 0);
            this.txtAmpl_curr.Name = "txtAmpl_curr";
            this.txtAmpl_curr.Size = new System.Drawing.Size(44, 20);
            this.txtAmpl_curr.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(161, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "LED  (x*10mA)";
            // 
            // txtLEDCurrent
            // 
            this.txtLEDCurrent.ForeColor = System.Drawing.Color.Black;
            this.txtLEDCurrent.Location = new System.Drawing.Point(244, 23);
            this.txtLEDCurrent.Name = "txtLEDCurrent";
            this.txtLEDCurrent.Size = new System.Drawing.Size(44, 20);
            this.txtLEDCurrent.TabIndex = 14;
            // 
            // ucModuleSpecificSetup_VasoIR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.tabControl1);
            this.Name = "ucModuleSpecificSetup_VasoIR";
            this.Size = new System.Drawing.Size(383, 78);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtMovingAVG_Buffersize_overload_asPowerof2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtMovingAVG_Buffersize_asPowerof2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtt_max_overload_time_ms;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtt_calc_new_scaling_ms;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtAmpl_curr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAmpl_max;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtAmpl_min;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLEDCurrent;
    }
}
