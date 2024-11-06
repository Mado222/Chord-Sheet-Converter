namespace Neuromaster_V5
{
    partial class frmSaveData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSaveData));
            this.txtComment = new System.Windows.Forms.TextBox();
            this.ctbSaving = new ComponentsLib_GUI.CToggleButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.saveFileDialog_data = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtComment
            // 
            this.txtComment.Location = new System.Drawing.Point(3, 72);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(269, 48);
            this.txtComment.TabIndex = 10;
            this.txtComment.Text = "Kommentar hier hereinschreiben: Wird im File abgespeichert";
            // 
            // ctbSaving
            // 
            this.ctbSaving.AcceptChange = false;
            this.ctbSaving.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ctbSaving.ColorState1 = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ctbSaving.ColorState2 = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.ctbSaving.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ctbSaving.Location = new System.Drawing.Point(184, 25);
            this.ctbSaving.Name = "ctbSaving";
            this.ctbSaving.Size = new System.Drawing.Size(85, 31);
            this.ctbSaving.TabIndex = 146;
            this.ctbSaving.Text = "Start Saving";
            this.ctbSaving.TextState1 = "Start Saving";
            this.ctbSaving.TextState2 = "Stop Saving";
            this.ctbSaving.UseVisualStyleBackColor = false;
            this.ctbSaving.ToState2 += new System.EventHandler(this.CtbSaving_ToState2);
            this.ctbSaving.ToState1 += new System.EventHandler(this.CtbSaving_ToState1);
            // 
            // label9
            // 
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(0, 57);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 23);
            this.label9.TabIndex = 9;
            this.label9.Text = "Comment:";
            // 
            // label8
            // 
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(0, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(148, 20);
            this.label8.TabIndex = 6;
            this.label8.Text = "Save Measure Data in File:";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(3, 34);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(145, 20);
            this.txtPath.TabIndex = 7;
            this.txtPath.Text = "c:\\temp\\Test.txt";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(154, 33);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 23);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.PictureBox1_Click);
            // 
            // saveFileDialog_data
            // 
            this.saveFileDialog_data.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            this.saveFileDialog_data.InitialDirectory = "c:\\";
            // 
            // frmSaveData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 130);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.ctbSaving);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtPath);
            this.Name = "frmSaveData";
            this.Text = "Save Data to File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSaveData_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.PictureBox pictureBox1;
        public ComponentsLib_GUI.CToggleButton ctbSaving;
        public System.Windows.Forms.TextBox txtComment;
        public System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.SaveFileDialog saveFileDialog_data;
    }
}