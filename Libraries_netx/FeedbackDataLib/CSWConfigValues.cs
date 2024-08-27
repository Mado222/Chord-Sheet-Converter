namespace FeedbackDataLib
{
    public class CSWConfigValues : ICloneable
    {
        /// <summary>
        /// Sample Interval
        /// </summary>
        /// <value>Interval [ms]</value>
        /// <remarks></remarks>
        public ushort SampleInt
        {
            get { return _SampleInt; }
            set { _SampleInt = value; }
        }
        private ushort _SampleInt;


        /// <summary>
        /// Flag: Channel data is sent
        /// </summary>
        /// <value><c>true</c> if [Channel should be sent]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Flag is extracted from _ConfigByte
        /// </remarks>
        public bool SendChannel
        {
            get
            {
                if ((ConfigByte & 0x0001) != 0) return true;
                return false;
            }
            set
            {
                ConfigByte &= 0xFFFE;
                if (value) ConfigByte = (ushort)(ConfigByte | 0x0001);
            }
        }

        /// <summary>
        /// Flag: Channel data is sved on SD card
        /// </summary>
        public bool SaveChannel
        {
            get
            {
                if ((ConfigByte & 0x0100) != 0) return true;
                return false;
            }
            set
            {
                ConfigByte &= 0xFEFF;
                if (value) ConfigByte = (ushort)(ConfigByte | 0x0100);
            }
        }
        public ushort ConfigByte;

        public CSWConfigValues(ushort sampleInt, bool sendChannel, bool saveChannel)
        {
            Update(sampleInt, sendChannel, saveChannel);
        }

        public void Update(ushort sampleInt, bool sendChannel, bool saveChannel)
        {
            SendChannel = sendChannel;
            SaveChannel = saveChannel;
            SampleInt = sampleInt;
        }

        public CSWConfigValues()
        {
            Update (200, false, false);
        }

        public void Update (CSWConfigValues sWConfigValues)
        {
            SendChannel = sWConfigValues.SendChannel;
            SaveChannel= sWConfigValues.SaveChannel;
            SampleInt = sWConfigValues.SampleInt;
        }

        public object Clone()
        {
            return (CSWConfigValues) MemberwiseClone();
        }
    }
}
