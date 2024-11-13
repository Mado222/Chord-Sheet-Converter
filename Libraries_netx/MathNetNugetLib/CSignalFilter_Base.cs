namespace MathNetNugetLib
{
    public class CSignalFilterBase(CSignalFilterBase.EnSignalFilterType SignalFilterType, double fg, double sampleRate, int order = 2)
    {
        public enum EnSignalFilterType
        {
            BandPass,
            BandStop,
            HighPass,
            LowPass
        }

        public double SampleRate { get; } = sampleRate;
        public double Fg { get; } = fg;
        public int Order { get; } = order;
        public EnSignalFilterType SignalFilterType { get; } = SignalFilterType;

        protected MathNet.Filtering.OnlineFilter? _SignalFilter;
    }
}



