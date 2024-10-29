namespace FeedbackDataLib.Modules
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CModuleMultisensor : CModuleBase
    {
        public CModuleMultisensor()
        {
            ModuleName = "Multisensor";
            ModuleColor = Color.GreenYellow; //Color.FromArgb(Convert.ToInt32("FFADFF2F", 16)); //Grey

            cSWChannelNames =
    [
                "SCL [S]",
                "Temperature [°C]",
                "Pulse [1]",
                "Herzfrequenz_Pulse [bpm]"
    ];

            cSWChannelTypes =
            [
                enumSWChannelType.cSWChannelTypeSCL,
                enumSWChannelType.cSWChannelTypeTemp,
                enumSWChannelType.cSWChannelTypePulse0,
                enumSWChannelType.cSWChannelTypePulse1
            ];

        }


        protected override void Setup_SWChannels()
        {
            base.Setup_SWChannels();
        }



        #region Multisensor_Params
        /******* Neu am 18.4.2017 ******/
        /// <summary>
        /// Gets or sets a value indicating whether Pulse is in run mode
        /// </summary>
        /// <value>
        ///   <c>true</c> if pulse is running; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// ModuleSpecific[1]
        /// bit 0: =0: Integrator is running
        ///        =1: Integrator short circuited
        /// bit 1: =0: Amplification Low
        ///        =1: Amplification High
        /// </remarks>
        public bool PulseRun
        {
            get
            {
                if ((ModuleSpecific[1] & 1) == 0)
                {
                    //Bit0 = 0
                    return true;
                }
                return false;
            }
            set
            {
                ModuleSpecific[1] &= 0xfe;  //bit 0 löschen
                if (!value)
                {
                    ModuleSpecific[1] += 1;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Amplification of pulse is low
        /// </summary>
        /// <value>
        ///   <c>true</c> if Amplification of pulse is low; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// ModuleSpecific[1]
        /// bit 0: =0: Integrator is running
        ///        =1: Integrator short circuited
        /// bit 1: =0: Amplification Low
        ///        =1: Amplification High
        /// </remarks>
        public bool Pulse_v_high
        {
            get
            {
                if ((ModuleSpecific[1] & 2) != 0)
                {
                    //Bit1 = 1
                    return true;
                }
                return false;
            }
            set
            {
                ModuleSpecific[1] &= 0xfd;  //bit 1 löschen
                if (value)
                {
                    ModuleSpecific[1] += 2;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SCL is in test mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is in test mode; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// ModuleSpecific[0]
        /// bit 0: =0: Measuring
        ///        =1: SCL in Test Mode, switched to resistors
        /// bit 1: =0: Low Resistor
        ///        =1: High Resistor
        /// </remarks>
        public bool SCLTest
        {
            get
            {
                if ((ModuleSpecific[0] & 0x01) != 0)
                    return true;
                return false;
            }
            set
            {
                ModuleSpecific[0] &= 0xfe;      //clear bit 0
                if (value)
                {
                    ModuleSpecific[0] += 1; //set bit 0
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SCL Test resistor ist high ohm.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SCL test resistor is high; low otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// ModuleSpecific[0]
        /// bit 0: =0: Measuring
        ///        =1: SCL in Test Mode, switched to resistors
        /// bit 1: =0: Low Resistor
        ///        =1: High Resistor
        /// </remarks>
        public bool SCLTest_Resistor_High
        {
            get
            {
                if ((ModuleSpecific[0] & 0x02) != 0)
                    return true;
                return false;
            }
            set
            {
                ModuleSpecific[0] &= 0xfd;      //clear bit 1
                if (value)
                {
                    ModuleSpecific[0] += 2; //set bit 1
                }
            }
        }
        /**************/
        #endregion

    }
}
