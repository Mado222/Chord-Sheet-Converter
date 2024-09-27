using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using WindControlLib;
using FeedbackDataLib;

namespace RunBatch
{
    public partial class frmRunBatch : Form
    {
        public frmRunBatch()
        {
            InitializeComponent();
        }

        public const string UART_Bootloader_dir = @"\PIC24F_UART_Bootloader";
        //public const string UART_Bootloader_dir = @"\PIC24F UART Bootloader\PIC24F UART Bootloader.X\nbproject";
        //public const string UART_Bootloader_Batch_dir = @"\PIC24F UART Bootloader\Build_batches";

        public const string SDCard_Bootloader_dir = @"\PIC24F_SDcard_Bootloader";
        //public const string SDCard_Bootloader_dir = @"\PIC24F SDcard Bootloader\PIC24F SDcard Bootloader.X\nbproject";
        //public const string SDCard_Bootloader_Batch_dir = @"\PIC24F SDcard Bootloader\Build_batches";

        public const string Modules_dir= @"\PIC24F_Modules";
        //public const string ModulesX_dir = Modules_dir + Modules_dir + @".X";
        //public const string Modules_nbproject_dir = ModulesX_dir + @"\nbproject";
        //public const string Modules_dist_dir = ModulesX_dir + @"\dist\";
        //public const string Modules_Batch_dir = Modules_dir + @"\Build_batches";
        //public const string Modules_production_hex_dir = @"\production" + Modules_dir + @".X.production.hex";

        public const string Neuromaster_dir = @"\PIC24F_Neuromaster_5";
        //public const string NeuromasterX_dir = Neuromaster_dir + Neuromaster_dir + @".X\nbproject";
        //public const string Neuromaster_nbproject_dir = NeuromasterX_dir + @"\nbproject";
        //public const string Neuromaster_Batch_dir = Neuromaster_dir + @"\Build_batches";


        Process batch;
        bool BatchExited = false;
        int cntErrors = 0;
        int cntWarnings = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
            batch = new Process();
            batch.OutputDataReceived += new DataReceivedEventHandler(batch_OutputDataReceived);
            batch.Exited += new EventHandler(batch_Exited);
            batch.ErrorDataReceived += new DataReceivedEventHandler(batch_ErrorDataReceived);
            batch.EnableRaisingEvents = true;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (batch != null)
                try
                {
                    batch.Kill();
                }
                catch (Exception ee)
                {
                }
        }

        List<FileInfo> mkFiles = [];

        private void GetAllMakefiles(DirectoryInfo Path_to_nbproject)
        {
            mkFiles.Clear();
            FileInfo[] files = Path_to_nbproject.GetFiles("*.mk");

            foreach (FileInfo f in files)
            {
                string fn = f.Name;
                if (!fn.Contains("local") && (fn.Contains("PIC24F") || fn.Contains("DSPIC33F")))
                {
                    txtStatus.AddStatusString(fn, Color.Blue);
                    mkFiles.Add(f);
                }
            }
        }

        private void Build_all_mk_files_in_Directory(string PartialPath_to_nbproject, string PartialPath_to_Generic_Build_bat)
        {
            GetAllMakefiles(new DirectoryInfo(CPic24_Pathes.GetBasicPath() + PartialPath_to_nbproject));
            txtStatus.Clear();
            cStatusErrors.Clear();
            cStatusWarnings.Clear();
            cntErrors = 0;
            cntWarnings = 0;

            string wd = CPic24_Pathes.GetBasicPath() + PartialPath_to_Generic_Build_bat;  
            string fp = wd + @"\GenericBuild.bat";  //"d:\\Daten\\Insight\\Bitbucket_Git\\Firmware\\PIC24F Modules\\Build_batches\\GenericBuild.bat"
            string mk = wd + @"\GenerateMakefile.bat";

            foreach (FileInfo f in mkFiles)
            {
                string cmd_params = f.Name.Replace(".mk", "");
                cmd_params = cmd_params.Replace("Makefile-", "");
               
                ExecuteBatch(mk, wd, cmd_params);
                lblErrorcnt.Text = cntErrors.ToString();
                lblWarningscnt.Text = cntWarnings.ToString();

                ExecuteBatch(fp, wd, cmd_params);
                lblErrorcnt.Text = cntErrors.ToString();
                lblWarningscnt.Text = cntWarnings.ToString();
            }
        }

        private void btBuildAllBootloaders_Click(object sender, EventArgs e)
        {
            if (cbNeuromodule.Checked)
                Build_all_mk_files_in_Directory(CPic24_Pathes.GetXPath_nbproject(UART_Bootloader_dir), CPic24_Pathes.GetBatch_dir(UART_Bootloader_dir));
            if (cbNeuromaster.Checked)
                Build_all_mk_files_in_Directory(CPic24_Pathes.GetXPath_nbproject(SDCard_Bootloader_dir), CPic24_Pathes.GetBatch_dir(SDCard_Bootloader_dir));

        }

        private void btBuildAllModules_Click(object sender, EventArgs e)
        {
            if (cbNeuromodule.Checked)
                Build_all_mk_files_in_Directory(CPic24_Pathes.GetXPath_nbproject(Modules_dir),
                    CPic24_Pathes.GetBatch_dir(Modules_dir));
            if (cbNeuromaster.Checked)
                Build_all_mk_files_in_Directory(CPic24_Pathes.GetXPath_nbproject(Neuromaster_dir),
                    CPic24_Pathes.GetBatch_dir(Neuromaster_dir));

        }


        private void btBuildFirmwareImages_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            cStatusErrors.Clear();
            cStatusWarnings.Clear();
            cntErrors = 0;
            cntWarnings = 0;

            string BasicPath = CPic24_Pathes.GetBasicPath();

            string wd = BasicPath + Modules_dir;
            string fp = wd + @"\Create Firmware-Images.bat";
            string newFileName = "";
            string[] binFiles;

            if (cbNeuromodule.Checked)
            {
                if (!cbInclude_cSWChannel.Checked)
                {
                    //prepare reduced hex files
                    Remove_cSWChannel_from_hex();
                    fp = wd + @"\Create Firmware-Images_noCalData.bat";
                }

                ExecuteBatch(fp, wd, "");

                //Get *.bin files and move them
                binFiles = Directory.GetFiles(wd, "*.bin");
                for (int i = 0; i < binFiles.Length; i++)
                {
                    
                    newFileName = BasicPath + @"\Neuromodule bin files\" + Path.GetFileName(binFiles[i]);
                    if (File.Exists(newFileName)) File.Delete(newFileName);     //Delete existing files in target dir
                    File.Move(binFiles[i], newFileName);
                    txtStatus.AddStatusString(Path.GetFileName(newFileName) + @" moved to \Neuromodule bin files", Color.Green);
                }
            }

            if (cbNeuromaster.Checked)
            {
                //wd = BasicPath + @"\PIC24F Neuromaster";
                wd = BasicPath + Neuromaster_dir;
                fp = wd + @"\Create Firmware-Images.bat";
                ExecuteBatch(fp, wd, "");

                //Get *.bin files and move them
                binFiles = Directory.GetFiles(wd, "*.bin");
                newFileName = "";
                for (int i = 0; i < binFiles.Length; i++)
                {
                    newFileName = BasicPath + @"\Neuromodule bin files\" + Path.GetFileName(binFiles[i]);
                    if (File.Exists(newFileName)) File.Delete(newFileName);     //Delete existing files in target dir
                    File.Move(binFiles[i], newFileName);
                    txtStatus.AddStatusString(Path.GetFileName(newFileName) + @" moved to \Neuromodule bin files", Color.Green);
                }
            }

        }

        private void Remove_cSWChannel_from_hex()
        {
            CPIC24_IntelHex ih_module = new CPIC24_IntelHex();

            string basicPath = CPic24_Pathes.GetBasicPath();
            string[] Dir_Names_PIC24 = new string[0];

            //Dir_Names_PIC24 = Get_Dir_Names_PIC24(basicPath + @"\PIC24F Modules\PIC24F Modules.X\dist\", "");
            Dir_Names_PIC24 = Get_Dir_Names_PIC24(CPic24_Pathes.GetBasicPath() + CPic24_Pathes.GetXPath_dist(Modules_dir), "");


            for (int i = 0; i < Dir_Names_PIC24.Length; i++)
            {
                //string Module_hex_Path = Dir_Names_PIC24[i] + @"\production\PIC24F_Modules.X.production.hex";
                string Module_hex_Path = Dir_Names_PIC24[i] + CPic24_Pathes.GetProduction_hex_file(Modules_dir);

                if (File.Exists(Module_hex_Path))
                {
                    //Keine Unterscheidung zw EEG und Rest notwendig da Config an selber Speicherstelle

                    ih_module.OpenHexFile(Module_hex_Path);
                    ih_module.Make_MemoryMirror();  //Memory Mirror of Module

                    byte[] data = new byte[0x400];
                    for (int j = 0; j < data.Length; j++)
                    {
                        data[j] = 0xff;
                    }
                    //Blanking des Datenbereichs -> Inkonstistent mit den *2 der Adressen :-( - aber so funktioniert es
                    ih_module.Add_to_MemoryMirror(data, 0x9C00, true, 0xff);

                    string fn = Path.GetFileNameWithoutExtension(Module_hex_Path) + "_noCalData";
                    fn = Path.GetDirectoryName(Module_hex_Path) + @"\" + fn + Path.GetExtension(Module_hex_Path);
                    ih_module.WriteHexFile_from_Memory(fn);
                }
            }
        }


        private bool ExecuteBatch(string full_path_to_bat, string working_directory, string commanddline_params)
        {
            bool ret = false;

            txtStatus.AddStatusString(@"Starting " + full_path_to_bat, Color.Orange);
            txtStatus.AddStatusString(@"Working Directory " + working_directory, Color.Orange);

            System.Diagnostics.ProcessStartInfo psi =
                new System.Diagnostics.ProcessStartInfo(full_path_to_bat);

            ////////Prozess-Konfiguration//////////////////////////
            psi.WorkingDirectory = working_directory;
            psi.UseShellExecute = false;
            psi.ErrorDialog = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.Arguments = commanddline_params;

            //Prozess-Start///////////////////////////////////
            //http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why

            batch.StartInfo = psi;

            BatchExited = false;
            
            batch.Start();
            try
            {
                batch.BeginOutputReadLine();
                batch.BeginErrorReadLine();
            }
            catch (Exception ee) { }


            DateTime timeout = DateTime.Now + new TimeSpan(0, 0, 0, 50, 0);

            while ((timeout > DateTime.Now) && !BatchExited)
            {
                lblErrorcnt.Text = cntErrors.ToString();
                lblWarningscnt.Text = cntWarnings.ToString();
                Application.DoEvents();
                //Thread.Sleep(100);
            }

            if (BatchExited == true)
                txtStatus.AddStatusString("Batch Exited", Color.Green);
            else
                txtStatus.AddStatusString("Timeout", Color.Red);

            batch.CancelErrorRead();
            batch.CancelOutputRead();

            batch.Close();

           return ret;

        }

        void batch_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string s = e.Data.ToString();
                string ss = s.ToLower();

                if (ss.Contains("warning"))
                {
                    txtStatus.AddStatusString(e.Data.ToString(), Color.Blue);
                    cStatusWarnings.AddStatusString(e.Data.ToString(), Color.Blue);
                    cntWarnings++;
                }
                else
                {
                    if (!ss.Contains("hpa has ended") && !ss.Contains("no error"))
                    {
                        cntErrors++;
                        cStatusErrors.AddStatusString(e.Data.ToString(), Color.Red);
                    }
                    txtStatus.AddStatusString(e.Data.ToString(), Color.Red);
                }
            }
        }


        void batch_Exited(object sender, EventArgs e)
        {
            BatchExited = true;
        }

        void batch_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string s = e.Data.ToString();
                string ss = s.ToLower();
                if (ss.Contains("warning"))
                {
                    txtStatus.AddStatusString(e.Data.ToString(), Color.Blue);
                    cStatusWarnings.AddStatusString(e.Data.ToString(), Color.Blue);
                    cntWarnings++;
                }
                else
                {
                    txtStatus.AddStatusString(e.Data.ToString(), Color.Black);
                }
            }
        }


        private void txtStatus_TextChanged(object sender, EventArgs e)
        {
            txtStatus.ScrollToCaret();
        }


        private string[] Get_Dir_Names_PIC24(string BasicDirectory, string name_contains)
        {
            List<string> dNames = new List<string>();
            string[] dn = Directory.GetDirectories(BasicDirectory);
            for (int i = 0; i < dn.Length; i++)
            {
                if (Path.GetFileName(dn[i]).Contains(name_contains))
                {
                    dNames.Add(dn[i]);
                }
            }
            return dNames.ToArray();
        }


        private void btMerge_Click(object sender, EventArgs e)
        {
            string basicPath = CPic24_Pathes.GetBasicPath();
            //string[] Dir_Names_PIC24 = Get_Dir_Names_PIC24(basicPath + @"\PIC24F Modules\PIC24F Modules.X\dist\", "PIC24");
            //string[] Dir_Names_dsPIC = Get_Dir_Names_PIC24(basicPath + @"\PIC24F Modules\PIC24F Modules.X\dist\", "DSPIC");
            string[] Dir_Names_PIC24 = new string[0];

            if (cbNeuromodule.Checked)
            {
                Dir_Names_PIC24 = Get_Dir_Names_PIC24(basicPath + CPic24_Pathes.GetXPath_dist(Modules_dir), "");

                for (int i = 0; i < Dir_Names_PIC24.Length; i++)
                {
                    string Module_hex_Path = Dir_Names_PIC24[i] + CPic24_Pathes.GetProduction_hex_file(Modules_dir); // @"\production\PIC24F_Modules.X.production.hex";

                    //string Bootloader_hex_Path = basicPath + @"\Pic24F UART Bootloader\PIC24F UART Bootloader.X\dist\" + Path.GetFileName(Dir_Names_PIC24[i]) + @"\production\PIC24F_UART_Bootloader.X.production.hex";
                    string Bootloader_hex_Path = basicPath + CPic24_Pathes.GetXPath_dist(UART_Bootloader_dir) + "\\" + CPic24_Pathes.GetConfigName_from_Path(Dir_Names_PIC24[i]) + CPic24_Pathes.GetProduction_hex_file(UART_Bootloader_dir);
                    string Output_hex_Path = CPic24_Pathes.GetCombinedHexPath() + Path.GetFileName(Dir_Names_PIC24[i]) + ".hex";
                    if (File.Exists(Module_hex_Path) && File.Exists(Bootloader_hex_Path))
                    {
                        Merge_PIC24(Module_hex_Path, Bootloader_hex_Path, Output_hex_Path);
                        txtStatus.AddStatusString(Output_hex_Path + " generated");

                        //Make bin file
                        CIPE_Neuromodul_PIC24 nm = new CIPE_Neuromodul_PIC24(new CMicrochip_Programmer("ICD 3", "")); //Dummy
                        CIPE_Neuromodul_PIC24.CModuleInformation cmi = nm.Get_FullInfo(Output_hex_Path);

                        string binFilename = CMakeBinFilename.GetBinFilename(cmi.ModuleType_enum, cmi.HWVersion);

                        //usage: hex2bin[-s xxxx][-e extension][-c] filename
                        //- s xxxx Starting address in hex
                        //- e extension output filename extension
                        //-c enable checksum verification
                        _ = ExecuteBatch(CPic24_Pathes.GetBasicPath() + @"\hex2bin.exe",
                            "",
                            @"""" + Output_hex_Path + @"""");

                        string original_binFilePath = Path.ChangeExtension(Output_hex_Path, ".bin");
                        //Move bin file
                        string FullbinPath = CPic24_Pathes.GetBinPath() + binFilename + ".bin";
                        if (File.Exists(FullbinPath))
                            File.Delete(FullbinPath);

                        File.Move(original_binFilePath, FullbinPath);
                        txtStatus.AddStatusString(cmi.ModuleType + " Ver: " + cmi.HWVersion_Full + " generated: "+ binFilename + ".bin", Color.Green);
                    }
                    else
                        txtStatus.AddStatusString(Output_hex_Path + " failed", Color.Red);

                }
            }

            if (cbNeuromaster.Checked)
            {
                Dir_Names_PIC24 = Get_Dir_Names_PIC24(CPic24_Pathes.GetBasicPath() + CPic24_Pathes.GetXPath_dist(Neuromaster_dir), ""); //@"\PIC24F_Neuromaster_5\PIC24F_Neuromaster_5.X\dist\", "") ;

                for (int i = 0; i < Dir_Names_PIC24.Length; i++)
                {
                    //string Module_hex_Path = Dir_Names_PIC24[i] + @"\production\PIC24F_Neuromaster_5.X.production.hex";
                    string Module_hex_Path = Dir_Names_PIC24[i] + CPic24_Pathes.GetProduction_hex_file(Neuromaster_dir);

                    //string Bootloader_hex_Path = basicPath + @"\PIC24F SDcard Bootloader\PIC24F SDCard Bootloader.X\dist\PIC24F_SDCard_Bootloader\production\PIC24F_SDCard_Bootloader.X.production.hex";
                    string Bootloader_hex_Path = CPic24_Pathes.GetBasicPath() + CPic24_Pathes.GetXPath_dist(SDCard_Bootloader_dir) + SDCard_Bootloader_dir + CPic24_Pathes.GetProduction_hex_file(SDCard_Bootloader_dir);
                    string Output_hex_Path = CPic24_Pathes.GetCombinedHexPath() + Path.GetFileName(Dir_Names_PIC24[i]) + ".hex";
                    if (!File.Exists(Module_hex_Path))
                    {
                        txtStatus.AddStatusString((Module_hex_Path + " failed"), Color.Red);
                        return;
                    }

                    if (!File.Exists(Bootloader_hex_Path))
                    {
                        txtStatus.AddStatusString((Bootloader_hex_Path + " failed"), Color.Red);
                        return;
                    }


                    Merge_PIC24(Module_hex_Path, Bootloader_hex_Path, Output_hex_Path);
                    txtStatus.AddStatusString(Output_hex_Path + " generated");
                    //Make bin file
                    ExecuteBatch(CPic24_Pathes.GetBasicPath() + @"\hex2bin.exe",
                        "",
                        @"""" + Output_hex_Path + @"""");

                    string binFilePath = Path.ChangeExtension(Output_hex_Path, ".bin");
                    string FullbinPath = CPic24_Pathes.GetBinPath() + Path.GetFileName(binFilePath);
                    //Move bin file
                    if (File.Exists(FullbinPath))
                        File.Delete(FullbinPath);

                    File.Move(binFilePath, FullbinPath);
                }
            }
        }

        private void Merge_PIC24 (string Module_hex_Path, string Bootloader_hex_Path, string Output_hex_Path)
        {

            CPIC24_IntelHex ih_module = new CPIC24_IntelHex();
            CPIC24_IntelHex ih_bootloader = new CPIC24_IntelHex();

            ih_module.OpenHexFile(Module_hex_Path);
            ih_bootloader.OpenHexFile(Bootloader_hex_Path);

            ih_module.Make_MemoryMirror();  //Memory Mirror of Module


            if (Bootloader_hex_Path.ToLower().Contains("dspic"))
            {
                //dsPIC33 / EEG
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(),
                    2 * CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_ORIGIN, //0x14C00
                    2 * (CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_ORIGIN + CPIC24_Bootloader_Params.dsPIC33_BOOTLOADER_LENGTH), //(0x158FF) ??? wie komm ich auf diesen Wert ??
                    true);   //Add Bootloader
                
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(), 2 * 0x0, 2 * 0x3, true);         //Add Reset Vector
            }
            else if (Bootloader_hex_Path.ToLower().Contains("sdcard"))
            {
                //Neuromaster
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(),
                    2 * CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_ORIGIN, //0x28800,
                    2 * (CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_ORIGIN + CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_LENGTH), //(0x28800 + 0x23F6), 
                    true);   //Add Bootloader
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(), 2 * 0, 2 * (0x2), true);   //Add Reset Vector


                //Calculate CRC
                byte[] crc = new byte[]  { 0, 0 };
                byte b2, b1, b0 = 0;

                uint i = 0;
                for (i = 0; i < (CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_CRC_ADDRESS) * 2; i += 4) 
                {
                    b2 = ih_module.Get_Memory_Value(i + 2);
                    b1 = ih_module.Get_Memory_Value(i + 1);
                    b0 = ih_module.Get_Memory_Value(i + 0);

                    crc[0] = CRC8.CRC8calcByte(b2, crc[0]);
                    crc[0] = CRC8.CRC8calcByte(b1, crc[0]);
                    crc[0] = CRC8.CRC8calcByte(b0, crc[0]);
                }
                //Add crc
                ih_module.Add_to_MemoryMirror(crc, CPIC24_Bootloader_Params.PIC24_NM_BOOTLOADER_CRC_ADDRESS, true);
            }
            else
            {
                //Neuromodule
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(), 
                    2 * CPIC24_Bootloader_Params.PIC24_BOOTLOADER_ORIGIN, //0xA000,
                    2 * (CPIC24_Bootloader_Params.PIC24_BOOTLOADER_ORIGIN+ CPIC24_Bootloader_Params.PIC24_BOOTLOADER_LENGTH), //(0xA000 + 0xBF8),
                    true);   //Add Bootloader
                ih_module.Add_to_MemoryMirror(ih_bootloader.HexFile.ToArray(), 2 * 0x0, 2 * 0x3, true);         //Add Reset Vector
            }

            ih_module.WriteHexFile_from_Memory(Output_hex_Path);
        }


        private void btClearAll_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();
            cStatusErrors.Clear();
            cStatusWarnings.Clear();
        }
    }
}
