namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public enum EnSWChannelType
    {
        cSWChannelTypeNotDefined = 1000,
        /// Multisensor: 
        cSWChannelTypeSCL = (EnModuleType.cModuleMultisensor << 8) + 0,
        cSWChannelTypeTemp = (EnModuleType.cModuleMultisensor << 8) + 1,
        cSWChannelTypePulse0 = (EnModuleType.cModuleMultisensor << 8) + 2,
        cSWChannelTypePulse1 = (EnModuleType.cModuleMultisensor << 8) + 3,  ////Heart rate

        //MultiSCL
        cSWChannelTypeMSCL0 = (EnModuleType.cModuleMultiSCL << 8) + 0,
        cSWChannelTypeMSCL1 = (EnModuleType.cModuleMultiSCL << 8) + 1,
        cSWChannelTypeMSCL2 = (EnModuleType.cModuleMultiSCL << 8) + 2,
        cSWChannelTypeMSCL3 = (EnModuleType.cModuleMultiSCL << 8) + 3,

        //EMG
        cSWChannelTypeEMG0 = (EnModuleType.cModuleEMG << 8) + 0,
        cSWChannelTypeEMG1 = (EnModuleType.cModuleEMG << 8) + 1,        //Gleitender Mittelwert 
        cSWChannelTypeEMG2 = (EnModuleType.cModuleEMG << 8) + 2,        //Mittelwert - notwendig???
        cSWChannelTypeEMG3 = (EnModuleType.cModuleEMG << 8) + 3,

        //EKG
        cSWChannelTypeECG0 = (EnModuleType.cModuleECG << 8) + 0,
        cSWChannelTypeECG1 = (EnModuleType.cModuleECG << 8) + 1,            //IPI[ms]
        cSWChannelTypeECG2 = (EnModuleType.cModuleECG << 8) + 2,         //R-Amplitude
        cSWChannelTypeECG3 = (EnModuleType.cModuleECG << 8) + 3,

#if VIRTUAL_EEG
        //EEG
        cSWChannelTypeEEG00 = (((UInt16)enumModuleType.cModuleEEG << 8) + 0),   //Raw -> Bezeichnungen sind mit Orginal-EEG kompatibel
        cSWChannelTypeEEG01 = (((UInt16)enumModuleType.cModuleEEG << 8) + 1),   //Delta
        cSWChannelTypeEEG02 = (((UInt16)enumModuleType.cModuleEEG << 8) + 2),   //Alpha
        cSWChannelTypeEEG03 = (((UInt16)enumModuleType.cModuleEEG << 8) + 3),   //Beta
        cSWChannelTypeEEG04 = (((UInt16)enumModuleType.cModuleEEG << 8) + 4),
        cSWChannelTypeEEG05 = (((UInt16)enumModuleType.cModuleEEG << 8) + 5),
        cSWChannelTypeEEG06 = (((UInt16)enumModuleType.cModuleEEG << 8) + 6),
        cSWChannelTypeEEG07 = (((UInt16)enumModuleType.cModuleEEG << 8) + 7),
        cSWChannelTypeEEG08 = (((UInt16)enumModuleType.cModuleEEG << 8) + 8),
        cSWChannelTypeEEG09 = (((UInt16)enumModuleType.cModuleEEG << 8) + 9),
        cSWChannelTypeEEG10 = (((UInt16)enumModuleType.cModuleEEG << 8) + 10),
        cSWChannelTypeEEG11 = (((UInt16)enumModuleType.cModuleEEG << 8) + 11),

#else
        //EEG
        cSWChannelTypeEEG0 = (EnModuleType.cModuleEEG << 8) + 0,
        cSWChannelTypeEEG1 = (EnModuleType.cModuleEEG << 8) + 1,
        cSWChannelTypeEEG2 = (EnModuleType.cModuleEEG << 8) + 2,
        cSWChannelTypeEEG3 = (EnModuleType.cModuleEEG << 8) + 3,
#endif

        //ExG_ADS
        cSWChannelTypeExGADS0 = (EnModuleType.cModuleExGADS94 << 8) + 0,
        cSWChannelTypeExGADS1 = (EnModuleType.cModuleExGADS94 << 8) + 1,
        cSWChannelTypeExGADS2 = (EnModuleType.cModuleExGADS94 << 8) + 2,
        cSWChannelTypeExGADS3 = (EnModuleType.cModuleExGADS94 << 8) + 3,

        //SCL_ADS
        cSWChannelTypeSCLADS0 = (EnModuleType.cModuleSCLADS << 8) + 0,
        cSWChannelTypeSCLADS1 = (EnModuleType.cModuleSCLADS << 8) + 1,
        cSWChannelTypeSCLADS2 = (EnModuleType.cModuleSCLADS << 8) + 2,
        cSWChannelTypeSCLADS3 = (EnModuleType.cModuleSCLADS << 8) + 3,

        //Atem
        cSWChannelTypeAtem0 = (EnModuleType.cModuleAtem << 8) + 0,
        cSWChannelTypeAtem1 = (EnModuleType.cModuleAtem << 8) + 1,
        cSWChannelTypeAtem2 = (EnModuleType.cModuleAtem << 8) + 2,
        cSWChannelTypeAtem3 = (EnModuleType.cModuleAtem << 8) + 3,

        //Atem_IR_Dig
        cSWChannelTypeAtemIRDig0 = (EnModuleType.cModuleAtemIRDig << 8) + 0,
        cSWChannelTypeAtemIRDig1 = (EnModuleType.cModuleAtemIRDig << 8) + 1,
        cSWChannelTypeAtemIRDig2 = (EnModuleType.cModuleAtemIRDig << 8) + 2,
        cSWChannelTypeAtemIRDig3 = (EnModuleType.cModuleAtemIRDig << 8) + 3,

        //Vasosensor
        cSWChannelTypeVaso0 = (EnModuleType.cModuleVaso << 8) + 0,
        cSWChannelTypeVaso1 = (EnModuleType.cModuleVaso << 8) + 1,       //IPI[ms]
        cSWChannelTypeVaso2 = (EnModuleType.cModuleVaso << 8) + 2,       //Pulse Amplitude
        cSWChannelTypeVaso3 = (EnModuleType.cModuleVaso << 8) + 3,

        //Vasosensor_IR_Dig
        cSWChannelTypeVasoIRDig0 = (EnModuleType.cModuleVasosensorDig << 8) + 0,
        cSWChannelTypeVasoIRDig1 = (EnModuleType.cModuleVasosensorDig << 8) + 1,       //IPI[ms]
        cSWChannelTypeVasoIRDig2 = (EnModuleType.cModuleVasosensorDig << 8) + 2,       //Pulse Amplitude
        cSWChannelTypeVasoIRDig3 = (EnModuleType.cModuleVasosensorDig << 8) + 3

    }
}
