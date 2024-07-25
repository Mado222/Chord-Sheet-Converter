using ComponentsLib_GUI;
using WindControlLib;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CMulti_Pulse : CMulti_Read_NM_base
    {
        //Test mit "Wuff Wuff Maschine
        public static readonly bool[] SendChannel_Multi_Pulse = { false, false, true, true }; //Pulse raw, Pulse_bpm
        public static double Multi_Pulse_soll_bpm = 60;
        public static double Multi_Pulse_soll_min_bpm = 57;
        public static double Multi_Pulse_soll_max_bpm = 63;

        public static double MeasureTime_Multi_Pulse_s = 20;
        public static readonly double[] MeasureTime_Multi_s_Pulse = { MeasureTime_Multi_Pulse_s, MeasureTime_Multi_Pulse_s }; //SCL Lo, Hi measure time

        public CMulti_Pulse(CFY6900 FY6900) : base(FY6900)
        {
            SendChannels = SendChannel_Multi_Pulse;
            Job_Message = "Multi_Pulse";
            my_name = "Check Multi Pulse";
            ModulePortNo = 1;
            Pre_Job_Message = "Multisensor auf Puls-Tester legen";
        }

        public override void Process_NM_Data(WindControlLib.CDataIn DataIn, int swcn = 0)
        {
            base.Process_NM_Data(DataIn, 2);
        }

        public bool IsMultiPulse_OK(double bpm)
        {
            if (bpm < Multi_Pulse_soll_max_bpm && bpm > Multi_Pulse_soll_min_bpm)
                return true;
            return false;
        }

        public override bool Perform_Specific_Measurement()
        {
            bool isOK = true;
            //Measure
            CntSettings = 0;
            NumSamplestoCollect = (int)(MeasureTime_Multi_Pulse_s / (double)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[2].SampleInt * 1000.0);
            CntSamplesCollected = 0;

            OnReportMeasurementProgress("Measuring Pulse for " + ((int)MeasureTime_Multi_Pulse_s).ToString() + "s", Color.Black);
            CDelay.Delay_ms_DoEvents(WaitToSettle_ms); //Wait to settle
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
                _dsManufacturing.Neuromodule_Daten[0].Multi_Pulsfrequenz = AllResults[0].ChannelResults[3].Umean;
                neuromodule_DatenTableAdapter.Update(_dsManufacturing.Neuromodule_Daten);
            }
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            bool b = false;
            ModuleTestResult = enModuleTestResult.Fail;
            if (IsMultiPulse_OK(AllResults[0].ChannelResults[3].Umean))    //bpm
            {
                ModuleTestResult = enModuleTestResult.OK;
                b = true;
            }

            //Test_Details += Job_Message + ": " + Environment.NewLine;
            Test_Details += "Pulse soll/ist  = " + Multi_Pulse_soll_bpm.ToString() + " / " + AllResults[0].ChannelResults[3].Umean.ToString("N1");
            Test_Details += " -> " + Get_TestString(b) +
                " Range = " + Multi_Pulse_soll_max_bpm.ToString() + " ... " + Multi_Pulse_soll_min_bpm.ToString() + ";";

            return ModuleTestResult ;
        }

        public override bool Add_TestDetails_toDB(string Test_Details)
        {
            return base.Add_TestDetails_toDB(Make_Multi_Test_Details_string(
               "", "", "", "", "", "", 
               Get_TestString(IsMultiPulse_OK(AllResults[0].ChannelResults[3].Umean))));
        }
    }
}
