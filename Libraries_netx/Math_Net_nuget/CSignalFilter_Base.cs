using MathNet.Filtering;

namespace MathNetNuget
{
    public class CSignalFilterBase
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


        public CSignalFilterBase(enumSignalFilterType SignalFilterType, double fg, double sampleRate, int order = 2)
        {
            this.sampleRate = sampleRate;
            this.fg = fg;
            this.order = order;
            this.SignalFilterType = SignalFilterType;
        }
    }
}



