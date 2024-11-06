using ComponentsLib_GUI;
using FeedbackDataLib;
using FeedbackDataLib.Modules;
using WindControlLib;


namespace FeedbackDataLib_GUI
{
    public class CNeuromasterTextfileImporterExporter
    {
        /// <summary>
        /// Data read from a recorded text file
        /// </summary>
        public class CDatafromFile
        {
            public int hwcn = 0;
            public int swcn = 0;
            public double y_scaled = 0;
            public double y_unscaled = 0;
            public DateTime dt_absolute = DateTime.MinValue;
            public int time_ms = 0;
        }

        private StreamWriter? FileWriter;
        private StreamReader? FileReader;

        private bool RecordingJustStarted = false;
        private DateTime FirstRecordingTime = DateTime.Now;


        private bool _File_Write_is_open = false;
        public bool File_Write_is_open
        {
            get { return _File_Write_is_open; }
        }

        private bool _File_Read_is_open = false;
        public bool File_Read_is_open
        {
            get { return _File_Read_is_open; }
        }

        ~CNeuromasterTextfileImporterExporter()
        {
            CloseFile();
        }

#if !NET6_0_OR_GREATERXX
        /// <summary>
        /// Opens the file_for_writing.
        /// Configuration saved with same name as xml if ModuleINfo != null
        /// </summary>
        /// <param name="FilePath">The file path.</param>
        /// <param name="Comment">The comment.</param>
        /// <param name="ModuleInfo">The module information - can be null</param>
        public void OpenFile_for_writing(string FilePath, string Comment, List<CModuleBase> ModuleInfo)
        {
            if (ModuleInfo != null)
            {
                //CSave_Binary_Objects.WriteToBinaryFile<List<CModuleBase>>(Path.ChangeExtension(FilePath, ".cfg"), ModuleInfo);
            }

            FileWriter = new StreamWriter(FilePath, false);
            _File_Write_is_open = true;
            DateTime dt = DateTime.Now;
            FileWriter.Write("Recording started: " + dt.ToString() + Environment.NewLine);
            RecordingJustStarted = true;

            FileWriter.WriteLine("-------------");
            FileWriter.WriteLine(Comment);
            FileWriter.WriteLine("-------------");

            FileWriter.WriteLine();

            FileWriter.WriteLine(
                "Time_abolute" + "\t" +
                "Time_relative" + "\t" +
                "Time_ms" + "\t" +
                "Value_scaled" + "\t" +
                "Value_unscaled" + "\t" +
                "HW CHannel number" + "\t" +
                "SW CHannel number" + "\t" +
                "Resync");
        }


        /// <summary>
        /// Opens the file_for_reading.
        /// </summary>
        /// <param name="FilePath_to_cfg">The file path to CFG.</param>
        /// <param name="Recorded_at">From File Header</param>
        /// <param name="Comment">From File Header</param>
        /// <param name="ModuleInfo">The module information.</param>
        /// <returns></returns>
        public bool Open_File_for_reading(string FilePath_to_cfg, ref DateTime Recorded_at, ref string Comment, ref List<CModuleBase> ModuleInfo)
        {
            bool ret = false;
            Comment = "";

            if (File.Exists(FilePath_to_cfg))
            {
                //Data file also here?
                string path_datafile = Path.ChangeExtension(FilePath_to_cfg, ".txt");
                try
                {
                    //ModuleInfo = CSave_Binary_Objects.ReadFromBinaryFile<List<CModuleBase>>(FilePath_to_cfg);
                }
                catch (Exception ee)
                {
                    ModuleInfo = [];
#if DEBUG
                    Console.WriteLine("Open_File_for_reading: " + ee.Message);
#endif
                }

                if (File.Exists(path_datafile))
                {
                    FileReader = new StreamReader(path_datafile);
                    if (FileReader is null) return false;

                    string? line = FileReader.ReadLine();        //Datum
                    if (line != null)
                    {
                        string[] ss = [" "];
                        string[] ls = line.Split(ss, StringSplitOptions.None);
                        Recorded_at = DateTime.Parse(ls[^2] + " " + ls[^1]);

                        _ = FileReader.ReadLine();   //--------
                        line = FileReader.ReadLine();   //First line Comment;
                        while (line != "-------------")
                        {
                            Comment += line + Environment.NewLine;
                            line = FileReader.ReadLine();
                        }
                        _ = FileReader.ReadLine();   //Leerzeile
                        _ = FileReader.ReadLine();   //Header
                                                     //... von nun an Daten
                        _File_Read_is_open = true;
                        ret = true;
                    }
                }
            }
            return ret;
        }
#endif
        /// <summary>
        /// Gets the next value from line in file
        /// </summary>
        public CDatafromFile? GetNextValueFrom_NMRecordedFile(string? Date = null)
        {
            if (FileReader != null)
            {
                if (!FileReader.EndOfStream)
                {
                    //Read an parse Line
                    string? line = FileReader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        return ParseLinefrom_NMRecordedFile((string)line, Date);
                }
            }
            return null;
        }

        public static CDatafromFile ParseLinefrom_NMRecordedFile(string line, string? Date = null)
        {
            return ParseLinefrom_NMRecordedFile(line.Split("\t".ToCharArray()), Date);
        }

        public static CDatafromFile ParseLinefrom_NMRecordedFile(string[] splitline, string? Date = null)
        {

            //Time_abolute .. [0]
            //Time_relative .. [1]
            //Time_ms .. [2]
            //Value_scaled .. [3]
            //Value_unscaled .. [4]
            //HW CHannel number .. [5]
            //SW CHannel number .. [6]
            //Resync .. [7]


            CDatafromFile cdf = new()
            {
                hwcn = Convert.ToInt16(splitline[5]), //hwcn
                swcn = Convert.ToInt16(splitline[6]), //swcn
                y_scaled = Convert.ToDouble(splitline[3].Replace(".", ",")),  //scaled
                y_unscaled = Convert.ToDouble(splitline[4]),   //unscaled
                dt_absolute = CMyTools.Get_DateTime_from_DateTime_with_ms(splitline[0], Date),
                time_ms = (int)Convert.ToDouble(splitline[2].Replace(".", ","))
            };
            return cdf;
        }


        /// <summary>
        /// Gets the next value from line in file
        /// </summary>
        public CDatafromFile? GetNextValueFrom_SDCardFile(string? Date = null)
        {
            if (FileReader != null)
            {
                if (!FileReader.EndOfStream)
                {
                    //Read an parse Line
                    string? line = FileReader.ReadLine();
                    if (line != null)
                        return ParseLinefrom_SDCardFile(line, Date);
                }
            }
            return null;
        }

        public static CDatafromFile? ParseLinefrom_SDCardFile(string line, string? Date = null)
        {
            return ParseLinefrom_SDCardFile(line.Split("\t".ToCharArray()), Date);
        }

        public static CDatafromFile? ParseLinefrom_SDCardFile(string[] splitline, string? Date = null)
        {
            CDatafromFile cdf = new()
            {
                hwcn = Convert.ToInt16(splitline[4]), //hwcn
                swcn = Convert.ToInt16(splitline[5]), //swcn
                y_scaled = Convert.ToDouble(splitline[2]),  //scaled
                y_unscaled = Convert.ToDouble(splitline[3]),   //unscaled
                dt_absolute = CMyTools.Get_DateTime_from_DateTime_with_ms(splitline[0], Date),
                time_ms = (int)Convert.ToDouble(splitline[1])
            };
            return cdf;
        }

        bool ResyncOK = false;

        public void SaveValue(CDataIn ci, double? scaled_value)
        {
            //Im File speichern
            if (_File_Write_is_open && FileWriter is not null)
            {
                //Wait for first resync
                if (ResyncOK)
                {
                    //c.yData[0] = DataReceiver.GetScaledValue(ci);
                    if (RecordingJustStarted)
                    {
                        //First value written to file
                        FirstRecordingTime = ci.DTAbsolute;
                        RecordingJustStarted = false;
                    }
                    double d = ci.Value;
                    if (scaled_value != null)
                        d = (double)scaled_value;
                    FileWriter.WriteLine(
                        CMyTools.Format_DateTime_with_ms(ci.DTAbsolute) + "\t" +
                        CMyTools.Format_DateTime_with_ms(ci.DTRelative) + "\t" +
                        (ci.DTAbsolute - FirstRecordingTime).TotalMilliseconds.ToString() + "\t" +
                        d.ToString() + "\t" +
                        ci.Value.ToString() + "\t" +
                        ci.HWcn.ToString() + "\t" +
                        ci.SWcn.ToString() + "\t" +
                        ci.Resync.ToString()); ;
                }
                else
                {
                    if (ci.Resync == true)
                        ResyncOK = true;
                }
            }
        }

        public void CloseFile()
        {
            if (_File_Write_is_open && FileWriter != null)
            {
                FileWriter.Close();
                _File_Write_is_open = false;
            }

            if (_File_Read_is_open && FileReader != null)
            {
                FileReader.Close();
                _File_Read_is_open = false;
            }
        }

        /// <summary>
        /// y_scaled: Calculated in GetValues
        /// </summary>
        public List<double> y_scaled = [];
        /// <summary>
        /// y_unscaled: Calculated in GetValues
        /// </summary>
        public List<double> y_unscaled = [];
        /// <summary>
        /// dt: Calculated in GetValues
        /// </summary>
        public List<DateTime> dt = [];
        /// <summary>
        /// time_ms: Calculated in GetValues
        /// </summary>
        public List<int> time_ms = [];

        /// <summary>
        /// Get all values of one specific channel
        /// Result in y_scaled, y_unscaled, dt, time_ms
        /// </summary>
        /// <param name="FilePath">The file path.</param>
        /// <param name="HW_cn">HW_cn</param>
        /// <param name="SW_cn">SW_cn</param>
        public void GetValues(string FilePath, int HW_cn, int SW_cn, int Rowstoignore = 6, bool NMrecorded = true)
        {
            if (File.Exists(FilePath))
            {
                Clear_Values();

                string header = "";
                List<string[]>? ss = CTextFileImporterDialog.ImportTextFile(ref FilePath, "\t", Rowstoignore, ref header);

                if (ss is not null && ss.Count > 0)
                {
                    foreach (string[] s in ss)
                    {
                        CDatafromFile? cdf;
                        if (NMrecorded)
                            cdf = ParseLinefrom_NMRecordedFile(s);
                        else
                            cdf = ParseLinefrom_SDCardFile(s);

                        if (cdf != null && (cdf.hwcn == HW_cn) && (SW_cn == cdf.swcn))
                        {
                            dt.Add(cdf.dt_absolute);
                            time_ms.Add(cdf.time_ms);
                            y_scaled.Add(cdf.y_scaled);
                            y_unscaled.Add(cdf.y_unscaled);
                        }
                    }
                }
            }
        }

        public void Clear_Values()
        {
            y_scaled.Clear();
            y_unscaled.Clear();
            dt.Clear();
            time_ms.Clear();
        }

        //public void AddValue(CSDCardImport.CDataIn_Scaled cds)
        //{
        //    y_scaled.Add(cds.Value_Scaled);
        //    y_unscaled.Add(cds.Value);
        //    dt.Add(cds.DTAbsolute);
        //    time_ms.Add((int)(cds.DTRelative - cds.ChannelStarted).TotalMilliseconds);
        //}

        public class CNMValues
        {
            public double time_ms { get; set; }
            public double time_ms_rebuilt { get; set; }
            public double y_scaled { get; set; }
            public double y_unscaled { get; set; }
            public DateTime dt { get; set; }

        }

        public class CValueChannel
        {
            private readonly List<double> time_ms1;
            private readonly List<double> time_ms_rebuilt1;
            private readonly List<double> y_scaled1;
            private readonly List<double> y_unscaled1;
            private readonly List<DateTime> dt1;

            public CValueChannel()
            {
                time_ms1 = [];
                time_ms_rebuilt1 = []; ;
                y_scaled1 = [];
                y_unscaled1 = [];
                dt1 = [];
            }

            public List<double> time_ms { get => time_ms1; }
            public List<double> time_ms_rebuilt { get => time_ms_rebuilt1; }
            public List<double> y_scaled { get => y_scaled1; }
            public List<double> y_unscaled { get => y_unscaled1; }
            public List<DateTime> dt { get => dt1; }

            public void Clear()
            {
                time_ms1.Clear();
                time_ms_rebuilt1.Clear();
                y_scaled1.Clear();
                y_unscaled1.Clear();
                dt1.Clear();
            }

            public void Add(double time_ms,
                double time_ms_rebuilt,
                double y_scaled,
                double y_unscaled,
                DateTime? dt)
            {
                time_ms1.Add(time_ms);
                time_ms_rebuilt1.Add(time_ms_rebuilt);
                y_scaled1.Add(y_scaled);
                y_unscaled1.Add(y_unscaled);
                if (dt != null)
                    dt1.Add((DateTime)dt);
            }

        }

        public static async Task<List<List<CValueChannel>>> GetAllValues(string FilePath, int Rowstoignore = 6, bool NMrecorded = true, List<CModuleBase>? ModuleInfo = null)
        {
            List<List<CValueChannel>> ret = [];
            int[,] sr = new int[7, 4];
            int[,] cnt_time_ms = new int[7, 4];

            for (int i = 0; i < 7; i++)
            {
                List<CValueChannel> cv = [];
                //List<int> _sr = [];
                for (int j = 0; j < 4; j++)
                {
                    cv.Add(new CValueChannel());

                }
                ret.Add(cv);
            }
            await Task.Delay(0);        //Just that function works

            //Get Sample ints
            if (ModuleInfo != null)
            {
                foreach (CModuleBase cmb in ModuleInfo)
                {
                    foreach (CSWChannel swc in cmb.SWChannels)
                    {
                        if (swc.SendChannel || swc.SendChannel)
                        {
                            sr[cmb.HWcn, swc.SWChannelNumber] = swc.SampleInt;
                        }
                    }
                }
            }

            if (File.Exists(FilePath))
            {
                string header = "";
                List<string[]>? ss = CTextFileImporterDialog.ImportTextFile(ref FilePath, "\t", Rowstoignore, ref header);
                if (ss != null)
                {
                    foreach (string[] s in ss)
                    {
                        CDatafromFile? cdf;
                        if (NMrecorded)
                            cdf = ParseLinefrom_NMRecordedFile(s);
                        else
                            cdf = ParseLinefrom_SDCardFile(s);

                        if (cdf != null)
                        {
                            ret[cdf.hwcn][cdf.swcn].Add(
                            cdf.time_ms,
                            cnt_time_ms[cdf.hwcn, cdf.swcn],
                            cdf.y_scaled,
                            cdf.y_unscaled,
                            cdf.dt_absolute);

                            if (sr[cdf.hwcn, cdf.swcn] != 0)
                            {
                                cnt_time_ms[cdf.hwcn, cdf.swcn] += sr[cdf.hwcn, cdf.swcn];
                            }
                        }
                    }
                }
            }
            return ret;
        }
    }
}
