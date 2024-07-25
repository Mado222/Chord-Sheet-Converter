using Microsoft.Win32;
using System.Collections;
using System.Management;


namespace BMTCommunication
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
            this._ComName = String.Empty;
            this._Driver = String.Empty;
            this._DriverDesc = String.Empty;
            this._FriendlyName = String.Empty;
            this._HardwareID = String.Empty;
            this._Manufacturer = String.Empty;
            this._MatchingDeviceId = String.Empty;
            this._Service = String.Empty;
            this._FTDIBusKeyName = String.Empty;
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
        public int Compare(CComPortInfo x, CComPortInfo y)
        {
            //return ((new CaseInsensitiveComparer()).Compare(x.EventName, y.EventName));
            if (String.Equals(x.MatchingDeviceId, y.MatchingDeviceId))
                return 0;
            return String.Compare(x.MatchingDeviceId, y.MatchingDeviceId);
        }
        #endregion

        #region IComparer Members

        public int Compare(object x, object y)
        {
            return Compare((CComPortInfo)x, (CComPortInfo)y);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return Compare((CComPortInfo)this, (CComPortInfo)obj);
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
        public static List<CComPortInfo> GetComPortInfo(String Driver_Device_Description_SearchString)
        {
            return GetComPortInfo(Driver_Device_Description_SearchString, String.Empty, String.Empty);
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
        public static List<CComPortInfo> GetComPortInfo(String Driver_Device_Description_SearchString, String PID_VID_SerachString, String CurrentControlSet_SearchString)
        {
            List<CComPortInfo> temp = new List<CComPortInfo>();
            List<CComPortInfo> ret = new List<CComPortInfo>();

            //lower case
            Driver_Device_Description_SearchString = Driver_Device_Description_SearchString.ToLower();

            RegistryKey rk;
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
                            CComPortInfo c = new CComPortInfo();
                            if (rk.GetValue("DriverDesc") != null)
                            {
                                object oDriverDesc = rk.GetValue("DriverDesc");
                                if (oDriverDesc != null)
                                {
                                    string DriverDesc = ((string)oDriverDesc).ToLower();  //lower case
                                    if (DriverDesc.Contains(Driver_Device_Description_SearchString))     // NEU
                                    {
                                        c.DriverDesc = (string)rk.GetValue("DriverDesc");

                                        object oMatchingDeviceId = rk.GetValue("MatchingDeviceId");

                                        if (oMatchingDeviceId != null)
                                        {
                                            string MatchingDeviceId = ((string)oMatchingDeviceId).ToLower();
                                            if (MatchingDeviceId.Contains(PID_VID_SerachString.ToLower()))
                                            {
                                                c.MatchingDeviceId = (string)rk.GetValue("MatchingDeviceId");
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
                        string MatchingDeviceId = (c.MatchingDeviceId).ToUpper();

                        int indexOfBackslash = MatchingDeviceId.IndexOf('\\');
                        string mainDir = "";
                        if (indexOfBackslash < 0)
                        {
                            //This is a Hardware COM Port
                            mainDir = MatchingDeviceId;
                        }
                        else
                        {
                            mainDir = MatchingDeviceId.Substring(0, indexOfBackslash);
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
                            List<string> SelectedEntriesOfMainDir = new List<string>();

                            //Select only relevant Subkeys
                            foreach (string entry in AllEntriesOfMainDir)
                            {
                                if (entry.ToLower().Contains(CurrentControlSet_SearchString.ToLower()))
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
                                            CComPortInfo cinfo = new CComPortInfo
                                            {
                                                FTDIBusKeyName = subkey
                                            };

                                            //device description
                                            if ((string)rk.GetValue("DeviceDesc") != null)
                                                cinfo.DriverDesc = (string)rk.GetValue("DeviceDesc");

                                            //matching device id
                                            cinfo.MatchingDeviceId = c.MatchingDeviceId;

                                            //hardware id
                                            if ((string[])rk.GetValue("HardwareID") != null)
                                            {
                                                string[] helpStringArray = (string[])rk.GetValue("HardwareID");
                                                foreach (string a in helpStringArray)
                                                {
                                                    cinfo.HardwareID += a;
                                                }
                                            }

                                            //Service
                                            if ((string)rk.GetValue("Service") != null)
                                                cinfo.Service = (string)rk.GetValue("Service");

                                            //Friendly Name
                                            if ((string)rk.GetValue("FriendlyName") != null)
                                                cinfo.FriendlyName = (string)rk.GetValue("FriendlyName");

                                            //Driver
                                            if ((string)rk.GetValue("Driver") != null)
                                                cinfo.Driver = (string)rk.GetValue("Driver");

                                            subkey2 += "\\Device Parameters";

                                            // Open Subkeys (depth 3)
                                            rk = Registry.LocalMachine.OpenSubKey(subkey2, false);

                                            //Port Name
                                            if ((rk != null) && (rk.GetValue("PortName") != null))
                                            {
                                                cinfo.ComName = (string)rk.GetValue("PortName");
                                                if (cinfo.DriverDesc != null)
                                                {
                                                    //if Driver Description contains the search string
                                                    string DriverDescLowerCase = cinfo.DriverDesc.ToLower();    //lower case
                                                    if (DriverDescLowerCase.Contains(Driver_Device_Description_SearchString))        //NEU
                                                    {
                                                        //Add the result list
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
        public static List<string> GetActiveComPorts(String SearchString)
        {
            List<string> ret = new List<string>();
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
                ManagementObjectCollection mo = searcher.Get();

                foreach (ManagementObject queryObj in mo)
                {

                    if (queryObj["Caption"] != null)
                    {
                        string s = queryObj["Caption"].ToString();
                        s= s.ToLower();
                        SearchString = SearchString.ToLower();
                        if (s.Contains("(com") && s.Contains(SearchString))
                        {
                            //Com Nummer (Bezeichnung) herausholen
                            // .... (COMxx)
                            int klammer_auf_idx = s.IndexOf("(");
                            int klammer_zu_idx = s.IndexOf(")");
                            string sub = s.Substring(klammer_auf_idx + 1, klammer_zu_idx - klammer_auf_idx - 1);
                            ret.Add(sub);
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
        public event USBDeviceConnectedHandler USBDeviceConnectedEvent;
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
        public event USBDeviceDisConnectedHandler USBDeviceDisConnectedEvent;
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
        
        private ManagementEventWatcher w_in, w_out;


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
            string pid_vid="";

            if (PID_VID != null)
                if (PID_VID != "")
                    pid_vid = PID_VID;
            
            if (pid_vid=="")
                pid_vid = "VID_" + VID + "&PID_" + PID;      //"VID_0403&PID_6001";

            WqlEventQuery q_in, q_out;// Represents a WMI event query in WQL format (Windows Query Language)
            ManagementScope scope = new ManagementScope("root\\CIMV2");
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
            catch (Exception e)
            {
                Close();
                ret = false;
            }

            return ret;
        }
        
        /// <summary>
        /// Starts the USB monitoring
        /// </summary>
        /// <param name="SearchString">String the device name must contain</param>
        /// <returns>
        /// true if started
        /// </returns>
        /// <remarks>
        /// Searches registry for COM ports containing the SearchString and reads PID and VID to link to the USB device
        /// if driver not properly installed default value VID_0403&PID_6001 is used
        /// </remarks>
        /*
        public bool StartUSBMonitoring (string SearchString)
        {
            string pid_vid = "VID_0403&PID_6010";

            List<CComPortInfo> cpi = CGetComPorts.GetComPortInfo(SearchString);

            if ((cpi != null) && (cpi.Count > 0))
            {
                int start_idx = cpi[0].HardwareID.IndexOf("&");
                int end_idx = cpi[0].HardwareID.Length - 1;

                pid_vid = cpi[0].HardwareID.Substring(start_idx + 1, end_idx - start_idx);
            }

            return StartUSBMonitoring("", "", pid_vid);
        }*/

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
                if (w_in != null)
                {
                    w_in.Stop();
                }
            }
            catch { };

            try
            {
                if (w_out != null)
                {
                    w_out.Stop();
                }
            }
            catch { };
            _IsMonitoring = false;
        }
    }
}