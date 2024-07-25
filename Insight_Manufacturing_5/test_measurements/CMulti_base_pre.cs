using ComponentsLib_GUI;
using FeedbackDataLib;
using System.Text.RegularExpressions;
using Insight_Manufacturing5_net8.dataSources;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CMulti_Read_NM_base : CRead_Neuromaster
    {
        public CMulti_Read_NM_base(CFY6900 FY6900= null) : base(FY6900)
        {
            InsightModuleTester_Settings = null;
            Configure_Multi();
            Job_Message = "Multi_base";
        }

        public override enModuleTestResult isModule_OK()
        {
            base.isModule_OK();
            ModuleTestResult = enModuleTestResult.Fail;
            enModuleTestResult ret = enModuleTestResult.Fail;

            dsManufacturing _dsManufacturing = new dsManufacturing();
            dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new dataSources.dsManufacturingTableAdapters.NeurodevicesTableAdapter();

            neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
            if (_dsManufacturing.Neurodevices.Count == 1)
            {
                if (!_dsManufacturing.Neurodevices[0].IsTest_DetailsNull())
                    if (_dsManufacturing.Neurodevices[0].Test_Details != "")
                    {
                        if (IsMulti_OK(_dsManufacturing.Neurodevices[0].Test_Details))
                        {
                            ModuleTestResult = enModuleTestResult.OK;
                            ret = enModuleTestResult.OK;
                        }
                    }
            }

            Add_TestDetails_toDB("");
            
            return ret;
        }

        public static bool IsMultiSCL1_OK(double SCL1_ist)
        {
            return Check_Value_Perecent(1 / SCL1_ist, Multi_R1_soll, Multi_SCL_Tolerance_Percent, Multi_SCL_Tolerance_Percent);
        }

        public static bool IsMultiSCL2_OK(double SCL2_ist)
        {
            return Check_Value_Perecent(1 / SCL2_ist, Multi_R2_soll, Multi_SCL_Tolerance_Percent, Multi_SCL_Tolerance_Percent);
        }

        public static bool IsMultiSCL3_OK(double SCL3_ist)
        {
            return Check_Value_Perecent(1 / SCL3_ist, Multi_R3_soll, Multi_SCL_Tolerance_Percent, Multi_SCL_Tolerance_Percent);
        }

        public static double Multi_Temp1_soll()
        {
            return Calc_Temp_from_R(Multi_TempR1_soll); //=26.57 Temp zu Multi_TempR1_soll
        }

        public static double Multi_Temp2_soll()
        {
            return Calc_Temp_from_R(Multi_TempR2_soll); //=37.18 Temp zu Multi_TempR2_soll
        }

        private static double Calc_Temp_from_R(double R)
        {
            return -23.80952381 * Math.Log(R * 6.97015E-06);
        }

        public static bool IsMulti_OK(string Multi_Test_Details)
        {
            if (Multi_Test_Details.Contains(":" + Test_failed))
                return false;

            if (Regex.Matches(Multi_Test_Details, ":" + Test_success).Count != Multi_Test_Details_keywords.Length)
                return false;

            return true;
        }

        public static bool IsMultiTEMP1_OK(double Temperature1_ist)
        {
            return IsMultiTEMP_OK(Temperature1_ist, Multi_Temp1_soll());
        }

        public static bool IsMultiTEMP2_OK(double Temperature2_ist)
        {
            return IsMultiTEMP_OK(Temperature2_ist, Multi_Temp2_soll());
        }

        public static bool IsMultiTEMP3_OK(double Temperature3_ist, double Temperature3_soll)
        {
            return IsMultiTEMP_OK(Temperature3_ist, Temperature3_soll);
        }

        public static bool IsMultiTEMP_OK(double Temperature_ist, double Temperature_soll)
        {
            if (Temperature_ist > Temperature_soll + Multi_Temp_Abweichung) return false;
            if (Temperature_ist < Temperature_soll - Multi_Temp_Abweichung) return false;
            return true;
        }

        public static string Make_Multi_Test_Details_string(string SCL1, string SCL2, string SCL3, string Temp1, string Temp2, string Temp3, string BPM)
        {
            string newString = "";

            if (SCL1 != "")
                newString += "SCL1:" + SCL1 + ";";

            if (SCL2 != "")
                newString += "SCL2:" + SCL2 + ";";

            if (SCL3 != "")
                newString += "SCL3:" + SCL3 + ";";

            if (Temp1 != "")
                newString += "Temp1:" + Temp1 + ";";

            if (Temp2 != "")
                newString += "Temp2:" + Temp2 + ";";

            if (Temp3 != "")
                newString += "Temp3:" + Temp3 + ";";

            if (BPM != "")
                newString += "BPM:" + BPM + ";";

            return newString;
        }
        public override List<CSWChannelInfo> Get_SWChannelInfo(List<CSWChannelInfo> OldSWChannelInfos, List<CNMChannelResults> AllResults)
        {
            List<CSWChannelInfo> ret = base.Get_SWChannelInfo(OldSWChannelInfos, AllResults);
            //SCL
            double hex1 = (1 / AllResults[0].ChannelResults[0].Umean - OldSWChannelInfos[0].Offset_d) / OldSWChannelInfos[0].SkalValue_k;
            double hex2 = (1 / AllResults[1].ChannelResults[0].Umean - OldSWChannelInfos[0].Offset_d) / OldSWChannelInfos[0].SkalValue_k;

            ret[0].SkalValue_k = (Multi_R1_soll - Multi_R2_soll) / (hex1 - hex2);
            ret[0].Offset_d = (Multi_R1_soll - hex1 * ret[0].SkalValue_k);

            //Temperature
            hex1 = (AllResults[0].ChannelResults[1].Umean - OldSWChannelInfos[1].Offset_d) / OldSWChannelInfos[1].SkalValue_k;
            hex2 = (AllResults[1].ChannelResults[1].Umean - OldSWChannelInfos[1].Offset_d) / OldSWChannelInfos[1].SkalValue_k;

            ret[1].SkalValue_k = (Multi_Temp1_soll() - Multi_Temp2_soll()) / (hex1 - hex2);
            ret[1].Offset_d = (Multi_Temp1_soll() - hex1 * ret[1].SkalValue_k);

            return ret;
        }
    }
}
