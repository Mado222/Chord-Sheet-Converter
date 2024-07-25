using FeedbackDataLib;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public partial class CBase_tests_measurements
    {

        /***************************************/
        //Text for Database
        /***************************************/
        public static string Test_success = "OK";
        public static string Test_failed = "Fail";
        public static string Partially_success = "Partially_OK";
        public static string Partially_failed = "Partially_Fail";
        public static string Test_not_done = "Test_not_done";
        public static string Test_fail_no_further_processing = "Fail_no_further_processing";

        public string Get_TestString()
        {
            return Get_TestString(ModuleTestResult);
        }

        public static string Get_TestString(bool ret)
        {
            if (ret)
                return Test_success;
            return Test_failed;
        }

        public static string Get_TestString(enModuleTestResult ModuleTestResult)
        {
            switch (ModuleTestResult)
            {
                case enModuleTestResult.OK:
                    return Test_success;
                case enModuleTestResult.Fail:
                    return Test_failed;
                case enModuleTestResult.Partially_OK:
                    return Partially_success;
                case enModuleTestResult.Partially_Failed:
                    return Partially_failed;
                case enModuleTestResult.notChecked:
                    return Test_not_done;
                case enModuleTestResult.Fail_no_further_processing:
                    return Test_fail_no_further_processing;
            }
            return "";
        }

        public enum enMeasureType
        {
            Sinus = 1,
            Notch = 2,
            Arbitrary = 3
        }

        public enum enModuleTestResult
        {
            notChecked = 1,
            OK = 0,
            Fail = -1,
            Partially_Failed = -2,
            Partially_OK = -3,
            Fail_no_further_processing = -4
        }


        #region Properties
        private DateTime? _MeasurementStarted = null;
        public DateTime? MeasurementStarted
        {
            get
            {
                if (_MeasurementStarted == null)
                    return DateTime.Now;
                else return _MeasurementStarted;
            }
            set { _MeasurementStarted = value; }
        }
        public bool? Default_Active { get; set; } = null;
        public string HexFilePath_Base { get; set; } = "";
        public string HexFilePath_Target { get; set; } = "";
        public string Dir_Results_Base { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Dir_Results
        {
            get
            { return Dir_Results_Base + SerialNumber + "\\"; }
        }
        public string my_name { get; set; } = "";
        public string Test_Details { get; set; } = "";
        public enumModuleType ConnectedModuleType { get; set; }
        public string Pre_Job_Message { get; set; } = "";
        public string Job_Message { get; set; } = "";
        public frm_image_text Pre_Job_MessageBox { get; set; } = null;
        public CInsightModuleTester_Settings InsightModuleTester_Settings { get; set; } = null;

        public CInsightModuleTesterV1 InsightModuleTestBoardV1 = null;

        public enModuleTestResult ModuleTestResult { get; set; } = enModuleTestResult.notChecked;
       
        #endregion

        public CBase_tests_measurements()
        {
        }

        public void Reset()
        {
            ModuleTestResult = enModuleTestResult.notChecked;
        }

    #region Events
    public delegate void ReportMeasurementProgressEventHandler(object sender, string text, System.Drawing.Color col);
        public event ReportMeasurementProgressEventHandler ReportMeasurementProgress;
        public virtual void OnReportMeasurementProgress(string text, System.Drawing.Color col)
        {
            ReportMeasurementProgress?.Invoke(this, text, col);
        }

        public delegate void MeasurementFinishedEventHandler(object sender);
        public event MeasurementFinishedEventHandler MeasurementFinished;
        protected virtual void OnMeasurementFinished()
        {
            MeasurementFinished?.Invoke(this);
        }

        public delegate void BeforeMeasurementStartsEventHandler(object sender);
        public event BeforeMeasurementStartsEventHandler BeforeMeasurementStarts;
        protected virtual void OnBeforeMeasurementStarts()
        {
            BeforeMeasurementStarts?.Invoke(this);
        }

        #endregion

        /// <summary>Calls OnBeforeMeasurementStarts</summary>
        /// <returns> true - always
        ///   <br />
        /// </returns>
        public virtual bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            OnBeforeMeasurementStarts();
            return true;
        }

        public double Get_Ueff_soll(double Upp_Sinus_Generator)
        {
            return Get_Ueff_soll(Upp_Sinus_Generator, ConnectedModuleType);
        }

        /// <summary>
        /// Berechnet den zu messenden Sollwert bei gegebener Generatorspannung
        /// </summary>
        /// <param name="Upp_Sinus_Generator">Upp sinus generator</param>
        /// <param name="k">Rückgabewert:Abschwächungsfakror des Spannungsteilers</param>
        /// <param name="ModuleType">Modultyp</param>
        /// <returns></returns>
        public static double Get_Ueff_soll(double Upp_Sinus_Generator, enumModuleType ModuleType)
        {
            double k = 0;
            switch (ModuleType)
            {
                case enumModuleType.cModuleEMG:
                case enumModuleType.cModuleECG:
                    {
                        const double R1_Ohm = 680000;
                        const double R2_Ohm = 470;
                        const double R3_Ohm = 470;

                        k = (R2_Ohm / (R1_Ohm + R2_Ohm + R3_Ohm));
                        break;
                    }
                case enumModuleType.cModuleEEG:
                    {
                        const double R1_Ohm = 680000;
                        const double R2_Ohm = 22;
                        const double R3_Ohm = 22;

                        k = (R2_Ohm / (R1_Ohm + R2_Ohm + R3_Ohm));
                        break;
                    }
                case enumModuleType.cModuleAtem:
                    {
                        const double R1_Ohm = 180000;
                        const double R2_Ohm = 10000;
                        const double R3_Ohm = 180000;

                        k = R2_Ohm / (R1_Ohm + R2_Ohm + R3_Ohm);
                        break;
                    }
            }
            return Upp_Sinus_Generator / 2 / Math.Sqrt(2) * k;
        }

        public virtual void Save_Results_to_DB()
        {
        }

        public virtual enModuleTestResult isModule_OK()
        {
            return enModuleTestResult.OK;
        }

        public virtual enModuleTestResult Post_Process()
        {
            return enModuleTestResult.Fail;
        }

        public void UpdateTestBoard ()
        {
            if (InsightModuleTestBoardV1!= null)
                InsightModuleTestBoardV1.Init(InsightModuleTester_Settings);
        }


        public virtual bool Add_TestDetails_toDB(string Test_Details)
        {
            dsManufacturing _dsManufacturing = new dsManufacturing();
            dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();

            Test_Details = Job_Message + ": " + Test_Details + Environment.NewLine;

            bool ret = true;
            try
            {
                neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
                if (_dsManufacturing.Neurodevices.Count == 1)
                {
                    if (_dsManufacturing.Neurodevices[0].IsTest_DetailsNull())
                        _dsManufacturing.Neurodevices[0].Test_Details = Test_Details;
                    else
                        _dsManufacturing.Neurodevices[0].Test_Details += DateTime.Now.ToString() +": " + Test_Details;

                    _dsManufacturing.Neurodevices[0].Testdatum = DateTime.Now;
                    neurodevicesTableAdapter.Update(_dsManufacturing.Neurodevices);
                }
            }
            catch (Exception ee)
            {
                OnReportMeasurementProgress(ee.ToString(), Color.Violet);
                ret = false;
            }
            return ret;
        }
    }
}
