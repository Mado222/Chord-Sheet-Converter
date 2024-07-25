using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8
{
    partial class frm_Multi_Temp
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
            this.nudTempSoll = new System.Windows.Forms.NumericUpDown();
            this.btOK = new System.Windows.Forms.Button();
            this.lblTempIst = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dsManufacturing = new dsManufacturing();
            this.neurodevicesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.neurodevicesTableAdapter = new Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();
            this.tableAdapterManager = new Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.TableAdapterManager();
            this.neuromodule_DatenTableAdapter = new Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter();
            this.neuromodule_DatenBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudTempSoll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsManufacturing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.neurodevicesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.neuromodule_DatenBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // nudTempSoll
            // 
            this.nudTempSoll.DecimalPlaces = 1;
            this.nudTempSoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudTempSoll.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudTempSoll.Location = new System.Drawing.Point(224, 63);
            this.nudTempSoll.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.nudTempSoll.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nudTempSoll.Name = "nudTempSoll";
            this.nudTempSoll.Size = new System.Drawing.Size(80, 26);
            this.nudTempSoll.TabIndex = 21;
            this.nudTempSoll.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btOK
            // 
            this.btOK.BackColor = System.Drawing.Color.Green;
            this.btOK.ForeColor = System.Drawing.Color.White;
            this.btOK.Location = new System.Drawing.Point(15, 92);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(159, 32);
            this.btOK.TabIndex = 20;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = false;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // lblTempIst
            // 
            this.lblTempIst.AutoSize = true;
            this.lblTempIst.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTempIst.Location = new System.Drawing.Point(221, 9);
            this.lblTempIst.Name = "lblTempIst";
            this.lblTempIst.Size = new System.Drawing.Size(22, 17);
            this.lblTempIst.TabIndex = 19;
            this.lblTempIst.Text = "xx";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 17);
            this.label1.TabIndex = 18;
            this.label1.Text = "Soll-Temperatur [°] eintragen:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(197, 17);
            this.label2.TabIndex = 17;
            this.label2.Text = "Temperatur gemessen [°]:";
            // 
            // dsManufacturing
            // 
            this.dsManufacturing.DataSetName = "dsManufacturing";
            this.dsManufacturing.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // neurodevicesBindingSource
            // 
            this.neurodevicesBindingSource.DataMember = "Neurodevices";
            this.neurodevicesBindingSource.DataSource = this.dsManufacturing;
            // 
            // neurodevicesTableAdapter
            // 
            this.neurodevicesTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            this.tableAdapterManager.BackupDataSetBeforeUpdate = false;
            this.tableAdapterManager.NeurodevicesTableAdapter = this.neurodevicesTableAdapter;
            this.tableAdapterManager.Neuromodule_DatenTableAdapter = this.neuromodule_DatenTableAdapter;
            this.tableAdapterManager.Neuromodule_KalibrierdatenTableAdapter = null;
            this.tableAdapterManager.UpdateOrder = dataSources.dsManufacturingTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // neuromodule_DatenTableAdapter
            // 
            this.neuromodule_DatenTableAdapter.ClearBeforeFill = true;
            // 
            // neuromodule_DatenBindingSource
            // 
            this.neuromodule_DatenBindingSource.DataMember = "Neuromodule_Daten";
            this.neuromodule_DatenBindingSource.DataSource = this.dsManufacturing;
            // 
            // frm_Multi_Temp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 132);
            this.Controls.Add(this.nudTempSoll);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.lblTempIst);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "frm_Multi_Temp";
            this.Text = "Multisensor Temperature";
            ((System.ComponentModel.ISupportInitialize)(this.nudTempSoll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsManufacturing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.neurodevicesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.neuromodule_DatenBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudTempSoll;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Label lblTempIst;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private dsManufacturing dsManufacturing;
        private System.Windows.Forms.BindingSource neurodevicesBindingSource;
        private Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter;
        private Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.TableAdapterManager tableAdapterManager;
        private Insight_Manufacturing5_net8.dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter;
        private System.Windows.Forms.BindingSource neuromodule_DatenBindingSource;
    }
}