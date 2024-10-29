using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;


namespace Insight_Manufacturing5_net8.tests_measurements
{
    public partial class CNeuromaster_Get_All_HW_Channel : CRead_Neuromaster
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CNeuromaster_Get_All_HW_Channel"/> class.
        /// </summary>

        public bool hw_channel_ok = false;

        public CNeuromaster_Get_All_HW_Channel(CFY6900 FY6900) : base(FY6900)
        {
        }

        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            OnBeforeMeasurementStarts();
            try
            {
                C8KanalReceiverV2.enumConnectionResult? conres = Connect_DataReceiver(false);
                if (conres == C8KanalReceiverV2.enumConnectionResult.Connected_via_USBCable)
                {
                    if (DataReceiver.Connection.GetDeviceConfig())
                    {
                        if (DataReceiver.Connection.Device.ModuleInfos != null && DataReceiver.Connection.Device.ModuleInfos.Count > 0)
                        {
                            CModuleBase[] cmi = new CModuleBase[DataReceiver.Connection.Device.ModuleInfos.Count];
                            DataReceiver.Connection.Device.ModuleInfos.CopyTo(cmi);
                            List<CModuleBase> ModuleInfo = new(cmi);
                            byte channel_ok = 0;
                            for (int i = 0; i < 7; i++)
                            {
                                if (ModuleInfo[i].ModuleType == enumModuleType.cModuleTypeEmpty && ModuleInfo[i].ModuleBootloaderError == false)
                                {
                                    OnReportMeasurementProgress("Channel: " + i.ToString() + ", Defekt / Kein Modul verbunden?", Color.Red);
                                }
                                else
                                {
                                    OnReportMeasurementProgress("Channel: " + i.ToString() + ", Modul: " + ModuleInfo[i].ModuleType_string, Color.Blue);
                                    channel_ok++;
                                }
                            }

                            if (channel_ok == 7)
                            {
                                OnReportMeasurementProgress("\nAlle HW-Kanäle OK!\n", Color.Green);
                                hw_channel_ok = true;
                                ModuleTestResult = enModuleTestResult.OK;
                            }
                            else
                            {
                                OnReportMeasurementProgress("\nHW-Kanal/Kanäle defekt!\n", Color.Red);
                                ModuleTestResult = enModuleTestResult.Fail;
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                OnReportMeasurementProgress("Fehler beim Verbinden..\n" + x.ToString(), Color.Red);
            }
            finally
            {
                DataReceiver.Close_All();
            }

            OnMeasurementFinished();
            return hw_channel_ok;
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
                    if (_dsManufacturing.Neurodevices[0].Test_Funk == true && _dsManufacturing.Neurodevices[0].Test_Kabel == true && hw_channel_ok == true)
                    {
                        _dsManufacturing.Neurodevices[0].TestOK = Test_success;
                    }
                    else
                    {
                        _dsManufacturing.Neurodevices[0].TestOK = Test_failed;
                    }

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
