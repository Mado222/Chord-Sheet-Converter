using MathNet.Filtering;

namespace MathNetNugetLib
{
    public class CSignalFilterFIR : CSignalFilterBase
    {

        public CSignalFilterFIR(EnSignalFilterType SignalFilterType, double fg, double sampleRate, int order = 2) :
            base(SignalFilterType, fg, sampleRate, order)
        {

            int half_order = (order - 1) / 2;
            switch (SignalFilterType)
            {
                case EnSignalFilterType.BandPass:
                    {
                        //_SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        break;
                    }
                case EnSignalFilterType.BandStop:
                    {
                        //_SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg, this.order);
                        break;
                    }
                case EnSignalFilterType.HighPass:
                    {
                        _SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateHighpass(ImpulseResponse.Finite, (double)this.SampleRate, (double)this.Fg, half_order);

                        break;
                    }
                case EnSignalFilterType.LowPass:
                    {
                        _SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateLowpass(ImpulseResponse.Finite, (double)this.SampleRate, this.Fg, this.Order);
                        break;
                    }
            }
        }

        public double ProcessSample(double newVal)
        {
            if (_SignalFilter is not null)
            {
                return _SignalFilter.ProcessSample(newVal);
            }
            return newVal;
        }
    }

}


