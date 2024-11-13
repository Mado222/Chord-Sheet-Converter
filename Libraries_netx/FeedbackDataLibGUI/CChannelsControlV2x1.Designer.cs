namespace FeedbackDataLibGUI
{
    using FeedbackDataLib.Modules;
    partial class CChannelsControlV2x1
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            sWChannelsBindingSource = new BindingSource(components);
            cModuleInfoDataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            SWRevision_string = new DataGridViewTextBoxColumn();
            cModuleInfoBindingSource = new BindingSource(components);
            sWChannelsBindingSource1 = new BindingSource(components);
            sWChannelsDataGridView = new DataGridView();
            dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn9 = new ComponentsLib_GUI.DataGridViewNumericUpDownColumn();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            dataGridViewCheckBoxColumn2 = new DataGridViewCheckBoxColumn();
            SkalMax = new DataGridViewTextBoxColumn();
            SkalMin = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn10 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn11 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn12 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn13 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn14 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn15 = new DataGridViewTextBoxColumn();
            sWChannelsBindingSource2 = new BindingSource(components);
            txtInfo1 = new TextBox();
            txtInfo2 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cModuleInfoDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cModuleInfoBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource2).BeginInit();
            SuspendLayout();
            // 
            // cModuleInfoDataGridView
            // 
            cModuleInfoDataGridView.AllowUserToAddRows = false;
            cModuleInfoDataGridView.AllowUserToDeleteRows = false;
            cModuleInfoDataGridView.AutoGenerateColumns = false;
            cModuleInfoDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            cModuleInfoDataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, Column1, SWRevision_string });
            cModuleInfoDataGridView.DataSource = cModuleInfoBindingSource;
            cModuleInfoDataGridView.Location = new Point(4, 19);
            cModuleInfoDataGridView.Margin = new Padding(4, 5, 4, 5);
            cModuleInfoDataGridView.MultiSelect = false;
            cModuleInfoDataGridView.Name = "cModuleInfoDataGridView";
            cModuleInfoDataGridView.ReadOnly = true;
            cModuleInfoDataGridView.RowHeadersVisible = false;
            cModuleInfoDataGridView.RowHeadersWidth = 51;
            cModuleInfoDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            cModuleInfoDataGridView.Size = new Size(257, 321);
            cModuleInfoDataGridView.TabIndex = 0;
            cModuleInfoDataGridView.RowEnter += CModuleInfoDataGridView_RowEnter;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.DataPropertyName = "ModuleTypeNumber";
            dataGridViewTextBoxColumn1.HeaderText = "ModuleTypeNumber";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Visible = false;
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.DataPropertyName = "ModuleType_string";
            dataGridViewTextBoxColumn3.HeaderText = "Module";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.DataPropertyName = "numSWChannels";
            dataGridViewTextBoxColumn5.HeaderText = "numSWChannels";
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            dataGridViewTextBoxColumn5.ReadOnly = true;
            dataGridViewTextBoxColumn5.Visible = false;
            dataGridViewTextBoxColumn5.Width = 125;
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.DataPropertyName = "HW_cn";
            dataGridViewTextBoxColumn6.HeaderText = "No";
            dataGridViewTextBoxColumn6.MinimumWidth = 6;
            dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            dataGridViewTextBoxColumn6.ReadOnly = true;
            dataGridViewTextBoxColumn6.Width = 40;
            // 
            // Column1
            // 
            Column1.DataPropertyName = "uuid";
            Column1.HeaderText = "uuid";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.Width = 125;
            // 
            // SWRevision_string
            // 
            SWRevision_string.DataPropertyName = "SWRevision_string";
            SWRevision_string.HeaderText = "SWRevision_string";
            SWRevision_string.MinimumWidth = 6;
            SWRevision_string.Name = "SWRevision_string";
            SWRevision_string.ReadOnly = true;
            SWRevision_string.Width = 125;
            // 
            // cModuleInfoBindingSource
            // 
            cModuleInfoBindingSource.DataSource = typeof(CModuleBase);
            // 
            // sWChannelsBindingSource1
            // 
            sWChannelsBindingSource1.DataMember = "SWChannels";
            sWChannelsBindingSource1.DataSource = cModuleInfoBindingSource;
            // 
            // sWChannelsDataGridView
            // 
            sWChannelsDataGridView.AllowUserToAddRows = false;
            sWChannelsDataGridView.AllowUserToDeleteRows = false;
            sWChannelsDataGridView.AllowUserToResizeColumns = false;
            sWChannelsDataGridView.AllowUserToResizeRows = false;
            sWChannelsDataGridView.AutoGenerateColumns = false;
            sWChannelsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            sWChannelsDataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9, dataGridViewCheckBoxColumn1, dataGridViewCheckBoxColumn2, SkalMax, SkalMin, dataGridViewTextBoxColumn10, dataGridViewTextBoxColumn11, dataGridViewTextBoxColumn12, dataGridViewTextBoxColumn13, dataGridViewTextBoxColumn14, dataGridViewTextBoxColumn15 });
            sWChannelsDataGridView.DataSource = sWChannelsBindingSource1;
            sWChannelsDataGridView.Location = new Point(269, 19);
            sWChannelsDataGridView.Margin = new Padding(4, 5, 4, 5);
            sWChannelsDataGridView.MultiSelect = false;
            sWChannelsDataGridView.Name = "sWChannelsDataGridView";
            sWChannelsDataGridView.RowHeadersVisible = false;
            sWChannelsDataGridView.RowHeadersWidth = 51;
            sWChannelsDataGridView.Size = new Size(505, 321);
            sWChannelsDataGridView.TabIndex = 1;
            sWChannelsDataGridView.RowEnter += SWChannelsDataGridView_RowEnter;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewTextBoxColumn7.DataPropertyName = "SWChannelName";
            dataGridViewTextBoxColumn7.HeaderText = "Name";
            dataGridViewTextBoxColumn7.MinimumWidth = 6;
            dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            dataGridViewTextBoxColumn7.ReadOnly = true;
            dataGridViewTextBoxColumn7.Width = 120;
            // 
            // dataGridViewTextBoxColumn8
            // 
            dataGridViewTextBoxColumn8.DataPropertyName = "SWChannelNumber";
            dataGridViewTextBoxColumn8.HeaderText = "No";
            dataGridViewTextBoxColumn8.MinimumWidth = 6;
            dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            dataGridViewTextBoxColumn8.ReadOnly = true;
            dataGridViewTextBoxColumn8.Width = 30;
            // 
            // dataGridViewTextBoxColumn9
            // 
            dataGridViewTextBoxColumn9.DataPropertyName = "SampleInt";
            dataGridViewTextBoxColumn9.HeaderText = "SampleInt";
            dataGridViewTextBoxColumn9.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            dataGridViewTextBoxColumn9.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            dataGridViewTextBoxColumn9.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            dataGridViewTextBoxColumn9.MinimumWidth = 6;
            dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            dataGridViewTextBoxColumn9.Resizable = DataGridViewTriState.True;
            dataGridViewTextBoxColumn9.SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewTextBoxColumn9.Width = 60;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.DataPropertyName = "SendChannel";
            dataGridViewCheckBoxColumn1.HeaderText = "Send";
            dataGridViewCheckBoxColumn1.MinimumWidth = 6;
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.Width = 40;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            dataGridViewCheckBoxColumn2.DataPropertyName = "SaveChannel";
            dataGridViewCheckBoxColumn2.HeaderText = "Save";
            dataGridViewCheckBoxColumn2.MinimumWidth = 6;
            dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            dataGridViewCheckBoxColumn2.Width = 40;
            // 
            // SkalMax
            // 
            SkalMax.DataPropertyName = "SkalMax";
            dataGridViewCellStyle1.Format = "G2";
            dataGridViewCellStyle1.NullValue = null;
            SkalMax.DefaultCellStyle = dataGridViewCellStyle1;
            SkalMax.HeaderText = "SkalMax";
            SkalMax.MinimumWidth = 6;
            SkalMax.Name = "SkalMax";
            SkalMax.Width = 70;
            // 
            // SkalMin
            // 
            SkalMin.DataPropertyName = "SkalMin";
            dataGridViewCellStyle2.Format = "G2";
            SkalMin.DefaultCellStyle = dataGridViewCellStyle2;
            SkalMin.HeaderText = "SkalMin";
            SkalMin.MinimumWidth = 6;
            SkalMin.Name = "SkalMin";
            SkalMin.Width = 70;
            // 
            // dataGridViewTextBoxColumn10
            // 
            dataGridViewTextBoxColumn10.DataPropertyName = "ADResolution";
            dataGridViewTextBoxColumn10.HeaderText = "ADResolution";
            dataGridViewTextBoxColumn10.MinimumWidth = 6;
            dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            dataGridViewTextBoxColumn10.ReadOnly = true;
            dataGridViewTextBoxColumn10.Visible = false;
            dataGridViewTextBoxColumn10.Width = 125;
            // 
            // dataGridViewTextBoxColumn11
            // 
            dataGridViewTextBoxColumn11.DataPropertyName = "MidofRange";
            dataGridViewTextBoxColumn11.HeaderText = "MidofRange";
            dataGridViewTextBoxColumn11.MinimumWidth = 6;
            dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            dataGridViewTextBoxColumn11.ReadOnly = true;
            dataGridViewTextBoxColumn11.Visible = false;
            dataGridViewTextBoxColumn11.Width = 125;
            // 
            // dataGridViewTextBoxColumn12
            // 
            dataGridViewTextBoxColumn12.DataPropertyName = "uref";
            dataGridViewTextBoxColumn12.HeaderText = "uref";
            dataGridViewTextBoxColumn12.MinimumWidth = 6;
            dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            dataGridViewTextBoxColumn12.ReadOnly = true;
            dataGridViewTextBoxColumn12.Visible = false;
            dataGridViewTextBoxColumn12.Width = 125;
            // 
            // dataGridViewTextBoxColumn13
            // 
            dataGridViewTextBoxColumn13.DataPropertyName = "Offset_hex";
            dataGridViewTextBoxColumn13.HeaderText = "Offset_hex";
            dataGridViewTextBoxColumn13.MinimumWidth = 6;
            dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            dataGridViewTextBoxColumn13.ReadOnly = true;
            dataGridViewTextBoxColumn13.Visible = false;
            dataGridViewTextBoxColumn13.Width = 125;
            // 
            // dataGridViewTextBoxColumn14
            // 
            dataGridViewTextBoxColumn14.DataPropertyName = "SkalValue_k";
            dataGridViewCellStyle3.Format = "G2";
            dataGridViewTextBoxColumn14.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewTextBoxColumn14.HeaderText = "SkalValue_k";
            dataGridViewTextBoxColumn14.MinimumWidth = 6;
            dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            dataGridViewTextBoxColumn14.ReadOnly = true;
            dataGridViewTextBoxColumn14.Visible = false;
            dataGridViewTextBoxColumn14.Width = 60;
            // 
            // dataGridViewTextBoxColumn15
            // 
            dataGridViewTextBoxColumn15.DataPropertyName = "Offset_d";
            dataGridViewCellStyle4.Format = "G2";
            dataGridViewTextBoxColumn15.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewTextBoxColumn15.HeaderText = "Offset_d";
            dataGridViewTextBoxColumn15.MinimumWidth = 6;
            dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            dataGridViewTextBoxColumn15.ReadOnly = true;
            dataGridViewTextBoxColumn15.Visible = false;
            dataGridViewTextBoxColumn15.Width = 60;
            // 
            // sWChannelsBindingSource2
            // 
            sWChannelsBindingSource2.DataMember = "SWChannels";
            sWChannelsBindingSource2.DataSource = cModuleInfoBindingSource;
            // 
            // txtInfo1
            // 
            txtInfo1.BorderStyle = BorderStyle.None;
            txtInfo1.Enabled = false;
            txtInfo1.Location = new Point(269, 371);
            txtInfo1.Margin = new Padding(4, 5, 4, 5);
            txtInfo1.Multiline = true;
            txtInfo1.Name = "txtInfo1";
            txtInfo1.Size = new Size(263, 75);
            txtInfo1.TabIndex = 2;
            // 
            // txtInfo2
            // 
            txtInfo2.BorderStyle = BorderStyle.None;
            txtInfo2.Enabled = false;
            txtInfo2.Location = new Point(4, 349);
            txtInfo2.Margin = new Padding(4, 5, 4, 5);
            txtInfo2.Multiline = true;
            txtInfo2.Name = "txtInfo2";
            txtInfo2.Size = new Size(257, 98);
            txtInfo2.TabIndex = 3;
            // 
            // CChannelsControlV2x1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(224, 224, 224);
            Controls.Add(txtInfo2);
            Controls.Add(txtInfo1);
            Controls.Add(sWChannelsDataGridView);
            Controls.Add(cModuleInfoDataGridView);
            Margin = new Padding(4, 5, 4, 5);
            Name = "CChannelsControlV2x1";
            Size = new Size(784, 458);
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)cModuleInfoDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)cModuleInfoBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource1).EndInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)sWChannelsBindingSource2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.BindingSource sWChannelsBindingSource;
        private System.Windows.Forms.BindingSource cModuleInfoBindingSource;
        private System.Windows.Forms.DataGridView cModuleInfoDataGridView;
        private System.Windows.Forms.BindingSource sWChannelsBindingSource1;
        private System.Windows.Forms.DataGridView sWChannelsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn SWRevision_string;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.BindingSource sWChannelsBindingSource2;
        private System.Windows.Forms.TextBox txtInfo1;
        private System.Windows.Forms.TextBox txtInfo2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private ComponentsLib_GUI.DataGridViewNumericUpDownColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn SkalMax;
        private System.Windows.Forms.DataGridViewTextBoxColumn SkalMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
    }
}
