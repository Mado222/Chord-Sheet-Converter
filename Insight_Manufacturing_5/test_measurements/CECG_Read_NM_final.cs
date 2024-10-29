using BMTCommunication;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using ComponentsLib_GUI;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CECG_Read_NM_final : CECG_Read_NM_base
    {
        public CECG_Read_NM_final(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "ECG_no_offset";
            Uoffset_mV = 0;
        }
    }

    public class CECG_Read_NM_Offset_Plus : CECG_Read_NM_final
    {
        public CECG_Read_NM_Offset_Plus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings.UoffLevel = enUoffLevel.UoffLevel_High;
            InsightModuleTester_Settings.UoffPolarity = enUoffPolarity.Polarity_Plus;
            InsightModuleTester_Settings.Uoff = enUoff.Uoff_On;

            Job_Message = "ECG_Offset_plus";
            Uoffset_mV = CInsightModuleTesterV1.Uoff_High_mV;
        }
    }

    public class CECG_Read_NM_Offset_Minus : CECG_Read_NM_final
    {
        public CECG_Read_NM_Offset_Minus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings.UoffLevel = enUoffLevel.UoffLevel_High;
            InsightModuleTester_Settings.UoffPolarity = enUoffPolarity.Polarity_Minus;
            InsightModuleTester_Settings.Uoff = enUoff.Uoff_On;

            Job_Message = "ECG_Offset_minus";
            Uoffset_mV = -1 * CInsightModuleTesterV1.Uoff_High_mV;
        }
    }

    public class CECG_AGain_Read_NM : CAGain_Read_NM
    {
        public CECG_AGain_Read_NM(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_Off);
            Configure_ECG();
            my_name = "CECG_AGain";
            Job_Message = my_name;
        }

        public override enModuleTestResult isModule_OK()
        {
            CECG_Read_NM_base ecgbase = new(_FY6900);
            ecgbase.AllResults = AllResults;
            ecgbase.AgainValues = AgainValues;
            ecgbase.my_name = my_name;
            ecgbase.Job_Message = Job_Message;
            ecgbase.SerialNumber= SerialNumber;
            ecgbase.ReportMeasurementProgress += Ecgbase_ReportMeasurementProgress; //dazu 8.8.2023
            enModuleTestResult ret = ecgbase.isModule_OK();
            // OnReportMeasurementProgress(Test_Details, Color.Violet);     //weg 8.8.2023
            return ret;
        }

        private void Ecgbase_ReportMeasurementProgress(object sender, string text, Color col)
        {
            OnReportMeasurementProgress(text, col);
        }
    }
}
