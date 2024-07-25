using MathNet.Filtering;

namespace Math_Net_nuget
{
    public class CSignalFilter_OnlineFilter: CSignalFilter_Base
    {

        public CSignalFilter_OnlineFilter(enumSignalFilterType SignalFilterType, double sampleRate, double fg, int order= 2):
            base (SignalFilterType, fg, sampleRate, order)
        {
            switch (SignalFilterType)
            {
                case enumSignalFilterType.BandPass:
                    {
                        /*
                        if (order != null)
                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg,  (int) this.order);
                        else
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        */
                        break;
                    }
                case enumSignalFilterType.BandStop:
                    {
                        /*                        if (order != null)
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg, (int) this.order);
                        else
                            _SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        */
                        break;
                    }
                case enumSignalFilterType.HighPass:
                    {
                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateHighpass(ImpulseResponse.Finite, this.sampleRate, this.fg, this.order);
                        break;
                    }
                case enumSignalFilterType.LowPass:
                    {

                        _SignalFilter = MathNet.Filtering.OnlineFilter.CreateLowpass(ImpulseResponse.Finite, this.sampleRate, this.fg, this.order);
                        break;
                    }
            }
        }

        public void Reset ()
        {
            if (_SignalFilter != null)
                _SignalFilter.Reset();
        }

        public double ProcessSample(double newVal) => _SignalFilter.ProcessSample(newVal);
    }

}
    

