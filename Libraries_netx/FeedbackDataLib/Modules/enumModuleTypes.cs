using System;
using System.Xml.Serialization;

namespace FeedbackDataLib
{
    /// <summary>
    /// Types of Modules
    /// </summary>
    /// <remarks>
    /// Muessen mit der Konfiguration im PIC zusammenstimmen
    /// MUSS in Aufsteigender Reihenfolge sein - siehe UbpdateModuleInfoFrom_ByteArray
    /// um die Anzahl der selben Module zu zählen
    /// </remarks>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public enum enumModuleType
    {
        /// <summary>
        /// ModuleType: Empty
        /// </summary>
        [XmlEnum(Name = "Empty")]
        cModuleTypeEmpty = 0x00,
        /// <summary>
        /// ModuleType: Multisensor
        /// </summary>
        [XmlEnum(Name = "Multisensor")]
        cModuleMultisensor = 0x01,
        /// ModuleType: MultiSCL
        /// </summary>
        [XmlEnum(Name = "MultiSCL")]
        cModuleMultiSCL = 0x0B,
        /// <summary>
        /// ModuleType: EMG
        /// </summary>
        [XmlEnum(Name = "EMG")]
        cModuleEMG = 0x02,
        /// <summary>
        /// ModuleType: ECG
        /// </summary>
        [XmlEnum(Name = "ECG")]
        cModuleECG = 0x03,
        /// <summary>
        /// ModuleType: EEG
        /// </summary>
        [XmlEnum(Name = "EEG")]
        cModuleEEG = 0x04,
        /// <summary>
        /// ModuleType: Atem
        /// </summary>
        [XmlEnum(Name = "Atem")]
        cModuleAtem = 0x05,
        /// <summary>
        /// ModuleType: Vaso VCNL4010
        /// </summary>
        [XmlEnum(Name = "Vaso_IR_Dig")]
        cModuleVasosensorDig = 0x09,
        /// <summary>
        /// ModuleType: Vasosensor
        /// </summary>
        [XmlEnum(Name = "Vaso")]
        cModuleVaso = 0x07,
        /// <summary>
        /// ModuleType: Atem Infrared Digital
        /// </summary>
        [XmlEnum(Name = "Atem_IR_Dig")]
        cModuleAtemIRDig = 0x06,
        /// <summary>
        /// ModuleType: EXG_ADS
        /// </summary>
        [XmlEnum(Name = "EXG_ADS")]
        cModuleExGADS = 0x0C,
        /// <summary>
        /// ModuleType: SCL_ADS
        /// </summary>
        [XmlEnum(Name = "SCL_ADS")]
        cModuleSCLADS = 0x0D,
        /// <summary>
        /// ModuleType: EXG_ADS94
        /// </summary>
        [XmlEnum(Name = "EXG_ADS94")]
        cModuleExGADS94 = 0x0E,
        /// <summary>
        /// ModuleType: Neuromaster / only for Manufacturing
        /// </summary>
        [XmlEnum(Name = "Neuromaster")]
        cNeuromaster = 100,
        /// <summary>
        /// ModuleType: Neurolink / only for Manufacturing
        /// </summary>
        [XmlEnum(Name = "Neurolink")]
        cNeurolink = 101,
        [XmlEnum(Name = "Not defined")]
        cNotDefined = 255
    }
}
