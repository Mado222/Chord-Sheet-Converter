using MathNet.Filtering;

namespace MathNetNugetLib
{
    public class CSignalFilterOnlineFilter : CSignalFilterBase
    {

        public CSignalFilterOnlineFilter(EnSignalFilterType SignalFilterType, double sampleRate, double fg, int order = 2) :
            base(SignalFilterType, fg, sampleRate, order)
        {
            switch (SignalFilterType)
            {
                case EnSignalFilterType.BandPass:
                    {
                        /*
                        if (order != null)
                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg,  (int) this.order);
                        else
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        */
                        break;
                    }
                case EnSignalFilterType.BandStop:
                    {
                        /*                        if (order != null)
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg, (int) this.order);
                        else
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        */
                        break;
                    }
                case EnSignalFilterType.HighPass:
                    {
                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateHighpass(ImpulseResponse.Finite, this.SampleRate, this.Fg, this.Order);
                        break;
                    }
                case EnSignalFilterType.LowPass:
                    {

                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateLowpass(ImpulseResponse.Finite, this.SampleRate, this.Fg, this.Order);
                        break;
                    }
            }
        }

        public void Reset()
        {
            _SignalFilter?.Reset();
        }

        public double ProcessSample(double newVal)
        {
            if (_SignalFilter != null)
            {
                return _SignalFilter.ProcessSample(newVal);
            }
            return newVal;
        }
    }

}


