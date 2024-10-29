namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleRespI : CModuleAtem
    {
        public CModuleRespI()
        {
            ModuleColor = Color.Blue;
            ModuleName = "Resp-I";

            cSWChannelNames =
                [
                "Atem raw [1]",
                "Atem-Frequenz [1/min]",
                "Atem-Amplitude [1]",
                "Atem-Unused"
                ];
            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeAtemIRDig0,
                enumSWChannelType.cSWChannelTypeAtemIRDig1,
                enumSWChannelType.cSWChannelTypeAtemIRDig2,
                enumSWChannelType.cSWChannelTypeAtemIRDig3
            ];
        }

        public override void Update_ModuleTypeFromDevice(ushort ModuleTypeFromDevice)
        {
            base.Update_ModuleTypeFromDevice(ModuleTypeFromDevice);
            //Make Infrared Atemsensor to Atemsensor 2.12.2014
            ModuleTypeNumber = (byte)enumModuleType.cModuleAtem;
        }

        #region Atem_IR_Params
        //positions of variables in byte array ModuleSpecific
        private const int pos_t_calc_new_scaling_ms = 0;
        private const int pos_t_max_overload_time_ms = pos_t_calc_new_scaling_ms + 2;
        private const int pos_t_inOverload_time_ms = pos_t_max_overload_time_ms + 2;
        private const int pos_post_shift_value = pos_t_inOverload_time_ms + 2;
        private const int pos_MovingAVG_Current_Buffersize_asPowerof2 = pos_post_shift_value + 2;
        private const int pos_MovingAVG_Max_Buffersize_asPowerof2 = pos_MovingAVG_Current_Buffersize_asPowerof2 + 1;
        private const int pos_MovingAVG_Buffersize_overload_asPowerof2 = pos_MovingAVG_Max_Buffersize_asPowerof2 + 1;
        private const int pos_ILED_10 = pos_MovingAVG_Buffersize_overload_asPowerof2 + 1;

        public ushort AtemIR_t_calc_new_scaling_ms
        {
            get { return BitConverter.ToUInt16(ModuleSpecific, pos_t_calc_new_scaling_ms); }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, pos_t_calc_new_scaling_ms, b.Length);
            }
        }
        public ushort AtemIR_t_max_overload_time_ms
        {
            get { return BitConverter.ToUInt16(ModuleSpecific, pos_t_max_overload_time_ms); }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, pos_t_max_overload_time_ms, b.Length);
            }
        }

        public ushort AtemIR_t_inOverload_time_ms
        {
            get { return BitConverter.ToUInt16(ModuleSpecific, pos_t_inOverload_time_ms); }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, pos_t_inOverload_time_ms, b.Length);
            }
        }

        public short AtemIR_post_shift_value
        {
            get { return BitConverter.ToInt16(ModuleSpecific, pos_post_shift_value); }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Buffer.BlockCopy(b, 0, ModuleSpecific, pos_post_shift_value, b.Length);
            }
        }

        public byte AtemIR_MovingAVG_Current_Buffersize_asPowerof2
        {
            get { return ModuleSpecific[pos_MovingAVG_Current_Buffersize_asPowerof2]; }
            set
            {
                ModuleSpecific[pos_MovingAVG_Current_Buffersize_asPowerof2] = value;
            }
        }

        public byte AtemIR_MovingAVG_Max_Buffersize_asPowerof2
        {
            get { return ModuleSpecific[pos_MovingAVG_Max_Buffersize_asPowerof2]; }
            set
            {
                ModuleSpecific[pos_MovingAVG_Max_Buffersize_asPowerof2] = value;
            }
        }

        public byte AtemIR_MovingAVG_Buffersize_overload_asPowerof2
        {
            get { return ModuleSpecific[pos_MovingAVG_Buffersize_overload_asPowerof2]; }
            set
            {
                ModuleSpecific[pos_MovingAVG_Buffersize_overload_asPowerof2] = value;
            }
        }

        public byte AtemIR_ILED_10
        {
            get { return ModuleSpecific[pos_ILED_10]; }
            set
            {
                ModuleSpecific[pos_ILED_10] = value;
            }
        }

        #endregion

    }
}
