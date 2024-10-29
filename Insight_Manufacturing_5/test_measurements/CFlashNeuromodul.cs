using FeedbackDataLib;
using WindControlLib;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CFlashNeuromodul : CBase_tests_measurements
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CFlashNeuromodul"/> class.
        /// </summary>
        protected CIPE_Neuromodul_PIC24 IPE_Neuromodul_PIC24;

        public CFlashNeuromodul(CMicrochip_Programmer mc_programmer)
        {
            if (ConnectedModuleType == enumModuleType.cModuleEEG)
                InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                    CInsightModuleTester_Settings.enICD.ICD_Connected, 
                    CInsightModuleTester_Settings.enEEG.EEG_On);
            else
                InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                    CInsightModuleTester_Settings.enICD.ICD_Connected);

            IPE_Neuromodul_PIC24 = new CIPE_Neuromodul_PIC24(mc_programmer);
            Job_Message = "Flash Neuromodul";
        }

        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            bool ret = base.Perform_Measurement();
            ModuleTestResult = enModuleTestResult.OK;
            if (HexFilePath_Base != "")
            {
                if (!flash_file(SerialNumber, true))
                {
                    ret = false;
                    ModuleTestResult = enModuleTestResult.Fail;
                }
            }
            OnMeasurementFinished();
            return ret;
        }

        /// <summary>Flash_files the specified hex_path.</summary>
        /// <param name="Save_to_DB"></param>
        /// <returns></returns>
        protected bool flash_file(string SerialNumber, bool copy_and_add_serial)
        {
            bool ret = false;
            IPE_Neuromodul_PIC24.ReportMeasurementProgress -= Icd3_ReportMeasurementProgress;
            IPE_Neuromodul_PIC24.ReportMeasurementProgress += Icd3_ReportMeasurementProgress;

            //Get hex file version
            //string serial = "";
            //icd3.Get_SerialNUmber_from_hexFile(hex_file, ref serial);

            string StatusString = "";
            if (copy_and_add_serial)
            {
                ret = IPE_Neuromodul_PIC24.FlashHexFile_Copy_AddSerial(HexFilePath_Base, HexFilePath_Target, SerialNumber, ref StatusString, ConnectedModuleType);
            }
            else
            {
                ret = IPE_Neuromodul_PIC24.FlashHexFile(HexFilePath_Target, ref StatusString, ConnectedModuleType);
            }
            OnReportMeasurementProgress(StatusString, System.Drawing.Color.Black);

            if (ret)
            {
                OnReportMeasurementProgress("Hex file " + HexFilePath_Base + " flashed", System.Drawing.Color.Green);
            }
            else
            {
                OnReportMeasurementProgress("Hex file " + HexFilePath_Base + " failed", System.Drawing.Color.Red);
                ModuleTestResult = enModuleTestResult.Fail;
            }
            return ret;
        }

        private void Icd3_ReportMeasurementProgress(object sender, string text, Color col)
        {
            OnReportMeasurementProgress(text, col);
        }



    }
}
