using ComponentsLib_GUI;
using WindControlLib;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CMulti_SCL_Sensor : CMulti_Read_NM_base
    {
        public CMulti_SCL_Sensor(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "SCL_Sensor";
            Pre_Job_Message = "Multisensor auf SCL Tester legen";
            ModulePortNo = 1;
            my_name = "Check Multi SCL";
        }

        public override void Process_NM_Data(WindControlLib.CDataIn DataIn, int swcn = 0)
        {
            base.Process_NM_Data(DataIn, 0);
        }

        public override bool Perform_Specific_Measurement()
        {
            bool isOK = true;
            //Measure SCL
            CntSettings = 0;
            NumSamplestoCollect = (int)(MeasureTime_Multi_SCL3_s / (double)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[0].SampleInt * 1000.0);
            CntSamplesCollected = 0;

            OnReportMeasurementProgress("Measuring SCL for " + ((int)MeasureTime_Multi_SCL3_s).ToString() + "s", Color.Black);
            CDelay.Delay_ms_DoEvents(WaitToSettle_ms); //Wait for SCL to settle
            Wait4Data();

            return isOK;
        }

        public override void Save_Results_to_DB()
        {
            dsManufacturing _dsManufacturing = new dsManufacturing();
            dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter = new dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter();

            neuromodule_DatenTableAdapter.FillBy_SerialNumber_Order_Desc_by_Date(_dsManufacturing.Neuromodule_Daten, SerialNumber);
            //zum datumsmäßig letzten Datensatz hinzufügen

            if (_dsManufacturing.Neuromodule_Daten.Count > 0)
            {
                _dsManufacturing.Neuromodule_Daten[0].SCL3 = AllResults[0].ChannelResults[0].Umean;
                neuromodule_DatenTableAdapter.Update(_dsManufacturing.Neuromodule_Daten);
            }
        }

        public override enModuleTestResult isModule_OK()
        {
            ModuleTestResult = base.isModule_OK();

            ModuleTestResult = enModuleTestResult.OK;

            //SCL3
            bool b = IsMultiSCL3_OK(AllResults[0].ChannelResults[0].Umean);
            Test_Details += "R3 soll/ist  = " + Multi_R3_soll.ToString() + " / " + ((int)(1 / AllResults[0].ChannelResults[0].Umean)).ToString();
            Test_Details += " -> " + Get_TestString(b) + " Tol = " + Multi_SCL_Tolerance_Percent.ToString() + "%;";
            if (!b) ModuleTestResult = enModuleTestResult.Fail;

            return ModuleTestResult;
        }

        public override bool Add_TestDetails_toDB(string Test_Details)
        {
            return base.Add_TestDetails_toDB(Make_Multi_Test_Details_string(                        
                        "", "", Get_TestString(), "", "", "", ""));
        }
    }
}
