using MathNet.Filtering;

namespace Math_Net_nuget
{
    public class CSignalFilter_Base
    {
        public enum enumSignalFilterType
        {
            BandPass,
            BandStop,
            HighPass,
            LowPass
        }

        public double sampleRate { get; }
        public double fg { get; }
        public int order { get; }
        public enumSignalFilterType SignalFilterType { get; }

        protected MathNet.Filtering.OnlineFilter _SignalFilter;


        public CSignalFilter_Base(enumSignalFilterType SignalFilterType, double fg, double sampleRate, int order=2)
        {
            this.sampleRate = sampleRate;
            this.fg = fg;
            this.order = order;
            this.SignalFilterType = SignalFilterType;
        }
    }
}

    

