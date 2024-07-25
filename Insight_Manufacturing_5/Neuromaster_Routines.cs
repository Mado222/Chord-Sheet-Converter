using FeedbackDataLib;
using Insight_Manufacturing5_net8.tests_measurements;
using System.Drawing;   
using System.IO;
using System.Windows.Forms;

namespace Insight_Manufacturing5_net8
{
    public partial class frmInsight_Manufacturing5
    {
        private void Update_dgvMeasurments()
        {
            dgvMeasurements.DataSource = null;
            dgvMeasurements.SuspendLayout();
            dgvMeasurements.DataSource = measurements.Measurements_Items;
            dgvMeasurements.ResumeLayout(true);
            dgvMeasurements.Update();
            dgvMeasurements.Refresh();
        }

        /// <summary>
        /// Posts the process neuromodul.
        /// </summary>
        /// <returns>Module was processes</returns>
        public bool Post_Process_Neuromaster(CBase_tests_measurements Current_Measurement_object)
        {
            bool ret = false;

            switch (Current_Measurement_object)
            {
                case CFlashNeuromaster _:
                    {
                        CFlashNeuromaster flash_master = (CFlashNeuromaster)Current_Measurement_object;
                        if (flash_master.my_name == "flash_nm")
                        {
                            if (measurements.Measurements_Items[cnt_scan].Measurement_SavetoDB)
                            {
                                flash_master.Save_Results_to_DB();
                            }
                        }
                        break;
                    }

/*
                case CNM_Test_Connection _:
                    {
                        CNM_Test_Connection test_connection = (CNM_Test_Connection)Current_Measurement_object;
                        if (measurements.Measurements_Items[cnt_scan].Measurement_SavetoDB)
                        {
                            test_connection.Save_Results_to_DB();
                        }

                        break;
                    }
*/
                case CNM_Test_All_HW_Channels _:
                    {
                        CNM_Test_All_HW_Channels get_hw_ch = (CNM_Test_All_HW_Channels)Current_Measurement_object;
                        if (measurements.Measurements_Items[cnt_scan].Measurement_SavetoDB)
                        {
                            get_hw_ch.Save_Results_to_DB();
                        }

                        break;
                    }

                default:
                    break;
            }

            Application.DoEvents();
            return ret;
        }

    }
}