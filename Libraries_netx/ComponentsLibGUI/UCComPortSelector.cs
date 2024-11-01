using BMTCommunicationLib;
using Microsoft.Win32;
using System.Collections;
using System.Diagnostics;

namespace ComponentsLib_GUI
{


    /// <summary>
    /// Summary description for UCComPortSelector.
    /// </summary>
    public class UCComPortSelector : System.Windows.Forms.ComboBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container? components = null;
        private readonly List<int> ComNo = [];
        private static readonly string RegKey = "Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\";
        private const int _NumberofComPortstoInvestigate = 16;

        private string DefaultCom = "";

        public UCComPortSelector()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        public void SavePorttoReg()
        {
            if (SelectedItem == null) return;

            try
            {
                string? selectedItemStr = SelectedItem?.ToString();

                // Check if the selected item is different from DefaultCom
                if (!string.IsNullOrEmpty(selectedItemStr) && selectedItemStr != DefaultCom)
                {
                    string regKey = $"{RegKey}{Name}\\";

                    // Open or create the registry key
                    using RegistryKey? rk = Registry.CurrentUser.OpenSubKey(regKey, true) ??
                                             Registry.CurrentUser.CreateSubKey(regKey);

                    if (rk != null)
                    {
                        // Save the selected COM port to the registry
                        DefaultCom = selectedItemStr;
                        rk.SetValue("COMPort", DefaultCom);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if needed (e.g., log error message)
                Debug.WriteLine($"SavePorttoReg failed: {ex.Message}");
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!DesignMode) SavePorttoReg();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion


        /// <summary>
        /// Searches for COM Ports containing the DriverName
        /// </summary>
        /// <remarks>
        /// Also reads registry to find DefaultCom and sets index accordingly
        /// </remarks>
        public void Init(string DriverName, WindControlLib.CVID_PID? VIDPID= null)
        {
            //Alten Wert fuer COM aus der Registry holen
            string regkey = RegKey + Name + "\\";
            RegistryKey? rk = Registry.CurrentUser.OpenSubKey(regkey, true);
            if (rk == null)
            {
                //Key existiert nicht also anlegen
                rk = Registry.CurrentUser.CreateSubKey(regkey);
                rk.SetValue("COMPort", "COM1");
            }
            DefaultCom = (string)rk.GetValue("COMPort", "COM1");
            rk.Close();

            //Get Coms from registry
            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Ports
            string key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Ports";

            try
            {
                // Access the registry key directly without asserting permissions.
                rk = Registry.LocalMachine.OpenSubKey(key, false);
            }
            catch
            {
                rk = null;
            }


            int Index = 0;
            if (rk != null)
            {
                string[] AllPorts;
                List<string> ComNames = [];

                if (DriverName == "" && VIDPID == null)
                {
                    AllPorts = rk.GetValueNames();
                    ComNo.Clear();
                    for (int i = 0; i < AllPorts.Length; i++)
                    {
                        if (AllPorts[i].Length > 4)
                        {
                            if (AllPorts[i][..3] == "COM")
                            {
                                string s = AllPorts[i][..^1];
                                ComNames.Add(s);
                                ComNo.Add(Convert.ToInt32(s[3..]));
                            }
                        }
                    }
                }
                else
                {
                    List<CComPortInfo> cpi = CGetComPorts.GetComPortInfo("");

                    if (DriverName != "")
                    {
                        foreach (CComPortInfo ci in cpi)
                        {
                            if (ci.FriendlyName.Contains(DriverName, StringComparison.CurrentCultureIgnoreCase))
                                ComNames.Add(ci.ComName);
                        }
                    }
                    if (VIDPID != null)
                    {
                        foreach (CComPortInfo ci in cpi)
                        {
                            if (ci.VID_PID.Contains(VIDPID.VID_PID, StringComparison.CurrentCultureIgnoreCase))
                                ComNames.Add(ci.ComName);
                        }
                    }
                }
                //Remove Duplicates
                ComNames = ComNames.Distinct().ToList();
                ComNames.Sort();
                Items.Clear();
                foreach (string s in ComNames)
                {
                    Items.Add(s);
                    if (s == DefaultCom)
                        Index = Items.Count - 1;
                }

                if (Items.Count != 0)
                    SelectedIndex = Index;
                rk.Close();
            }
        }

        public void DisplayCom(int ComNo)
        {
            // Construct the search string once for efficiency
            string comString = "Com " + Convert.ToString(ComNo);

            // Find Item
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] as string == comString)
                {
                    SelectedItem = comString;
                    break;
                }
            }
        }

        public int SelectedComPortNo
        {
            get
            {
                if (!DesignMode && ComNo?.Count > SelectedIndex)
                {
                    return ComNo[SelectedIndex];
                }
                return 0;
            }
        }
    }
}
