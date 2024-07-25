using FeedbackDataLib;
using WindControlLib;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="frmInsight_Manufacturing5.tests_measurements.uc_Base_tests_measurements"/>
    public partial class CFlashNeuromaster : CFlashNeuromodul //CBase_tests_measurements
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CFlashNeuromaster"/> class.
        /// </summary>
        public CFlashNeuromaster(CMicrochip_Programmer mc_programmer): base (mc_programmer)
        {
            InsightModuleTester_Settings = null;
        }
    }
}
