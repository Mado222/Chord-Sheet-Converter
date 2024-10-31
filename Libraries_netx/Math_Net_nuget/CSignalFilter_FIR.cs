using MathNet.Filtering;

namespace MathNetNuget
{
    public class CSignalFilterFIR : CSignalFilterBase
    {

        public CSignalFilterFIR(enumSignalFilterType SignalFilterType, double fg, double sampleRate, int order = 2) :
            base(SignalFilterType, fg, sampleRate, order)
        {

            int half_order = (order - 1) / 2;
            switch (SignalFilterType)
            {
                case enumSignalFilterType.BandPass:
                    {
                        //_SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateBandpass(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg);
                        break;
                    }
                case enumSignalFilterType.BandStop:
                    {
                        //_SignalFilter = MathNet.Filtering.OnlineFilter.CreateBandstop(ImpulseResponse.Finite, (double)this.sampleRate, 1 / this.fg, this.order);
                        break;
                    }
                case enumSignalFilterType.HighPass:
                    {
                        _SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateHighpass(ImpulseResponse.Finite, (double)this.sampleRate, (double)this.fg, half_order);

                        break;
                    }
                case enumSignalFilterType.LowPass:
                    {
                        _SignalFilter = MathNet.Filtering.FIR.OnlineFirFilter.CreateLowpass(ImpulseResponse.Finite, (double)this.sampleRate, this.fg, this.order);
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


