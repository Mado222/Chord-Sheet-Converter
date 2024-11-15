using System.Management;
using WindControlLib;

namespace BMTCommunicationLib
{
#pragma warning disable CA1416 // Validate platform compatibility
    public class CUSBDeviceMonitor(CVidPid vidpid)
    {
        private readonly CVidPid _deviceVidPid = vidpid;
        private ManagementEventWatcher? _creationWatcher;
        private ManagementEventWatcher? _deletionWatcher;

        public event EventHandler? USBDeviceConnectedEvent;
        public event EventHandler? USBDeviceDisConnectedEvent;

        public void StartMonitoring()
        {
            if (USBDeviceConnectedEvent != null)
            {
                _creationWatcher = StartWatcher("__InstanceCreationEvent", USBDeviceConnectedEvent);
            }
            if (USBDeviceDisConnectedEvent != null)
            {
                _deletionWatcher = StartWatcher("__InstanceDeletionEvent", USBDeviceDisConnectedEvent);
            }
        }

        public void StopMonitoring()
        {
            _creationWatcher?.Stop();
            _creationWatcher?.Dispose();
            _deletionWatcher?.Stop();
            _deletionWatcher?.Dispose();
        }

        private ManagementEventWatcher StartWatcher(string eventType, EventHandler eventHandler)
        {
            string queryString = $"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'";
            ManagementEventWatcher watcher = new (new WqlEventQuery(queryString));
            watcher.EventArrived += (sender, args) =>
            {
                if (args.NewEvent["TargetInstance"] is ManagementBaseObject targetInstance)
                {
                    string deviceId = targetInstance["DeviceID"]?.ToString() ?? "";
                    if (deviceId != null && deviceId.Contains(_deviceVidPid.VID_PID))
                    {
                        eventHandler?.Invoke(this, EventArgs.Empty);
                    }
                }
            };
            watcher.Start();
            return watcher;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility