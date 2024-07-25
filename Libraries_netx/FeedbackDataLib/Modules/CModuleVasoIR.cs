namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleVasoIR: CModuleBase
    {
        public CModuleVasoIR()
        {
            ModuleColor = Color.DarkViolet;
            ModuleName = "Vaso";
            ModuleType = enumModuleType.cModuleVasosensorDig;

            cSWChannelNames =
                [
                "Puls [1]",
                "Herzfrequenz_Puls [bpm]",
                "Puls Amplitude",
                "Vaso-Unused"
                ];
            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeVaso0,
                enumSWChannelType.cSWChannelTypeVaso1,
                enumSWChannelType.cSWChannelTypeVaso2,
                enumSWChannelType.cSWChannelTypeVaso3
            ];
        }

        public override void Update_ModuleTypeFromDevice(ushort ModuleTypeFromDevice)
        {
            base.Update_ModuleTypeFromDevice(ModuleTypeFromDevice);
            //Make Infrared Dig Vasosensor to Vasosensor 13.6.2017
            ModuleTypeNumber = (byte)enumModuleType.cModuleVaso;
        }

        #region Vaso_IR_Params
        /*
        unsigned short t_calc_new_scaling_ms;
        unsigned short t_max_overload_time_ms;
        unsigned short t_inOverload_time_ms;
        short post_shift_value;
        unsigned char MovingAVG_Buffersize_asPowerof2;
        unsigned char MovingAVG_Buffersize_overload_asPowerof2;
        */
        private double _vasoIR_Current_scalingfactor;

        //positions of variables in byte array ModuleSpecific
        private const int vpos_t_calc_new_scaling_ms = 0;

        private const int vpos_t_max_overload_time_ms = vpos_t_calc_new_scaling_ms + 2;

        private const int vpos_led_current_for_proximity = vpos_t_max_overload_time_ms + 2;

        private const int vpos_Min_ScalingFactor_asPowerof2 = vpos_led_current_for_proximity + 1;
        private const int vpos_Max_ScalingFactor_asPowerof2 = vpos_Min_ScalingFactor_asPowerof2 + 1;
        private const int vpos_Current_scalingfactor_asPowerof2 = vpos_Max_ScalingFactor_asPowerof2 + 1;

        private const int vpos_MovingAVG_Current_Buffersize_asPowerof2 = vpos_Current_scalingfactor_asPowerof2 + 1;
        private const int vpos_MovingAVG_Max_Buffersize_asPowerof2 = vpos_MovingAVG_Current_Buffersize_asPowerof2 + 1;
        private const int vpos_MovingAVG_Buffersize_overload_asPowerof2 = vpos_MovingAVG_Max_Buffersize_asPowerof2 + 1;

        public ushort VasoIR_t_calc_new_scaling_ms
        {
            get => BitConverter.ToUInt16(ModuleSpecific, vpos_t_calc_new_scaling_ms);
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, vpos_t_calc_new_scaling_ms, b.Length);
            }
        }
        public ushort VasoIR_t_max_overload_time_ms
        {
            get => BitConverter.ToUInt16(ModuleSpecific, vpos_t_max_overload_time_ms);
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, vpos_t_max_overload_time_ms, b.Length);
            }
        }

        public byte VasoIR_led_current_for_proximity
        {
            get => ModuleSpecific[vpos_led_current_for_proximity];
            set => ModuleSpecific[vpos_led_current_for_proximity] = value;
        }

        public byte VasoIR_Min_ScalingFactor_asPowerof2
        {
            get => ModuleSpecific[vpos_Min_ScalingFactor_asPowerof2];
            set => ModuleSpecific[vpos_Min_ScalingFactor_asPowerof2] = value;
        }

        public byte VasoIR_Max_ScalingFactor_asPowerof2
        {
            get => ModuleSpecific[vpos_Max_ScalingFactor_asPowerof2];
            set => ModuleSpecific[vpos_Max_ScalingFactor_asPowerof2] = value;
        }

        public byte VasoIR_Current_scalingfactor_asPowerof2
        {
            get => ModuleSpecific[vpos_Current_scalingfactor_asPowerof2];
            set => ModuleSpecific[vpos_Current_scalingfactor_asPowerof2] = value;
        }

        private byte _last_VasoIR_Current_scalingfactor_asPowerof2;
        private double _VasoIR_Current_scalingfactor;

        public double VasoIR_Current_scalingfactor
        {
            get
            {
                if (VasoIR_Current_scalingfactor_asPowerof2 != _last_VasoIR_Current_scalingfactor_asPowerof2)
                {
                    _last_VasoIR_Current_scalingfactor_asPowerof2 = VasoIR_Current_scalingfactor_asPowerof2;
                    _VasoIR_Current_scalingfactor = Math.Pow(2, _last_VasoIR_Current_scalingfactor_asPowerof2);
                }
                return _VasoIR_Current_scalingfactor;
            }

            set => _vasoIR_Current_scalingfactor = value;
        }


        public byte VasoIR_MovingAVG_Current_Buffersize_asPowerof2
        {
            get => ModuleSpecific[vpos_MovingAVG_Current_Buffersize_asPowerof2];
            set => ModuleSpecific[vpos_MovingAVG_Current_Buffersize_asPowerof2] = value;
        }

        public byte VasoIR_MovingAVG_Max_Buffersize_asPowerof2
        {
            get => ModuleSpecific[vpos_MovingAVG_Max_Buffersize_asPowerof2];
            set => ModuleSpecific[vpos_MovingAVG_Max_Buffersize_asPowerof2] = value;
        }

        public byte VasoIR_MovingAVG_Buffersize_overload_asPowerof2
        {
            get => ModuleSpecific[vpos_MovingAVG_Buffersize_overload_asPowerof2];
            set => ModuleSpecific[vpos_MovingAVG_Buffersize_overload_asPowerof2] = value;
        }
        #endregion

    }
}
