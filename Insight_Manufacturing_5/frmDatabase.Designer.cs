using Insight_Manufacturing5_net8.dataSources;
namespace Insight_Manufacturing5_net8
{
    partial class frmDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDatabase));
            dsManufacturing = new dsManufacturing();
            neurodevicesBindingSource = new BindingSource(components);
            neurodevicesTableAdapter = new dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();
            tableAdapterManager = new dataSources.dsManufacturingTableAdapters.TableAdapterManager();
            neurodevicesBindingNavigator = new BindingNavigator(components);
            bindingNavigatorAddNewItem = new ToolStripButton();
            bindingNavigatorCountItem = new ToolStripLabel();
            bindingNavigatorDeleteItem = new ToolStripButton();
            bindingNavigatorMoveFirstItem = new ToolStripButton();
            bindingNavigatorMovePreviousItem = new ToolStripButton();
            bindingNavigatorSeparator = new ToolStripSeparator();
            bindingNavigatorPositionItem = new ToolStripTextBox();
            bindingNavigatorSeparator1 = new ToolStripSeparator();
            bindingNavigatorMoveNextItem = new ToolStripButton();
            bindingNavigatorMoveLastItem = new ToolStripButton();
            bindingNavigatorSeparator2 = new ToolStripSeparator();
            neurodevicesBindingNavigatorSaveItem = new ToolStripButton();
            neurodevicesDataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            dataGridViewCheckBoxColumn2 = new DataGridViewCheckBoxColumn();
            dsManufacturingBindingSource = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)dsManufacturing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingNavigator).BeginInit();
            neurodevicesBindingNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)neurodevicesDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dsManufacturingBindingSource).BeginInit();
            SuspendLayout();
            // 
            // dsManufacturing
            // 
            dsManufacturing.DataSetName = "dsManufacturing";
            dsManufacturing.Namespace = "http://tempuri.org/dsManufacturing.xsd";
            dsManufacturing.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // neurodevicesBindingSource
            // 
            neurodevicesBindingSource.DataMember = "Neurodevices";
            neurodevicesBindingSource.DataSource = dsManufacturing;
            // 
            // neurodevicesTableAdapter
            // 
            neurodevicesTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            tableAdapterManager.AmplitudengainTableAdapter = null;
            tableAdapterManager.BackupDataSetBeforeUpdate = false;
            tableAdapterManager.NeurodevicesTableAdapter = neurodevicesTableAdapter;
            tableAdapterManager.Neuromodule_DatenTableAdapter = null;
            tableAdapterManager.Neuromodule_KalibrierdatenTableAdapter = null;
            tableAdapterManager.UpdateOrder = dataSources.dsManufacturingTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // neurodevicesBindingNavigator
            // 
            neurodevicesBindingNavigator.AddNewItem = bindingNavigatorAddNewItem;
            neurodevicesBindingNavigator.BindingSource = neurodevicesBindingSource;
            neurodevicesBindingNavigator.CountItem = bindingNavigatorCountItem;
            neurodevicesBindingNavigator.DeleteItem = bindingNavigatorDeleteItem;
            neurodevicesBindingNavigator.ImageScalingSize = new Size(20, 20);
            neurodevicesBindingNavigator.Items.AddRange(new ToolStripItem[] { bindingNavigatorMoveFirstItem, bindingNavigatorMovePreviousItem, bindingNavigatorSeparator, bindingNavigatorPositionItem, bindingNavigatorCountItem, bindingNavigatorSeparator1, bindingNavigatorMoveNextItem, bindingNavigatorMoveLastItem, bindingNavigatorSeparator2, bindingNavigatorAddNewItem, bindingNavigatorDeleteItem, neurodevicesBindingNavigatorSaveItem });
            neurodevicesBindingNavigator.Location = new Point(0, 0);
            neurodevicesBindingNavigator.MoveFirstItem = bindingNavigatorMoveFirstItem;
            neurodevicesBindingNavigator.MoveLastItem = bindingNavigatorMoveLastItem;
            neurodevicesBindingNavigator.MoveNextItem = bindingNavigatorMoveNextItem;
            neurodevicesBindingNavigator.MovePreviousItem = bindingNavigatorMovePreviousItem;
            neurodevicesBindingNavigator.Name = "neurodevicesBindingNavigator";
            neurodevicesBindingNavigator.PositionItem = bindingNavigatorPositionItem;
            neurodevicesBindingNavigator.Size = new Size(1288, 27);
            neurodevicesBindingNavigator.TabIndex = 0;
            neurodevicesBindingNavigator.Text = "bindingNavigator1";
            // 
            // bindingNavigatorAddNewItem
            // 
            bindingNavigatorAddNewItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorAddNewItem.Enabled = false;
            bindingNavigatorAddNewItem.Image = (Image)resources.GetObject("bindingNavigatorAddNewItem.Image");
            bindingNavigatorAddNewItem.Name = "bindingNavigatorAddNewItem";
            bindingNavigatorAddNewItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorAddNewItem.Size = new Size(29, 24);
            bindingNavigatorAddNewItem.Text = "Add new";
            bindingNavigatorAddNewItem.Visible = false;
            // 
            // bindingNavigatorCountItem
            // 
            bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            bindingNavigatorCountItem.Size = new Size(45, 24);
            bindingNavigatorCountItem.Text = "of {0}";
            bindingNavigatorCountItem.ToolTipText = "Total number of items";
            // 
            // bindingNavigatorDeleteItem
            // 
            bindingNavigatorDeleteItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorDeleteItem.Image = (Image)resources.GetObject("bindingNavigatorDeleteItem.Image");
            bindingNavigatorDeleteItem.Name = "bindingNavigatorDeleteItem";
            bindingNavigatorDeleteItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorDeleteItem.Size = new Size(29, 24);
            bindingNavigatorDeleteItem.Text = "Delete";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            bindingNavigatorMoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorMoveFirstItem.Image = (Image)resources.GetObject("bindingNavigatorMoveFirstItem.Image");
            bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorMoveFirstItem.Size = new Size(29, 24);
            bindingNavigatorMoveFirstItem.Text = "Move first";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            bindingNavigatorMovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorMovePreviousItem.Image = (Image)resources.GetObject("bindingNavigatorMovePreviousItem.Image");
            bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            bindingNavigatorMovePreviousItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorMovePreviousItem.Size = new Size(29, 24);
            bindingNavigatorMovePreviousItem.Text = "Move previous";
            // 
            // bindingNavigatorSeparator
            // 
            bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            bindingNavigatorSeparator.Size = new Size(6, 27);
            // 
            // bindingNavigatorPositionItem
            // 
            bindingNavigatorPositionItem.AccessibleName = "Position";
            bindingNavigatorPositionItem.AutoSize = false;
            bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            bindingNavigatorPositionItem.Size = new Size(65, 27);
            bindingNavigatorPositionItem.Text = "0";
            bindingNavigatorPositionItem.ToolTipText = "Current position";
            // 
            // bindingNavigatorSeparator1
            // 
            bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            bindingNavigatorSeparator1.Size = new Size(6, 27);
            // 
            // bindingNavigatorMoveNextItem
            // 
            bindingNavigatorMoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorMoveNextItem.Image = (Image)resources.GetObject("bindingNavigatorMoveNextItem.Image");
            bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            bindingNavigatorMoveNextItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorMoveNextItem.Size = new Size(29, 24);
            bindingNavigatorMoveNextItem.Text = "Move next";
            // 
            // bindingNavigatorMoveLastItem
            // 
            bindingNavigatorMoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigatorMoveLastItem.Image = (Image)resources.GetObject("bindingNavigatorMoveLastItem.Image");
            bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            bindingNavigatorMoveLastItem.RightToLeftAutoMirrorImage = true;
            bindingNavigatorMoveLastItem.Size = new Size(29, 24);
            bindingNavigatorMoveLastItem.Text = "Move last";
            // 
            // bindingNavigatorSeparator2
            // 
            bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            bindingNavigatorSeparator2.Size = new Size(6, 27);
            // 
            // neurodevicesBindingNavigatorSaveItem
            // 
            neurodevicesBindingNavigatorSaveItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            neurodevicesBindingNavigatorSaveItem.Image = (Image)resources.GetObject("neurodevicesBindingNavigatorSaveItem.Image");
            neurodevicesBindingNavigatorSaveItem.Name = "neurodevicesBindingNavigatorSaveItem";
            neurodevicesBindingNavigatorSaveItem.Size = new Size(29, 24);
            neurodevicesBindingNavigatorSaveItem.Text = "Save Data";
            neurodevicesBindingNavigatorSaveItem.Click += neurodevicesBindingNavigatorSaveItem_Click;
            // 
            // neurodevicesDataGridView
            // 
            neurodevicesDataGridView.AutoGenerateColumns = false;
            neurodevicesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            neurodevicesDataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewCheckBoxColumn1, dataGridViewCheckBoxColumn2 });
            neurodevicesDataGridView.DataSource = neurodevicesBindingSource;
            neurodevicesDataGridView.Dock = DockStyle.Fill;
            neurodevicesDataGridView.Location = new Point(0, 27);
            neurodevicesDataGridView.Margin = new Padding(4, 5, 4, 5);
            neurodevicesDataGridView.Name = "neurodevicesDataGridView";
            neurodevicesDataGridView.RowHeadersWidth = 51;
            neurodevicesDataGridView.Size = new Size(1288, 821);
            neurodevicesDataGridView.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.DataPropertyName = "SerialNumber";
            dataGridViewTextBoxColumn1.HeaderText = "SerialNumber";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.DataPropertyName = "Typ";
            dataGridViewTextBoxColumn2.HeaderText = "Typ";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.DataPropertyName = "Programmierdatum";
            dataGridViewTextBoxColumn3.HeaderText = "Programmierdatum";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.DataPropertyName = "Testdatum";
            dataGridViewTextBoxColumn4.HeaderText = "Testdatum";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.Width = 125;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.DataPropertyName = "TestOK";
            dataGridViewTextBoxColumn5.HeaderText = "TestOK";
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            dataGridViewTextBoxColumn5.Width = 125;
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.DataPropertyName = "Version";
            dataGridViewTextBoxColumn6.HeaderText = "Version";
            dataGridViewTextBoxColumn6.MinimumWidth = 6;
            dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            dataGridViewTextBoxColumn6.Width = 125;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewTextBoxColumn7.DataPropertyName = "Hex_File_geflashed";
            dataGridViewTextBoxColumn7.HeaderText = "Hex_File_geflashed";
            dataGridViewTextBoxColumn7.MinimumWidth = 6;
            dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            dataGridViewTextBoxColumn7.Width = 125;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.DataPropertyName = "Test_Kabel";
            dataGridViewCheckBoxColumn1.HeaderText = "Test_Kabel";
            dataGridViewCheckBoxColumn1.MinimumWidth = 6;
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.Width = 125;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            dataGridViewCheckBoxColumn2.DataPropertyName = "Test_Funk";
            dataGridViewCheckBoxColumn2.HeaderText = "Test_Funk";
            dataGridViewCheckBoxColumn2.MinimumWidth = 6;
            dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            dataGridViewCheckBoxColumn2.Width = 125;
            // 
            // dsManufacturingBindingSource
            // 
            dsManufacturingBindingSource.DataSource = dsManufacturing;
            dsManufacturingBindingSource.Position = 0;
            // 
            // frmDatabase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1288, 848);
            Controls.Add(neurodevicesDataGridView);
            Controls.Add(neurodevicesBindingNavigator);
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmDatabase";
            Text = "Insight Datenbank";
            Load += frmDatabase_Load;
            ((System.ComponentModel.ISupportInitialize)dsManufacturing).EndInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)neurodevicesBindingNavigator).EndInit();
            neurodevicesBindingNavigator.ResumeLayout(false);
            neurodevicesBindingNavigator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)neurodevicesDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)dsManufacturingBindingSource).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private dsManufacturing dsManufacturing;
        private System.Windows.Forms.BindingSource neurodevicesBindingSource;
        private Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter;
        private Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.TableAdapterManager tableAdapterManager;
        private System.Windows.Forms.BindingNavigator neurodevicesBindingNavigator;
        private System.Windows.Forms.ToolStripButton bindingNavigatorAddNewItem;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorDeleteItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.ToolStripButton neurodevicesBindingNavigatorSaveItem;
        private System.Windows.Forms.DataGridView neurodevicesDataGridView;
        private BindingSource dsManufacturingBindingSource;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
    }
}