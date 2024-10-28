using Microsoft.Win32;
using System;

namespace WindControlLib
{
    //https://stackoverflow.com/questions/13728491/opensubkey-returns-null-for-a-registry-key-that-i-can-see-in-regedit-exe
    //A 32-bit application on a 64-bit OS will be looking at the HKLM\Software\Wow6432Node node by default.
    //To read the 64-bit version of the key, you'll need to specify the RegistryView:
    //using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    //using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
    //{
    // key now points to the 64-bit key
    //}

    //https://www.rhyous.com/2011/01/24/how-read-the-64-bit-registry-from-a-32-bit-application-or-vice-versa/

#pragma warning disable CA1416 // Validate platform compatibility

    public static class CRegistryAccess_64_32
    {
        public static string[] Read_Subkeys_64bitRegistryFrom32bitApp(string RegKey)
        {
            string[] ret = new string[0];
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey = localKey.OpenSubKey(RegKey);
                if (localKey != null)
                {
                    ret = localKey.GetSubKeyNames(); //value64
                }
            }
            else
            {
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey32 = localKey32.OpenSubKey(RegKey);
                if (localKey32 != null)
                {
                    ret = localKey32.GetSubKeyNames(); //value32
                }
            }

            return ret;
        }

        public static string Read_Value_64bitRegistryFrom32bitApp(string RegKey, string SubKey)
        {
            string ret = "";
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey = localKey.OpenSubKey(RegKey);
                if (localKey != null)
                {
                    ret = localKey.GetValue(SubKey, "").ToString(); //value64
                }
            }
            else
            {
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey32 = localKey32.OpenSubKey(RegKey);
                if (localKey32 != null)
                {
                    ret = localKey32.GetValue(SubKey, "").ToString(); //value32
                }
            }

            return ret;
        }

    }
#pragma warning restore CA1416 // Validate platform compatibility

}
