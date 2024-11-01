using Microsoft.Win32;
using System.Collections;
using System.Management;

#pragma warning disable CA1416 // Validate platform compatibility
namespace BMTCommunicationLib
{
    /*
     * http://www.mikrocontroller.net/topic/108036
     in der registry sind unter

HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E978-E325-11CE-BFC1-08002BE10318}

alle comports gelistet.
dort kannst du ueber DriverDesc oder MatchingDeviceId das richtige
device rausfinden.
wenn du die hardware-id kennst, kannst du den ersten schritt weglassen.
unter

HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\

werden anhand der MatchinDeviceId die Devices ebenfalls nocheinmal
gelistet.
wenn das device aktiv ist, kannst du auf

HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\<MatchingDeviceId>\Control\ActiveService

(wobei <MatchingDeviceId> natuerlich ersetzt werden sollte ;))
zugreifen, wenn es nicht aktiv ist, bekommst du einen FILE_NOT_FOUND
zurueck.
in

HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\<MatchingDeviceId>\Device
Parameters\PortName

schliesslich stehen die vergebenen comports, wie zb 'COM5' oder so.

zumindest kann man auf diesem weg eine spezielle id zu einem device
herausfinden. ob es einen einfacheren weg fuer eine liste aller
vergebenen ids gibt, weiss ich nicht.
     * */

    public class CComPortInfo : IComparer<CComPortInfo>, IComparer, IComparable
    {

        public CComPortInfo()
        {
            _ComName = string.Empty;
            _Driver = string.Empty;
            _DriverDesc = string.Empty;
            _FriendlyName = string.Empty;
            _HardwareID = string.Empty;
            _Manufacturer = string.Empty;
            _MatchingDeviceId = string.Empty;
            _Service = string.Empty;
            _FTDIBusKeyName = string.Empty;
        }

        #region Properties
        private string _ComName;
        public string ComName
        {
            get { return _ComName; }
            set { _ComName = value; }
        }

        private string _Manufacturer;
        public string Manufacturer
        {
            get { return _Manufacturer; }
            set { _Manufacturer = value; }
        }

        private string _DriverDesc;
        public string DriverDesc
        {
            get { return _DriverDesc; }
            set { _DriverDesc = value; }
        }

        private string _MatchingDeviceId;
        public string MatchingDeviceId
        {
            get { return _MatchingDeviceId; }
            set { _MatchingDeviceId = value; }
        }

        public string VID_PID
        {
            get
            {
                //"ftdibus\\comport&vid_0403&pid_6001"
                int idx = _MatchingDeviceId.IndexOf("vid");
                return _MatchingDeviceId.Substring(idx, 17);
            }
        }

        private string _Service;
        public string Service
        {
            get { return _Service; }
            set { _Service = value; }
        }

        private string _FriendlyName;
        public string FriendlyName
        {
            get { return _FriendlyName; }
            set { _FriendlyName = value; }
        }

        private string _HardwareID;
        public string HardwareID
        {
            get { return _HardwareID; }
            set { _HardwareID = value; }
        }

        private string _Driver;
        public string Driver
        {
            get { return _Driver; }
            set { _Driver = value; }
        }

        private string _FTDIBusKeyName;
        public string FTDIBusKeyName
        {
            get { return _FTDIBusKeyName; }
            set { _FTDIBusKeyName = value; }
        }


        #endregion    }

        #region IComparer<CComPortInfo> Members
        public int Compare(CComPortInfo? x, CComPortInfo? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (string.Equals(x.MatchingDeviceId, y.MatchingDeviceId))
                return 0;
            return string.Compare(x.MatchingDeviceId, y.MatchingDeviceId);
        }
        #endregion

        #region IComparer Members

        public int Compare(object? x, object? y) => Compare(x, y);

        #endregion

        #region IComparable Members

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1; // Return a default comparison for null
            if (obj is CComPortInfo other)
            {
                return Compare(this, other);
            }
            throw new ArgumentException("Object is not a CComPortInfo", nameof(obj));
        }

        #endregion
    }

    /// <summary>
    /// Class for various COM Port finding strategies
    /// </summary>
    public class CGetComPorts
    {
        private static readonly string key_List_of_ComPorts = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E978-E325-11CE-BFC1-08002BE10318}";
        private static readonly string key_Devices = @"SYSTEM\CurrentControlSet\Enum";

        /// <summary>
        /// returns a list of available COM Ports
        /// </summary>
        /// <param name="Driver_Device_Description_SearchString">Search String (is compared with the "driver description" and "device description")</param>
        /// <remarks>
        /// to get all COM-Ports please set Search String to String.Empty,
        /// search string is not case-sensitive
        /// </remarks>
        /// <returns>list of available COM Ports, which meet the search string</returns>
        public static List<CComPortInfo> GetComPortInfo(string Driver_Device_Description_SearchString)
        {
            return GetComPortInfo(Driver_Device_Description_SearchString, string.Empty, string.Empty);
        }

        /// <summary>
        /// returns a list of available COM Ports
        /// </summary>
        /// <param name="Driver_Device_Description_SearchString">Search String (is compared with the "driver description" and "device description")</param>
        /// <param name="PID_VID_SerachString">PID VID in the form "vid_0403&pid_6010";</param>
        /// <param name="CurrentControlSet_SearchString">This string is e.g. in
        /// HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\FTDIBUS\VID_0403+PID_6010+NeurolinkA\0000</param>
        /// <returns>
        /// list of available COM Ports, which meet the search string
        /// </returns>
        /// <remarks>
        /// to get all COM-Ports please set Search String to String.Empty,
        /// search string is not case-sensitive
        /// </remarks>
        public static List<CComPortInfo> GetComPortInfo(string Driver_Device_Description_SearchString, string PID_VID_SerachString, string CurrentControlSet_SearchString)
        {
            List<CComPortInfo> temp = [];
            List<CComPortInfo> ret = [];

            //lower case
            Driver_Device_Description_SearchString = Driver_Device_Description_SearchString.ToLower();

            RegistryKey? rk;
            rk = Registry.LocalMachine.OpenSubKey(key_List_of_ComPorts, false);

            if (rk != null)
            {
                string[] AllEntries = rk.GetSubKeyNames();
                if (AllEntries.Length > 0)
                {
                    foreach (string s in AllEntries)
                    {
                        string subkey = key_List_of_ComPorts + "\\" + s;
                        try
                        {
                            rk = Registry.LocalMachine.OpenSubKey(subkey, false);
                        }
                        catch
                        {
                            rk = null;
                        }
                        if (rk != null)
                        {
                            CComPortInfo c = new();
                            if (rk.GetValue("DriverDesc") != null)
                            {
                                object? oDriverDesc = rk.GetValue("DriverDesc");
                                if (oDriverDesc != null)
                                {
                                    string DriverDesc = ((string)oDriverDesc).ToLower();  //lower case
                                    if (DriverDesc.Contains(Driver_Device_Description_SearchString))     // NEU
                                    {
                                        c.DriverDesc = (string?)rk.GetValue("DriverDesc") ?? string.Empty;

                                        object? oMatchingDeviceId = rk.GetValue("MatchingDeviceId");

                                        if (oMatchingDeviceId != null)
                                        {
                                            string MatchingDeviceId = ((string)oMatchingDeviceId).ToLower(System.Globalization.CultureInfo.CurrentCulture);
                                            if (MatchingDeviceId.Contains(PID_VID_SerachString, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                c.MatchingDeviceId = (string?)rk.GetValue("MatchingDeviceId") ?? string.Empty;
                                                temp.Add(c);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //sort list and delete duplicate entries
                    temp.Sort();
                    if (temp.Count > 1)
                    {
                        for (int i = temp.Count - 1; i > 0; i--)
                        {
                            if (temp[i].MatchingDeviceId == temp[i - 1].MatchingDeviceId)
                            {
                                temp.RemoveAt(i);
                            }
                        }
                    }

                    //find COM port number
                    foreach (CComPortInfo c in temp)
                    {
                        string subkey;
                        string MatchingDeviceId = c.MatchingDeviceId.ToUpper();

                        int indexOfBackslash = MatchingDeviceId.IndexOf('\\');
                        string mainDir = "";
                        if (indexOfBackslash < 0)
                        {
                            //This is a Hardware COM Port
                            mainDir = MatchingDeviceId;
                        }
                        else
                        {
                            mainDir = MatchingDeviceId[..indexOfBackslash];
                        }

                        if (MatchingDeviceId[0] == '*')
                        {
                            //Subkey ACPI
                            mainDir = mainDir.Remove(0, 1);
                            subkey = key_Devices + "\\ACPI\\" + mainDir;
                        }
                        else
                        {
                            subkey = key_Devices + "\\" + mainDir;
                        }

                        rk = Registry.LocalMachine.OpenSubKey(subkey, false);

                        if (rk != null)
                        {
                            string[] AllEntriesOfMainDir = rk.GetSubKeyNames();
                            List<string> SelectedEntriesOfMainDir = [];

                            //Select only relevant Subkeys
                            foreach (string entry in AllEntriesOfMainDir)
                            {
                                if (entry.Contains(CurrentControlSet_SearchString, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    SelectedEntriesOfMainDir.Add(entry);
                                }
                            }

                            //search in all subkeys (depth 1)
                            string subkeyold = subkey;
                            foreach (string entry in SelectedEntriesOfMainDir)
                            {
                                subkey = subkeyold + "\\" + entry;
                                rk = Registry.LocalMachine.OpenSubKey(subkey, false);
                                //subkey:
                                //SYSTEM\CurrentControlSet\Enum\ACPI\PNP0501\1
                                //SYSTEM\\CurrentControlSet\\Enum\\FTDIBUS\\VID_0403+PID_6010+NeurolinkA
                                if (rk != null)
                                {
                                    string[] subnames = rk.GetSubKeyNames();
                                    string subkey2 = subkey;

                                    if (!subkey.Contains("ACPI"))
                                    {
                                        //FTDI Port
                                        subkey2 += "\\" + subnames[0];
                                    }

                                    //Open Subkeys (depth 2)
                                    rk = Registry.LocalMachine.OpenSubKey(subkey2, false);
                                    if (rk != null)
                                    {
                                        //if (rk.Name.ToLower().Contains(CurrentControlSet_SearchString.ToLower()))        //NEU
                                        {
                                            //read information
                                            CComPortInfo cinfo = new()
                                            {
                                                FTDIBusKeyName = subkey
                                            };

                                            //device description
                                            string? deviceDesc = (string?)rk.GetValue("DeviceDesc");
                                            if (deviceDesc != null)
                                            {
                                                cinfo.DriverDesc = deviceDesc;
                                            }

                                            //matching device id
                                            cinfo.MatchingDeviceId = c.MatchingDeviceId;

                                            //hardware id
                                            string[]? helpStringArray = (string[]?)rk.GetValue("HardwareID");
                                            if (helpStringArray != null)
                                            {
                                                foreach (string a in helpStringArray)
                                                {
                                                    cinfo.HardwareID += a;
                                                }
                                            }

                                            // Service
                                            string? service = (string?)rk.GetValue("Service");
                                            if (service != null)
                                            {
                                                cinfo.Service = service;
                                            }

                                            // Friendly Name
                                            string? friendlyName = (string?)rk.GetValue("FriendlyName");
                                            if (friendlyName != null)
                                            {
                                                cinfo.FriendlyName = friendlyName;
                                            }

                                            // Driver
                                            string? driver = (string?)rk.GetValue("Driver");
                                            if (driver != null)
                                            {
                                                cinfo.Driver = driver;
                                            }

                                            subkey2 += "\\Device Parameters";

                                            // Open Subkeys (depth 3)
                                            rk = Registry.LocalMachine.OpenSubKey(subkey2, false);

                                            //Port Name
                                            if (rk != null && rk.GetValue("PortName") is string portName)
                                            {
                                                cinfo.ComName = portName;
                                                if (!string.IsNullOrEmpty(cinfo.DriverDesc))
                                                {
                                                    // Convert Driver Description to lowercase
                                                    string DriverDescLowerCase = cinfo.DriverDesc!.ToLower(); // `!` is safe because of the previous null check
                                                    if (DriverDescLowerCase.Contains(Driver_Device_Description_SearchString))
                                                    {
                                                        // Add to the result list
                                                        ret.Add(cinfo);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Returns a list of active COM Ports as shown in Device manager
        /// </summary>
        /// <param name="SearchString">Search String (is compared with the "driver description" and "device description")</param>
        /// <remarks>
        /// to get all COM-Ports please set Search String to String.Empty,
        /// search string is not case-sensitive
        /// </remarks>
        /// <returns>list of available COM Ports, containing the search string</returns>
        public static List<string> GetActiveComPorts(string SearchString)
        {
            List<string> ret = [];
            try
            {
                ManagementObjectSearcher searcher = new("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
                ManagementObjectCollection mo = searcher.Get();

                string searchStringLower = SearchString.ToLower();
                foreach (ManagementObject queryObj in mo.Cast<ManagementObject>())
                {
                    if (queryObj["Caption"] is string caption)
                    {
                        string captionLower = caption.ToLower();
                        if (captionLower.Contains("(com") && captionLower.Contains(searchStringLower))
                        {
                            int startIdx = captionLower.IndexOf('(') + 1;
                            int endIdx = captionLower.IndexOf(')');
                            if (startIdx > 0 && endIdx > startIdx)
                            {
                                ret.Add(captionLower[startIdx..endIdx]);
                            }
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                ret.Clear();
            }

            return ret;
        }
    }

    /// <summary>
    /// Implements two events to monitor USB device connection / disconnection
    /// </summary>
    public class USBMonitor
    {

        public delegate void USBDeviceConnectedHandler();
        /// <summary>
        /// Occurs when USB device is connected
        /// </summary>
        public event USBDeviceConnectedHandler? USBDeviceConnectedEvent;
        /// <summary>
        /// Called when USB device is connected
        /// </summary>
        protected virtual void OnUSBDeviceConnected()
        {
            USBDeviceConnectedEvent?.Invoke();
        }

        public delegate void USBDeviceDisConnectedHandler();
        /// <summary>
        /// Occurs when USB device disconnected
        /// </summary>
        public event USBDeviceDisConnectedHandler? USBDeviceDisConnectedEvent;
        /// <summary>
        /// Called when USB device disconnected
        /// </summary>
        protected virtual void OnUSBDeviceDisConnected()
        {
            USBDeviceDisConnectedEvent?.Invoke();
        }

        private bool _IsMonitoring = false;
        public bool IsMonitoring
        {
            get { return _IsMonitoring; }
            set { _IsMonitoring = value; }
        }


        ~USBMonitor()
        {
            Close();
        }

        private ManagementEventWatcher w_in = new();
        private ManagementEventWatcher w_out = new();


        /// <summary>
        /// Starts the USB monitoring
        /// Use either params PID and VID OR PID_VID in the form "VID_0403&PID_6001";
        /// Not used params can be null or ""
        /// </summary>
        /// <param name="VID">VID or null or "" </param>
        /// <param name="PID">VID or null or ""</param>
        /// <param name="PID_VID">String of the form "VID_0403&PID_6001" or null or ""</param>
        /// <returns>true if started</returns>
        public bool StartUSBMonitoring(string VID, string PID, string PID_VID)
        {
            bool ret = true;
            string pid_vid = "";

            if (PID_VID != null)
                if (PID_VID != "")
                    pid_vid = PID_VID;

            if (pid_vid == "")
                pid_vid = "VID_" + VID + "&PID_" + PID;      //"VID_0403&PID_6001";

            WqlEventQuery q_in, q_out;// Represents a WMI event query in WQL format (Windows Query Language)
            ManagementScope scope = new("root\\CIMV2");
            // Represents a scope (namespace) for management operations.
            scope.Options.EnablePrivileges = true;
            try
            {
                if (w_in == null)
                {
                    q_in = new WqlEventQuery
                    {
                        EventClassName = "__InstanceCreationEvent",
                        WithinInterval = new TimeSpan(0, 0, 1),
                        Condition = @"TargetInstance ISA 'Win32_USBHub' AND TargetInstance.DeviceID LIKE 'USB\\" + pid_vid + "%'"
                    };

                    w_in = new ManagementEventWatcher(scope, q_in);
                    w_in.EventArrived += new EventArrivedEventHandler(USBDisConnected);
                }


                if (w_out == null)
                {
                    q_out = new WqlEventQuery
                    {
                        EventClassName = "__InstanceDeletionEvent",
                        WithinInterval = new TimeSpan(0, 0, 1),
                        Condition = @"TargetInstance ISA 'Win32_USBHub' AND TargetInstance.DeviceID LIKE 'USB\\" + pid_vid + "%'"
                    };
                    w_out = new ManagementEventWatcher(scope, q_out);
                    w_out.EventArrived += new EventArrivedEventHandler(USBConnected);
                }

                w_in.Start(); //run the watcher
                w_out.Start();
                _IsMonitoring = true;

            }
            catch (Exception)
            {
                Close();
                ret = false;
            }

            return ret;
        }


        //Event kommt 3x ... warum?
        private void USBConnected(object sender, EventArgs e)
        {
            OnUSBDeviceDisConnected();
        }

        //Event kommt 3x ... warum?
        public void USBDisConnected(object sender, EventArgs e)
        {
            //Thread.Sleep(1000); //Because there is a timlag between event and info available
            OnUSBDeviceConnected();
        }

        /// <summary>
        /// Closes USM monitoring
        /// </summary>
        public void Close()
        {
            try
            {
                w_in?.Stop();
            }
            catch { };

            try
            {
                w_out?.Stop();
            }
            catch { };
            _IsMonitoring = false;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility