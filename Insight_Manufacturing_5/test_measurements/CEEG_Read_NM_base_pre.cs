using ComponentsLib_GUI;
using FeedbackDataLib;
using Math_Net_nuget;
using static Insight_Manufacturing5_net8.CInsightModuleTester_Settings;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CEEG_Read_NM_pre : CEEG_Read_NM_base
    {
        public CEEG_Read_NM_pre(CFY6900 FY6900) : base(FY6900)
        {
            Configure_EEG();
            Uoffset_mV = 0;
            Job_Message = "EEG_pre";
        }
    }

    public class CEEG_Read_NM_base : CRead_Neuromaster
    {
        /*
        //Beta 12Hz < f <= 32Hz
        public const double Beta_upper = 32;
        public const double Beta_lower = 12;

        //Alpha 8Hz < f <= 12Hz
        public const double Alpha_upper = Beta_lower;
        public const double Alpha_lower = 8;

        //Theta_Delta 0.5Hz < f <= 8Hz
        public const double Theta_Delta_upper = Alpha_lower;
        public const double Theta_Delta_lower = 0.5;
        */


        public CEEG_Read_NM_base(CFY6900 FY6900) : base(FY6900)
        {
            InsightModuleTester_Settings = new CInsightModuleTester_Settings(
                enICD.ICD_DisConnected,
                enEEG.EEG_On,
                enUoff.Uoff_Off);

            Job_Message = "EEG_base";
            Uoffset_mV = 0;
        }


        protected override void Additional_DB_Values(CAgainValue setting,
            CNMChannelResults Results, ref dsManufacturing.Neuromodule_DatenRow nrow)
        {
            nrow.EEGTheta_V2 = -1; // Results.ChannelResults[1].Umean;
            nrow.EEGAlpha_V2 = -1; // Results.ChannelResults[2].Umean;
            nrow.EEGBeta_V2 = -1; // Results.ChannelResults[3].Umean;
            nrow.Uoffin_V = Uoffset_mV;
        }

        public override enModuleTestResult isModule_OK()
        {
            enModuleTestResult ret = base.isModule_OK();

            if (ret != enModuleTestResult.Fail_no_further_processing)
            {
                OpenLogFile(@"EEG_Amplitudengain_");
                WriteLogFile("f \t Vpp \t Ueffist \t Ueffsoll \t MeanofMaxima_hex \t MeanofMinima_hex");// \t Alphaist \t Alphasoll \t Betaist \t Betasoll \t Thetaist \t Theta soll");

                for (int i = 0; i < AgainValues.Count; i++)
                {
                    string line = "";
                    CAgainValue againValue = AgainValues.AgainValues[i];

                    //Get Data
                    dsManufacturing _dsManufacturing = new();
                    dsManufacturing.Neuromodule_DatenRow nrow = _dsManufacturing.Neuromodule_Daten.NewNeuromodule_DatenRow();

                    Add_MeasuementResults_to_Row(ref nrow, AllResults[i], againValue);
                    //double f = againValue.f_Hz;
                    double Ueff_ist = nrow.Ueffout_V;

                    //Test Sinus
                    if (!EvaluateAGainValue_Ueff_Percent_Saturation(againValue, AllResults[i].ChannelResults[0]))
                        ret = enModuleTestResult.Fail;

                    if (IsLogFileOpen)
                    {
                        CPeakDetect cp = new();
                        AllResults[i].ChannelResults[0].Get_hexVals_Statistics();  //hexvals sind 0
                        double[] data = new double[AllResults[i].ChannelResults[0].hexvals.Length];
                        Array.Copy(AllResults[i].ChannelResults[0].hexvals, data, AllResults[i].ChannelResults[0].hexvals.Length);
                        cp.CalcPeaks(data, (int)(againValue.MeasureTime_s * againValue.f_Hz));

                        line += againValue.f_Hz.ToString("G0") + "\t" +         //f
                            againValue.Vppin.ToString() + "\t" +                  //Vpp
                            Ueff_ist.ToString() + "\t" +                                        //Ueffist
                            AgainValues.Ueff_EEG_soll[i].ToString() + "\t" +                                //Ueffsoll
                            ((int)cp.Mean_of_Maxima).ToString("X4") + "\t" +                    //MeanofMaxima_hex
                            ((int)cp.Mean_of_Minima).ToString("X4") + "\t";                     //MeanofMinima_hex
                    }

                    WriteLogFile(line);
                }
            }
            CloseLogFile();
            ModuleTestResult = ret;
            return ret;
        }

        public override List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            List<CSWChannelInfo> ret = base.Get_SWChannelInfo(OldSWChannelInfos, AllResults);

            //bissl bled programmiert - aber aus Kompatibilitätsgründen so gemacht
            int[] idx_fx_EEG = new int[f_calibration_EEG.Length];

            for (int j = 0; j < f_calibration_EEG.Length; j++)
            {
                for (int i = 0; i < AgainValues.AgainValues.Count; i++)
                {
                    if (f_calibration_EEG[j] == AgainValues.f[i])
                    {
                        idx_fx_EEG[j] = i;
                        break;
                    }
                }
            }
            int idx_f1_EEG = idx_fx_EEG[0];
            int idx_f2_EEG = idx_fx_EEG[1];
            int idx_f3_EEG = idx_fx_EEG[2];

            int idx_fnotch_EEG = 0;
            for (int i = 0; i < AgainValues.AgainValues.Count; i++)
            {
                if (fnotch_EEG == AgainValues.f[i])
                {
                    idx_fnotch_EEG = i;
                    break;
                }
            }

            double Ueff_sin_soll_mean = (AgainValues.Ueff_EEG_soll[idx_f1_EEG] + AgainValues.Ueff_EEG_soll[idx_f2_EEG] + AgainValues.Ueff_EEG_soll[idx_f3_EEG]) / 3;

            //Perioden im Fenster beschneiden
            CNMChannelResult.CVals_Statistics stat_f1_EEG_hex = AllResults[idx_f1_EEG].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(OldSWChannelInfos[0].Offset_hex);
            CNMChannelResult.CVals_Statistics stat_f2_EEG_hex = AllResults[idx_f2_EEG].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(OldSWChannelInfos[0].Offset_hex);
            CNMChannelResult.CVals_Statistics stat_f3_EEG_hex = AllResults[idx_f3_EEG].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(OldSWChannelInfos[0].Offset_hex);
            CNMChannelResult.CVals_Statistics stat_fnotch_EEG_hex = AllResults[idx_fnotch_EEG].ChannelResults[0].Get_hexVals_Statistics();

            if (stat_f1_EEG_hex.isSaturated)
                OnReportMeasurementProgress("EEG f1 saturated!!", Color.Red);

            if (stat_f2_EEG_hex.isSaturated)
                OnReportMeasurementProgress("EEG f2 saturated!!", Color.Red);

            if (stat_f3_EEG_hex.isSaturated)
                OnReportMeasurementProgress("EEG f3 saturated!!", Color.Red);


            //Ueff ... sin + offset
            double Ueff_ist_mean_hex = (stat_f1_EEG_hex.ueff + stat_f2_EEG_hex.ueff + stat_f3_EEG_hex.ueff) / 3;

            //Offset
            double UOffset_ist_hex = (stat_f2_EEG_hex.umean + stat_f3_EEG_hex.umean + (stat_fnotch_EEG_hex.umean - OldSWChannelInfos[0].Offset_hex)) / 3;

            //remove offset from Ueff
            double Ueff_sin_ist_hex = Math.Sqrt(Math.Pow(Ueff_ist_mean_hex, 2) - Math.Pow(UOffset_ist_hex, 2));

            // ;ScaledValue = (HexValue-Offset_hex)*SkalValue_k+ Offset_d
            ret[0].SkalValue_k = Ueff_sin_soll_mean / Ueff_sin_ist_hex;
            ret[0].Offset_hex = OldSWChannelInfos[0].Offset_hex;
            ret[0].Offset_d = -UOffset_ist_hex * ret[0].SkalValue_k;

            return ret;
        }
    }
}
