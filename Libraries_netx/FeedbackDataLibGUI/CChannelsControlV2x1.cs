using FeedbackDataLib;
using FeedbackDataLib.Modules;
using System.Xml.Serialization;


namespace FeedbackDataLib_GUI
{
    public partial class CChannelsControlV2x1 : UserControl
    {
        private readonly Color colNotActive = Color.LightYellow;
        private readonly Color colActive = Color.LightGreen;
        private readonly Color colEmpty = Color.LightGray;

        private readonly Color colError = Color.Red;


        /********* Reading default values from file *********/
        public class Default_Scaling_Values
        {
            public class Scaling_Values
            {
                public double Default_Max { get; set; }

                public double Default_Min { get; set; }

                public string ModuleName        //just for better reading
                { get; set; } = "";


                public Scaling_Values()
                {
                    Default_Max = 0;
                    Default_Min = 0;
                }
            }

            private Scaling_Values[][] DefaultVals;

            public Default_Scaling_Values()
            {
                DefaultVals = new Scaling_Values[Enum.GetNames(typeof(enumModuleType)).Length][];

                for (int i = 0; i < DefaultVals.Length; i++)
                {
                    DefaultVals[i] = new Scaling_Values[C8KanalReceiverV2_CommBase.max_num_SWChannels];

                    for (int j = 0; j < DefaultVals[i].Length; j++)
                    {
                        DefaultVals[i][j] = new Scaling_Values
                        {
                            ModuleName = Enum.GetNames(typeof(enumModuleType))[j] + "_SW" + j.ToString()
                        };
                    }
                }
            }

            public void GetScalingValues(ref double DefMax, ref double DefMin, enumModuleType moduleType, int sw_cn)
            {
                DefMax = DefaultVals[(int)moduleType][sw_cn].Default_Max;
                DefMin = DefaultVals[(int)moduleType][sw_cn].Default_Min;
            }

            public void SetScalingValues(double DefMax, double DefMin, enumModuleType moduleType, int sw_cn)
            {
                DefaultVals[(int)moduleType][sw_cn].Default_Max = DefMax;
                DefaultVals[(int)moduleType][sw_cn].Default_Min = DefMin;
                DefaultVals[(int)moduleType][sw_cn].ModuleName = Enum.GetName(typeof(enumModuleType), moduleType); //??
            }


            public static void GetConfigPath(ref string ConfigXMLPath)
            {
                string fullPath = System.IO.Directory.GetCurrentDirectory();
                ConfigXMLPath = fullPath + @"\LocalDevice.xml";
            }

            public bool SaveConfigData()
            {
                /* Create a StreamWriter to write with. First create a FileStream
                   object, and create the StreamWriter specifying an Encoding to use. */
                try
                {
                    string ConfigXMLPath = "";
                    GetConfigPath(ref ConfigXMLPath);

                    FileStream fs = new(ConfigXMLPath, FileMode.Create);
                    TextWriter writer = new StreamWriter(fs);
                    XmlSerializer ser = new(typeof(Scaling_Values[][]));
                    ser.Serialize(writer, DefaultVals);
                    writer.Close();
                }
                catch
                {
                    return false;
                }
                return true;
            }

            public bool ReadConfigData()
            {
                try
                {
                    string ConfigXMLPath = "";
                    GetConfigPath(ref ConfigXMLPath);

                    FileStream fs = new(ConfigXMLPath, FileMode.Open);
                    TextReader reader = new StreamReader(fs);
                    XmlSerializer ser = new(typeof(Scaling_Values[][]));
                    object? o = ser.Deserialize(reader);
                    if (o != null)
                    {
                        DefaultVals = (Scaling_Values[][])o;
                    }
                    reader.Close();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }


        public CChannelsControlV2x1()
        {
            InitializeComponent();
            tmrTimeout = new System.Windows.Forms.Timer
            {
                Enabled = false,
                Interval = 1000
            };
            tmrTimeout.Tick += TmrTimeout_Tick;

            txtInfo1.BackColor = BackColor;
            txtInfo2.BackColor = BackColor;
        }

        private void TmrTimeout_Tick(object? sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            DateTime timeout = dt - new TimeSpan(0, 0, 0, 0, tmrTimeout.Interval);
            for (int hw_cn = 0; hw_cn < _ModuleInfos.Count; hw_cn++)
            {

                if (_ModuleInfos[hw_cn].ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    if (ChannelTimeouts[hw_cn] < timeout)
                        cModuleInfoDataGridView.Rows[hw_cn].HeaderCell.Style.BackColor = colNotActive;
                }
                else
                {
                    cModuleInfoDataGridView.Rows[hw_cn].HeaderCell.Style.BackColor = colEmpty;
                }
            }
        }

        private List<DateTime> ChannelTimeouts = [];
        private readonly System.Windows.Forms.Timer tmrTimeout = new();

        private List<CModuleBase> _ModuleInfos;

        /// <summary>
        /// Returns clone of _ModuleInfo
        /// </summary>
        /// <returns></returns>
        public CModuleBase? GetModuleInfo(int HW_cn)
        {
            CModuleBase? ret = null;
            if (_ModuleInfos != null)
            {
                ret = (CModuleBase)_ModuleInfos[HW_cn].Clone();
                ret.SetModuleSpecific(GetModuleSpecific(HW_cn));
            }
            return ret;
        }

        public byte[] GetModuleSpecific(int HW_cn)
        {
            if (_ModuleInfos[HW_cn].ModuleType_Unmodified == enumModuleType.cModuleMultisensor && ucModuleSpecificSetup_MultiSensor != null)
            {
                CModuleMultisensor ModuleInfo = new();
                ucModuleSpecificSetup_MultiSensor.ReadModuleSpecificInfo(ref ModuleInfo);
                return ModuleInfo.GetModuleSpecific();
            }
            else if (_ModuleInfos[HW_cn].ModuleType_Unmodified == enumModuleType.cModuleAtemIRDig && ucModuleSpecificSetup_AtemIR != null)
            {
                CModuleRespI ModuleInfo = new();
                ucModuleSpecificSetup_AtemIR.ReadModuleSpecificInfo(ref ModuleInfo);
                return ModuleInfo.GetModuleSpecific();
            }
            return _ModuleInfos[HW_cn].GetModuleSpecific();
        }

        /// <summary>
        /// Clones value for internal usage
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetModuleInfos(List<CModuleBase> value)
        {
            if (value is not null && cModuleInfoDataGridView is not null && cModuleInfoDataGridView.SelectedRows is not null)
            {
                _ModuleInfos = [];
                for (int i = 0; i < value.Count; i++)
                {
                    _ModuleInfos.Add((CModuleBase)value[i].Clone());
                }
                UpdateDatabinding();
                ChannelTimeouts = [];
                for (int hw_cn = 0; hw_cn < _ModuleInfos.Count; hw_cn++)
                {
                    if (_ModuleInfos[hw_cn].ModuleBootloaderError)
                    {
                        cModuleInfoDataGridView.Rows[hw_cn].DefaultCellStyle.BackColor = colError;
                    }
                    else
                    {
                        cModuleInfoDataGridView.Rows[hw_cn].DefaultCellStyle.BackColor = _ModuleInfos[hw_cn].ModuleColor;
                    }
                    ChannelTimeouts.Add(DateTime.Now);
                }

                cModuleInfoDataGridView.Rows[0].Selected = true;
                if (cModuleInfoDataGridView?.SelectedRows != null &&
                    cModuleInfoDataGridView.SelectedRows.Count > 0 &&
                    cModuleInfoDataGridView.SelectedRows[0]?.DataBoundItem is CModuleBase module)
                {
                    SelectedModuleRow = module;
                }


                sWChannelsDataGridView.Rows[0].Selected = true;
                SelectedSWChannel = sWChannelsDataGridView.Rows[0].DataBoundItem as CSWChannel;

                UpdateInfo();
                UpdateModuleSpecificInfo(SelectedModuleRow);

                tmrTimeout.Enabled = true;
            }
        }

        public void SetUserChangeableData(List<CModuleBase> ModuleInfo)
        {
            for (int hw_cn = 0; hw_cn < ModuleInfo.Count; hw_cn++)
            {
                for (int sw_cn = 0; sw_cn < ModuleInfo[hw_cn].SWChannels.Count; sw_cn++)
                {
                    SetUserChangeableData(hw_cn, sw_cn, ModuleInfo[hw_cn].SWChannels[sw_cn].GetUserChangableValues());
                }
            }
            Refresh();
        }

        public void SetUserChangeableData(CSWConfigValues[][] SWConfigValues)
        {

            for (int hw_cn = 0; hw_cn < _ModuleInfos.Count; hw_cn++)
            {
                for (int sw_cn = 0; sw_cn < _ModuleInfos[hw_cn].SWChannels.Count; sw_cn++)
                {
                    SetUserChangeableData(hw_cn, sw_cn, SWConfigValues[hw_cn][sw_cn]);
                }
            }
            Refresh();
        }

        public void SetUserChangeableData(int hw_cn, int sw_cn, CSWConfigValues SWConfigValues)
        {
            _ModuleInfos[hw_cn].SWChannels[sw_cn].SampleInt = SWConfigValues.SampleInt;
            _ModuleInfos[hw_cn].SWChannels[sw_cn].SendChannel = SWConfigValues.SendChannel;
            _ModuleInfos[hw_cn].SWChannels[sw_cn].SaveChannel = SWConfigValues.SaveChannel;
            _ModuleInfos[hw_cn].SWChannels[sw_cn].SkalMax = SWConfigValues.SkalMax;
            _ModuleInfos[hw_cn].SWChannels[sw_cn].SkalMin = SWConfigValues.SkalMin;
        }

        public CSWConfigValues[][] GetUserChangeableData()
        {
            CSWConfigValues[][] cv = new CSWConfigValues[_ModuleInfos.Count][];
            for (int _hw_cn = 0; _hw_cn < _ModuleInfos.Count; _hw_cn++)
            {
                cv[_hw_cn] = new CSWConfigValues[_ModuleInfos[_hw_cn].NumSWChannels];
                for (int _sw_cn = 0; _sw_cn < _ModuleInfos[_hw_cn].NumSWChannels; _sw_cn++)
                {
                    cv[_hw_cn][_sw_cn] = _ModuleInfos[_hw_cn].SWChannels[_sw_cn].GetUserChangableValues();
                }
            }
            return cv;
        }

        /// <summary>
        /// Signals that there was activiity on the channel
        /// </summary>
        /// <param name="hw_cn">hw_cn</param>
        public void SetChannelActivity(int hw_cn)
        {
            cModuleInfoDataGridView.Rows[hw_cn].HeaderCell.Style.BackColor = colActive;
            ChannelTimeouts[hw_cn] = DateTime.Now;
        }

        //Events
        #region Events
        public delegate void ModuleRowChangedEventHandler(object sender, CModuleBase ModuleInfo);

        /// <summary>
        /// User selected another Module Row
        /// </summary>
        public event ModuleRowChangedEventHandler? ModuleRowChanged;
        protected virtual void OnModuleRowChanged(CModuleBase ModuleInfo)
        {
            if (ModuleInfo != null)
            {
                ModuleRowChanged?.Invoke(this, ModuleInfo);
            }
        }

        public delegate void SWChannelRowChangedEventHandler(object sender, CSWChannel SelectedSWChannel);

        /// <summary>
        /// User selected another SWChannel Row
        /// </summary>
        public event SWChannelRowChangedEventHandler SWChannelRowChanged;
        protected virtual void OnSWChannelRowChanged(CSWChannel? SelectedSWChannel)
        {
            if (SelectedSWChannel != null)
            {
                SWChannelRowChanged?.Invoke(this, SelectedSWChannel);
            }
        }

        #endregion


        private void UpdateDatabinding()
        {
            if (_ModuleInfos != null)
            {
                cModuleInfoBindingSource.DataSource = _ModuleInfos;
                cModuleInfoDataGridView.Update();
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            cModuleInfoDataGridView.Refresh();
            sWChannelsDataGridView.Refresh();
            UpdateInfo();
        }

        public CModuleBase SelectedModuleRow { get; private set; }

        public CSWChannel? SelectedSWChannel { get; private set; }

        private void CModuleInfoDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (cModuleInfoDataGridView.SelectedRows.Count > 0)
            {
                if (cModuleInfoDataGridView?.SelectedRows != null &&
                    cModuleInfoDataGridView.SelectedRows.Count > 0 &&
                    cModuleInfoDataGridView.SelectedRows[0]?.DataBoundItem is CModuleBase module)
                {
                    SelectedModuleRow = module;
                }
                UpdateInfo();
                UpdateModuleSpecificInfo(SelectedModuleRow);

                OnModuleRowChanged(SelectedModuleRow);


                if (cModuleInfoDataGridView is not null && cModuleInfoDataGridView.RowHeadersDefaultCellStyle is not null)
                {
                    if (_ModuleInfos[e.RowIndex].ModuleBootloaderError)
                    {
                        cModuleInfoDataGridView.RowHeadersDefaultCellStyle.SelectionBackColor = colError;
                    }
                    else
                    {
                        cModuleInfoDataGridView.RowHeadersDefaultCellStyle.SelectionBackColor = _ModuleInfos[e.RowIndex].ModuleColor;
                    }

                    cModuleInfoDataGridView.DefaultCellStyle.SelectionBackColor = cModuleInfoDataGridView.RowHeadersDefaultCellStyle.SelectionBackColor;
                }
            }
        }

        private void SWChannelsDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            if (rowIndex >= 0 && rowIndex < sWChannelsDataGridView.Rows.Count)
            {
                SelectedSWChannel = sWChannelsDataGridView.Rows[rowIndex]?.DataBoundItem as CSWChannel;
                UpdateInfo();
                OnSWChannelRowChanged(SelectedSWChannel);

            }
        }

        private void SaveCurrentValuesAsDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Default_Scaling_Values default_Scaling_Values = new();
            if (!SelectedModuleRow.ModuleBootloaderError)
            {
                if (SelectedModuleRow.ModuleType != enumModuleType.cModuleTypeEmpty)
                {
                    for (int sw_cn = 0; sw_cn < SelectedModuleRow.NumSWChannels; sw_cn++)
                    {
                        default_Scaling_Values.SetScalingValues(SelectedModuleRow.SWChannels[sw_cn].SkalMax,
                            SelectedModuleRow.SWChannels[sw_cn].SkalMin,
                            SelectedModuleRow.ModuleType, sw_cn);
                    }
                }
            }
            default_Scaling_Values.SaveConfigData();
        }

        ucModuleSpecificSetup_AtemIR ucModuleSpecificSetup_AtemIR = new();
        ucModuleSpecificSetup_VasoIR ucModuleSpecificSetup_VasoIR = new();
        ucModuleSpecificSetup_MultiSensor ucModuleSpecificSetup_MultiSensor = new();
        ucModuleExGADS_Impedance ucModuleExGADS_Impedance = new();
        ucModuleEEG? ucModuleEEG = new();


        private void UpdateInfo()
        {
            if ((SelectedModuleRow != null) && (SelectedSWChannel != null))
            {
                string s1 = "";
                s1 += "MidofRange: " + SelectedSWChannel.MidofRange.ToString() + Environment.NewLine;
                s1 += "Offset_d: " + SelectedSWChannel.Offset_d.ToString() + Environment.NewLine;
                s1 += "Offset_hex: " + SelectedSWChannel.Offset_hex.ToString() + Environment.NewLine;
                s1 += "SkalValue_k: " + SelectedSWChannel.SkalValue_k.ToString();
                txtInfo2.Text = s1;

                string s2 = "";
                s2 += "Module_Revision: " + SelectedModuleRow.ModuleRevision.ToString() + Environment.NewLine;
                s2 += "SWRevision: " + SelectedModuleRow.SWRevision_string + Environment.NewLine;
                s2 += "uuid: " + SelectedModuleRow.UUID.ToString() + Environment.NewLine;
                txtInfo1.Text = s2;
            }
        }

        public void UpdateModuleSpecificInfo(CModuleBase ModuleInfo)
        {
            //Calculate Position of Module specific box
            int Left = sWChannelsDataGridView.Left;
            int Top = sWChannelsDataGridView.Top + sWChannelsDataGridView.Height + 5;
            int Width = sWChannelsDataGridView.Width;

            if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleAtemIRDig)
            {
                if (ucModuleSpecificSetup_AtemIR == null)
                {
                    ucModuleSpecificSetup_AtemIR = new ucModuleSpecificSetup_AtemIR();
                    Controls.Add(ucModuleSpecificSetup_AtemIR);
                    ucModuleSpecificSetup_AtemIR.Location = new Point(Left, Top);
                    ucModuleSpecificSetup_AtemIR.Name = "ucModuleSpecificSetup_AtemIR";
                    ucModuleSpecificSetup_AtemIR.Width = Width;
                }
                ucModuleSpecificSetup_AtemIR.UpdateModuleInfo((CModuleRespI)ModuleInfo);
                ucModuleSpecificSetup_AtemIR.Visible = true;
            }
            else
            {
                if (ucModuleSpecificSetup_AtemIR != null)
                    ucModuleSpecificSetup_AtemIR.Visible = false;
            }

            if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleVasosensorDig)
            {
                if (ucModuleSpecificSetup_VasoIR == null)
                {
                    ucModuleSpecificSetup_VasoIR = new ucModuleSpecificSetup_VasoIR();
                    Controls.Add(ucModuleSpecificSetup_VasoIR);
                    ucModuleSpecificSetup_VasoIR.Location = new Point(Left, Top);
                    ucModuleSpecificSetup_VasoIR.Name = "ucModuleSpecificSetup_VasoIR";
                    ucModuleSpecificSetup_VasoIR.Width = Width;
                }
                ucModuleSpecificSetup_VasoIR.UpdateModuleInfo((CModuleVasoIR)ModuleInfo);
                ucModuleSpecificSetup_VasoIR.Visible = true;
            }
            else
            {
                if (ucModuleSpecificSetup_VasoIR != null)
                    ucModuleSpecificSetup_VasoIR.Visible = false;
            }


            if ((ModuleInfo.ModuleType == enumModuleType.cModuleMultisensor) ||
                (ModuleInfo.ModuleType == enumModuleType.cModuleVaso))
            {
                if (ucModuleSpecificSetup_MultiSensor == null)
                {
                    ucModuleSpecificSetup_MultiSensor = new ucModuleSpecificSetup_MultiSensor();
                    Controls.Add(ucModuleSpecificSetup_MultiSensor);


                    ucModuleSpecificSetup_MultiSensor.Location = new System.Drawing.Point(Left, Top);
                    ucModuleSpecificSetup_MultiSensor.Name = "ucModuleSpecificSetup_MultiSensor";
                    ucModuleSpecificSetup_MultiSensor.Width = Width;
                }
                ucModuleSpecificSetup_MultiSensor.UpdateModuleInfo((FeedbackDataLib.Modules.CModuleMultisensor)ModuleInfo);
                ucModuleSpecificSetup_MultiSensor.Visible = true;
            }
            else
            {
                if (ucModuleSpecificSetup_MultiSensor != null)
                    ucModuleSpecificSetup_MultiSensor.Visible = false;
            }

            if (ModuleInfo.ModuleType == enumModuleType.cModuleEEG)
            {
                //Also Update skal Vals
                int hw_cn = ModuleInfo.HWcn;
                for (int sw_cn = 0; sw_cn < ModuleInfo.SWChannels.Count; sw_cn++)
                {
                    _ModuleInfos[hw_cn].SWChannels[sw_cn].Offset_d = ModuleInfo.SWChannels[sw_cn].Offset_d;
                    _ModuleInfos[hw_cn].SWChannels[sw_cn].Offset_hex = ModuleInfo.SWChannels[sw_cn].Offset_hex;
                    _ModuleInfos[hw_cn].SWChannels[sw_cn].SkalValue_k = ModuleInfo.SWChannels[sw_cn].SkalValue_k;
                }

                if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleExGADS94)
                {
                    if (ucModuleExGADS_Impedance == null)
                    {
                        ucModuleExGADS_Impedance = new ucModuleExGADS_Impedance();
                        Controls.Add(ucModuleExGADS_Impedance);


                        ucModuleExGADS_Impedance.Location = new Point(Left, Top);
                        ucModuleExGADS_Impedance.Name = "ucModuleExGADS_Impedance";
                        ucModuleExGADS_Impedance.Width = Width;
                    }
                    ucModuleExGADS_Impedance.Visible = true;

#if DEBUG
                    //if (frmModuleSpecificSetup_ExGADS == null)
                    //{
                    //    frmModuleSpecificSetup_ExGADS = new frmModuleSpecificSetup_ExGADS1292
                    //    {
                    //        Name = "frmModuleSpecificSetup_ExGADS"
                    //    };
                    //}
                    //frmModuleSpecificSetup_ExGADS.SetModuleSpecificInfo((CModuleExGADS1292)ModuleInfo);
                    //frmModuleSpecificSetup_ExGADS.Visible = true;
                    //((CModuleExGADS1294)ModuleInfo).visible = true;

#endif
                }
                else if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleExGADS94)
                {

                }

                else if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleEEG)
                {
                    if (ucModuleEEG == null)
                    {
                        ucModuleEEG = new ucModuleEEG();
                        Controls.Add(ucModuleEEG);

                        ucModuleEEG.Location = new System.Drawing.Point(Left, Top);
                        ucModuleEEG.Name = "ucModuleEEG";
                        ucModuleEEG.Width = Width;
                    }
                    ucModuleEEG.Visible = true;
                }
            }
            else
            {
                //ucModuleExGADS_Impedance?.Visible = false;

                if (ucModuleExGADS_Impedance != null)
                    ucModuleExGADS_Impedance.Visible = false;

                if (ucModuleEEG != null)
                    ucModuleEEG.Visible = false;

                //if (frmModuleSpecificSetup_ExGADS != null)
                //{
                //    frmModuleSpecificSetup_ExGADS.Visible = false;
                //}

                //if (ModuleInfo is CModuleExGADS1292)
                //{
                //    ((CModuleExGADS1292)ModuleInfo).visible = false;
                //}
                //else if (ModuleInfo is CModuleEEG)
                //{
                //    ((CModuleEEG)ModuleInfo).visible = false;
                //}
            }
        }

        public void Update_ucModuleExGADS_Impedance(CADS1294x_ElectrodeImp mi)
        {
            ucModuleExGADS_Impedance?.SetImpedanceBoxes(mi);
        }

        public int GetSR_ms(int chNo)
        {
            if (ucModuleExGADS_Impedance != null)
                return ucModuleExGADS_Impedance.GetSR_ms(chNo);
            return 200;
        }



        public void ReadModuleSpecificInfo(ref CModuleBase ModuleInfo)
        {
            if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleAtemIRDig)
            {
                if (ucModuleSpecificSetup_AtemIR != null)
                {
                    CModuleRespI mr = (CModuleRespI)ModuleInfo;
                    ucModuleSpecificSetup_AtemIR.ReadModuleSpecificInfo(ref mr);
                    ModuleInfo = mr;
                }
            }

            else if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleVasosensorDig)
            {
                CModuleVasoIR mr = (CModuleVasoIR)ModuleInfo;
                ucModuleSpecificSetup_VasoIR.ReadModuleSpecificInfo(ref mr);
                ModuleInfo = mr;
            }

            else if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleMultisensor ||
                ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleVaso)
            {
                CModuleMultisensor mr = (CModuleMultisensor)ModuleInfo;
                ucModuleSpecificSetup_MultiSensor.ReadModuleSpecificInfo(ref mr);
                ModuleInfo = mr;
            }

            else if (ModuleInfo.ModuleType_Unmodified == enumModuleType.cModuleExGADS94)
            {
                //frmModuleSpecificSetup_ExGADS.ReadModuleSpecificInfo(ref ModuleInfo);
            }


        }
    }
}
