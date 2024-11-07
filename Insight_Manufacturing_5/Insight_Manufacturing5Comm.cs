using FeedbackDataLib;
using FeedbackDataLib.Modules;
using Insight_Manufacturing5_net8.tests_measurements;
using WindControlLib;
using static FeedbackDataLib.C8KanalReceiverCommandCodes;

namespace Insight_Manufacturing5_net8.test_measurements
{
    /// <summary>
    /// Class to connect to Neuromaster
    /// </summary>
    /// <seealso cref="frmInsight_Manufacturing5.tests_measurements.uc_Base_tests_measurements" />
    public partial class CRead_Neuromaster : CBase_tests_measurements
    {
        public void GetFirmwareVersion()
        {
            DataReceiver.Connection.GetNMFirmwareVersion();
        }

        public string UUID {  get; set; }






        /*******************************************************************************/
        /************************Feedback from the Communication ***********************/
        /*******************************************************************************/

        private void Connection_CommandProcessedResponse(object? sender, C8KanalReceiverV2_CommBase.CommandProcessedResponseEventArgs e)
        {
            OnReportMeasurementProgress(e.Command.ToString() + ": " + e.Message, e.MessageColor);
            if (IsDeviceAvailable())
            {
                switch (e.Command)
                {
                    case EnNeuromasterCommand.SetConnectionClosed:
                        if (e.Success)
                        {

                        }
                        break;

                    case EnNeuromasterCommand.GetFirmwareVersion:
                        if (e.Success)
                        {
                            CNMFirmwareVersion NMFirmwareVersion = new();
                            NMFirmwareVersion.UpdateFrom_ByteArray(e.ResponseData, 0);
                            OnReportMeasurementProgress("Neuromaster: " + NMFirmwareVersion.Uuid.ToString() + " connected", Color.Blue);
                            OnReportMeasurementProgress("Neuromaster HW-Version: " + NMFirmwareVersion.HWVersionString, Color.Blue);
                            OnReportMeasurementProgress("Neuromaster SW-Version: " + NMFirmwareVersion.SWVersionString, Color.Blue);

                            OnUUIDChanged(NMFirmwareVersion.Uuid);
                        }
                        break;
                    case EnNeuromasterCommand.ScanModules:
                        if (e.Success)
                        {
                            OnReportMeasurementProgress("ScanModules OK", Color.Green);
                        }
                        else
                        {
                            OnReportMeasurementProgress("Scan Modules failed", Color.Red);
                        }
                        break;
                    case EnNeuromasterCommand.GetDeviceConfig:
                        if (e.Success)
                        {
                        }
                        break;
                    case EnNeuromasterCommand.SetModuleConfig:
                        if (e.Success)
                        {

                        }
                        break;
                    case EnNeuromasterCommand.SetConfigAllModules:
                        if (e.Success)
                        {
                        }
                        break;
                    case EnNeuromasterCommand.GetModuleInfoSpecific:
                        {

                        }
                        break;
                    case EnNeuromasterCommand.GetClock:
                        break;
                    case EnNeuromasterCommand.SetClock:
                        break;

                }
            }
        }

    }
}