namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public enum enumSWChannelType
    {
        cSWChannelTypeNotDefined =1000,
        /// Multisensor: 
        cSWChannelTypeSCL = (enumModuleType.cModuleMultisensor << 8) + 0,
        cSWChannelTypeTemp = (enumModuleType.cModuleMultisensor << 8) + 1,
        cSWChannelTypePulse0 = (enumModuleType.cModuleMultisensor << 8) + 2,
        cSWChannelTypePulse1 = (enumModuleType.cModuleMultisensor << 8) + 3,  ////Heart rate

        //MultiSCL
        cSWChannelTypeMSCL0 = (enumModuleType.cModuleMultiSCL << 8) + 0,
        cSWChannelTypeMSCL1 = (enumModuleType.cModuleMultiSCL << 8) + 1,
        cSWChannelTypeMSCL2 = (enumModuleType.cModuleMultiSCL << 8) + 2,
        cSWChannelTypeMSCL3 = (enumModuleType.cModuleMultiSCL << 8) + 3,

        //EMG
        cSWChannelTypeEMG0 = (enumModuleType.cModuleEMG << 8) + 0,
        cSWChannelTypeEMG1 = (enumModuleType.cModuleEMG << 8) + 1,        //Gleitender Mittelwert 
        cSWChannelTypeEMG2 = (enumModuleType.cModuleEMG << 8) + 2,        //Mittelwert - notwendig???
        cSWChannelTypeEMG3 = (enumModuleType.cModuleEMG << 8) + 3,

        //EKG
        cSWChannelTypeECG0 = (enumModuleType.cModuleECG << 8) + 0,
        cSWChannelTypeECG1 = (enumModuleType.cModuleECG << 8) + 1,            //IPI[ms]
        cSWChannelTypeECG2 = (enumModuleType.cModuleECG << 8) + 2,         //R-Amplitude
        cSWChannelTypeECG3 = (enumModuleType.cModuleECG << 8) + 3,

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
            cSWChannelTypeEEG0 = (enumModuleType.cModuleEEG << 8) + 0,
            cSWChannelTypeEEG1 = (enumModuleType.cModuleEEG << 8) + 1,
            cSWChannelTypeEEG2 = (enumModuleType.cModuleEEG << 8) + 2,
            cSWChannelTypeEEG3 = (enumModuleType.cModuleEEG << 8) + 3,
#endif

        //EEG_ADS
        //cSWChannelTypeEEGADS0 = (((UInt16)enumModuleType.cModuleEEGADS << 8) + 0),
        //cSWChannelTypeEEGADS1 = (((UInt16)enumModuleType.cModuleEEGADS << 8) + 1),
        //cSWChannelTypeEEGADS2 = (((UInt16)enumModuleType.cModuleEEGADS << 8) + 2),
        //cSWChannelTypeEEGADS3 = (((UInt16)enumModuleType.cModuleEEGADS << 8) + 3),

        //ExG_ADS
        cSWChannelTypeExGADS0 = (enumModuleType.cModuleExGADS << 8) + 0,
        cSWChannelTypeExGADS1 = (enumModuleType.cModuleExGADS << 8) + 1,
        cSWChannelTypeExGADS2 = (enumModuleType.cModuleExGADS << 8) + 2,
        cSWChannelTypeExGADS3 = (enumModuleType.cModuleExGADS << 8) + 3,
        cSWChannelTypeImpADS0 = (enumModuleType.cModuleExGADS << 8) + 4,
        cSWChannelTypeImpADS1 = (enumModuleType.cModuleExGADS << 8) + 5,
        cSWChannelTypeImpADS2 = (enumModuleType.cModuleExGADS << 8) + 6,
        cSWChannelTypeImpADS3 = (enumModuleType.cModuleExGADS << 8) + 7,


        //SCL_ADS
        cSWChannelTypeSCLADS0 = (enumModuleType.cModuleSCLADS << 8) + 0,
        cSWChannelTypeSCLADS1 = (enumModuleType.cModuleSCLADS << 8) + 1,
        cSWChannelTypeSCLADS2 = (enumModuleType.cModuleSCLADS << 8) + 2,
        cSWChannelTypeSCLADS3 = (enumModuleType.cModuleSCLADS << 8) + 3,

        //Atem
        cSWChannelTypeAtem0 = (enumModuleType.cModuleAtem << 8) + 0,
        cSWChannelTypeAtem1 = (enumModuleType.cModuleAtem << 8) + 1,
        cSWChannelTypeAtem2 = (enumModuleType.cModuleAtem << 8) + 2,
        cSWChannelTypeAtem3 = (enumModuleType.cModuleAtem << 8) + 3,

        //Atem_IR_Dig
        cSWChannelTypeAtemIRDig0 = (enumModuleType.cModuleAtemIRDig << 8) + 0,
        cSWChannelTypeAtemIRDig1 = (enumModuleType.cModuleAtemIRDig << 8) + 1,
        cSWChannelTypeAtemIRDig2 = (enumModuleType.cModuleAtemIRDig << 8) + 2,
        cSWChannelTypeAtemIRDig3 = (enumModuleType.cModuleAtemIRDig << 8) + 3,

        //Vasosensor
        cSWChannelTypeVaso0 = (enumModuleType.cModuleVaso << 8) + 0,
        cSWChannelTypeVaso1 = (enumModuleType.cModuleVaso << 8) + 1,       //IPI[ms]
        cSWChannelTypeVaso2 = (enumModuleType.cModuleVaso << 8) + 2,       //Pulse Amplitude
        cSWChannelTypeVaso3 = (enumModuleType.cModuleVaso << 8) + 3,

        //Vasosensor_IR_Dig
        cSWChannelTypeVasoIRDig0 = (enumModuleType.cModuleVasosensorDig << 8) + 0,
        cSWChannelTypeVasoIRDig1 = (enumModuleType.cModuleVasosensorDig << 8) + 1,       //IPI[ms]
        cSWChannelTypeVasoIRDig2 = (enumModuleType.cModuleVasosensorDig << 8) + 2,       //Pulse Amplitude
        cSWChannelTypeVasoIRDig3 = (enumModuleType.cModuleVasosensorDig << 8) + 3

    }
}
