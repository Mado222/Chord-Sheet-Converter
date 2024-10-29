using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using ComponentsLib_GUI;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CEEG_Read_NM_final : CEEG_AGain_Read_NM //CEEG_Read_NM_base
    {
        public CEEG_Read_NM_final(CFY6900 FY6900) : base(FY6900)
        {
            Configure_EEG();
            Job_Message = "EEG_final";
            Uoffset_mV = 0;
            Default_Active = true;
        }
    }

    public class CEEG_Read_NM_Offset_Plus : CEEG_AGain_Read_NM //CEEG_Read_NM_final
    {
        public CEEG_Read_NM_Offset_Plus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_On,
                enUoff.Uoff_On,
                enUoffLevel.UoffLevel_Low,
                enUoffPolarity.Polarity_Plus);

            Job_Message = "EEG_Offset_plus";
            Uoffset_mV = CInsightModuleTesterV1.Uoff_Low_mV;
            Default_Active = false;
        }
    }

    public class CEEG_Read_NM_Offset_Minus : CEEG_AGain_Read_NM //CEEG_Read_NM_final
    {

        public CEEG_Read_NM_Offset_Minus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_On,
                enUoff.Uoff_On,
                enUoffLevel.UoffLevel_Low,
                enUoffPolarity.Polarity_Minus);

            Job_Message = "EEG_Offset_minus";
            Uoffset_mV = -CInsightModuleTesterV1.Uoff_Low_mV;
            Default_Active = false;
        }
    }

    public class CEEG_AGain_Read_NM : CAGain_Read_NM
    {
        public CEEG_AGain_Read_NM(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_On,
                enUoff.Uoff_Off);
            Configure_EEG();
            my_name = "CEEG_AGain";
            Job_Message = my_name;
        }

        public override enModuleTestResult isModule_OK()
        {
            CEEG_Read_NM_base eegbase = new(_FY6900);
            eegbase.AllResults = AllResults;
            eegbase.AgainValues = AgainValues;
            eegbase.my_name = my_name;
            eegbase.Job_Message = Job_Message;
            eegbase.SerialNumber = SerialNumber;
            eegbase.ReportMeasurementProgress += Eegbase_ReportMeasurementProgress;
            enModuleTestResult ret = eegbase.isModule_OK();
            //OnReportMeasurementProgress(eegbase.Test_Details, Color.Violet);
            return ret;
        }

        private void Eegbase_ReportMeasurementProgress(object sender, string text, Color col)
        {
            OnReportMeasurementProgress(text, col);
        }
    }
}
