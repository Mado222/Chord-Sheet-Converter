using ExcelDataReader;
using FeedbackDataLib;
using System.Data;
using ComponentsLib_GUI;

namespace Insight_Manufacturing5_net8.tests_measurements
{
    /***************************************
    //Allgemeine Konfiguration
    ****************************************/

    public static class CManufacturing5_Config
    {
        public static enumModuleType[] get_enumModuleTypes_toEvaluate()
        {
            enumModuleType[] enVals = (enumModuleType[])Enum.GetValues(typeof(enumModuleType));
            List<enumModuleType> lenVals = new List<enumModuleType>(enVals);
            lenVals.Remove(enumModuleType.cNotDefined);
            //lenVals.Remove(enumModuleType.cModuleAtemIRDig);
            lenVals.Remove(enumModuleType.cModuleExGADS94);
            lenVals.Remove(enumModuleType.cModuleMultiSCL);
            lenVals.Remove(enumModuleType.cModuleTypeEmpty);
            lenVals.Remove(enumModuleType.cModuleVaso);
            lenVals.Remove(enumModuleType.cModuleSCLADS);
            //lenVals.Remove(enumModuleType.cModuleAtem);

            return lenVals.ToArray();
        }
    }
}

