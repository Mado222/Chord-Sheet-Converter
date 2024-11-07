using ComponentsLib_GUI;
using WindControlLib;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CMulti_Temp : CMulti_Read_NM_base
    {
        //Test Temperaturschrank
        public static readonly bool[] SendChannel_Multi_only_temp = [false, true, false, false];

        private frm_Multi_Temp f_Multi_Temp = null;
        double Temp_ist = 0;
        bool UserResponded = false;
        decimal TempSoll = -1;

        public CMulti_Temp(CFY6900 FY6900) : base(FY6900)
        {
            SendChannels = SendChannel_Multi_only_temp;
            Job_Message = "Multi_Temp";
            my_name = "Check Multi Temp";
            ModulePortNo = 1;
            Pre_Job_Message = "Fingersensor in den Wärmeschrank legen";
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            bool b = false;
            ModuleTestResult = enModuleTestResult.Fail;
            if (IsMultiTEMP3_OK(Temp_ist, (double)TempSoll))    //Temperatur
            {
                ModuleTestResult = enModuleTestResult.OK;
                b = true;
            }

            //Test_Details += Job_Message + ": " + Environment.NewLine;
            Test_Details += "Pulse soll/ist  = " + TempSoll.ToString("N2") + " / " + Temp_ist.ToString("N2");
            Test_Details += " -> " + Get_TestString(b) + " Tol = +-" + Multi_Temp_Abweichung.ToString() + "°;";

            return ModuleTestResult;
        }

        public override bool Add_TestDetails_toDB(string Test_Details)
        {
            return base.Add_TestDetails_toDB(Make_Multi_Test_Details_string(
                        "", "", "", "", "", Get_TestString(), ""));
        }

        public override bool Perform_Specific_Measurement()
        {
            //show Form
            if (f_Multi_Temp == null)
            {
                f_Multi_Temp = new frm_Multi_Temp();
                f_Multi_Temp.Multi_Temp_Closing += F_Multi_Temp_Multi_Temp_Closing;
            }
            f_Multi_Temp.Show();

            //Measure Temperature
            CntSettings = 0;
            AllResults = null;
            Wait4Data();
            return true;
        }

        public override void Wait4Data()
        {
            //Now we are waiting for Data to receive
            UserResponded = false;
            AcceptData = true;
            NumSamplestoCollect = 1000; //To keep receiving "forever"

            while (!UserResponded)
            {
                CntSamplesCollected = 0; //To keep receiving "forever"
                Application.DoEvents();
            }
            AcceptData = false;
        }


        private void F_Multi_Temp_Multi_Temp_Closing(object sender, decimal Sollwert)
        {
            TempSoll = Sollwert;
            UserResponded = true;
        }

        public override void Process_NM_Data(CDataIn DataIn, int swcn = 0)
        {
            base.Process_NM_Data(DataIn, 1);

            if (DataIn.HWcn == ModulePortNo)
            {
                Temp_ist = DataReceiver?.Connection?.GetScaledValue(DataIn) ?? 0;
                if (DataIn.SWcn== 1)
                {
                    //Temperatur Anzeigen
                    if (f_Multi_Temp != null)
                        f_Multi_Temp.SetIstTemperature(Temp_ist);
                }
            }
        }


        public override void Save_Results_to_DB()
        {
            dsManufacturing _dsManufacturing = new();
            dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter = new();
            neuromodule_DatenTableAdapter.FillBy_SerialNumber_Order_Desc_by_Date(_dsManufacturing.Neuromodule_Daten, SerialNumber);

            if (_dsManufacturing.Neuromodule_Daten.Count > 0)
            {
                _dsManufacturing.Neuromodule_Daten[0].Temp3 = Temp_ist;
                _dsManufacturing.Neuromodule_Daten[0].Temp3ref = (double)TempSoll;
                neuromodule_DatenTableAdapter.Update(_dsManufacturing.Neuromodule_Daten);
            }
        }
   }
}
