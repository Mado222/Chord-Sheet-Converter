namespace Insight_Manufacturing5_net8
{
    partial class frmDatabase2
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDatabase2));
            dataGridView1 = new DataGridView();
            serialNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            typDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            programmierdatumDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            testdatumDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            testOKDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            versionDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            hexFilegeflashedDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            testKabelDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
            testFunkDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
            testDetailsDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            neurodevicesBindingSource = new BindingSource(components);
            menuStrip1 = new MenuStrip();
            toolStrip1 = new ToolStrip();
            tsbDelete = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingSource).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { serialNumberDataGridViewTextBoxColumn, typDataGridViewTextBoxColumn, programmierdatumDataGridViewTextBoxColumn, testdatumDataGridViewTextBoxColumn, testOKDataGridViewTextBoxColumn, versionDataGridViewTextBoxColumn, hexFilegeflashedDataGridViewTextBoxColumn, testKabelDataGridViewCheckBoxColumn, testFunkDataGridViewCheckBoxColumn, testDetailsDataGridViewTextBoxColumn });
            dataGridView1.DataSource = neurodevicesBindingSource;
            dataGridView1.Dock = DockStyle.Bottom;
            dataGridView1.Location = new Point(0, 281);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1363, 416);
            dataGridView1.TabIndex = 0;
            // 
            // serialNumberDataGridViewTextBoxColumn
            // 
            serialNumberDataGridViewTextBoxColumn.DataPropertyName = "SerialNumber";
            serialNumberDataGridViewTextBoxColumn.HeaderText = "SerialNumber";
            serialNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            serialNumberDataGridViewTextBoxColumn.Name = "serialNumberDataGridViewTextBoxColumn";
            serialNumberDataGridViewTextBoxColumn.Width = 125;
            // 
            // typDataGridViewTextBoxColumn
            // 
            typDataGridViewTextBoxColumn.DataPropertyName = "Typ";
            typDataGridViewTextBoxColumn.HeaderText = "Typ";
            typDataGridViewTextBoxColumn.MinimumWidth = 6;
            typDataGridViewTextBoxColumn.Name = "typDataGridViewTextBoxColumn";
            typDataGridViewTextBoxColumn.Width = 125;
            // 
            // programmierdatumDataGridViewTextBoxColumn
            // 
            programmierdatumDataGridViewTextBoxColumn.DataPropertyName = "Programmierdatum";
            programmierdatumDataGridViewTextBoxColumn.HeaderText = "Programmierdatum";
            programmierdatumDataGridViewTextBoxColumn.MinimumWidth = 6;
            programmierdatumDataGridViewTextBoxColumn.Name = "programmierdatumDataGridViewTextBoxColumn";
            programmierdatumDataGridViewTextBoxColumn.Width = 125;
            // 
            // testdatumDataGridViewTextBoxColumn
            // 
            testdatumDataGridViewTextBoxColumn.DataPropertyName = "Testdatum";
            testdatumDataGridViewTextBoxColumn.HeaderText = "Testdatum";
            testdatumDataGridViewTextBoxColumn.MinimumWidth = 6;
            testdatumDataGridViewTextBoxColumn.Name = "testdatumDataGridViewTextBoxColumn";
            testdatumDataGridViewTextBoxColumn.Width = 125;
            // 
            // testOKDataGridViewTextBoxColumn
            // 
            testOKDataGridViewTextBoxColumn.DataPropertyName = "TestOK";
            testOKDataGridViewTextBoxColumn.HeaderText = "TestOK";
            testOKDataGridViewTextBoxColumn.MinimumWidth = 6;
            testOKDataGridViewTextBoxColumn.Name = "testOKDataGridViewTextBoxColumn";
            testOKDataGridViewTextBoxColumn.Width = 125;
            // 
            // versionDataGridViewTextBoxColumn
            // 
            versionDataGridViewTextBoxColumn.DataPropertyName = "Version";
            versionDataGridViewTextBoxColumn.HeaderText = "Version";
            versionDataGridViewTextBoxColumn.MinimumWidth = 6;
            versionDataGridViewTextBoxColumn.Name = "versionDataGridViewTextBoxColumn";
            versionDataGridViewTextBoxColumn.Width = 125;
            // 
            // hexFilegeflashedDataGridViewTextBoxColumn
            // 
            hexFilegeflashedDataGridViewTextBoxColumn.DataPropertyName = "Hex_File_geflashed";
            hexFilegeflashedDataGridViewTextBoxColumn.HeaderText = "Hex_File_geflashed";
            hexFilegeflashedDataGridViewTextBoxColumn.MinimumWidth = 6;
            hexFilegeflashedDataGridViewTextBoxColumn.Name = "hexFilegeflashedDataGridViewTextBoxColumn";
            hexFilegeflashedDataGridViewTextBoxColumn.Width = 125;
            // 
            // testKabelDataGridViewCheckBoxColumn
            // 
            testKabelDataGridViewCheckBoxColumn.DataPropertyName = "Test_Kabel";
            testKabelDataGridViewCheckBoxColumn.HeaderText = "Test_Kabel";
            testKabelDataGridViewCheckBoxColumn.MinimumWidth = 6;
            testKabelDataGridViewCheckBoxColumn.Name = "testKabelDataGridViewCheckBoxColumn";
            testKabelDataGridViewCheckBoxColumn.Width = 125;
            // 
            // testFunkDataGridViewCheckBoxColumn
            // 
            testFunkDataGridViewCheckBoxColumn.DataPropertyName = "Test_Funk";
            testFunkDataGridViewCheckBoxColumn.HeaderText = "Test_Funk";
            testFunkDataGridViewCheckBoxColumn.MinimumWidth = 6;
            testFunkDataGridViewCheckBoxColumn.Name = "testFunkDataGridViewCheckBoxColumn";
            testFunkDataGridViewCheckBoxColumn.Width = 125;
            // 
            // testDetailsDataGridViewTextBoxColumn
            // 
            testDetailsDataGridViewTextBoxColumn.DataPropertyName = "Test_Details";
            testDetailsDataGridViewTextBoxColumn.HeaderText = "Test_Details";
            testDetailsDataGridViewTextBoxColumn.MinimumWidth = 6;
            testDetailsDataGridViewTextBoxColumn.Name = "testDetailsDataGridViewTextBoxColumn";
            testDetailsDataGridViewTextBoxColumn.Width = 125;
            // 
            // neurodevicesBindingSource
            // 
            neurodevicesBindingSource.DataMember = "Neurodevices";
            neurodevicesBindingSource.DataSource = typeof(dataSources.dsManufacturing);
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1363, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbDelete, toolStripButton2 });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1363, 27);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbDelete
            // 
            tsbDelete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageTransparentColor = Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new Size(29, 24);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += tsbDelete_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(29, 24);
            toolStripButton2.Text = "toolStripButton2";
            // 
            // frmDatabase2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1363, 697);
            Controls.Add(toolStrip1);
            Controls.Add(dataGridView1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "frmDatabase2";
            Text = "frmDatabase2";
            Load += frmDatabase2_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingSource).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn serialNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn typDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn programmierdatumDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn testdatumDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn testOKDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn versionDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn hexFilegeflashedDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn testKabelDataGridViewCheckBoxColumn;
        private DataGridViewCheckBoxColumn testFunkDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn testDetailsDataGridViewTextBoxColumn;
        private BindingSource neurodevicesBindingSource;
        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbDelete;
        private ToolStripButton toolStripButton2;
    }
}