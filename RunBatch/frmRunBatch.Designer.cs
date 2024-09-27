namespace RunBatch
{
    partial class frmRunBatch
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
            this.lblErrors = new System.Windows.Forms.Label();
            this.lblErrorcnt = new System.Windows.Forms.Label();
            this.btBuildAllModules = new System.Windows.Forms.Button();
            this.lblWarnings = new System.Windows.Forms.Label();
            this.lblWarningscnt = new System.Windows.Forms.Label();
            this.btBuildFirmwareImages = new System.Windows.Forms.Button();
            this.btBuildAllBootloaders = new System.Windows.Forms.Button();
            this.btMerge = new System.Windows.Forms.Button();
            this.tabErrors = new System.Windows.Forms.TabControl();
            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.tabPageWarnings = new System.Windows.Forms.TabPage();
            this.cbNeuromodule = new System.Windows.Forms.CheckBox();
            this.cbNeuromaster = new System.Windows.Forms.CheckBox();
            this.cbInclude_cSWChannel = new System.Windows.Forms.CheckBox();
            this.btClearAll = new System.Windows.Forms.Button();
            this.cStatusErrors = new ComponentsLib_GUI.CStatusText();
            this.cStatusWarnings = new ComponentsLib_GUI.CStatusText();
            this.txtStatus = new ComponentsLib_GUI.CStatusText();
            this.label1 = new System.Windows.Forms.Label();
            this.tabErrors.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.tabPageWarnings.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Location = new System.Drawing.Point(12, 586);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(37, 13);
            this.lblErrors.TabIndex = 162;
            this.lblErrors.Text = "Errors:";
            // 
            // lblErrorcnt
            // 
            this.lblErrorcnt.AutoSize = true;
            this.lblErrorcnt.ForeColor = System.Drawing.Color.Red;
            this.lblErrorcnt.Location = new System.Drawing.Point(89, 586);
            this.lblErrorcnt.Name = "lblErrorcnt";
            this.lblErrorcnt.Size = new System.Drawing.Size(10, 13);
            this.lblErrorcnt.TabIndex = 163;
            this.lblErrorcnt.Text = "-";
            // 
            // btBuildAllModules
            // 
            this.btBuildAllModules.Enabled = false;
            this.btBuildAllModules.Location = new System.Drawing.Point(12, 344);
            this.btBuildAllModules.Name = "btBuildAllModules";
            this.btBuildAllModules.Size = new System.Drawing.Size(194, 36);
            this.btBuildAllModules.TabIndex = 164;
            this.btBuildAllModules.Text = "1) Build all Modules";
            this.btBuildAllModules.UseVisualStyleBackColor = true;
            this.btBuildAllModules.Click += new System.EventHandler(this.btBuildAllModules_Click);
            // 
            // lblWarnings
            // 
            this.lblWarnings.AutoSize = true;
            this.lblWarnings.Location = new System.Drawing.Point(12, 609);
            this.lblWarnings.Name = "lblWarnings";
            this.lblWarnings.Size = new System.Drawing.Size(55, 13);
            this.lblWarnings.TabIndex = 165;
            this.lblWarnings.Text = "Warnings:";
            // 
            // lblWarningscnt
            // 
            this.lblWarningscnt.AutoSize = true;
            this.lblWarningscnt.ForeColor = System.Drawing.Color.Blue;
            this.lblWarningscnt.Location = new System.Drawing.Point(89, 609);
            this.lblWarningscnt.Name = "lblWarningscnt";
            this.lblWarningscnt.Size = new System.Drawing.Size(10, 13);
            this.lblWarningscnt.TabIndex = 166;
            this.lblWarningscnt.Text = "-";
            // 
            // btBuildFirmwareImages
            // 
            this.btBuildFirmwareImages.Location = new System.Drawing.Point(15, 508);
            this.btBuildFirmwareImages.Name = "btBuildFirmwareImages";
            this.btBuildFirmwareImages.Size = new System.Drawing.Size(194, 36);
            this.btBuildFirmwareImages.TabIndex = 167;
            this.btBuildFirmwareImages.Text = "3b) Build Firmware Images";
            this.btBuildFirmwareImages.UseVisualStyleBackColor = true;
            this.btBuildFirmwareImages.Visible = false;
            this.btBuildFirmwareImages.Click += new System.EventHandler(this.btBuildFirmwareImages_Click);
            // 
            // btBuildAllBootloaders
            // 
            this.btBuildAllBootloaders.Enabled = false;
            this.btBuildAllBootloaders.Location = new System.Drawing.Point(12, 386);
            this.btBuildAllBootloaders.Name = "btBuildAllBootloaders";
            this.btBuildAllBootloaders.Size = new System.Drawing.Size(194, 36);
            this.btBuildAllBootloaders.TabIndex = 168;
            this.btBuildAllBootloaders.Text = "2) Build all Bootloaders";
            this.btBuildAllBootloaders.UseVisualStyleBackColor = true;
            this.btBuildAllBootloaders.Click += new System.EventHandler(this.btBuildAllBootloaders_Click);
            // 
            // btMerge
            // 
            this.btMerge.Location = new System.Drawing.Point(12, 428);
            this.btMerge.Name = "btMerge";
            this.btMerge.Size = new System.Drawing.Size(194, 74);
            this.btMerge.TabIndex = 169;
            this.btMerge.Text = "3) Merge Bootloader and Module, build bin files";
            this.btMerge.UseVisualStyleBackColor = true;
            this.btMerge.Click += new System.EventHandler(this.btMerge_Click);
            // 
            // tabErrors
            // 
            this.tabErrors.Controls.Add(this.tabPageErrors);
            this.tabErrors.Controls.Add(this.tabPageWarnings);
            this.tabErrors.Location = new System.Drawing.Point(428, 344);
            this.tabErrors.Name = "tabErrors";
            this.tabErrors.SelectedIndex = 0;
            this.tabErrors.Size = new System.Drawing.Size(571, 295);
            this.tabErrors.TabIndex = 170;
            // 
            // tabPageErrors
            // 
            this.tabPageErrors.Controls.Add(this.cStatusErrors);
            this.tabPageErrors.Location = new System.Drawing.Point(4, 22);
            this.tabPageErrors.Name = "tabPageErrors";
            this.tabPageErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageErrors.Size = new System.Drawing.Size(563, 269);
            this.tabPageErrors.TabIndex = 0;
            this.tabPageErrors.Text = "Errors";
            this.tabPageErrors.UseVisualStyleBackColor = true;
            // 
            // tabPageWarnings
            // 
            this.tabPageWarnings.Controls.Add(this.cStatusWarnings);
            this.tabPageWarnings.Location = new System.Drawing.Point(4, 22);
            this.tabPageWarnings.Name = "tabPageWarnings";
            this.tabPageWarnings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWarnings.Size = new System.Drawing.Size(563, 269);
            this.tabPageWarnings.TabIndex = 1;
            this.tabPageWarnings.Text = "Warnings";
            this.tabPageWarnings.UseVisualStyleBackColor = true;
            // 
            // cbNeuromodule
            // 
            this.cbNeuromodule.AutoSize = true;
            this.cbNeuromodule.Checked = true;
            this.cbNeuromodule.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbNeuromodule.Location = new System.Drawing.Point(226, 363);
            this.cbNeuromodule.Name = "cbNeuromodule";
            this.cbNeuromodule.Size = new System.Drawing.Size(89, 17);
            this.cbNeuromodule.TabIndex = 171;
            this.cbNeuromodule.Text = "Neuromodule";
            this.cbNeuromodule.UseVisualStyleBackColor = true;
            // 
            // cbNeuromaster
            // 
            this.cbNeuromaster.AutoSize = true;
            this.cbNeuromaster.Checked = true;
            this.cbNeuromaster.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbNeuromaster.Location = new System.Drawing.Point(226, 386);
            this.cbNeuromaster.Name = "cbNeuromaster";
            this.cbNeuromaster.Size = new System.Drawing.Size(86, 17);
            this.cbNeuromaster.TabIndex = 172;
            this.cbNeuromaster.Text = "Neuromaster";
            this.cbNeuromaster.UseVisualStyleBackColor = true;
            // 
            // cbInclude_cSWChannel
            // 
            this.cbInclude_cSWChannel.AutoSize = true;
            this.cbInclude_cSWChannel.Location = new System.Drawing.Point(226, 489);
            this.cbInclude_cSWChannel.Name = "cbInclude_cSWChannel";
            this.cbInclude_cSWChannel.Size = new System.Drawing.Size(203, 17);
            this.cbInclude_cSWChannel.TabIndex = 173;
            this.cbInclude_cSWChannel.Text = "Include Default Config (cSWChannel)";
            this.cbInclude_cSWChannel.UseVisualStyleBackColor = true;
            // 
            // btClearAll
            // 
            this.btClearAll.Location = new System.Drawing.Point(298, 426);
            this.btClearAll.Name = "btClearAll";
            this.btClearAll.Size = new System.Drawing.Size(113, 24);
            this.btClearAll.TabIndex = 174;
            this.btClearAll.Text = "Clear all";
            this.btClearAll.UseVisualStyleBackColor = true;
            this.btClearAll.Click += new System.EventHandler(this.btClearAll_Click);
            // 
            // cStatusErrors
            // 
            this.cStatusErrors.AddDate = false;
            this.cStatusErrors.AddTime = false;
            this.cStatusErrors.ClearLines = 0;
            this.cStatusErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cStatusErrors.Location = new System.Drawing.Point(3, 3);
            this.cStatusErrors.MaxLines = 0;
            this.cStatusErrors.Name = "cStatusErrors";
            this.cStatusErrors.RedTextAddedd = false;
            this.cStatusErrors.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.cStatusErrors.Size = new System.Drawing.Size(557, 263);
            this.cStatusErrors.TabIndex = 161;
            this.cStatusErrors.Text = "";
            // 
            // cStatusWarnings
            // 
            this.cStatusWarnings.AddDate = false;
            this.cStatusWarnings.AddTime = false;
            this.cStatusWarnings.ClearLines = 0;
            this.cStatusWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cStatusWarnings.Location = new System.Drawing.Point(3, 3);
            this.cStatusWarnings.MaxLines = 0;
            this.cStatusWarnings.Name = "cStatusWarnings";
            this.cStatusWarnings.RedTextAddedd = false;
            this.cStatusWarnings.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.cStatusWarnings.Size = new System.Drawing.Size(557, 263);
            this.cStatusWarnings.TabIndex = 162;
            this.cStatusWarnings.Text = "";
            // 
            // txtStatus
            // 
            this.txtStatus.AddDate = false;
            this.txtStatus.AddTime = false;
            this.txtStatus.ClearLines = 0;
            this.txtStatus.Location = new System.Drawing.Point(12, 12);
            this.txtStatus.MaxLines = 0;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.RedTextAddedd = false;
            this.txtStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(987, 316);
            this.txtStatus.TabIndex = 160;
            this.txtStatus.Text = "";
            this.txtStatus.TextChanged += new System.EventHandler(this.txtStatus_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(5, 356);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(215, 65);
            this.label1.TabIndex = 176;
            this.label1.Text = "\r\nTo build hex-files - use MPLABX Batch Build\r\n\r\nBOTH!! FW AND Bootloader!\r\n\r\n";
            // 
            // frmRunBatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 651);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btClearAll);
            this.Controls.Add(this.cbInclude_cSWChannel);
            this.Controls.Add(this.cbNeuromaster);
            this.Controls.Add(this.cbNeuromodule);
            this.Controls.Add(this.tabErrors);
            this.Controls.Add(this.btMerge);
            this.Controls.Add(this.btBuildAllBootloaders);
            this.Controls.Add(this.btBuildFirmwareImages);
            this.Controls.Add(this.lblWarningscnt);
            this.Controls.Add(this.lblWarnings);
            this.Controls.Add(this.btBuildAllModules);
            this.Controls.Add(this.lblErrorcnt);
            this.Controls.Add(this.lblErrors);
            this.Controls.Add(this.txtStatus);
            this.Name = "frmRunBatch";
            this.Text = "Run Batch 2022_04_08";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabErrors.ResumeLayout(false);
            this.tabPageErrors.ResumeLayout(false);
            this.tabPageWarnings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentsLib_GUI.CStatusText txtStatus;
        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.Label lblErrorcnt;
        private System.Windows.Forms.Button btBuildAllModules;
        private System.Windows.Forms.Label lblWarnings;
        private System.Windows.Forms.Label lblWarningscnt;
        private System.Windows.Forms.Button btBuildFirmwareImages;
        private System.Windows.Forms.Button btBuildAllBootloaders;
        private System.Windows.Forms.Button btMerge;
        private System.Windows.Forms.TabControl tabErrors;
        private System.Windows.Forms.TabPage tabPageErrors;
        private ComponentsLib_GUI.CStatusText cStatusErrors;
        private System.Windows.Forms.TabPage tabPageWarnings;
        private ComponentsLib_GUI.CStatusText cStatusWarnings;
        private System.Windows.Forms.CheckBox cbNeuromodule;
        private System.Windows.Forms.CheckBox cbNeuromaster;
        private System.Windows.Forms.CheckBox cbInclude_cSWChannel;
        private System.Windows.Forms.Button btClearAll;
        private System.Windows.Forms.Label label1;
    }
}

