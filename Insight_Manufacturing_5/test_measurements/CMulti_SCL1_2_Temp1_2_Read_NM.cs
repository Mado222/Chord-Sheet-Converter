using ComponentsLib_GUI;
using WindControlLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{

    public class CMulti_SCL1_2_Temp1_2_Read_NM : CMulti_Read_NM_base
    {
        public CMulti_SCL1_2_Temp1_2_Read_NM(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Multi_pre";
            Pre_Job_Message = "Multisensor in Neuromaster einsetzen";
            ModulePortNo = 1;
        }

        public CMulti_SCL1_2_Temp1_2_Read_NM(CFY6900 FY6900, string JobMessage):base(FY6900)
        {
            Job_Message = JobMessage;
            Pre_Job_Message = "Multisensor in Neuromaster einsetzen";
            ModulePortNo = 1;
        }

        public override void Process_NM_Data(CDataIn DataIn, int swcn = 0)
        {
            base.Process_NM_Data(DataIn);
        }

        public override bool Perform_Specific_Measurement()
        {
            bool isOK = false;
            //Measure SCL
            CntSettings = 0;
            NumSamplestoCollect = (int)((double)AgainValues.AgainValues[CntSettings].MeasureTime_s / (double)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[0].SampleInt * 1000.0);
            CntSamplesCollected = 0;
            if (!SetSCL(true, false))
            {
                frm_image_text fit;
                
                int res = (int)(Multi_TempR1_soll / 1000);
                fit = new frm_image_text(
                    "Nächster Schritt:",
                    "Violett_400.jpg",
                    res.ToString() + "k Teststecker (violett) mit Multisensor-Buchse (D) verbinden");

                //if (MessageBox.Show(res.ToString() +"k Teststecker (violett) mit Multisensor-Buchse (D) verbinden", "Nächster Schritt:", MessageBoxButtons.OKCancel) == DialogResult.OK)
                if (fit.ShowDialog() == DialogResult.OK)
                {
                    OnReportMeasurementProgress("Measuring: SCL1 and Temperature", Color.Black);
                    CDelay.Delay_ms_DoEvents(1000); //Wait for SCL to settle
                    Wait4Data();
                    CntSettings++;
                    NumSamplestoCollect = (int)((double)AgainValues.AgainValues[CntSettings].MeasureTime_s / (double)DataReceiver.Connection.Device.ModuleInfos[ModulePortNo].SWChannels[0].SampleInt * 1000.0);
                    CntSamplesCollected = 0;
                    res = (int)(Multi_TempR2_soll / 1000);

                    fit = new frm_image_text(
                        "Nächster Schritt:",
                        "Orange_400.jpg",
                        res.ToString() + "k Teststecker (orange) mit Multisensor-Buchse (D) verbinden");

                    //if (MessageBox.Show(res.ToString()+ "k Teststecker (orange) mit Multisensor-Buchse (D) verbinden", "Nächster Schritt:", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    if (fit.ShowDialog() == DialogResult.OK)
                    {
                        if (!SetSCL(true, true))
                        {
                            OnReportMeasurementProgress("Measuring: SCL2 and Temperature", Color.Black);
                            CDelay.Delay_ms_DoEvents(1000); //Wait for SCL to settle
                            Wait4Data();
                            isOK = true;
                        }
                    }
                }
                if (!isOK)
                {
                    OnReportMeasurementProgress("Failed to set SCL", Color.Red);
                }
            }
            return isOK;
        }

        private bool SetSCL (bool TestOn, bool TestHi)
        {
            bool error = true;

            int cnt = 10;       //10x probieren

            while (error && cnt > 0)
            {
                ((CModuleMultisensor)DataReceiver.Connection.Device.ModuleInfos[1]).SCLTest = TestOn;
                ((CModuleMultisensor)DataReceiver.Connection.Device.ModuleInfos[1]).SCLTest_Resistor_High = TestHi;

                if (DataReceiver.Connection.SetModuleInfoSpecific(1))
                {
                    error = false;
                    break;
                }
                Thread.Sleep(50);
            }
            return error;
        }

        public override void Save_Results_to_DB()
        {
            DateTime dt_created = DateTime.Now;
            Guid id_neuromodule_kalibrierdaten = Guid.NewGuid();

            Save_Calibration_Values(id_neuromodule_kalibrierdaten);

            dsManufacturing _dsManufacturing = new();
            dsManufacturing.Neuromodule_DatenRow nrow = _dsManufacturing.Neuromodule_Daten.NewNeuromodule_DatenRow();
            Add_MeasuementResults_to_Row(ref nrow, AllResults[0]);

            nrow.SCL1 = AllResults[0].ChannelResults[0].Umean;
            nrow.SCL2 = AllResults[1].ChannelResults[0].Umean;
            nrow.Temp1 = AllResults[0].ChannelResults[1].Umean;
            nrow.Temp2 = AllResults[1].ChannelResults[1].Umean;

            nrow.Testdatum = dt_created;
            nrow.id_neuromodule_daten = Guid.NewGuid();
            nrow.id_neuromodule_kalibrierdaten = id_neuromodule_kalibrierdaten;
            nrow.SerialNumber = SerialNumber;
            nrow.Bemerkung = my_name;

            dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter = new();

            _dsManufacturing.Neuromodule_Daten.Rows.Add(nrow);
            neuromodule_DatenTableAdapter.Update(_dsManufacturing.Neuromodule_Daten);
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            //Test_Details += Job_Message + ": " + Environment.NewLine;

            ModuleTestResult = enModuleTestResult.OK;

            //SCL1
            bool b = IsMultiSCL1_OK(AllResults[0].ChannelResults[0].Umean);
            Test_Details += "R1 soll/ist  = " + Multi_R1_soll.ToString() + " / " + ((int)(1/AllResults[0].ChannelResults[0].Umean)).ToString();
            Test_Details += " -> " + Get_TestString(b) + " Tol = " + Multi_SCL_Tolerance_Percent.ToString() +"%;";
            if (!b) ModuleTestResult = enModuleTestResult.Fail;

            //SCL2
            b = IsMultiSCL2_OK(AllResults[1].ChannelResults[0].Umean);
            Test_Details += "R2 soll/ist  = " + Multi_R2_soll.ToString() + " / " + ((int)(1 / AllResults[1].ChannelResults[0].Umean)).ToString();
            Test_Details += " -> " + Get_TestString(b) + " Tol = " + Multi_SCL_Tolerance_Percent.ToString() + "%;";
            if (!b) ModuleTestResult = enModuleTestResult.Fail;

            //Temp1
            b = IsMultiTEMP1_OK(AllResults[0].ChannelResults[1].Umean);
            Test_Details += "Temp1 soll/ist  = " + Multi_Temp1_soll().ToString("N2") + " / " + AllResults[0].ChannelResults[1].Umean.ToString("N2");
            Test_Details += " -> " + Get_TestString(b) + " Tol = +-" + Multi_Temp_Abweichung.ToString() + "°;";
            if (!b) ModuleTestResult = enModuleTestResult.Fail;

            //Temp2
            b = IsMultiTEMP2_OK(AllResults[1].ChannelResults[1].Umean);
            Test_Details += "Temp2 soll/ist  = " + Multi_Temp2_soll().ToString("N2") + " / " + AllResults[1].ChannelResults[1].Umean.ToString("N2");
            Test_Details += " -> " + Get_TestString(b) + " Tol = +-" + Multi_Temp_Abweichung.ToString() + "°;";
            if (!b) ModuleTestResult = enModuleTestResult.Fail;

            return ModuleTestResult;
        }


        public override bool Add_TestDetails_toDB(string Test_Details)
        {
            return base.Add_TestDetails_toDB(
                        Make_Multi_Test_Details_string(
                        Get_TestString(IsMultiSCL1_OK(AllResults[0].ChannelResults[0].Umean)),
                        Get_TestString(IsMultiSCL2_OK(AllResults[1].ChannelResults[0].Umean)),
                        "",
                        Get_TestString(IsMultiTEMP1_OK(AllResults[0].ChannelResults[1].Umean)),
                        Get_TestString(IsMultiTEMP2_OK(AllResults[1].ChannelResults[1].Umean)),
                        "",
                        ""));
        }
    }
}
