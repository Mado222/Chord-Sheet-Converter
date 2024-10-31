using System.Drawing;
using Phidget22;
using Phidget22.Events;
using System.Collections.Generic;

namespace PhidgetLib
{
    /// <summary>
    /// Umstieg von Version 2.1 auf 2.2: 2019.11.06
    /// https://www.phidgets.com/docs/Upgrading_Code_from_Phidget21_to_Phidget22
    /// </summary>
    public class CPhidgetLib22
    {
        #region Properties
        public string PhName { get; set; } = "";
        public string PhSerial { get; private set; } = "-1";
        public string PhVersion { get; private set; } = "";
        public bool PhAttached
        { 
            get 
            {
                if (ph_digitalOutputs != null)
                    return ph_digitalOutputs[0].Attached;
                return false;
            } 
        }
        #endregion

        #region Events
        public delegate void ReportStatusEventHandler(object sender, string text, System.Drawing.Color col);
        public event ReportStatusEventHandler? ReportMeasurementProgress;
        protected virtual void OnReportMeasurementProgress(string text, System.Drawing.Color col)
            => ReportMeasurementProgress?.Invoke(this, text, col);
        #endregion

        private List<DigitalOutput> ph_digitalOutputs = [];

        public CPhidgetLib22()
        {
            try
            {
                //Prepare Phidget
                for (int i = 0; i < 8; i++)
                {
                    DigitalOutput ph_digitalOutput = new()
                    {
                        Channel = i,
                        DeviceSerialNumber = -1,
                        HubPort = -1,
                        IsHubPortDevice = false,
                        IsLocal = true
                    };
                    ph_digitalOutput.Error -= CPhidgetLib22_Error;
                    ph_digitalOutput.Attach -= CPhidgetLib22_Attach;
                    ph_digitalOutput.Detach -= CPhidgetLib22_Detach;

                    ph_digitalOutput.Error += CPhidgetLib22_Error;
                    ph_digitalOutput.Attach += CPhidgetLib22_Attach;
                    ph_digitalOutput.Detach += CPhidgetLib22_Detach;

                    ph_digitalOutputs.Add(ph_digitalOutput);
                }
            }
            catch (PhidgetException ex)
            {
                OnReportMeasurementProgress("Phidget: " + ex.ToString(), Color.Red);
            }
        }

        ~CPhidgetLib22()
        {
            Close();
        }

        private void CPhidgetLib22_Detach(object sender, DetachEventArgs e)
        {
            PhName = "";
            PhSerial = "";
            PhVersion = "";
            ph_digitalOutputs[0].Close();
            OnReportMeasurementProgress("Phidget: Disconnected", Color.Red);
        }

        private void CPhidgetLib22_Attach(object sender, AttachEventArgs e)
        {
            DigitalOutput attachedDevice = (DigitalOutput)sender;

            if (attachedDevice.Attached)
            {
                try
                {
                    PhName = attachedDevice.DeviceName;
                    PhSerial = attachedDevice.DeviceSerialNumber.ToString();
                    PhVersion = attachedDevice.DeviceVersion.ToString();
                    OnReportMeasurementProgress("Phidget: " + PhName + " channel " + attachedDevice.ChannelClassName + " found", Color.Green);
                }
                catch (PhidgetException fe)
                {
                    OnReportMeasurementProgress("Phidget Error: " + fe.Description, Color.Red);
                }
            }
        }

        private void CPhidgetLib22_Error(object sender, Phidget22.Events.ErrorEventArgs e)
        {
            //Phidget phid = (Phidget)sender;
            OnReportMeasurementProgress("Phidget Error: " + e.Description, Color.Red);
        }

        public bool Open()
        {
            bool ret = true;
            if (!PhAttached)
            {
                try
                {
                    foreach (DigitalOutput dout in ph_digitalOutputs)
                    {
                        dout.Open(5000); //open the device specified by the above parameters
                        ret = dout.Attached;
                    }
                }
                catch (PhidgetException ph)
                {
                    OnReportMeasurementProgress("Error opening Phidget: " + ph.Message, Color.Red);
                    ret = false;
                }
            }
            return ret;
        }

        public void Close()
        {
            try
            {
                foreach (DigitalOutput dout in ph_digitalOutputs)
                    if (dout.Attached) dout.Close();

            }
            catch (PhidgetException ph)
            {
                OnReportMeasurementProgress("Error closing Phidget: " + ph.Message, Color.Red);
            }
        }

        public void SetOutPut(int output_number, bool value)
        {
            if (PhAttached)
            {
                ph_digitalOutputs[output_number].State = value;
            }
            else
                OnReportMeasurementProgress("Error during set: Phidget channel " + output_number.ToString() + " not attached", Color.Red);
        }

        public bool GetOutPut(int output_number)
        {
            if (PhAttached)
            {
                return ph_digitalOutputs[output_number].State;
            }
            OnReportMeasurementProgress("Error during get: Phidget channel " + output_number.ToString() + " not attached", Color.Red);
            return false;
        }
    }
}
