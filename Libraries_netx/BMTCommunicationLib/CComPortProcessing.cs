using Microsoft.Win32;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using WindControlLib;

#pragma warning disable CA1416 // Validate platform compatibility
namespace BMTCommunicationLib
{
        /// <summary>
    /// Class for various COM Port finding strategies
    /// </summary>
    public class CComPortProcessing
    {
        public class CComPortInfo : IComparer<CComPortInfo>, IComparer, IComparable
        {
            #region Properties
            public string ComName { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string DriverDesc { get; set; } = string.Empty;
            public string MatchingDeviceId { get; set; } = string.Empty;
            public CVidPid VID_PID { get; } = new();
            public string Service { get; set; } = string.Empty;
            public string FriendlyName { get; set; } = string.Empty;
            public string HardwareID { get; set; } = string.Empty;
            public string Driver { get; set; } = string.Empty;
            public string FTDIBusKeyName { get; set; } = string.Empty;
            #endregion

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

        private static readonly string key_List_of_ComPorts = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E978-E325-11CE-BFC1-08002BE10318}";
        //private static readonly string key_Devices = @"SYSTEM\CurrentControlSet\Enum";

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
            return GetComPortInfo(Driver_Device_Description_SearchString, string.Empty);
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
        public static List<CComPortInfo> GetComPortInfo(string driverSearchString, string pidVidSearchString)
        {
            List<CComPortInfo> result = [];
            driverSearchString = driverSearchString.ToLower();

            using (RegistryKey? rk = Registry.LocalMachine.OpenSubKey(key_List_of_ComPorts, false))
            {
                if (rk == null) return result;

                var allEntries = rk.GetSubKeyNames();
                var comPortInfoList = new ConcurrentBag<CComPortInfo>();

                Parallel.ForEach(allEntries, s =>
                {
                    string subkey = $"{key_List_of_ComPorts}\\{s}";
                    using (RegistryKey? subRk = Registry.LocalMachine.OpenSubKey(subkey, false))
                    {
                        if (subRk != null && subRk.GetValue("DriverDesc") is string driverDesc && driverDesc.ToLower().Contains(driverSearchString))
                        {
                            if (subRk.GetValue("MatchingDeviceId") is string matchingDeviceId && matchingDeviceId.Contains(pidVidSearchString, StringComparison.CurrentCultureIgnoreCase))
                            {
                                var comPortInfo = new CComPortInfo
                                {
                                    DriverDesc = driverDesc,
                                    MatchingDeviceId = matchingDeviceId
                                };
                                comPortInfoList.Add(comPortInfo);
                            }
                        }
                    }
                });

                // Remove duplicates
                result = comPortInfoList.GroupBy(c => c.MatchingDeviceId).Select(g => g.First()).ToList();
            }
            return result;
        }


        /// <summary>
        /// Returns a list of active COM Ports as shown in Device manager
        /// </summary>
        /// <param name="SearchString">Search String (is compared with the "driver description" and "device description")</param>
                public static List<string> GetActiveComPorts(string searchString)
        {
            List<string> comPorts = [];
            try
            {
                using ManagementObjectSearcher searcher = new("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
                using ManagementObjectCollection results = searcher.Get();

                string searchStringLower = searchString.ToLower();

                foreach (ManagementObject queryObj in results)
                {
                    if (queryObj["Caption"] is string caption)
                    {
                        string captionLower = caption.ToLower();
                        if (captionLower.Contains("(com") && captionLower.Contains(searchStringLower))
                        {
                            // Extract COM port name using Regex
                            var match = Regex.Match(captionLower, @"\(com\d+\)");
                            if (match.Success)
                            {
                                // Extract the port name without parentheses
                                string comPort = match.Value.Trim('(', ')');
                                comPorts.Add(comPort.ToUpper()); // Store in uppercase, e.g., "COM3"
                            }
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                // You might consider logging the exception for debugging purposes
                Debug.WriteLine($"Error retrieving COM ports: {ex.Message}");
                comPorts.Clear(); // Clear the list to ensure no partial results are returned
            }

            return comPorts;
        }

    }
}
#pragma warning restore CA1416 // Validate platform compatibility