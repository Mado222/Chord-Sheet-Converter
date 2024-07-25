using ComponentsLib_GUI;

namespace Insight_Manufacturing5_net8.tests_measurements
{

    public class CNL_Test_Cable_Connection : CNM_Test_Cable_Connection
    {
        public CNL_Test_Cable_Connection(CFY6900 FY6900) : base(FY6900)
        {
            Test_kabel = true;
            Job_Message = "Test Kabelverbindung";
            Pre_Job_Message = "Neurolink mit Strom versorgen und mit PC/Neuromaster verbinden.\nNeuromaster einschalten..";
        }
    }

    public class CNL_Test_XBEE_Connection : CNM_Test_XBEE_Connection
    {
        public CNL_Test_XBEE_Connection(CFY6900 FY6900) : base(FY6900)
        {
            Test_kabel = false;
            Job_Message = "Test XBEE Verbindung";
            Pre_Job_Message = "Neurolink mit PC verbinden, Neuromaster mit Batterien versorgen.\nNeuromaster einschalten.";
        }
    }
}
