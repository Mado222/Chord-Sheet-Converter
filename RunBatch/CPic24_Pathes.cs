using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using WindControlLib;

namespace RunBatch
{
    public static class CPic24_Pathes
    {
        //public const string _localPath = @"d:\Daten\Insight\Bitbucket_Git\Firmware"; //working from dev environment
        public const string _localPath = @"d:\Daten\Insight\SW_Versioncontrol\Firmware"; //working from dev environment
        private static string basicPath = "";

        public static string BasicPath
        {
            get
            {
                if (basicPath == "")
                {
                    basicPath = GetBasicPath();
                }
                return basicPath;
            }
        }


        public static string GetBasicPath()
        {
            string localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string[] search = { "debug", "release" };
            int cnt = localPath.ToLower().Split(search, StringSplitOptions.None).Length - 1;


            if (cnt > 0)
            {
                //working from dev environment
                localPath = _localPath;
            }
            return localPath;
        }

        public static string GetCombinedHexPath()
        {
            return GetBasicPath () + @"\Combined Hex\";
        }

        public static string GetBinPath()
        {
            return GetBasicPath() + @"\Neuromodule bin files\";
        }



        //e.g. PicDirName = "@"\PIC24F Modules"
        public static string GetXPath (string PicDirName)
        {
            return PicDirName + PicDirName + @".X";    //"PIC24F Modules\PIC24F Modules.X" 
        }

        //e.g. PicDirName = "@"\PIC24F Modules"
        public static string GetXPath_nbproject(string PicDirName)
        {
            return GetXPath (PicDirName)+ @"\nbproject";    //"PIC24F Modules\PIC24F Modules.X\nbproject" 
        }
        public static string GetXPath_dist(string PicDirName)
        {
            return GetXPath(PicDirName) + @"\dist";    //"PIC24F Modules\PIC24F Modules.X\dist" 
        }

        public static string GetProduction_hex_file(string PicDirName)
        {
            return @"\production" + PicDirName + @".X.production.hex";
        }

        public static string GetBatch_dir(string PicDirName)
        {
            return PicDirName + @"\Build_batches";
        }

        public static string GetConfigName_from_Path(string ConfigPath)
        {
            string[] sep = { @"\" };
            string[] dirs = ConfigPath.Split(sep, StringSplitOptions.None);
            return dirs[dirs.Length - 1];
        }

    }
}
