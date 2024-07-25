using ComponentsLib_GUI;
using ExcelDataReader;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using System.Data;
using WindControlLib;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    /// <summary>
    /// Class to connect to Neuromaster
    /// </summary>
    /// <seealso cref="frmInsight_Manufacturing5.tests_measurements.uc_Base_tests_measurements" />
    public class CRead_Neuromaster : CBase_tests_measurements
    {
        /***************************************/
        //Testbedingungen bei Übersteuerung
        /***************************************/
        /// <summary>We consider it overload if more that overload_vals_per_peak are above or below thresholds</summary>
        public static int overload_vals_per_peak = 3;
        /// <summary>A measured value above overload_threshold_max is counted as overload</summary>
        public static int overload_threshold_max = 0xFFFD;
        /// <summary>A measured value below overload_threshold_min is counted as overload</summary>
        public static int overload_threshold_min = 0x0003;

        #region Events
        public event DataReady_xy_EventHandler DataReady_xy;

        public delegate void DataReady_xy_EventHandler(object sender, double y_calibrated, WindControlLib.CDataIn DataIn);
        public virtual void OnDataReady(double y_calibrated, CDataIn DataIn)
        {
            DataReady_xy?.Invoke(this, y_calibrated, DataIn);
        }

        //Event to Scale Chart
        public event ModuleInfoAvailable_EventHandler ModuleInfoAvailable;

        public delegate void ModuleInfoAvailable_EventHandler(object sender, CModuleBase ModuleInfo);
        protected virtual void OnModuleInfoAvailable(CModuleBase ModuleInfo)
        {
            ModuleInfoAvailable?.Invoke(this, ModuleInfo);
        }

        //Event for Amplitude Gain
        public event UdpateAmplitudeGain_EventHandler UdpateAmplitudeGain;

        public delegate void UdpateAmplitudeGain_EventHandler(object sender, double f, double v_db, double ueff_out);
        protected virtual void OnUdpateAmplitudeGain(double f, double v_db, double ueff_out)
        {
            UdpateAmplitudeGain?.Invoke(this, f, v_db, ueff_out);
        }

        //Event to populate id_neuromodule_kalibrierdaten
        public event Udpate_id_neuromodule_kalibrierdaten_EventHandler Udpate_id_neuromodule_kalibrierdaten;

        public delegate void Udpate_id_neuromodule_kalibrierdaten_EventHandler(object sender, Guid id_neuromodule_kalibrierdaten);
        protected virtual void OnUdpate_id_neuromodule_kalibrierdaten(Guid id_neuromodule_kalibrierdaten)
        {
            Udpate_id_neuromodule_kalibrierdaten?.Invoke(this, id_neuromodule_kalibrierdaten);
        }


        #endregion

        #region Properties
        public FeedbackDataLib.C8KanalReceiverV2 DataReceiver;
        public C8KanalReceiverV2.enumConnectionResult lastConRes;

        protected CFY6900 _FY6900;

        /// <summary>
        /// Channel number where module is connected to
        /// </summary>
        public int ModulePortNo { get; set; } = 0;

        //private List<COutput_Settings> _Output_Settings;
        //public List<COutput_Settings> AgainValues { get => _Output_Settings; set => _Output_Settings = value; }
        public string Path_to_save_xml { get; set; } = "";
        public bool DataConnection_Required { get; set; } = true;
        public int CntSamplesCollected { get; set; } = 0;
        public int WaitToSettle_ms { get; set; } = 2000;
        public int CntSettings { get; set; } = 0;
        public int NumSamplestoCollect { get; set; } = 0;

        public bool[] SendChannels;
        public int[] SampleInt_of_channels_ms;
        public double[] ChartMaximum;
        public double[] ChartMinimum;

        public bool AcceptData = false;

        public double Uoffset_mV = 0;

        public List<CNMChannelResults> AllResults;      //According to how many measures are done

        public Guid last_id_neuromodule_kalibrierdaten;

        public CAgainValues AgainValues = null;
        #endregion

        public CRead_Neuromaster(CFY6900 FY6900)
        {
            Job_Message = "Read_Neuromaster_base";
            Uoffset_mV = 0;
            _FY6900 = FY6900;
        }


        /// <summary>
        /// Handles the Load event of the uc_Neuromodules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Neuromodules_Load(object sender, EventArgs e)
        {
        }

        /// <summary>Performs the measurement.</summary>
        /// <param name="SaveToDB">if set to <c>true</c> [save to database].</param>
        /// <returns> true if success
        ///   <br />
        /// </returns>
        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            const int GetDeviceConfig_Repeats = 10;
            bool isOK = true;
            base.Perform_Measurement(ignore_serial_number_check);//calls OnBeforeMeasurementStarts

            try
            {
                if (DataConnection_Required)
                {
                    if (Connect_DataReceiver(false) == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                    {
                        bool BootloaderError = true;
                        int cntGetConfig = 0;
                        while (BootloaderError && cntGetConfig < GetDeviceConfig_Repeats)
                        {
                            DataReceiver.Connection.GetDeviceConfig();
                            BootloaderError = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].ModuleBootloaderError;
                            if (BootloaderError)
                            {
                                OnReportMeasurementProgress("Try to GetConfig cnt: " + cntGetConfig.ToString(), Color.Orange);
                                CDelay.Delay_ms_DoEvents(500);
                                cntGetConfig++;
                            }
                        }

                        if ((cntGetConfig < GetDeviceConfig_Repeats) && (DataReceiver.Connection.Device.ModuleInfos != null) && !BootloaderError)
                        {
                            if (DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].ModuleType == ConnectedModuleType)
                            {
                                string ser = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].uuid;
                                ser = ser.Replace("\0", string.Empty);
                                ser = ser.Replace(" ", string.Empty);
                                ser = ser.Replace("ty", string.Empty);
                                if (ser == SerialNumber || ignore_serial_number_check)
                                {
                                    ser = SerialNumber;
                                    //Setup Module
                                    Setup_Channels();
                                    if (DataReceiver.Connection.SetConfigModule(ModulePortNo))
                                    {
                                        for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels.Count; i++)
                                        {
                                            DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SkalMax = ChartMaximum[i];
                                            DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SkalMin = ChartMinimum[i];
                                        }
                                        OnModuleInfoAvailable(DataReceiver.Connection.Device.ModuleInfos[ModulePortNo]);
                                        //All Ready
                                        AllResults = new List<CNMChannelResults>();

                                        if (AgainValues != null && AgainValues.Count > 0)
                                        {
                                            DataReceiver.Connection.DataReady -= DataReceiver_DataReady;
                                            DataReceiver.Connection.DataReady += DataReceiver_DataReady;
                                            DataReceiver.Connection.SetConfigModule(ModulePortNo);
                                            DataReceiver.Connection.EnableDataReadyEvent = true;

                                            //AllResults vorher herrichten sonst gibts im Thread Probleme
                                            AllResults.Clear();

                                            for (int i = 0; i < AgainValues.Count; i++)
                                            {
                                                AllResults.Add(new CNMChannelResults());
                                            }

                                            for (int i = 0; i < AllResults.Count; i++)
                                            {
                                                int cnt_visible = 0;
                                                for (int j = 0; j < SendChannels.Length; j++)
                                                {
                                                    if (SendChannels[j])
                                                    {
                                                        double a = AgainValues.noSamplesIgnored_in_the_Beginning[i];
                                                        //An Abtastfrequnz anpassen
                                                        a = a * SampleInt_of_channels_ms[0] / SampleInt_of_channels_ms[j];
                                                        AllResults[i].ChannelResults[cnt_visible].noSamplesIgnored_in_the_Beginning = (int)a;

                                                        /*
                                                        if (AllResults.Ignore_Time_percent. != null)
                                                        {
                                                            double d = (double)Sample_Ignore_Time_percent[j] / 100 * (double)AgainValues.AgainValues[i].MeasureTime_s; //time to ignore
                                                            d = d / SampleInt_of_channels[j] * 1000;
                                                            AllResults[i].ChannelResults[cnt_visible].noSamplesIgnored_in_the_Beginning = (int)d;

                                                        }
                                                        else
                                                        {
                                                            if (Samples_To_ignore[j] != null)
                                                                AllResults[i].ChannelResults[cnt_visible].noSamplesIgnored_in_the_Beginning = (int)Samples_To_ignore[j];
                                                        }*/
                                                        cnt_visible++;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //OnReportMeasurementProgress("No Information for FY6900 provided", Color.Red);
                                            OnReportMeasurementProgress("No Test-Values provided", Color.Red);
                                            isOK = false;
                                        }
                                    }
                                    else
                                    {
                                        OnReportMeasurementProgress("Could not set Config!", Color.Red);
                                        isOK = false;
                                    }
                                }
                                else
                                {
                                    OnReportMeasurementProgress("Seriennummer stimmt nicht überein. Modul: " + DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].uuid, Color.Red);
                                    isOK = false;
                                }
                            }
                            else
                            {
                                OnReportMeasurementProgress("Wrong Module Connected! -> " +
                                    DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].ModuleType.ToString(), Color.Red);
                                isOK = false;
                            }
                        }
                        else
                        {
                            OnReportMeasurementProgress("Could not get Config!", Color.Red);
                            isOK = false;
                        }
                    }
                }
                if (isOK)
                    isOK = Perform_Specific_Measurement();
            }
            finally
            {
                DataReceiver?.Close_All();
            }

            return isOK;
        }

        public C8KanalReceiverV2.enumConnectionResult? Connect_DataReceiver(bool CloseAfterwards)
        {
            if (DataReceiver == null)
            {
                DataReceiver = new C8KanalReceiverV2();
            }

            OnReportMeasurementProgress("Searching for Neurolink  ....", Color.Green);
            C8KanalReceiverV2.enumConnectionResult? conres = DataReceiver.Init_via_D2XX();

            if (conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
            {
                OnReportMeasurementProgress("Neurolink: " + DataReceiver.NeurolinkSerialNumber, Color.Green);
                C8KanalReceiverV2.enumConnectionResult LastConnectionResult = DataReceiver.Connect();
                if (LastConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                {
                    if (DataReceiver.Connection.ScanModules())
                    {
                        OnReportMeasurementProgress("ScanModules OK", Color.Green);
                    }
                    else
                    {
                        OnReportMeasurementProgress("Scan Modules failed", Color.Red);
                    }
                } //if (LastConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                else
                {
                    OnReportMeasurementProgress("Scan Modules failed", Color.Red);
                }
            }
            else if (conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_XBee)
            {
                OnReportMeasurementProgress("Neurolink: " + DataReceiver.NeurolinkSerialNumber, Color.Green);
                C8KanalReceiverV2.enumConnectionResult LastConnectionResult = DataReceiver.Connect();
                if (LastConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_XBee)
                {
                    if (DataReceiver.Connection.ScanModules())
                    {
                        OnReportMeasurementProgress("ScanModules OK", Color.Green);
                    }
                    else
                    {
                        OnReportMeasurementProgress("Scan Modules failed", Color.Red);
                    }
                } //if (LastConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                else
                {
                    OnReportMeasurementProgress("Scan Modules failed", Color.Red);
                    conres = C8KanalReceiverV2.enumConnectionResult.Error_during_XBee_connection;   //18.9.2023
                }
            }
            else
            {
                if (conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_XBee)
                {
                    OnReportMeasurementProgress("Keine Verbindung zum Neuromaster. Eingeschaltet??", Color.Red);
                }
                else
                {
                    OnReportMeasurementProgress(conres.ToString(), Color.Red);
                }
            }

            if (CloseAfterwards)
                DataReceiver.Close_All();

            return conres;
        }

        /// <summary>
        /// Setups the Channels - which to receive ... 
        /// </summary>
        public virtual void Setup_Channels()
        {
            CModuleBase ModuleInfo = (CModuleBase)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].Clone();
            //for (int i = 0; i < ModuleInfo.SWChannels_Module.Count; i++)
            for (int i = 0; i < ModuleInfo.SWChannels.Count; i++)
            {
                ModuleInfo.SWChannels[i].SendChannel = SendChannels[i];
                ModuleInfo.SWChannels[i].SampleInt = (ushort)SampleInt_of_channels_ms[i];
            }
            DataReceiver.Connection.Device.ModuleInfos[ModulePortNo] = ModuleInfo;
        }

        private bool StartFunctionGenerator(CAgainValue cFY6900_Output_Setting)
        {
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 2, 0); //2s Timeout

#if DEBUG1
            return true; //DEBUG!!!!!
#warning ("Funktionsgenerator deaktiviert")
#endif
            while (!_FY6900.Open())
            {
                Application.DoEvents();
                if (DateTime.Now > dt)
                    return false;
            }
            return cFY6900_Output_Setting.SetGenerator(_FY6900);
        }

        /// <summary>
        /// That this class can also be used by Multisensor
        /// </summary>
        /// <returns>true if success</returns>
        public virtual bool Perform_Specific_Measurement()
        {
            bool OK = false;

            //Wait to settle
            if (WaitToSettle_ms > 0)
            {
                OnReportMeasurementProgress("Waiting " + ((int)(WaitToSettle_ms / 1000)).ToString() + "s to settle ....", Color.Black);
                CDelay.Delay_ms_DoEvents(WaitToSettle_ms);
            }

            for (CntSettings = 0; CntSettings < AgainValues.AgainValues.Count; CntSettings++)
            {
                UpdateTestBoard(); //Just to be safe - if the Phidget resetted in between
                CAgainValue setting = AgainValues.AgainValues[CntSettings];
                if (StartFunctionGenerator(setting))
                {
                    NumSamplestoCollect = (int)(setting.MeasureTime_s / DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[0].SampleInt * 1000.0);
                    CntSamplesCollected = 0;

                    double Veff = setting.Vppin / 2 / Math.Sqrt(2);
                    string msg = "Measuring: f=" + setting.f_Hz.ToString() + "Hz @ Vppin= " + setting.Vppin.ToString("G2") + " / Veffin = " + Veff.ToString("G2") + " => Veffout_soll = " + setting.Ueff_soll.ToString("G2");
                    if (setting.ArbitraryFile != "")
                    {
                        msg += "V, File: " + setting.ArbitraryFile;
                    }
                    OnReportMeasurementProgress(msg, Color.Blue);

                    //Now we are waiting for Data to receive
                    Wait4Data();
                    if (ConnectedModuleType == enumModuleType.cModuleEEG)
                    {
                        if (CntSettings < AllResults.Count && CntSettings < AgainValues.Ueff_EEG_soll.Length)
                        {
                            msg = "Ueff_ist= " + AllResults[CntSettings].ChannelResults[0].Ueff.ToString("0.##E+00") +
                                    " -- Ueff_soll= " + AgainValues.Ueff_EEG_soll[CntSettings].ToString("0.##E+00");

                            if (AgainValues != null)
                            {
                                msg += ";  v[db] soll= " + AgainValues.AgainValues[CntSettings].Again_soll_db.ToString("0.##") +
                                " -- v[db] ist=  " + Get_v0_dB().ToString("0.##");
                            }
                            OnReportMeasurementProgress(msg, Color.Black);
                        }
                    }
                    OK = true;
                }
                else
                {
                    OnReportMeasurementProgress("Could not set FY6900", Color.Red);
                    OK = false;
                    break;
                }
            }
            return OK;
        }

        protected virtual double Get_v0_dB()
        {
            return 0;
        }

        public virtual void Wait4Data()
        {
            //Now we are waiting for Data to receive
            //Time to collect samples
            //Empty Receiving Buffer
            Application.DoEvents();
            DateTime dt = DateTime.Now;
            AcceptData = true;

            while (CntSamplesCollected < NumSamplestoCollect)
            {
                Application.DoEvents();
            }
            AcceptData = false;
            TimeSpan ts = DateTime.Now - dt;
            OnReportMeasurementProgress("Sampled for: " + ((int)ts.TotalMilliseconds).ToString() + "ms", Color.Brown);
        }


        private void DataReceiver_DataReady(object sender, List<WindControlLib.CDataIn> DataRead)
        {
            if (AcceptData)
            {
                if (CntSamplesCollected < NumSamplestoCollect)
                {
                    foreach (CDataIn di in DataRead)
                    {
                        Process_NM_Data(di);
                    }
                }
            }
        }

        public virtual void Process_NM_Data(CDataIn DataIn, int swcn = 0)
        {
            if (DataIn.HWChannelNumber == ModulePortNo)
            {
                double d = DataReceiver.Connection.GetScaledValue(DataIn);

                if (AllResults != null)
                {
                    if (CntSettings < AllResults.Count)
                    {
                        if (DataIn.SWChannelNumber == swcn) CntSamplesCollected++;
                        AllResults[CntSettings].ChannelResults[DataIn.SWChannelNumber].AddValue(d, DataIn);
                    }
                }
                OnDataReady(d, DataIn);
            }
        }

        public override void Save_Results_to_DB()
        {
            Save_Results_to_DB(AllResults);
        }


        public void Save_Results_to_DB(List<CNMChannelResults> AllResults)
        {
            DateTime dt_created = DateTime.Now;
            last_id_neuromodule_kalibrierdaten = Guid.NewGuid();

            Save_Calibration_Values(last_id_neuromodule_kalibrierdaten);

            dsManufacturing _dsManufacturing = new dsManufacturing();
            dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter = new dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter();

            //Save Measurement Data
            for (int i = 0; i < AgainValues.Count; i++)
            {
                CAgainValue setting = AgainValues.AgainValues[i];
                dsManufacturing.Neuromodule_DatenRow nrow = _dsManufacturing.Neuromodule_Daten.NewNeuromodule_DatenRow();
                Add_MeasuementResults_to_Row(ref nrow, AllResults[i], AgainValues.AgainValues[i]);

                nrow.f = setting.f_Hz;
                nrow.Ueffin_V = Get_Ueff_soll(setting.Vppin, ConnectedModuleType);
                nrow.Testdatum = dt_created;
                nrow.id_neuromodule_daten = Guid.NewGuid();
                nrow.id_neuromodule_kalibrierdaten = last_id_neuromodule_kalibrierdaten;
                nrow.SerialNumber = SerialNumber;
                nrow.Bemerkung = my_name;

                _dsManufacturing.Neuromodule_Daten.Rows.Add(nrow);
            }
            neuromodule_DatenTableAdapter.Update(_dsManufacturing.Neuromodule_Daten);
        }

        public void Save_Calibration_Values(Guid id_neuromodule_kalibrierdaten)
        {
            dsManufacturing _dsManufacturing = new dsManufacturing();
            dataSources.dsManufacturingTableAdapters.Neuromodule_KalibrierdatenTableAdapter neuromodule_KalibrierdatenTableAdapter = new dataSources.dsManufacturingTableAdapters.Neuromodule_KalibrierdatenTableAdapter();

            //Save CalibrationVals
            int i = 0;
            dsManufacturing.Neuromodule_KalibrierdatenRow kalibrow = _dsManufacturing.Neuromodule_Kalibrierdaten.NewNeuromodule_KalibrierdatenRow();
            kalibrow.ADResolution = (short)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.ADResolution;
            kalibrow.uref = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.uref;


            kalibrow.MidofRange_0 = (short)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.MidofRange;
            kalibrow.Offset_d_0 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_d;
            kalibrow.Offset_hex_0 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_hex;
            kalibrow.SkalValue_k_0 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.SkalValue_k;

            if (DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels.Count >= 1)
            {
                i = 1;
                kalibrow.MidofRange_1 = (short)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.MidofRange;
                kalibrow.Offset_d_1 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_d;
                kalibrow.Offset_hex_1 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_hex;
                kalibrow.SkalValue_k_1 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.SkalValue_k;
            }

            if (DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels.Count >= 2)
            {
                i = 2;
                kalibrow.MidofRange_2 = (short)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.MidofRange;
                kalibrow.Offset_d_2 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_d;
                kalibrow.Offset_hex_2 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_hex;
                kalibrow.SkalValue_k_2 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.SkalValue_k;
            }

            if (DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels.Count >= 3)
            {
                i = 3;
                kalibrow.MidofRange_3 = (short)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.MidofRange;
                kalibrow.Offset_d_3 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_d;
                kalibrow.Offset_hex_3 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.Offset_hex;
                kalibrow.SkalValue_k_3 = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.SkalValue_k;
            }

            kalibrow.uref = DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo.uref;
            kalibrow.id_neuromodule_kalibrierdaten = id_neuromodule_kalibrierdaten;
            kalibrow.SerialNumber = SerialNumber;

            kalibrow.Bemerkung = my_name;
            _dsManufacturing.Neuromodule_Kalibrierdaten.Rows.Add(kalibrow);

            neuromodule_KalibrierdatenTableAdapter.Update(_dsManufacturing.Neuromodule_Kalibrierdaten);
            OnUdpate_id_neuromodule_kalibrierdaten(id_neuromodule_kalibrierdaten);
        }

        public virtual void Add_MeasuementResults_to_Row(
            ref dsManufacturing.Neuromodule_DatenRow nrow,
            CNMChannelResults Results,
            CAgainValue setting = null)
        {
            Additional_DB_Values(setting, Results, ref nrow);
            CNMChannelResult.CVals_Statistics scs0 = Results.ChannelResults[0].Get_scaledVals_Statistics();
            nrow.Ueffout_V = scs0.ueff;
            nrow.Umeanout_V = scs0.umean;
            nrow.Uglmittelwert_V = Results.ChannelResults[0].Urectified_mean;
            if (setting != null)
                nrow.Uoffin_V = setting.Offset_V;
        }

        protected virtual void Additional_DB_Values(CAgainValue setting,
            CNMChannelResults Results, ref dsManufacturing.Neuromodule_DatenRow nrow)
        {
        }

        public override enModuleTestResult isModule_OK()
        {
            ModuleTestResult = enModuleTestResult.OK;
            Test_Details = "";

            if (AllResults != null && AllResults.Count > 0)
            {
                //Do some prechecking
                foreach (CNMChannelResults cress in AllResults)
                {
                    CNMChannelResult cres = cress.ChannelResults[0];
                    CNMChannelResult.CVals_Statistics stat = cres.Get_hexVals_Statistics();
                    if (stat.u_maximum == stat.u_minimum)
                        ModuleTestResult = enModuleTestResult.Fail_no_further_processing;
                    if (stat.isSaturated)
                        ModuleTestResult = enModuleTestResult.Partially_Failed;
                }
            }
            return ModuleTestResult;
        }

        protected enModuleTestResult EvaluateAGainValues_Ueff_Percent_Saturation(CAgainValues againValues, List<CNMChannelResults> chrs)
        {
            bool ret = true;
            ModuleTestResult = enModuleTestResult.OK;
            for (int i = 0; i < AgainValues.Count; i++)
            {
                ret &= EvaluateAGainValue_Ueff_Percent_Saturation(againValues.AgainValues[i], chrs[i].ChannelResults[0]);
            }
            if (ret == false)
            {
                ModuleTestResult = enModuleTestResult.Fail;
            }
            return ModuleTestResult;
        }

        protected bool EvaluateAGainValue_Ueff_Percent_Saturation(CAgainValue againValue, CNMChannelResult ch)
        {
            double Ueff_ist = ch.Ueff;
            string ovl_detected = "";

            bool ret = againValue.isValueOK_Ueff_Percent(Ueff_ist);

            if (againValue.MeasureType == enMeasureType.Notch)
            {
                Test_Details += "Notch: ";
            }
            else if (againValue.MeasureType == enMeasureType.Sinus)
            {
                Test_Details += "Sinus: ";
                //Test auf Übersteuerung - wenn nicht Notch
                CNMChannelResult.CVals_Statistics stat_hex = ch.Get_hexVals_Statistics();
                if (stat_hex.isSaturated)
                {
                    //Overload detected
                    ovl_detected = "- Saturation detected";
                    ret = false;
                }
            }
            else
                return ret;

            string result_string = againValue.f_Hz.ToString("G0") + "Hz/ " + againValue.Vppin.ToString("G2") + "Vpp -> ";
            result_string += "soll / ist  = " + againValue.Ueff_soll.ToString("G2") + " / " + Math.Abs(ch.Ueff).ToString("G2") +
                " -> " + Get_TestString(ret)
                + " ~ Tol= " + againValue.Tolerance_percent.ToString("##.0") + "% " + ovl_detected + ";";
            Test_Details += result_string;

            Color col = Color.Red;
            if (ret) col = Color.Green;
            OnReportMeasurementProgress(result_string, col);

            return ret;
        }

        public virtual List<CSWChannelInfo> Get_Calibrated_SWChannelInfo()
        {
            if (AllResults != null)
            {
                List<CSWChannelInfo> swi = new List<CSWChannelInfo>();
                for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels.Count; i++)
                {
                    swi.Add(DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[i].SWChannelInfo);
                }
                return Get_SWChannelInfo(swi, AllResults);  //Gets new calibrtion values
            }
            return null;
        }

        public virtual List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            CSWChannelInfo[] ret = new CSWChannelInfo[OldSWChannelInfos.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (CSWChannelInfo)OldSWChannelInfos[i].Clone();
            }
            return new List<CSWChannelInfo>(ret);
        }

        /// <summary>Determines whether the specified hexadecimal vals is overload.</summary>
        /// <param name="hexVals">The hexadecimal vals.</param>
        /// <param name="numPeaks">The number peaks.</param>
        /// <returns>
        ///   <c>true</c> if channel is saturated; otherwise, <c>false</c>.</returns>
        /*
        protected bool isSaturated(CNMChannelResult hexVals, int numPeaks) //, double Value_soll_Max, double Value_soll_Min, double PlusTolerance_Percent, double MinusTolerane_Percent)
        {
            bool ret = false;

            //Änderung in V3.1.0 6.8.2019
            //Wenn 3 Werte pro Peak größer als 0xFFFD sind oder kleiner als 0x0003 wird Übersteuerung erkannt
            int numValsOverload = overload_vals_per_peak * numPeaks;

            int cnt_maxovl = 0, cnt_minovl = 0;
            for (int i = 0; i < hexVals.hexvals.Length; i++)
            {
                if (hexVals.hexvals[i] >= overload_threshold_max)
                {
                    cnt_maxovl++;
                }
                else if (hexVals.hexvals[i] <= overload_threshold_min)
                {
                    cnt_minovl++;
                }
            }
            if ((cnt_maxovl >= numValsOverload) || (cnt_minovl >= numValsOverload))
                ret = true;

            return ret;
        }*/

        public static bool Check_Value_Perecent(double Value_ist, double Value_soll, double PlusTolerance_Percent, double MinusTolerane_Percent)
        {
            bool ret = false;
            {
                double max = Value_soll * (1 + PlusTolerance_Percent / 100);
                double min = Value_soll * (1 - MinusTolerane_Percent / 100);
                if ((Value_ist < max) && (Value_ist > min))
                    ret = true;
            }
            return ret;
        }


        #region Logfile
        private StreamWriter sw_logFile = null;

        protected void OpenLogFile(string BasicFileName)
        {
            try
            {
                string pa = frmInsight_Manufacturing5.Dir_Results_Base + SerialNumber + @"\";
                string fi = DateTime.Now.ToString();
                fi = fi.Replace(".", "_");
                fi = fi.Replace(":", "_");
                fi = pa + @"\" + BasicFileName + fi + ".txt";
                if (!Directory.Exists(pa))
                    Directory.CreateDirectory(pa);
                sw_logFile = new StreamWriter(fi, true);   //Log detailed values

                sw_logFile.WriteLine("===========================");
                sw_logFile.WriteLine(DateTime.Now.ToString() + " : " + Job_Message);
                sw_logFile.WriteLine("===========================");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        protected void CloseLogFile()
        {
            if (sw_logFile != null)
            {
                sw_logFile.Close();
            }
        }

        protected void WriteLogFile(string LogMessage)
        {
            if (sw_logFile != null)
            {
                sw_logFile.WriteLine(LogMessage);
            }
        }

        public bool isLogFileOpen
        {
            get
            {
                if (sw_logFile != null) return true;
                return false;
            }
        }
        #endregion
        //Excel Helpers

        private DataTable? Get_DataTable_from_Excel(string sheetname)
        {
            DataTable? dt = new DataTable();
            string strExeFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Neuromodul_TestSettings.xlsx";

            try
            {
                using (var stream = File.Open(strExeFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });
                        dt = result.Tables[sheetname];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                // Handle exceptions or throw them as needed
            }

            return dt;
        }


        /***************************************
        Atem Testbedingungen
        ****************************************/

        /// <summary>Configure Atem-Modul</summary>
        public void Configure_Atem()
        {
            WaitToSettle_ms = 60000;
            int MeasureTime_s = 20;
            double Tolerance_Percent = 3.0;  //1 Atemzug pro Minute

            SendChannels = new bool[] { true, true, false, false };   //Welche SW Kanäle werden übertragen
            SampleInt_of_channels_ms = new int[] { 50, 50, 50, 50 }; //internal sampling = 128Hz -> 7.8ms
            ChartMaximum = new double[] { 1, 50, 1, 1 };
            ChartMinimum = new double[] { -1, 0, -1, -1 };

            double SampleInt_ms = SampleInt_of_channels_ms[0];

            AgainValues = new CAgainValues(enumModuleType.cModuleAtem);

            double f_Atem = 0.5;
            double Vppin_Atem = 0.2;
            double Ueff_Atem_soll = Get_Ueff_soll(Vppin_Atem, enumModuleType.cModuleAtem) * 102.6;       //siehe "Module Kalibrierung 2.xlsx" 

            AgainValues.Add_Sinus(f_Atem, Vppin_Atem, Ueff_Atem_soll, Tolerance_Percent, MeasureTime_s, SampleInt_ms, Ignore_Time_percent: 20);
        }

        /***************************************
        ECG Testbedingungen
        ****************************************/
        //Heart Rate (Pulse) via ArbitraryFile_ECG
        protected double ECG_HR_Tolerance_Percent = 5;       //bzw +-5bpm
        public void Configure_ECG()
        {
            WaitToSettle_ms = 3000;
            int Measure_duration_s_or_periodes_neg = -20; //Sin
            int sinus_tolerance_percent = 5;

            int f_calibration = 30;

            //double ECG_at_notchfrequency = -30e-6;        //Notch values 

            SendChannels = new bool[] { true, true, false, false };
            SampleInt_of_channels_ms = new int[] { 2, 100, 100, 100 };
            ChartMaximum = new double[] { 0.0005, 210, 1, 1 };
            ChartMinimum = new double[] { -0.0005, 0, 0, 0 };

            double SampleInt_ms = SampleInt_of_channels_ms[0];
            double Ignore_Time_percent = 20;

            AgainValues = new CAgainValues(enumModuleType.cModuleECG);

            if (GetType().Name == "CECG_Read_NM_pre")
            {
                //Only 30Hz for calibration
                AgainValues.Add_Sinus(f_calibration, 0.7, Get_Ueff_soll(0.7, enumModuleType.cModuleECG), sinus_tolerance_percent,
                    Measure_duration_s_or_periodes_neg: 6, SampleInt_ms, Ignore_Time_percent);
            }
            else
            {
                DataTable dt = Get_DataTable_from_Excel("ECG");

                //f_Again	vppin_Again	again_soll_db	againAgain_Tolerance_db	only_used_in	ArbitraryFile	measure_duration	comment

                foreach (DataRow row in dt.Rows)
                {
                    string only_used_in = "";
                    if (row["only_used_in"] != DBNull.Value)
                        only_used_in = (string)row["only_used_in"];

                    string type_name = GetType().Name;

                    if (only_used_in == "" || only_used_in.Contains(type_name))
                    {
                        if (row["again_soll_db"].ToString() == "")
                        {
                            //Notch
                            AgainValues.Add_Notch(
                                (double)row["f_Again"],
                                (double)row["vppin_Again"],
                                (double)row["againAgain_Tolerance_db"],
                                Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                        }

                        else if (row["ArbitraryFile"] != System.DBNull.Value)
                        {
                            //Arbitrary File
                            string af = (string)row["ArbitraryFile"];
                            AgainValues.Add_Arbitrary(
                                af,
                                (double)row["f_Again"],
                                (double)row["vppin_Again"],
                                (double)row["again_soll_db"],
                                (double)row["againAgain_Tolerance_db"],
                                (double)row["measure_duration"],
                                SampleInt_ms, -1, 0);
                        }
                        else
                        {
                            //Again
                            AgainValues.Add_Again(
                                (double)row["f_Again"],
                                (double)row["vppin_Again"],
                                (double)row["again_soll_db"],
                                (double)row["againAgain_Tolerance_db"],
                                Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                        }
                    }
                }

                /*
                                //"d:\Daten\Insight\Svn_Doku\trunk\Manufacturing\PC Software\Korrekturvorschlag_EKG.xlsx"
                                if (GetType().Name == "CECG_AGain_Read_NM")
                                {
                                    AgainValues.Add_Again(5, 4, -23.86, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                }

                                AgainValues.Add_Again(10, 4, -12.35, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);

                                if (GetType().Name == "CECG_AGain_Read_NM")
                                {
                                    AgainValues.Add_Again(20, 1.5, -2.76, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(25, 0.9, -0.75, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                }

                                AgainValues.Add_Again(f_calibration, 1, 0.00, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);

                                if (GetType().Name == "CECG_AGain_Read_NM")
                                {
                                    AgainValues.Add_Again(35, 1, -0.55, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(40, 1, -2.71, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(45, 1.9, -7.97, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                }

                                AgainValues.Add_Notch(50, 4, ECG_at_notchfrequency, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent); ;    //< -23,5db, Tol 5dB

                                if (GetType().Name == "CECG_AGain_Read_NM")
                                {
                                    AgainValues.Add_Again(55, 3, -10.29, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(60, 1.6, -5.82, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(70, 1.5, -7.00, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                #if !DEBUG
                                    AgainValues.Add_Again(80, 1.7, -10.07, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(90, 2.2, -14.24, 1.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(100, 2.2, -16.16, 2.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(110, 2.2, -17.98, 2.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(120, 2.2, -19.91, 2.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(130, 2.2, -22.44, 2.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Again(140, 2.2, -28.67, 3.00, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                #endif
                                }

                                AgainValues.Add_Notch(150, 4, ECG_at_notchfrequency, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);

                                if (GetType().Name == "CECG_Read_NM_final" ||
                                    GetType().Name == "CECG_Read_NM_Offset_Plus" ||
                                    GetType().Name == "CECG_Read_NM_Offset_Minus")
                                {
                                    Ignore_Time_percent = 0;
                #if !DEBUG
                                    AgainValues.Add_Arbitrary("1_ArtifECG_002", 0.5, 0.72, 30, ECG_HR_Tolerance_Percent, 25, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Arbitrary("1_ArtifECG_002", 0.5, 7.24, 30, ECG_HR_Tolerance_Percent, 15, SampleInt_ms, Ignore_Time_percent);

                                    AgainValues.Add_Arbitrary("1_ArtifECG_003", 0.5, 0.72, 30, ECG_HR_Tolerance_Percent, 20, SampleInt_ms, Ignore_Time_percent);
                #endif
                                    AgainValues.Add_Arbitrary("1_ArtifECG_003", 0.5, 7.24, 30, ECG_HR_Tolerance_Percent, 15, SampleInt_ms, Ignore_Time_percent);

                                    AgainValues.Add_Arbitrary("1_ArtifECG_004", 3.33, 0.72, 200, ECG_HR_Tolerance_Percent, 10, SampleInt_ms, 0);
                #if !DEBUG
                                    AgainValues.Add_Arbitrary("1_ArtifECG_004", 3.33, 7.24, 200, ECG_HR_Tolerance_Percent, 10, SampleInt_ms, Ignore_Time_percent);

                                    AgainValues.Add_Arbitrary("1_ArtifECG_005", 3.33, 0.72, 200, ECG_HR_Tolerance_Percent, 10, SampleInt_ms, Ignore_Time_percent);
                                    AgainValues.Add_Arbitrary("1_ArtifECG_005", 3.33, 7.24, 200, ECG_HR_Tolerance_Percent, 10, SampleInt_ms, Ignore_Time_percent);
                #endif
                                }
                */

            }
        }

        /***************************************
        EMG Testbedingungen
        ****************************************/
        //public const int idxEMG_TUV = 0;            //Position of f_calibration in AGainValues
        public void Configure_EMG()
        {
            WaitToSettle_ms = 3000;
            int Measure_duration_s_or_periodes_neg = -20; //Sin
            int sinus_tolerance_percent = 5;

            int f_calibration = 70;

            //double EMG_at_notchfrequency = -30e-6;        //Notch values 

            SendChannels = new bool[] { true, true, false, false };   //Welche SW Kanäle werden übertragen
            SampleInt_of_channels_ms = new int[] { 3, 50, 100, 100 };               //Zugehörige Abtastintervalle 
            ChartMaximum = new double[] { 0.0005, 0.001, 1, 1 };
            ChartMinimum = new double[] { -0.0005, 0, 0, 0 };

            double SampleInt_ms = SampleInt_of_channels_ms[0];
            double Ignore_Time_percent = 20;

            AgainValues = new CAgainValues(enumModuleType.cModuleEMG);

            if (GetType().Name != "CEMG_AGain_Read_NM")
            {
                //Only 70Hz for calibration
                AgainValues.Add_Sinus(f_calibration, 1, Get_Ueff_soll(1, enumModuleType.cModuleEMG), sinus_tolerance_percent,
                    Measure_duration_s_or_periodes_neg: 6, SampleInt_ms, Ignore_Time_percent);
            }
            else
            {
                DataTable dt = Get_DataTable_from_Excel("EMG");

                //f_Again	vppin_Again	again_soll_db	againAgain_Tolerance_db
                foreach (DataRow row in dt.Rows)
                {
                    if (row["again_soll_db"].ToString() == "")
                    {
                        //Notch
                        AgainValues.Add_Notch(
                            (double)row["f_Again"],
                            (double)row["vppin_Again"],
                            (double)row["againAgain_Tolerance_db"],
                            Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                    }
                    else
                    {
                        AgainValues.Add_Again(
                           (double)row["f_Again"],
                            (double)row["vppin_Again"],
                            (double)row["again_soll_db"],
                            (double)row["againAgain_Tolerance_db"],
                            Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                    }
                }

                /* vor Version 5.8.3
                #if !DEBUG
                                AgainValues.Add_Again(5, 4, -15.96, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(10, 1.5, -6.7, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(20, 1.5, -1.85, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(25, 1.5, -1.4, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                #endif
                                AgainValues.Add_Again(30, 1, -0.67, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(35, 1, -1.68, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(40, 1, -3.97, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(45, 1.9, -9.05, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Notch(50, 4, EMG_at_notchfrequency, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(55, 3, -8.59, 1, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(60, 1.6, -3.23, 2, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(f_calibration, 1.5, -1.38, 2, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(80, 1.7, -2.38, 2, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                #if !DEBUG
                                AgainValues.Add_Again(90, 2.2, -4.6, 2, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(100, 2.2, -5.0, 2, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(110, 3, -6.64, 3, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(120, 3, -7.49, 3, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(130, 3, -10.46, 3, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(140, 4, -16.9, 3, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Notch(150, 4, EMG_at_notchfrequency, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);   //-38.23dB
                                AgainValues.Add_Again(160, 4, -17.11, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(170, 4, -13.52, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(180, 4, -13.42, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(200, 4, -14.24, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(220, 4, -15.50, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(250, 4, -17.96, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(270, 4, -19.67, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                                AgainValues.Add_Again(300, 4, -22.25, 10, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                #endif
                */
            }
        }

        /***************************************
        EEG Testbedingungen
        ****************************************/
        public const double fnotch_EEG = 50;
        public const double Ueff_Soll_Notch_EEG = -3e-6;  //- da Notch Frequency
        public readonly double[] f_calibration_EEG = new double[] { 4, 10, 14 };    //these f's are used for calculating the calibration factor{ 5, 10, 15 };    //these f's are used for calculating the calibration factor

        public void Configure_EEG()
        {
            WaitToSettle_ms = 15000;
            int Measure_duration_s_or_periodes_neg = -20;     //neg. value -> number of periodes
                                                              //int sinus_tolerance_percent = 15;    //20190821: war 10

            SendChannels = new bool[] { true, false, false, false };
            ChartMaximum = new double[] { 2e-5, 1e-10, 1e-10, 1e-10 };
            ChartMinimum = new double[] { -2e-5, 0, 0, 0 };
            SampleInt_of_channels_ms = new int[] { 8, 50, 50, 50 }; //internal sampling = 128Hz -> 7.8ms
            double Ignore_Time_percent = 30;
            //double Vppin_EEG = 0.85;
#if DEBUGi
            double Vppin_EEG = 0.95;
#else
            double Vppin_EEG = 0.80; //1.8.2023 mit Martin
#endif

            double SampleInt_ms = SampleInt_of_channels_ms[0];
            AgainValues = new CAgainValues(enumModuleType.cModuleEEG);
            double Ueff_EEG_soll = Get_Ueff_soll(Vppin_EEG, enumModuleType.cModuleEEG);

            DataTable dt = Get_DataTable_from_Excel("EEG");

            //f_Again	vppin_Again	again_soll_db	againAgain_Tolerance_db	only_used_in	comment
            foreach (DataRow row in dt.Rows)
            {
                string only_used_in = "";
                if (row["only_used_in"] != DBNull.Value)
                    only_used_in = (string)row["only_used_in"];
                string type_name = GetType().Name;

                if (only_used_in == "" || only_used_in.Contains(type_name))
                {
                    string again_soll_db = row["again_soll_db"].ToString();
                    if (!double.TryParse(again_soll_db, out double EEG_soll))
                        EEG_soll = Ueff_EEG_soll;

                    if (again_soll_db == "")
                    {
                        //Notch
                        AgainValues.Add_Notch(
                            (double)row["f_Again"],
                            (double)row["vppin_Again"],
                            (double)row["againAgain_Tolerance_db"],
                            Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                    }
                    else
                    {
                        //Sinus
                        AgainValues.Add_Again(
                           (double)row["f_Again"],
                            (double)row["vppin_Again"],
                            EEG_soll,
                            (double)row["againAgain_Tolerance_db"],
                            Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                    }
                }
            }

            /*
                //Aktualisiert 17.7.2023
                //"d:\Daten\Insight\Svn_Doku\trunk\Manufacturing\PC Software\Korrekturvorschlag_EEG.xlsx" 
                if (this.GetType().Name == "CEEG_Read_NM_pre")
            {
                AgainValues.Add_Sinus(4, Vppin_EEG, Ueff_EEG_soll, sinus_tolerance_percent, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Sinus(10, Vppin_EEG, Ueff_EEG_soll, sinus_tolerance_percent, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Sinus(14, Vppin_EEG, Ueff_EEG_soll, sinus_tolerance_percent, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                //AgainValues.Add_Notch(50, Vppin_EEG, Ueff_Soll_Notch_EEG, MeasureTime_s / 2); //10.12.2021
            }
            else
            {
#if !DEBUGI
                AgainValues.Add_Again(1, Vppin_EEG, -1.50, 0.50, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(2, Vppin_EEG, -0.36, 0.50, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
#endif
                //caibration f's
                for (int i = 0; i < f_calibration_EEG.Length; i++)
                    AgainValues.Add_Again(f_calibration_EEG[i], Vppin_EEG, 0.0, 0.5, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
#if !DEBUGI
                //double _MeasureTime_s = 0.5;
                AgainValues.Add_Again(20, Vppin_EEG, -0.00, 0.50, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(25, Vppin_EEG, -0.32, 0.50, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(30, Vppin_EEG, -1.53, 0.50, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(31, Vppin_EEG, -2.03, 0.80, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(32, Vppin_EEG, -2.69, 0.60, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(33, Vppin_EEG, -3.45, 0.60, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(34, Vppin_EEG, -4.39, 0.60, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(35, Vppin_EEG, -5.46, 0.60, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Again(40, Vppin_EEG, -12.42, 2.00, 2 * Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
                AgainValues.Add_Notch(50, 0.85, Ueff_Soll_Notch_EEG, Measure_duration_s_or_periodes_neg, SampleInt_ms, Ignore_Time_percent);
#endif
            }*/
        }

        /***************************************
        Multi-Sensor Testbedingungen
        ****************************************/

        //Test Temperaturschrank
        public const double Multi_Temp_Abweichung = 1.5;    //+- Abweichung

        //Andere Messungen
        public const double Multi_R1_soll = 330000.0;
        public const double Multi_R2_soll = 3300000.0;
        public const double Multi_SCL_Tolerance_Percent = 5;

        public const double Multi_TempR1_soll = 47000.0;        //Externer Testwiderstand1
        public const double Multi_TempR2_soll = 30100.0;        //Externer Testwiderstand2

        //Keywords for 
        public static string[] Multi_Test_Details_keywords = { "SCL1", "SCL2", "SCL3", "Temp1", "Temp2", "Temp3", "BPM" };

        //Test von SCL3 - 1MOhm an Sensorkontakte
        public static double MeasureTime_Multi_SCL3_s = 5;
        public static readonly bool[] SendChannel_Multi_SCL = { true, false, false, false };
        public const double Multi_R3_soll = 1e6;

        public void Configure_Multi()
        {
            WaitToSettle_ms = 2000;
            int MeasureTime_s = 10;
#if DEBUG
            MeasureTime_s = 3;
#endif

            SendChannels = new bool[] { true, true, false, false };
            SampleInt_of_channels_ms = new int[] { 35, 200, 20, 200 };
            ChartMaximum = new double[] { 5e-6, 40, 1, 100 };
            ChartMinimum = new double[] { 0, 10, -1, 0 };
            double?[] Sample_Ignore_Time_percent = new double?[] { 50, 0, 0, 50 }; //Die ersten 50% der SCL und bpm ignorieren

            AgainValues = new CAgainValues(enumModuleType.cModuleMultisensor);
            AgainValues.Add_Sinus(1, 1, -1.5, 0.5, MeasureTime_s, SampleInt_of_channels_ms[0], (double)Sample_Ignore_Time_percent[0]);
            AgainValues.Add_Sinus(2, 2, -0.4, 0.5, MeasureTime_s, SampleInt_of_channels_ms[0], (double)Sample_Ignore_Time_percent[0]);
        }

    }

}



