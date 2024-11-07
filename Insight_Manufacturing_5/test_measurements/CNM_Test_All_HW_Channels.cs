using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public partial class CNM_Test_All_HW_Channels : CNeuromaster_Get_All_HW_Channel
    {
        public CNM_Test_All_HW_Channels(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Test Modul-Kommunikation";
            Pre_Job_Message = "Alle Eingänge des Neuromaster mit einem Modul verbinden.";
        }

        private string fail_result = "";

        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            fail_result = "";
            return base.Perform_Measurement(ignore_serial_number_check);
        }

        public override enModuleTestResult isModule_OK()
        {
            //base.isModule_OK();
            return ModuleTestResult;
        }

        public override enModuleTestResult Post_Process()
        {
            enModuleTestResult ret = isModule_OK();
            Save_Results_to_DB();
            return ret;
        }

        public override void Save_Results_to_DB()
        {
            DateTime dt_tested = DateTime.Now;
            try
            {
                dsManufacturing _dsManufacturing = new();
                dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new();

                neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
                if (_dsManufacturing.Neurodevices.Count == 1)
                {
                    if (_dsManufacturing.Neurodevices[0].IsTest_DetailsNull())
                        _dsManufacturing.Neurodevices[0].Test_Details = "";
                    else
                        _dsManufacturing.Neurodevices[0].Test_Details += "; ";

                    fail_result= fail_result.Replace(@" / Kein Modul verbunden?", "");
                    fail_result = fail_result.Replace(@";", "_");

                    _dsManufacturing.Neurodevices[0].Test_Details += Job_Message + ": " + Get_TestString() + fail_result;

                    _dsManufacturing.Neurodevices[0].Testdatum = dt_tested;
                    neurodevicesTableAdapter.Update(_dsManufacturing.Neurodevices);
                }
            }
            catch (Exception x)
            {
                OnReportMeasurementProgress("Device not found in DB / No Connection to DB\n" + x.ToString(), Color.Violet);
            }
        }
    }
}
