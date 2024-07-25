using System;

namespace Math_Net_nuget
{
    public class CPeakDetect
    {

        public double[] Maxima_y { get; set; } = new double[0];
        public double[] Minima_y { get; set; } = new double[0];

        public double Mean_of_Maxima
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Median(Maxima_y);
            }
        }

        public double Mean_of_Minima
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Median(Minima_y);
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


            Maxima_y = new double[max_Length];
            Minima_y = new double[min_Length];

            Array.Copy(y, idx_max_Start, Maxima_y, 0, max_Length);
            Array.Copy(y, idx_min_Start, Minima_y, 0, min_Length);
        }
    }
}


