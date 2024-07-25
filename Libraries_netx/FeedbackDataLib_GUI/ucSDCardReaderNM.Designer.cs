namespace FeedbackDataLib_GUI
{
    using FeedbackDataLib;
    partial class SDCardReaderNM
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SDCardReaderNM));
            this.btReadBack = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.btCancleJob = new System.Windows.Forms.Button();
            this.tmrStatusMessages = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtDestination = new System.Windows.Forms.TextBox();
            this.pbDestination = new System.Windows.Forms.PictureBox();
            this.saveFileDialog_data = new System.Windows.Forms.SaveFileDialog();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSync_has_4bytes = new System.Windows.Forms.CheckBox();
            this.cChannelsControlV2x11 = new CChannelsControlV2x1();
            this.cbUseSourcePath = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDestination)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // btReadBack
            // 
            this.btReadBack.Location = new System.Drawing.Point(36, 134);
            this.btReadBack.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btReadBack.Name = "btReadBack";
            this.btReadBack.Size = new System.Drawing.Size(745, 38);
            this.btReadBack.TabIndex = 0;
            this.btReadBack.Text = "Start Import";
            this.btReadBack.UseVisualStyleBackColor = true;
            this.btReadBack.Click += new System.EventHandler(this.btReadBack_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(924, 15);
            this.txtStatus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(456, 445);
            this.txtStatus.TabIndex = 147;
            this.txtStatus.Text = "";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // btCancleJob
            // 
            this.btCancleJob.Location = new System.Drawing.Point(924, 517);
            this.btCancleJob.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancleJob.Name = "btCancleJob";
            this.btCancleJob.Size = new System.Drawing.Size(457, 41);
            this.btCancleJob.TabIndex = 149;
            this.btCancleJob.Text = "Cancel Job";
            this.btCancleJob.UseVisualStyleBackColor = true;
            this.btCancleJob.Click += new System.EventHandler(this.btCancleJob_Click);
            // 
            // tmrStatusMessages
            // 
            this.tmrStatusMessages.Enabled = true;
            this.tmrStatusMessages.Interval = 200;
            this.tmrStatusMessages.Tick += new System.EventHandler(this.tmrStatusMessages_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(924, 468);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(457, 28);
            this.progressBar1.TabIndex = 150;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(512, 34);
            this.lblPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(168, 17);
            this.lblPath.TabIndex = 151;
            this.lblPath.Text = "Filename of result text file";
            // 
            // txtDestination
            // 
            this.txtDestination.Location = new System.Drawing.Point(516, 58);
            this.txtDestination.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDestination.Name = "txtDestination";
            this.txtDestination.Size = new System.Drawing.Size(192, 22);
            this.txtDestination.TabIndex = 152;
            this.txtDestination.Text = "c:\\Temp\\Test.txt";
            // 
            // pbDestination
            // 
            this.pbDestination.Image = ((System.Drawing.Image)(resources.GetObject("pbDestination.Image")));
            this.pbDestination.Location = new System.Drawing.Point(717, 39);
            this.pbDestination.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pbDestination.Name = "pbDestination";
            this.pbDestination.Size = new System.Drawing.Size(48, 48);
            this.pbDestination.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbDestination.TabIndex = 153;
            this.pbDestination.TabStop = false;
            this.pbDestination.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // saveFileDialog_data
            // 
            this.saveFileDialog_data.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            this.saveFileDialog_data.InitialDirectory = "c:\\";
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Location = new System.Drawing.Point(36, 58);
            this.txtSourcePath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.Size = new System.Drawing.Size(305, 22);
            this.txtSourcePath.TabIndex = 155;
            this.txtSourcePath.Text = "c:\\Temp\\17-11-28\\17-08-22";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(351, 39);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(48, 48);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 156;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 17);
            this.label1.TabIndex = 154;
            this.label1.Text = "Path to NM Directory:";
            // 
            // cbSync_has_4bytes
            // 
            this.cbSync_has_4bytes.AutoSize = true;
            this.cbSync_has_4bytes.Checked = true;
            this.cbSync_has_4bytes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSync_has_4bytes.Location = new System.Drawing.Point(580, 180);
            this.cbSync_has_4bytes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbSync_has_4bytes.Name = "cbSync_has_4bytes";
            this.cbSync_has_4bytes.Size = new System.Drawing.Size(196, 21);
            this.cbSync_has_4bytes.TabIndex = 157;
            this.cbSync_has_4bytes.Text = "Sync package has 4 bytes";
            this.cbSync_has_4bytes.UseVisualStyleBackColor = true;
            // 
            // cChannelsControlV2x11
            // 
            this.cChannelsControlV2x11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cChannelsControlV2x11.Location = new System.Drawing.Point(-1, 218);
            this.cChannelsControlV2x11.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.cChannelsControlV2x11.Name = "cChannelsControlV2x11";
            this.cChannelsControlV2x11.Size = new System.Drawing.Size(784, 422);
            this.cChannelsControlV2x11.TabIndex = 148;
            // 
            // cbUseSourcePath
            // 
            this.cbUseSourcePath.AutoSize = true;
            this.cbUseSourcePath.Checked = true;
            this.cbUseSourcePath.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseSourcePath.Location = new System.Drawing.Point(515, 88);
            this.cbUseSourcePath.Name = "cbUseSourcePath";
            this.cbUseSourcePath.Size = new System.Drawing.Size(137, 21);
            this.cbUseSourcePath.TabIndex = 158;
            this.cbUseSourcePath.Text = "Use Source Path";
            this.cbUseSourcePath.UseVisualStyleBackColor = true;
            // 
            // SDCardReaderNM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbUseSourcePath);
            this.Controls.Add(this.cbSync_has_4bytes);
            this.Controls.Add(this.txtSourcePath);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDestination);
            this.Controls.Add(this.pbDestination);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btCancleJob);
            this.Controls.Add(this.cChannelsControlV2x11);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.btReadBack);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SDCardReaderNM";
            this.Size = new System.Drawing.Size(1397, 743);
            ((System.ComponentModel.ISupportInitialize)(this.pbDestination)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btReadBack;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.RichTextBox txtStatus;
        private CChannelsControlV2x1 cChannelsControlV2x11;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button btCancleJob;
        private System.Windows.Forms.Timer tmrStatusMessages;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtDestination;
        private System.Windows.Forms.PictureBox pbDestination;
        private System.Windows.Forms.SaveFileDialog saveFileDialog_data;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbSync_has_4bytes;
        private System.Windows.Forms.CheckBox cbUseSourcePath;
    }
}

