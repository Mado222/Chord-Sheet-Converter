using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunBatch
{
    internal static class CMakeBinFilename
    {
        public static string GetBinFilename(FeedbackDataLib.enumModuleType ModuleType_enum, string HWVersion)
        {
            string ret;

            UInt16 binFilenamehex = (byte)ModuleType_enum;
            binFilenamehex = (UInt16)(binFilenamehex << 8);

            /*
            switch (ModuleType_enum)
            {
                case FeedbackDataLib.enumModuleType.cModuleMultisensor:
                    switch (HWVersion)
                    {

                    }

                    break;
                case FeedbackDataLib.enumModuleType.cModuleEMG:
                    break;
                case FeedbackDataLib.enumModuleType.cModuleECG:
                    break;
                case FeedbackDataLib.enumModuleType.cModuleEEG:
                    break;
                case FeedbackDataLib.enumModuleType.cModuleAtem:
                    break;
                case FeedbackDataLib.enumModuleType.cModuleVasoIRDig:
                    break;
                case FeedbackDataLib.enumModuleType.cModuleAtemIRDig:
                    break;
                case FeedbackDataLib.enumModuleType.cNotDefined:
                    break;
                default:
                    break;
            };*/

            binFilenamehex += Convert.ToUInt16(HWVersion);
            ret = binFilenamehex.ToString("X4");


            return ret;
        }
    }
}
