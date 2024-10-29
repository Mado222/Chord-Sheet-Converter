using System;

namespace MathNetNuget
{
    public class CPeakDetect
    {

        public double[] MaximaY { get; set; } = [];
        public double[] MinimaY { get; set; } = [];

        public double MeanOfMaxima
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Median(MaximaY);
            }
        }

        public double MeanOfMinima
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Median(MinimaY);
            }
        }


        public void CalcPeaks(double[] y, int numPeaks)
        {
            Array.Sort(y);

            //Wir nehmen die oberen und unteren 10% für min und max
            //Berechnen der Indizes
            /*
            int idx_max_Start = y.Length - (int)((double)y.Length * .1);
            int max_Length = y.Length - idx_max_Start;

            int idx_min_Start = 0;
            int min_Length = max_Length;*/

            int idx_max_Start = y.Length - numPeaks;
            int max_Length = numPeaks;

            int idx_min_Start = 0;
            int min_Length = max_Length;


            MaximaY = new double[max_Length];
            MinimaY = new double[min_Length];

            Array.Copy(y, idx_max_Start, MaximaY, 0, max_Length);
            Array.Copy(y, idx_min_Start, MinimaY, 0, min_Length);
        }
    }
}


