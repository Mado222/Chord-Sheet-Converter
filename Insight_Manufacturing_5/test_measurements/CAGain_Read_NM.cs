using ComponentsLib_GUI;
using WindControlLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.dataSources;
using Insight_Manufacturing5_net8.test_measurements;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    public class CAGain_Read_NM : CRead_Neuromaster
    {
        public CAGain_Read_NM(CFY6900 FY6900) : base(FY6900)
        {
        }
        public override void Wait4Data()
        {
            ModuleTestResult = enModuleTestResult.notChecked;
            base.Wait4Data();
            //Add last datapoints to Diagram
            double f = AgainValues.AgainValues[CntSettings].f_Hz;
            OnUdpateAmplitudeGain(f, Get_v0_dB(), AllResults[CntSettings].ChannelResults[0].Ueff);
        }

        protected override double Get_v0_dB()
        {
            double uin = Get_Ueff_soll(AgainValues.AgainValues[CntSettings].Vppin);
            double vU0 = AllResults[CntSettings].ChannelResults[0].Ueff / uin;
            return 20 * Math.Log10(vU0);
        }

        public override enModuleTestResult isModule_OK()
        {
            return base.isModule_OK();
        }

        public override void Save_Results_to_DB()
        {
            dsManufacturing _dsManufacturing = new();

            //is there already a dataset from wtihin the last hour?
            bool Measured_within_one_hour = true; //false
            dataSources.dsManufacturingTableAdapters.Neuromodule_DatenTableAdapter neuromodule_DatenTableAdapter = new();
            neuromodule_DatenTableAdapter.FillBy_SerialNumber_Order_Desc_by_Date(_dsManufacturing.Neuromodule_Daten, SerialNumber);

            /*
            if (_dsManufacturing.Neuromodule_Daten.Count > 0)
            {
                DateTime dt = _dsManufacturing.Neuromodule_Daten[0].Testdatum;
                if (dt > DateTime.Now - new TimeSpan (0,1,0,0))
                {
                    Measured_within_one_hour = true;
                }
            }*/

            dataSources.dsManufacturingTableAdapters.AmplitudengainTableAdapter amplitudengainTableAdapter = new();
            
            string path = Path.GetDirectoryName(Dir_Results) + @"\" + Job_Message + "_Amplitudegain.csv";
            StreamWriter sw = new(path, false);

            CSave_Binary_Objects.WriteToJsonFile(Path.ChangeExtension(path, ".cfg"), DataReceiver.Connection.Device.ModuleInfos);
            sw.WriteLine("f [Hz]\tUineff [V]\tU0eff [V]\tv0ist [dB]\tv0soll [dB]\tTolerance [+-dB]\tIsOK\tcomment");

            ModuleTestResult = enModuleTestResult.OK;
            bool thisisOK;
            for (int i = 0; i < AllResults.Count; i++)
            {
                CAgainValue agv = AgainValues.AgainValues[i];
                dsManufacturing.AmplitudengainRow arow = _dsManufacturing.Amplitudengain.NewAmplitudengainRow();

                double uin = Get_Ueff_soll(AgainValues.AgainValues[i].Vppin, ConnectedModuleType);
                double f = AgainValues.AgainValues[i].f_Hz;
                double U0eff = AllResults[i].ChannelResults[0].Ueff;
                double vdb = 20 * Math.Log10(U0eff / uin);
                thisisOK = agv.isValueOK_dB(vdb);

                //check saturation
                bool sat = false;
                string comment = "-";
                if (AgainValues.AgainValues[i].MeasureType == enMeasureType.Sinus)
                {
                    int offset = DataReceiver.Connection.Device.ModuleInfos[0].SWChannels[0].SWChannelInfo.Offset_hex;
                    CNMChannelResult.CVals_Statistics stat_ExG_hex = AllResults[i].ChannelResults[0].Get_hexVals_Statistics_cut_sinus_no_hexOffset(offset);
                    sat = stat_ExG_hex.isSaturated;
                }
                else if (AgainValues.AgainValues[i].MeasureType == enMeasureType.Notch)
                    comment = "notch";

                if (!thisisOK || sat)
                    ModuleTestResult = enModuleTestResult.Partially_Failed;

                if (sat) comment = "saturation";

                string s = f.ToString() + "\t" +
                    uin.ToString() + "\t" +
                    U0eff.ToString() + "\t" +
                    vdb.ToString() + "\t" +
                    agv.Again_soll_db.ToString() + "\t" +
                    agv.Again_Tolerance_db.ToString() + "\t" +
                    thisisOK.ToString() + "\t" +
                    comment;
                sw.WriteLine(s);

                if (Measured_within_one_hour)
                {
                    arow.id_amplitudengain = Guid.NewGuid();
                    arow.SerialNumber = SerialNumber;
                    arow.id_neuromodule_kalibrierdaten = last_id_neuromodule_kalibrierdaten;
                    arow.f = f;
                    arow.Uineff = uin;
                    arow.U0eff = U0eff;
                    arow.v0ist = vdb;
                    arow.v0soll = agv.Again_soll_db;
                    arow.Tolerance = agv.Again_Tolerance_db;
                    arow.IsOK = thisisOK;
                    arow.Comment = my_name + " / " + comment;
                    _dsManufacturing.Amplitudengain.Rows.Add(arow);
                }
            }
            sw.Close();
            Test_Details = "AGain: " + Get_TestString();
            if (Measured_within_one_hour)
            {
                amplitudengainTableAdapter.Update(_dsManufacturing.Amplitudengain);
            }
        }
    }
}
