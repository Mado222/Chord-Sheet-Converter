using ComponentsLib_GUI;
using FeedbackDataLib;
using Math_Net_nuget;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CECG_Read_NM_pre : CECG_Read_NM_base
    {
        public CECG_Read_NM_pre(CFY6900 FY6900) : base(FY6900)
        {
            Job_Message = "ECG_pre";
            Uoffset_mV = 0;
        }
    }
    public class CECG_Read_NM_base : CRead_Neuromaster
    {
        public CECG_Read_NM_base(CFY6900 FY6900) : base(FY6900)
        {
            Configure_ECG();
            Job_Message = "ECG_base";
            Uoffset_mV = 0;
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_Off,
                enUoff.Uoff_Off);
        }

        public override enModuleTestResult isModule_OK()
        {
            ModuleTestResult = base.isModule_OK();

            if (ModuleTestResult != enModuleTestResult.Fail_no_further_processing && !Job_Message.ToLower().Contains("gain"))
            {
                bool _ret;
                string line;

                OpenLogFile(Job_Message + @"_Detailed_Results_");
                WriteLogFile("f \t Vpp \t Ueffist \t Ueffsoll \t MeanofMaxima_hex \t MeanofMinima_hex ");

                for (int i = 0; i < AgainValues.Count; i++)
                {
                    CAgainValue againValue = AgainValues.AgainValues[i];
                    CNMChannelResult.CVals_Statistics scs0 = AllResults[i].ChannelResults[0].Get_scaledVals_Statistics();
                    double Ueff_ECG_ist = scs0.ueff;
                    _ret = false;

                    if ((againValue.MeasureType == enMeasureType.Sinus) || (againValue.MeasureType == enMeasureType.Notch))
                    {
                        //Test Sinus
                        _ret = EvaluateAGainValue_Ueff_Percent_Saturation(againValue, AllResults[i].ChannelResults[0]);

                        if (IsLogFileOpen)
                        {
                            CPeakDetect cp = new();
                            AllResults[i].ChannelResults[0].Get_hexVals_Statistics();
                            double[] data = new double[AllResults[i].ChannelResults[0].hexvals.Length];
                            Array.Copy(AllResults[i].ChannelResults[0].hexvals, data, AllResults[i].ChannelResults[0].hexvals.Length);
                            cp.CalcPeaks(data, (int)(againValue.MeasureTime_s * againValue.f_Hz));

                            //Prepare log file
                            line = againValue.f_Hz.ToString("G0") + "\t" +
                             againValue.Vppin.ToString() + "\t" +
                             Ueff_ECG_ist.ToString() + "\t" +
                             Math.Abs(againValue.Ueff_soll).ToString() + "\t" +
                             ((int)cp.Mean_of_Maxima).ToString("X4") + "\t" +
                             ((int)cp.Mean_of_Minima).ToString("X4");// + "\t" +
                            WriteLogFile(line);
                        }
                    }
                    else
                    {
                        //Puls testen
                        if (Check_Value_Perecent(AllResults[i].ChannelResults[1].Get_scaledVals_Statistics().umedian, //HR_bpm_ist
                            againValue.hr_bpm_soll, //ECG_HR_bpm_soll[idx_Abitrary],
                            ECG_HR_Tolerance_Percent,
                            ECG_HR_Tolerance_Percent))
                        {
                            _ret = true;
                        }

                        Test_Details += Get_Arbitrary_File_String(againValue) + ":";
                        Test_Details += Get_TestString(_ret) + ";";
                    }
                    if (!_ret)
                        ModuleTestResult = enModuleTestResult.Fail;
                }
                CloseLogFile();
            }
            return ModuleTestResult;
        }

        private string Get_Arbitrary_File_String(CAgainValue setting)
        {
            string ret = "";
            if (setting.ArbitraryFile != "")
            {
                ret = setting.ArbitraryFile + "_" + setting.f_Hz.ToString("G2") + "Hz_" + setting.Vppin.ToString("G2") + "Vpp";
            }
            return ret;
        }

        protected override void Additional_DB_Values(CAgainValue setting,
                CNMChannelResults Results, ref dsManufacturing.Neuromodule_DatenRow nrow)
        {
            if (setting.ArbitraryFile != "")
            {
                nrow.ECG_Lib_File_Name = Get_Arbitrary_File_String(setting);

                CNMChannelResult.CVals_Statistics scs1 = Results.ChannelResults[1].Get_scaledVals_Statistics();
                double hr = scs1.umedian;
                nrow.ECG_BPM = hr;
                nrow.Uoffin_V = Uoffset_mV;
            }
        }
        
        public override List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            int idxECG_TUV = 0;
            List<CSWChannelInfo> ret = base.Get_SWChannelInfo(OldSWChannelInfos, AllResults);
            double Ueff_in_soll = AgainValues.AgainValues[idxECG_TUV].Ueff_soll;

            CNMChannelResult.CVals_Statistics stat_f_ECG_TUV_hex = AllResults[idxECG_TUV].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(OldSWChannelInfos[0].Offset_hex);
            ret[0].SkalValue_k = Ueff_in_soll / stat_f_ECG_TUV_hex.ueff;
            ret[0].Offset_hex = OldSWChannelInfos[0].Offset_hex;

            if (stat_f_ECG_TUV_hex.isSaturated)
                OnReportMeasurementProgress("ECG saturated!!", Color.Red);

            ret[2].SkalValue_k = ret[0].SkalValue_k;
            ret[2].Offset_hex = OldSWChannelInfos[2].Offset_hex;

            return ret;
        }
    }
      
}
