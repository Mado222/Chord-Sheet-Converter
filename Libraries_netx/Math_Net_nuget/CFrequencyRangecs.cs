namespace Math_Net_nuget
{
    /// <summary>
    /// Encapsulates Parameters related to frequency ranges
    /// </summary>
    public class CFrequencyRange
    {
        /// <summary>
        /// Lower frequency defining the frequency range
        /// </summary>
        private double f_Low = 0;

        /// <summary>
        /// Higher frequency defining the frequency range
        /// </summary>
        private double f_High = 0;

        protected double Calibration_factor = 1;

        protected double _value = 0;  //Uncalibrated value!!
        /// <summary>
        /// Value
        /// </summary>
        public double value
        {
            get { return _value * Calibration_factor; }
            set { _value = value / Calibration_factor; }
        }

        /// <summary>
        /// Frequency related to Peak
        /// </summary>
        public double Peak_Hz { get; set; } = 0;

        /// <summary>
        /// Max. value in the frequency range
        /// </summary>
        public double Peak { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CFrequencyRange" /> class.
        /// </summary>
        /// <param name="f_Low">f_Low</param>
        /// <param name="f_High">f_High</param>
        /// <param name="Calibration_factor">calibration_factor for value</param>
        public CFrequencyRange(double f_Low, double f_High, double Calibration_factor)
        {
            Init("NoName", f_Low, f_High, Calibration_factor);
        }
        public CFrequencyRange(string Name, double f_Low, double f_High, double Calibration_factor)
        {
            Init(Name, f_Low, f_High, Calibration_factor);
        }
        public void Init (string Name, double f_Low, double f_High, double Calibration_factor)
        {
            this.f_High = f_High;
            this.f_Low = f_Low;
            this.Calibration_factor = Calibration_factor;
            this.Name = Name;
            _value = 0;
        }


        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _value = 0;
            Peak_Hz = 0;
            Peak = 0;
        }

        /// <summary>
        /// Adds value if f is in the range of f_Low, f_High
        /// </summary>
        /// <param name="f">frequency</param>
        /// <param name="value">value</param>
        public virtual void Add(double f, double value)
        {
            if ((f >= f_Low) && (f <= f_High))
            {
                _value += value;
                if (value > Peak)
                {
                    Peak = value;
                    Peak_Hz = f;
                }
            }
        }
    }
}
