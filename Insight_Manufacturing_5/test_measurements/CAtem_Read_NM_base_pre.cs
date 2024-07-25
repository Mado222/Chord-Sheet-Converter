using ComponentsLib_GUI;
using FeedbackDataLib;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CAtem_Read_NM_final : CAtem_Read_NM_pre
    {
        public CAtem_Read_NM_final(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Atem_final";
        }
    }

    public class CAtem_Read_NM_pre : CAtem_Read_NM_base
    {
        public CAtem_Read_NM_pre(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Atem_pre";
            Uoffset_mV = 0;
            Configure_Atem();
        }
    }

    public class CAtem_Read_NM_base : CRead_Neuromaster
    {
        public CAtem_Read_NM_base(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "Atem_base";
            Uoffset_mV = 0;
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_Off);
        }

        protected override void Additional_DB_Values(CAgainValue setting,
            CNMChannelResults Results, ref dsManufacturing.Neuromodule_DatenRow nrow)
        {
            nrow.Atemfrequenz = Results.ChannelResults[1].Urectified_mean;
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            ModuleTestResult = enModuleTestResult.OK;
            bool _ret;

            for (int i = 0; i < AgainValues.Count; i++)
            {
                CAgainValue agv = AgainValues.AgainValues[i];
                //Now Compare
                double Atemfrequenz_ist = AllResults[i].ChannelResults[1].Ueff;

                double Ueff_Atem_ist = Math.Sqrt(Math.Pow(AllResults[i].ChannelResults[0].Ueff, 2) - Math.Pow(AllResults[i].ChannelResults[0].Umean, 2));
                double Atemfrequenz_soll = agv.f_Hz * 60;

                _ret = isAtem_OK(Ueff_Atem_ist, agv, Atemfrequenz_ist, Atemfrequenz_soll);

                Test_Details += "Atem_Amplitude_ist = " + Ueff_Atem_ist.ToString("G2") + "Veff -- ";
                Test_Details += "soll = " + agv.Ueff_soll.ToString("G2") + Environment.NewLine;

                Test_Details += "Atem_Frequenz_ist = " + Atemfrequenz_ist.ToString("N1") + " min-1 --";
                Test_Details += "soll = " + Atemfrequenz_soll.ToString("N1") + Environment.NewLine;
                Test_Details += Get_TestString(_ret) + "; ";
                if (!_ret)
                    ModuleTestResult = enModuleTestResult.Fail;
            }
            return ModuleTestResult;
        }

        public bool isAtem_OK(double Ueff_Atem_ist, CAgainValue agv, double Atemfrequenz_ist, double Atemfrequenz_soll)
        {
            bool ret = false;
            if (agv.isValueOK_Ueff_Percent(Ueff_Atem_ist))
                if (Check_Value_Perecent(Atemfrequenz_ist, Atemfrequenz_soll, agv.Tolerance_percent, agv.Tolerance_percent))
                    ret = true;

            return ret;
        }

        public override List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            List<CSWChannelInfo> ret = base.Get_SWChannelInfo(OldSWChannelInfos, AllResults);
            ret[0].SkalValue_k = OldSWChannelInfos[0].SkalValue_k / AllResults[0].ChannelResults[0].Ueff * AgainValues.AgainValues[0].Ueff_soll;
            return ret;
        }
    }

}
