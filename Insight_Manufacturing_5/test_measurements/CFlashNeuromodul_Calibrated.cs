using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using FeedbackDataLib;
using WindControlLib;


namespace Insight_Manufacturing5_net8.tests_measurements
{
    public partial class CFlashNeuromodul_Calibrated : CFlashNeuromodul
    {
        public List<CSWChannelInfo> NewCalibrationValues;
        public CFlashNeuromodul_Calibrated(CMicrochip_Programmer Microchip_Programmer) : base(Microchip_Programmer)
        {
        }

        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            ModuleTestResult = enModuleTestResult.Fail;

            CIPE_Neuromodul_PIC24 Neuromodule_handle_hexfile = new(IPE_Neuromodul_PIC24.mc_programmer);
            Neuromodule_handle_hexfile.ReportMeasurementProgress -= Neuromodule_handle_hexfile_ReportMeasurementProgress;
            Neuromodule_handle_hexfile.ReportMeasurementProgress += Neuromodule_handle_hexfile_ReportMeasurementProgress;

            //Berechnen der neuen Werte
            CIPE_Neuromodul_PIC24.Make_Combined_Hex_File_with_new_ChannelInfo(
                HexFilePath_Base,
                HexFilePath_Target,
                [.. NewCalibrationValues], ConnectedModuleType);

            //Flashen
            if (flash_file("", false)) //Serial nuumber already in file
            {
                ModuleTestResult = enModuleTestResult.OK;
                return true;
            }
            return false;
        }

        private void Neuromodule_handle_hexfile_ReportMeasurementProgress(object sender, string text, Color col)
        {
            OnReportMeasurementProgress(text, col);
        }
    }
}
