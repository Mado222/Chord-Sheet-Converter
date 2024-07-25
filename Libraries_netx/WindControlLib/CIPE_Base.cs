using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WindControlLib
{
    public class CIPE_Base
    {
        //c:\Program Files (x86)\Microchip\MPLABX\v5.30\docs\Readme for MPLAB IPE.htm

        public delegate void ReportMeasurementProgressEventHandler(object sender, string text, System.Drawing.Color col);
        public event ReportMeasurementProgressEventHandler ReportMeasurementProgress;
        protected virtual void OnReportMeasurementProgress(string text, System.Drawing.Color col)
        {
            ReportMeasurementProgress?.Invoke(this, text, col);
        }


        /// <summary>
        /// Send IPE Command
        /// </summary>
        /// <param name="Arguments">Command Line Arguments</param>
        /// <param name="StatusString">The status string.</param>
        /// <returns></returns>
        protected bool Operate_IPE(string Arguments, ref string StatusString)
        {
            bool ret = false;

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                ///Prozess-Konfiguration//////////////////////////
                FileName = GetMPLABX_ipecmd_Path() //@"c:\Program Files (x86)\Microchip\MPLABX\v5.30\mplab_platform\mplab_ipe\c" 
            };

            if (psi.FileName != "")
            {
                psi.Arguments = Arguments;

                psi.UseShellExecute = false;
                psi.ErrorDialog = true;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                //Prozess-Start///////////////////////////////////
                Process ipecmd = new Process
                {
                    StartInfo = psi
                };
                _ = ipecmd.Start(); //processStarted
                //StreamReader opRead = ipecmd.StandardOutput;
                StreamReader erRead = ipecmd.StandardError;

                string progress = "";
                while (!ipecmd.StandardOutput.EndOfStream)
                {
                    string l = ipecmd.StandardOutput.ReadLine();
                    progress += l + Environment.NewLine;
                    OnReportMeasurementProgress(l, Color.Black);
                    //Application.DoEvents();

                }
                string error = erRead.ReadToEnd();
                ret = Check_IPE_Output(progress, error, ref StatusString);

                ipecmd.WaitForExit();
            }
            return ret;
        }

        private class CMicrochip_reg_key
        {
            public List<string> SubkeyNames = new List<string>();
            public string RegKey = "";

            public CMicrochip_reg_key(string[] subkeyNames, string regKey)
            {
                SubkeyNames = new List<string>(subkeyNames);
                RegKey = regKey;
            }

            public CMicrochip_reg_key(List<string> subkeyNames, string regKey)
            {
                SubkeyNames = subkeyNames;
                RegKey = regKey;
            }
        }

        public string GetMPLABX_ipecmd_Path()
        {
            string path = @"c:\Program Files\Microchip\MPLABX\v6.00\mplab_platform\mplab_ipe\ipecmd.exe";
            if (File.Exists(path))
                return path;
            
            List<CMicrochip_reg_key> Microchip_reg_keys = new List<CMicrochip_reg_key>();
            
            //Achtung ob von 32 oder 64 bit SW darauf zugegriffen wird ... ohne Massnahmen 
            //liest 32bit SW immer von "SOFTWARE\WOW6432Node\Microchip"
            //siehe CRegistryAccess_64_32
            List<string> Regkeys = new List<string>(new string[] {
                 @"SOFTWARE\WOW6432Node\Microchip",
                @"SOFTWARE\Microchip" //location >= MPLABX Version 5.45
            });

            foreach (string srk in Regkeys)
            {
                Microchip_reg_keys.Add(new CMicrochip_reg_key(CRegistryAccess_64_32.Read_Subkeys_64bitRegistryFrom32bitApp(srk), srk));
            }

            //Search for highest version
            string regkey = "";
            string subkey = "";
            int Version = -1;

            foreach (CMicrochip_reg_key cMicrochip_Reg_Key in Microchip_reg_keys)
            {
                foreach (string s in cMicrochip_Reg_Key.SubkeyNames)
                {
                    if (s.Contains("MPLAB X IDE v"))
                    {
                        //Versionsnummer extrahieren, Key mit höchster Nummer nehmen
                        int i = int.Parse(Regex.Match(s.Replace(".", ""), @"\d+").Value, NumberFormatInfo.InvariantInfo);
                        if (i > Version)
                        {
                            regkey = cMicrochip_Reg_Key.RegKey;
                            subkey = s;
                            Version = i;
                        }
                    }
                }
            }

            string Full_Regkey = regkey + @"\" + subkey;
            path = CRegistryAccess_64_32.Read_Value_64bitRegistryFrom32bitApp(Full_Regkey, "Location");
            if (path != "")
            {
                path += @"\mplab_platform\mplab_ipe\ipecmd.exe";
            }

            if (!File.Exists(path))
                path = "";

            return path;
        }

        public string Add_File_to_CommandLineParameters(string CommandLineParameters, string file_name)
        {
            string ret = CommandLineParameters + @"""" + file_name + @"""";
            return ret;
        }

        public List<CMicrochip_Programmer> Get_Available_Programmers ()
        {
            List<CMicrochip_Programmer> ret = new List<CMicrochip_Programmer>();
            string status = "";

            if (Operate_IPE("-T", ref status))
            {
                if (status.Contains(@"Available Tool List"))
                {
                    List<string> res = new List<string>(Regex.Split(status, "\r\n|\r|\n"));
                    string[] seperator = new string[] { "  " };
                    for (int i = 0; i < res.Count; i++)
                    {
                        if (res[i].Contains("ICD") | res[i].ToLower().Contains("pickit"))
                        {
                            //"1  ICD 3 S.No : JIT124112949"
                            //"1  PICkit 4 S.No : BUR225175376"
                            res[i] = res[i].Replace(" S.No : ", "  ");
                            string[] ss = res[i].Split(seperator, StringSplitOptions.None);
                            ret.Add(new CMicrochip_Programmer(ss[1], ss[2]));
                        }
                    }
                }
            }
            else
            {
                ret = null;
            }
            return ret;
        }

        private bool Check_IPE_Output(string progress, string error, ref string StatusString)
        {
            StatusString = "";// "Progress: " + progress;
            if (error != "")
            {
                StatusString += "\r\n error: " + error;
            }

            //string success = "Programming/Verify complete";
            string success = "Operation Succeeded";
            Match match = Regex.Match(progress, success, RegexOptions.IgnoreCase);
            bool ret = match.Success;
            success = @"Target Detected";
            if (ret || Regex.Match(progress, success, RegexOptions.IgnoreCase).Success)
            {
                ret = true;
            }
            else
            {
                success = @"Available Tool List";
                if (Regex.Match(progress, success, RegexOptions.IgnoreCase).Success)
                {
                    StatusString = progress;
                    ret = true;
                }

                success = @"No Device Connected";
                if ((Regex.Match(progress, success, RegexOptions.IgnoreCase)).Success)
                {
                    StatusString += "\r\n No Device Connected " + error;
                }

                if (StatusString.Contains("Target Device ID") && StatusString.Contains("does not match"))
                {
                    StatusString = StatusString.Replace("Programmer not Connected !!!", "");
                }
            }
            return ret;
        }
    }
}

