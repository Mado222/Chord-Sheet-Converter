namespace ComponentsLibGUI
{
    partial class FrmShowLogging
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
            richTextBoxLogs = new RichTextBox();
            SuspendLayout();
            // 
            // richTextBoxLogs
            // 
            richTextBoxLogs.Dock = DockStyle.Fill;
            richTextBoxLogs.Location = new Point(0, 0);
            richTextBoxLogs.Name = "richTextBoxLogs";
            richTextBoxLogs.Size = new Size(836, 632);
            richTextBoxLogs.TabIndex = 2;
            richTextBoxLogs.Text = "";
            // 
            // FrmShowLogging
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(836, 632);
            Controls.Add(richTextBoxLogs);
            Margin = new Padding(4);
            Name = "FrmShowLogging";
            Text = "Console Output";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBoxLogs;
    }
}