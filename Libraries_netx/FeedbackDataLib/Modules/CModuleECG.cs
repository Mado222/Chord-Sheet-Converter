using WindControlLib;

namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleECG : CModuleBase
    {
        public CModuleECG()
        {
            ModuleColor = Color.Orange;
            ModuleName = "ECG";
            ModuleType = enumModuleType.cModuleECG;

            cSWChannelNames =
                [
                "ECG raw [V]",
                "Herzfrequenz_ECG [bpm]",
                "ECG Amplitude",
                "ECG3-unused"
                ];

            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeECG0,
                enumSWChannelType.cSWChannelTypeECG1,
                enumSWChannelType.cSWChannelTypeECG2,
                enumSWChannelType.cSWChannelTypeECG3
            ];
        }

        public override List<CDataIn> Processdata(CDataIn di)
        {
            List<CDataIn> _DataIn = [];
            //Not sending of 0 values in ECG bpm channel added 24.10.2014
            if (SWChannels[di.SW_cn].SWChannelType_enum == enumSWChannelType.cSWChannelTypeECG1)
            {
                if (di.Value != 0)
                    _DataIn.Add(di);
            }
            else
            {
                _DataIn.Add(di);
            }
            return _DataIn;
        }
    }
}
