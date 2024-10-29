using BMTCommunication;
using ComponentsLib_GUI;
using FeedbackDataLib;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CNM_Test_Cable_Connection_pre : CNM_Test_Connection_base
    {
        public CNM_Test_Cable_Connection_pre(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Test Kabelverbindung_pre_dummy";
            Uoffset_mV = 0;
        }
    }


    public class CNM_Test_Cable_Connection : CNM_Test_Connection_base
    {
        public CNM_Test_Cable_Connection(CFY6900 FY6900) : base(FY6900)
        {
            Test_kabel = true;
            Job_Message = "Test Kabelverbindung";
            Pre_Job_Message = "Neurolink mit Strom versorgen und mit PC/Neuromaster verbinden.\nNeuromaster einschalten..";
        }
    }

    public class CNM_Test_XBEE_Connection : CNM_Test_Connection_base
    {
        public CNM_Test_XBEE_Connection(CFY6900 FY6900) : base(FY6900)
        {
            Test_kabel = false;
            Job_Message = "Test XBEE Verbindung";
            Pre_Job_Message = "Neurolink mit PC verbinden, Neuromaster mit Batterien versorgen.\nNeuromaster einschalten..";

            //delete XBEE configuration files
            //c:\Users\xxxxx\AppData\Local\Insight\RemoteDevice.xml
            //c: \Users\xxxxx\AppData\Local\Insight\LocalDevice.xml

            CXBeeConnection xbc = new();
            string rd = "";
            string ld = "";
            xbc.GetConfigPath(ref ld, ref rd);
            if (File.Exists(rd))
                File.Delete(rd);
            if (File.Exists(ld))
                File.Delete(ld);
        }
    }

    public class CNM_Test_Connection_base : CRead_Neuromaster
    {
        public bool Test_kabel { get; set; } = true;

        public C8KanalReceiverV2.enumConnectionResult? ConnectionResult = C8KanalReceiverV2.enumConnectionResult.NoConnection;
        //public bool TestOK = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CNM_Test_Connection"/> class.
        /// </summary>
        public CNM_Test_Connection_base(CFY6900 FY6900) : base(FY6900)
        {

        }
        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            ModuleTestResult = enModuleTestResult.Fail;
            OnBeforeMeasurementStarts();
            ConnectionResult = Connect_DataReceiver(true);
            if (Test_kabel)
            {
                //Kabeltest
                if (ConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                {
                    ModuleTestResult = enModuleTestResult.OK;
                    OnReportMeasurementProgress("USB-Kabeltest OK", Color.Green);
                }
                else
                {
                    OnReportMeasurementProgress("USB-Kabeltest Failed", Color.Red);
                }
            }
            else
            {
                //XBee Test
                if (ConnectionResult == C8KanalReceiverV2.enumConnectionResult.Connected_via_XBee && Test_kabel == false)
                {
                    //Sicherheitshalber checken, ob die Konfigurationsfiles angelegt wurden
                    CXBeeConnection xbc = new();
                    string rd = "";
                    string ld = "";
                    xbc.GetConfigPath(ref ld, ref rd);

                    bool file_exists = true;
                    if (!File.Exists(ld)) file_exists = false;
                    if (!File.Exists(rd)) file_exists = false;

                    if (file_exists)
                    {
                        ModuleTestResult = enModuleTestResult.OK;
                        OnReportMeasurementProgress("XBEE-Test OK", Color.Green);
                    }
                }
                if (ModuleTestResult != enModuleTestResult.OK)
                {
                    OnReportMeasurementProgress("XBEE-Test Failed", Color.Red);
                }
            }

            OnMeasurementFinished();
            if (ModuleTestResult == enModuleTestResult.OK)
                return true;
            return false;
        }

        public override enModuleTestResult Post_Process()
        {
            enModuleTestResult ret = isModule_OK();
            Save_Results_to_DB();
            return ret;
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            return ModuleTestResult;
        }

        public override void Save_Results_to_DB()
        {
            try
            {
                dsManufacturing _dsManufacturing = new();
                dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new();

                neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
                if (_dsManufacturing.Neurodevices.Count == 1)
                {
                    DateTime dt_tested = DateTime.Now;
                    _dsManufacturing.Neurodevices[0].Testdatum = dt_tested;
                    if (Test_kabel == true)
                    {
                        if (ModuleTestResult == enModuleTestResult.OK)
                            _dsManufacturing.Neurodevices[0].Test_Kabel = true;
                        else
                            _dsManufacturing.Neurodevices[0].Test_Kabel = false;
                    }
                    else if (Test_kabel == false)
                    {
                        if (ModuleTestResult == enModuleTestResult.OK)
                            _dsManufacturing.Neurodevices[0].Test_Funk = true;
                        else
                            _dsManufacturing.Neurodevices[0].Test_Funk = false;
                    }
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
