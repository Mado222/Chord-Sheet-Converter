using ComponentsLib_GUI;
using FeedbackDataLib;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CEMG_Read_NM_pre : CEMG_Read_NM_base
    {
        public CEMG_Read_NM_pre(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "EMG_pre";
            Uoffset_mV = 0;
        }
    }

    public class CEMG_Read_NM_base : CRead_Neuromaster
    {
        public CEMG_Read_NM_base(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_Off);
            Configure_EMG();
            Job_Message = "EMG_Base";
            Uoffset_mV = 0;
        }

        public override enModuleTestResult isModule_OK()
        {
            enModuleTestResult ret = base.isModule_OK(); //check for const val and saturation
            if (ret == enModuleTestResult.OK)
                  ret = base.EvaluateAGainValues_Ueff_Percent_Saturation(AgainValues, AllResults);  //Auswertung über Ueff und Tol [%]
            return ret;
        }

        public override List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            int idxEMG_TUV = 0;
            List<CSWChannelInfo> ret = base.Get_SWChannelInfo(OldSWChannelInfos, AllResults);
            double Ueff_in_soll = AgainValues.AgainValues[idxEMG_TUV].Ueff_soll;

            CNMChannelResult.CVals_Statistics stat_f_EMG_TUV_hex = AllResults[idxEMG_TUV].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(OldSWChannelInfos[0].Offset_hex);
            ret[0].SkalValue_k = Ueff_in_soll / stat_f_EMG_TUV_hex.ueff;
            ret[0].Offset_hex = OldSWChannelInfos[0].Offset_hex;

            if (stat_f_EMG_TUV_hex.isSaturated)
                OnReportMeasurementProgress("EMG saturated!!", Color.Red);

            CNMChannelResult.CVals_Statistics stat_f_EMGGMW_TUV_hex = AllResults[idxEMG_TUV].ChannelResults[1].Get_hexVals_Statistics();
            ret[1].SkalValue_k = Ueff_in_soll / 1.11 / stat_f_EMGGMW_TUV_hex.umean;
            ret[1].Offset_hex = OldSWChannelInfos[1].Offset_hex;

            return ret;
        }

    }
}
