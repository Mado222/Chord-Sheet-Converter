
namespace Neuromaster_V5
{
    partial class frmSDCardReader
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
            this.sdCardReaderNM1 = new FeedbackDataLib_GUI.SDCardReaderNM();
            this.SuspendLayout();
            // 
            // sdCardReaderNM1
            // 
            this.sdCardReaderNM1.Location = new System.Drawing.Point(0, 0);
            this.sdCardReaderNM1.Name = "sdCardReaderNM1";
            this.sdCardReaderNM1.Size = new System.Drawing.Size(1048, 604);
            this.sdCardReaderNM1.TabIndex = 0;
            // 
            // frmSDCardReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 530);
            this.Controls.Add(this.sdCardReaderNM1);
            this.Name = "frmSDCardReader";
            this.Text = "SD Card Reader";
            this.ResumeLayout(false);

        }

        #endregion

        private FeedbackDataLib_GUI.SDCardReaderNM sdCardReaderNM1;
    }
}