using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedbackDataLib;
using WindControlLib;

namespace Insight_Manufacturing5_net8
{
    public class CNMChannelResults
    {
        public CNMChannelResult[] ChannelResults;

        public CNMChannelResults()
        {
            ChannelResults = new CNMChannelResult[C8KanalReceiverV2_CommBase.max_num_SWChannels];
            for (int i = 0; i < ChannelResults.Length; i++)
            {
                ChannelResults[i] = new CNMChannelResult();
            }
        }
    }

    public class CNMChannelResult
    {
        public double sumData = 0;
        public long sumDataHex = 0;
        public double sumsqaredData = 0;
        public double sumrectifiedData = 0;
        public int SamplesCollected = 0;
        public int noSamplesIgnored_in_the_Beginning = 0;
        public int SamplesAdded = 0;

        public List<double> Scaledvalues = new List<double>();

        private readonly CRingpuffer cache_hex = new CRingpuffer(10000);
        private double[] _hexvals = null;

        private readonly List<double> cache_scaled = new List<double>();
        private double[] _scaledvals = null;

        public double Umean => CalcMean(sumData);

        public double Urectified_mean => CalcMean(sumrectifiedData);

        public double Ueff => Math.Sqrt(CalcMean(sumsqaredData));

        public double[] hexvals
        {
            get
            {
                if (_hexvals == null)
                {
                    cache_hex.PopAll(ref _hexvals);
                    cache_hex.Clear();
                }
                return _hexvals;
            }
        }

        public double[] scaledvals
        {
            get
            {
                if (_scaledvals == null)
                {
                    _scaledvals = cache_scaled.ToArray();
                    //cache_scaled.PopAll(ref _scaledvals);
                    cache_scaled.Clear();
                }
                return _scaledvals;
            }
        }


        private double CalcMean(double val)
        {
            if (SamplesAdded != 0)
                return val / SamplesAdded;
            return 0;
        }

        public class CVals_Statistics
        {
            public double ueff = 0;
            public double u_maximum = int.MinValue;
            public double u_minimum = int.MaxValue;
            public double umean = 0;
            public double umedian = 0;
            public int numVals_pos_saturated = 0;
            public int numVals_neg_saturated = 0;
            public bool isSaturated = false;
        }

        public CVals_Statistics Get_scaledVals_Statistics()
        {
            return Get_Vals_Statistics(scaledvals);
        }

        public CVals_Statistics Get_scaledVals_Statistics_cut_sinus()
        {
            double[] dd = Cut_sin_vals(scaledvals);
            if (_scaledvals != null)
            {
                _scaledvals = dd;
            }
            //Wenn kein Sinus erkannt - normale Auswertun
            return Get_Vals_Statistics(_scaledvals);
        }

        public CVals_Statistics Get_hexVals_Statistics_cut_sinus_no_hexOffset(int hexOffset)
        {
            CVals_Statistics ret = null;
            if (_hexvals != null)
            {
                double[] hv = new double[_hexvals.Length];
                Array.Copy(_hexvals, hv, _hexvals.Length);

                if (hexOffset != 0)
                {
                    for (int i = 0; i < hv.Length; i++)
                    {
                        hv[i] -= hexOffset;
                    }
                }
                ret = Get_Vals_Statistics(Cut_sin_vals(hv));
            }
            return ret;
        }

        public double[] Cut_sin_vals(double[] scaledvals)
        {
            double[] ret = null;
            //Search rising index
            int idx_rising = -1;
            for (int i = 0; i < scaledvals.Length - 2; i++)
            {
                if ((scaledvals[i] <= 0) && (scaledvals[i + 1] > 0))
                {
                    idx_rising = i;
                    break;
                }
            }
            //Search falling index
            int idx_falling = -1;
            if (idx_rising >= 0)
            {
                for (int i = scaledvals.Length - 2; i > idx_rising; i--)
                {
                    if ((scaledvals[i] <= 0) && (scaledvals[i + 1] > 0))
                    {
                        idx_falling = i;
                        break;
                    }
                }

                if (idx_falling > 0 && idx_rising >= 0)
                {
                    List<double> sv = new List<double>(scaledvals);
                    sv.RemoveRange(idx_falling, sv.Count - idx_falling);
                    sv.RemoveRange(0, idx_rising);
                    ret = sv.ToArray();
                }
            }
            return ret;
        }

        public CVals_Statistics Get_hexVals_Statistics(string exception_text = "")
        {
            CVals_Statistics ret = Get_Vals_Statistics(hexvals);
            // Check saturation
            if (hexvals != null)
            {
                foreach (int d in hexvals)
                {
                    if (d > 0xFFFE) ret.numVals_pos_saturated++;
                    if (d < 0x1) ret.numVals_neg_saturated++;
                }
                int sum = ret.numVals_pos_saturated + ret.numVals_neg_saturated;
                if (sum > 0.03 * hexvals.Count())
                {
                    ret.isSaturated = true;
                    if (exception_text != "")
                        throw new InvalidOperationException(exception_text);
                }
            }
            return ret;
        }

        public CVals_Statistics Get_Vals_Statistics(IEnumerable<double> vals)
        {
            CVals_Statistics ret = new CVals_Statistics();
            if (vals != null)
            {
                ret.ueff = MathNet.Numerics.Statistics.Statistics.RootMeanSquare(vals);
                ret.u_maximum = MathNet.Numerics.Statistics.Statistics.Maximum(vals);
                ret.u_minimum = MathNet.Numerics.Statistics.Statistics.Minimum(vals);
                ret.umean = MathNet.Numerics.Statistics.Statistics.Mean(vals);
                ret.umedian = MathNet.Numerics.Statistics.Statistics.Median(vals);
                ret.numVals_pos_saturated = 0;
                ret.numVals_neg_saturated = 0;
                ret.isSaturated = false;
            }
            return ret;
        }

        public void AddValue(double value, CDataIn DataIn = null)
        {
            SamplesCollected++;
            if (SamplesCollected >= noSamplesIgnored_in_the_Beginning)
            {
                SamplesAdded++;
                sumData += value;
                if (DataIn != null) cache_hex.Push((double)DataIn.Value);
                sumsqaredData += value * value;
                sumrectifiedData += Math.Abs(value);
                cache_scaled.Add(value);
            }
            else
            {
                cache_hex.Clear();
                cache_scaled.Clear();
            }
        }
    }
}
