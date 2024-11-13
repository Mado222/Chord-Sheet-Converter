namespace MathNetNugetLib
{
    /// <summary>
    /// Encapsulates Parameters related to frequency ranges
    /// </summary>
    public class CFrequencyRange
    {
        /// <summary>
        /// Lower frequency defining the frequency range
        /// </summary>
        private double f_Low;

        /// <summary>
        /// Higher frequency defining the frequency range
        /// </summary>
        private double f_High;

        protected double Calibration_factor = 1;

        protected double _value = 0;  //Uncalibrated value!!
        /// <summary>
        /// Value
        /// </summary>
        public double Value
        {
            get { return _value * Calibration_factor; }
            set { _value = value / Calibration_factor; }
        }

        /// <summary>
        /// Frequency related to Peak
        /// </summary>
        public double PeakHz { get; set; } = 0;

        /// <summary>
        /// Max. value in the frequency range
        /// </summary>
        public double Peak { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CFrequencyRange" /> class.
        /// </summary>
        /// <param name="fLow">f_Low</param>
        /// <param name="fHigh">f_High</param>
        /// <param name="CalibrationFactor">calibration_factor for value</param>
        public CFrequencyRange(double fLow, double fHigh, double CalibrationFactor)
        {
            Init("NoName", fLow, fHigh, CalibrationFactor);
        }
        public CFrequencyRange(string Name, double fLow, double fHigh, double CalibrationFactor)
        {
            Init(Name, fLow, fHigh, CalibrationFactor);
        }
        public void Init(string Name, double fLow, double fHigh, double CalibrationFactor)
        {
            f_High = fHigh;
            f_Low = fLow;
            Calibration_factor = CalibrationFactor;
            this.Name = Name;
            _value = 0;
        }


        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _value = 0;
            PeakHz = 0;
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
                    PeakHz = f;
                }
            }
        }
    }
}
