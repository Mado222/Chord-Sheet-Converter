using FeedbackDataLib;
using FeedbackDataLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnNeuromasterCommand = FeedbackDataLib.C8KanalReceiverCommandCodes.EnNeuromasterCommand;

namespace Neuromaster_V5
{
        public partial class NeuromasterV5 :Form
    {
        /*******************************************************************************/
        /************************Feedback from the Communication ***********************/
        /*******************************************************************************/

        private void Connection_CommandProcessedResponse(object? sender, C8KanalReceiverV2_CommBase.CommandProcessedResponseEventArgs e)
        {
            RunOnUiThread(() =>
            {
                AddStatusString(e.Command.ToString() + ": " + e.Message, e.MessageColor);
                if (IsDeviceAvailable())
                {
                    switch (e.Command)
                    {
                        case EnNeuromasterCommand.SetConnectionClosed:
                            if (e.Success)
                            {
                                AddStatusString("Close Connection OK", Color.Green);
                            }
                            else
                            {
                                AddStatusString("Close Connection not confirmed", Color.Red);
                            }

                            GoDisconnected();
                            DontReconnectOntbConnect_ToState1 = true;
                            tbConnect.Enabled = true;
                            break;
                        
                        case EnNeuromasterCommand.GetFirmwareVersion:
                            CNMFirmwareVersion NMFirmwareVersion = new();
                            NMFirmwareVersion.UpdateFrom_ByteArray(e.ResponseData, 0);
                            AddStatusString("NM UID: " + NMFirmwareVersion.Uuid, Color.DarkOliveGreen);
                            AddStatusString("NM HW Version: " + NMFirmwareVersion.HWVersionString, Color.DarkOliveGreen);
                            AddStatusString("NM SW Version: " + NMFirmwareVersion.SWVersionString, Color.DarkOliveGreen);
                            break;

                        case EnNeuromasterCommand.ScanModules:
                            break;
                        case EnNeuromasterCommand.GetDeviceConfig:
                            if (e.Success)
                            {
                                if ((DataReceiver!.Connection!.Device!.ModuleInfos != null) && (DataReceiver.Connection.Device.ModuleInfos.Count > 0))
                                {
                                    //Backup module configuration
                                    CModuleBase[] cmi = new CModuleBase[DataReceiver.Connection.Device.ModuleInfos.Count];
                                    DataReceiver.Connection.Device.ModuleInfos.CopyTo(cmi);
                                    BU_ModuleInfo = new List<CModuleBase>(cmi);
                                }

                                DataReceiver.Connection.EnableDataReadyEvent = true;

                                /*
                                SignalFilters.Clear();
                                for (int i = 0; i < DataReceiver.Connection.Device.ModuleInfos.Count; i++)
                                {
                                    SignalFilters.Add(new CSignalFilter(enumSignalFilterType.BandStop, 2, DataReceiver.Connection.Device.ModuleInfos[i].SWChannels[0].SampleInt, 2));
                                }*/
                                cChannelsControlV2x11.SetModuleInfos(DataReceiver.Connection.Device.GetModuleInfo_Clone());
                                cChannelsControlV2x11.Refresh();
                                SetupFlowChart();
                                Init_Graphs();

                                //Is EEG dabei?
                                for (int i = 0; i < DataReceiver!.Connection!.Device!.ModuleInfos!.Count; i++)
                                {
                                    if (DataReceiver.Connection.Device.ModuleInfos[i].ModuleType == enumModuleType.cModuleEEG)
                                        tmrUpdateFFT.Start();
                                }
                            }
                            break;
                        case EnNeuromasterCommand.SetModuleConfig:
                            if (e.Success)
                            {
                                AddStatusString("Config set: " + e.HWcn.ToString(), Color.Green);
                                pbXBeeChannelCapacity.Value = DataReceiver!.Connection!.GetChannelCapcity();
                                lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();
                                ConfigSetOK = true;
                                SetupFlowChart();
                            }
                            break;
                        case EnNeuromasterCommand.SetConfigAllModules:
                            if (e.Success)
                            {
                                pbXBeeChannelCapacity.Value = DataReceiver!.Connection!.GetChannelCapcity();
                                lblXBeeCapacity.Text = pbXBeeChannelCapacity.Value.ToString();
                                ConfigSetOK = true;
                                cChannelsControlV2x11.Refresh();
                            }
                            break;
                        case EnNeuromasterCommand.GetModuleInfoSpecific:
                            {
                                //Ok
                                AddStatusString("Module specific read OK", Color.Green);
                                if (IsDeviceAvailable())
                                {
                                    cChannelsControlV2x11.UpdateModuleSpecificInfo(DataReceiver!.Connection!.Device!.ModuleInfos[e.HWcn]);
                                    cChannelsControlV2x11.Refresh();


                                    byte[] buf = DataReceiver.Connection.Device.ModuleInfos[e.HWcn].GetModuleSpecific();
                                    string s = "Reci: ";
                                    for (int i = 0; i < buf.Length; i++)
                                    {
                                        s += buf[i].ToString() + ", ";
                                    }
                                    AddStatusString(s, Color.Blue);
                                    SetupFlowChart();
                                    Init_Graphs();
                                }
                            }
                            break;
                        case EnNeuromasterCommand.GetClock:
                            txtTime.Text = DataReceiver!.Connection!.DeviceClock.Dt.ToString();
                            break;
                        case EnNeuromasterCommand.SetClock:
                            txtTime.Text = DateTime.Now.ToString();
                            break;

                    }
                }
            });
        }
        
        public void RunOnUiThread(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }
        public bool IsDeviceAvailable()
        {
            if (DataReceiver is null) return false;
            if (DataReceiver.Connection is null) return false;
            if (DataReceiver.Connection.Device is null) return false;
            return true;
        }


    }
}
