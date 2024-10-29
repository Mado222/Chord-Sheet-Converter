using ComponentsLib_GUI;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CEMG_Read_NM_final : CEMG_Read_NM_base
    {
        public CEMG_Read_NM_final(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "EMG_no_offset";
            Uoffset_mV = 0;
            Default_Active = false;
        }

        protected override void Additional_DB_Values(CAgainValue setting,
            CNMChannelResults Results, ref dsManufacturing.Neuromodule_DatenRow nrow)
        {
            nrow.Uoffin_V = Uoffset_mV;
        }
    }

    public class CEMG_Read_NM_Offset_Plus : CEMG_Read_NM_final
    {
        public CEMG_Read_NM_Offset_Plus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_On,
                enUoffLevel.UoffLevel_Low,
                enUoffPolarity.Polarity_Plus);

            Job_Message = "EMG_Offset_plus";
            Uoffset_mV = CInsightModuleTesterV1.Uoff_Low_mV;

            Default_Active = false;
        }
    }

    public class CEMG_Read_NM_Offset_Minus : CEMG_Read_NM_final
    {
        public CEMG_Read_NM_Offset_Minus(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_On,
                enUoffLevel.UoffLevel_Low,
                enUoffPolarity.Polarity_Minus);

            Job_Message = "EMG_Offset_minus";
            Uoffset_mV = -CInsightModuleTesterV1.Uoff_Low_mV;

            Default_Active = false;
        }
    }

    public class CEMG_AGain_Read_NM : CAGain_Read_NM
    {
        public CEMG_AGain_Read_NM(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_Off);
            Configure_EMG();
            my_name = "CEMG_AGain";
            Job_Message = my_name;
        }

        public override enModuleTestResult isModule_OK()
        {
            CEMG_Read_NM_base emgbase = new(_FY6900);
            emgbase.AllResults = AllResults;
            emgbase.AgainValues = AgainValues;
            emgbase.my_name = my_name;
            emgbase.Job_Message = Job_Message;
            emgbase.SerialNumber = SerialNumber;
            emgbase.ReportMeasurementProgress += Emgbase_ReportMeasurementProgress;
            enModuleTestResult ret = emgbase.isModule_OK();
            //OnReportMeasurementProgress(emgbase.Test_Details, Color.Violet);
            return ret;
        }

        private void Emgbase_ReportMeasurementProgress(object sender, string text, Color col)
        {
            OnReportMeasurementProgress(text, col);
        }
    }
}
