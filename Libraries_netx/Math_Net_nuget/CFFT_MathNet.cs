using System;
using System.Collections.Generic;
using WindControlLib;
using System.Linq;
using MathNet.Numerics.IntegralTransforms;

namespace Math_Net_nuget
{
    /// <summary>
    /// FFT class
    /// </summary>
    public class CFFT_MathNet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CFFT_MathNet"/> class.
        /// </summary>
        public CFFT_MathNet()
        {
        }

        /// <summary>
        /// Zur Amplitude gehörende Zeitwerte
        /// </summary>
        public double[] Data_x;

       
        /// <summary>
        /// Amplitude des Signals
        /// </summary>
        public double[] Data_y;
 

        /// <summary>
        /// Amplitude des 2. Signals
        /// </summary>
        /// <remarks>
        /// For correlation. MUST have Data_x as time base.
        /// Use: ResampleData_to_xBase
        /// </remarks>
        public double[] Data_y2;

        /// <summary>
        /// Smplingrate [1/s]
        /// </summary>
        public double SamplingRate
        {
            get { return 1 / SamplingIntervall; }
        }

        /// <summary>
        /// Gets the sampling intervall.
        /// </summary>
        /// <value>
        /// The sampling intervall.
        /// </value>
        public double SamplingIntervall
        {
            get
            {
                if ((Data_x != null) && (Data_x.Length > 1))
                {
                    return Data_x[1] - Data_x[0];
                }
                else
                    return 1;
            }
        }

        /// <summary>
        /// Amplitudenspektrum, SPITZENWERT
        /// </summary>
        public double[] fftAmplitudePeak { get; private set; }

        /// <summary>
        /// Sets fftAmplitude[0]=0
        /// </summary>
        public void SetDCZero()
        {
            fftAmplitudePeak[0]=0;
        }

        /// <summary>
        /// Amplitudenspektrum, RMS
        /// </summary>
        public double[] fftAmplitudeRMS
        {
            get
            {
                if (fftAmplitudePeak != null)
                {
                    double[] ret = new double[fftAmplitudePeak.Length];
                    double sqrt2 = Math.Sqrt(2);
                    for (int i = 1; i < ret.Length; i++)
                        ret[i] = fftAmplitudePeak[i] / sqrt2;
                    ret[0] = fftAmplitudePeak[0];
                    return ret;
                }
                return null;
            }
        }

        /// <summary>
        /// Leistungsdichtespektrum [s2/Hz]
        /// Power spectral density
        /// </summary>
        /// <remarks>
        /// Power spectrum is calculated first, and its values are divided with the of the frequency bin.
        /// This “normalization” makes comparisons between FFT sequences from different signal with different sample rates possible.
        /// 
        /// See: "FFT Zusammenfassung.docx", verified with SigView
        /// </remarks>
        /* --- removed 31.10.2017 --- does NOT have a sample rate INDEPENDENT result */
        public double[] fftAmplitudePowerDensity
        {
            get
            {
                throw (new Exception("Not implemented due to uninterpretable results"));
                /*
                double[] ret = new double[_fftAmplitude.Length];
                double Divisor = fftFrequ[1]*2; //binWidth*2
                //double Divisor = fftFrequ[_fftAmplitude.Length-1] *2; //binWidth*2

                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = Math.Pow(_fftAmplitude[i], 2);
                    ret[i] /= Divisor;
                }
                ret[0] += ret[0];   //*2
                return ret;*/
                //return null;
            }
        }


        /// <summary>
        /// Leistungsspektrum
        /// Power Spectrum
        /// </summary>
        /// <remarks>
        /// Jede Spektrallinie zeigt die richtige Power
        /// 
        /// See: "FFT Zusammenfassung.docx", verified with SigView
        /// </remarks>
        public double[] fftAmplitudePower
        {
            get
            {
                double[] ret = new double[fftAmplitudePeak.Length];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = Math.Pow(fftAmplitudePeak[i], 2);
                    ret[i] /= 2;   // /2
                }
                ret[0] += ret[0];   //*2
                return ret;
            }
        }

        private double[] _fftFrequ;
        /// <summary>
        /// Zugehörige Frequenzachse
        /// </summary>
        public double[] fftFrequ
        {
            get { return _fftFrequ; }
        }


        #region Spline
        /// <summary>
        /// Resamples y_data, x_data using spline Interpolation
        /// Results in Data_x (new time äquidistant time base), Data_y
        /// </summary>
        /// <param name="y_data">y data</param>
        /// <param name="x_data">x data</param>
        /// <param name="NumberOfPoints">Number of points to generate from data</param>
        /// <remarks>
        /// Data points MUST be equidistant
        /// </remarks>
        public void ResampleData(double[] y_data, double[] x_data, int NumberOfPoints)
        {
            ResampleData(ref y_data, ref x_data, NumberOfPoints);
            Data_x = x_data;
            Data_y = y_data;
        }

        public void ResampleData(ref double[] y_data, ref double[] x_data, int NumberOfPoints)
        {
            double x_FirstPoint = x_data[0];
            double x_LastPoint = x_data[x_data.Length - 1];

            double IntervalLength = x_LastPoint - x_FirstPoint;
            double incr = (double)(IntervalLength / (double)(y_data.Length - 1));


            //Make x-base
            double[] x = new double[y_data.Length];
            double d = x_FirstPoint;
            for (int i = 0; i < y_data.Length; i++)
            {
                x[i] = d;
                d += incr;
            }

            //MathNet.Numerics.Interpolation.IInterpolation sp = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkimaSorted(x_data, y_data);
            MathNet.Numerics.Interpolation.IInterpolation sp = MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(x_data, y_data);

            incr = (double)(IntervalLength / (double)(NumberOfPoints - 1));
            d = x_FirstPoint;

            double [] Data_x = new double[NumberOfPoints];
            double[] Data_y = new double[NumberOfPoints];

            for (int i = 0; i < NumberOfPoints; i++)
            {
                Data_x[i] = d;
                Data_y[i] = sp.Interpolate(d);
                d += incr;
            }

            y_data = Data_y;
            x_data = Data_x;
            
        }

        /// <summary>
        /// Resamples Data with differnt time base according to Data_X and stores result in Data_y2
        /// </summary>
        /// <param name="y_data">The y_data.</param>
        /// <param name="x_data">The x_data.</param>
        /// <remarks>
        /// Be carful that timeranges of Data_x and the provided x_data match
        /// if deviation is more than 10% of the range an error is thrown
        /// </remarks>
        public void ResampleData(double[] y_data, double[] x_data)
        {
            //Make x-base
            double IntervalLengthIn_10 = (x_data[x_data.Length-1] - x_data[0])*0.1;
            //double IntervalLengthx = (Data_x[Data_x.Length-1] - Data_x[0]);

            if ((x_data[x_data.Length-1] > (Data_x[Data_x.Length-1]+ IntervalLengthIn_10))  ||
                (x_data[0] > (Data_x[0]- IntervalLengthIn_10)))
            {
                throw (new Exception ("x ranges do not match"));
            }

            //AkimaSplineInterpolation sp = new AkimaSplineInterpolation();
            //sp.Init(x_data, y_data);
            MathNet.Numerics.Interpolation.IInterpolation sp = MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(x_data, y_data);

            Data_y2 = new double[Data_x.Length];

            for (int i = 0; i < Data_x.Length; i++)
            {
                Data_y2[i] = sp.Interpolate(Data_x[i]);
            }
        }


        /// <summary>
        /// Resamples according to Data_X and stores result in Data_y2
        /// </summary>
        /// <param name="y_data">The y_data.</param>
        public void ResampleData_to_xBase (double[] y_data)
        {
            //AkimaSplineInterpolation sp = new AkimaSplineInterpolation();
            //sp.Init(Data_x, y_data);

            MathNet.Numerics.Interpolation.IInterpolation sp = MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(Data_x, y_data);

            Data_y2 = new double[Data_x.Length];

            for (int i = 0; i < Data_x.Length; i++)
            {
                Data_y2[i] = sp.Interpolate(Data_x[i]);
            }
        }

        
        #endregion

        #region FFT

     
        /// <summary>
        /// FFT from Data_x, Data_y
        /// Result in fftAmplitudePeak, fftAmplitudeRMS, fftAmplitudePower, fftFrequ
        /// Zerstört _Data_cx
        /// </summary>
        public void FFT()
        {
            if (!CMyTools.IsPower_of_2((ulong)Data_y.Length))
            {
                throw new Exception("Number of Samples must be Power of 2; Consider to use Spline Upsampling");
            }

            System.Numerics.Complex[] _Data_cx = new System.Numerics.Complex[Data_y.Length];
            for (int i = 0; i < Data_y.Length; i++)
            {
                _Data_cx[i] = new System.Numerics.Complex(Data_y[i], 0);
            }


            //Fourier.Radix2Forward(_Data_cx, FourierOptions.Matlab); //bis Mathnet.Numerics 4.15
            Fourier.Forward(_Data_cx, FourierOptions.Matlab);  //ab Mathnet.Numerics 5.0

            //Da die FFT nach MAtlab Standard erstellt wird, sind noch folgende Skalierungen durchzuführen:

            //1) Berechnung des Amplitudenbetrags ampl= sqrt(re^2+img^2) / halbe Sampleanzahl bei Matlab-Konvention !!!!!!
            //2) Gleichzeitig generieren eines zugehörigen arrays mit den Frequenzen

            _fftFrequ = Fourier.FrequencyScale(_Data_cx.Length, SamplingRate);

            //scaleFFT();
            double HalfeNoSamples = ((double)(_Data_cx.Length / 2));

            fftAmplitudePeak = new double[_Data_cx.Length / 2];

            //Amplitude Spectrum
            for (int i = 0; i < _Data_cx.Length / 2; i++)     //Betrag eines jeden berechneten Spektrumanteils berechnen nur bis Length/2, da negative Frequqnzen unerheblich
            {
                fftAmplitudePeak[i] = _Data_cx[i].Magnitude / HalfeNoSamples;
            }
            fftAmplitudePeak[0] /= 2;  //Sonst ist DC um Fator 2 zu groß
        }


        /// <summary>
        /// Applies Hanning Window to Data_y
        /// </summary>
        /// <remarks>http://en.wikipedia.org/wiki/Window_function</remarks>
        public void Hanning()
        {
            double TwoPi = Math.PI * 2;
            double Nminus1 = Data_y.Length - 1;

            for (int i = 0; i < Data_y.Length; i++)
            {
                Data_y[i] *= 0.5 * (1 - Math.Cos(TwoPi * i / Nminus1));
            }
        }

        /// <summary>
        /// Applies Hanning Window to first and last Percent of Data_y
        /// </summary>
        /// <param name="percent">Percentage of datapoints that will be treated with Hanning window at the beginning AND the end</param>
        /// <param name="mean">Mean value of the IPIS (Hannin supresses to this value and NOT 0)</param>
        public void Hanning_Percent(double percent, double mean)
        {
            double TwoPi = Math.PI * 2;

            int numDataToProcess_halbe = (int) ((double) Data_y.Length * percent / 100);
            int numDataToProcess = 2*numDataToProcess_halbe;

            int idxUpperSlope = Data_y.Length - numDataToProcess_halbe;

            double Nminus1 = numDataToProcess - 1;
            for (int i = 0; i < numDataToProcess; i++)
            {

                double d;
                if (i < numDataToProcess_halbe)
                {
                    d = Data_y[i] - mean;
                    d *= 0.5 * (1 - Math.Cos(TwoPi * i / Nminus1));
                    Data_y[i] = d + mean;
                }
                else
                {
                    d = Data_y[idxUpperSlope + i - numDataToProcess_halbe] - mean;
                    d *= 0.5 * (1 - Math.Cos(TwoPi * i / Nminus1));
                    Data_y[idxUpperSlope + i - numDataToProcess_halbe] = d + mean;
                }
            }
        }


        /// <summary>
        /// Applies Hamming Window to Data_y
        /// </summary>
        /// <remarks>http://en.wikipedia.org/wiki/Window_function</remarks>
        public void Hamming()
        {
            double TwoPi = Math.PI * 2;
            double Nminus1 = Data_y.Length - 1;

            for (int i = 0; i < Data_y.Length; i++)
            {
                Data_y[i] *= (0.54 - 0.46 * (Math.Cos(TwoPi * i / Nminus1)));
            }
        }

        /// <summary>
        /// Power of fftAmplitude
        /// </summary>
        /// <param name="fupper">Upper frequeny of range for power calculation</param>
        /// <param name="flower">Lower frequeny of range for power calculation</param>
        /// <returns>
        /// Power of specified frequency range
        /// </returns>
        /// <remarks>
        /// FFT must have been run first
        /// Be carful with DC
        /// </remarks>
        public double GetPower(double fupper, double flower)
        {
            //pow = (1/2 * Sqrt(Summe aller Uspitze^2))^2

            double pow = 0;
            if ((fftAmplitudePeak == null) || fftAmplitudePeak.Length == 0)
            {
                throw new Exception("FFT must run first");
            }

            for (int i = 1; i < _fftFrequ.Length; i++)
            {
                if (_fftFrequ[i] >= flower)
                {
                    if (_fftFrequ[i] <= fupper)
                    {
                        pow += Math.Pow(fftAmplitudePeak[i], 2);
                    }
                }
            }

            pow /= 4;
            //ggf DC dazu
            if (flower == 0)
                pow += Math.Pow(fftAmplitudePeak[0], 2);

            return pow;
        }

        /// <summary>
        /// Ueff of fftAmplitude
        /// </summary>
        /// <param name="fupper">Upper frequeny of range for Ueff calculation</param>
        /// <param name="flower">Lower frequeny of range for Ueff calculation</param>
        /// <returns>
        /// Ueff of specified frequency range
        /// </returns>
        /// <remarks>
        /// FFT must have been run first
        /// Be carful with DC
        /// </remarks>
        public double GetUeff(double fupper, double flower)
        {
            //Ueff = 1/2 * Wurzel(Summe aller Uspitz^2)
            return Math.Sqrt(GetPower(fupper, flower));
        }

        
        /*
        private void scaleFFT()
        {
            _fftAmplitude = new double[Data_y.Length / 2];                                // Amplitudenspektrum
            //_fftPhase = new double[_Data_y.Length / 2];                                    // Phasenspektrum
            _fftFrequ = new double[Data_y.Length / 2];                                    // Zugehörige Frequenzachse

            double FrequencyStep = (SamplingRate) / (double)Data_y.Length;    //Abtastfrequ/Anzahl der Datenpunkte
            double CountFrequ = 0;

            for (int i = 0; i < Data_y.Length / 2; i++)     //Betrag eines jeden berechneten Spektrumanteils berechnenn nur bis Length/2, da negative Frequqnzen unerheblich
            {
                fftFrequ[i] = CountFrequ;
                CountFrequ += FrequencyStep;
            }
        }*/

        #endregion

        #region Correlation

        /// <summary>
        /// AutoCorrelation of Data_y
        /// </summary>
        /// <returns>
        /// Autokorrelation
        /// </returns>
        public double[] AutoCorrelation()
        {
            int size = Data_y.Length;
            double[] R = new double[size];
            double sum;
            for (int tau = 0; tau < size; tau++)
            {
                sum = 0;
                for (int t = 0; t < size - tau; t++)
                {
                    sum += Data_y[t] * Data_y[t + tau];
                }
                R[tau] = sum;
            }
           return R;
        }

        /// <summary>
        /// Crossescorrelation of Data_y and funct2
        /// </summary>
        /// <param name="funct2">Data to correlate with</param>
        /// <returns>
        /// Cross Correlation
        /// </returns>
        /// <remarks>
        /// funct 1 and funct 2 must have same no of samples
        /// </remarks>
        public double[] CrossCorrelation(double[] funct2)
        {
            return CrossCorrelation (Data_y, funct2);
        }

        /// <summary>
        /// Crossescorrelation of funct1 and funct2
        /// </summary>
        /// <param name="funct1">Data</param>
        /// <param name="funct2">Data to correlate with</param>
        /// <returns>
        /// Cross Correlation
        /// </returns>
        /// <remarks>
        /// funct 1 and funct 2 must have same no of samples
        /// </remarks>
        public double[] CrossCorrelation(double[] funct1, double[] funct2)
        {
            double[] R = new double[funct1.Length];
            for (int tau = 0; tau < funct1.Length; tau++)
            {
                R[tau] = CrossCorrelation(funct1, funct2, tau);
            }
            return R;
        }

        public double CrossCorrelation(double[] funct1, double[] funct2, int tau)
        {
            double sum;

            sum = 0;
            for (int t = 0; t < funct2.Length - tau; t++)
            {
                sum += funct1[t] * funct2[t + tau];
            }
            return sum;
        }

        /// <summary>
        /// Normalised Crossescorrelation of funct1 and funct2
        /// </summary>
        /// <param name="funct1">Data 1</param>
        /// <param name="funct2">Data 2</param>
        /// <returns>
        /// Normalised Cross Correlation
        /// </returns>
        /// <remarks>
        /// funct 1 and funct 2 must have same no of samples
        /// </remarks>
        public double[] CrossCorrelationNormalized(double[] funct1, double[] funct2)
        {
            double[] R = new double[funct1.Length];

            //Calculate mean and Standarddeviation
            CalcAvgStdv(funct1, out double mean1, out double stddev1);
            CalcAvgStdv(funct2, out double mean2, out double stddev2);
            double stddev = stddev1 * stddev2;

            for (int tau = 0; tau < funct1.Length; tau++)
            {
                R[tau] = CrossCorrelationNormalized(funct1, funct2, mean1, mean2, stddev, tau);
            }
            return R;
        }

        /// <summary>
        /// Normalised Crossescorrelation of funct1 and funct2 for one tau
        /// </summary>
        /// <param name="funct1">Data 1</param>
        /// <param name="funct2">Data 2</param>
        /// <param name="mean1">mean value of function 1</param>
        /// <param name="mean2">mean value of function 2</param>
        /// <param name="stddev">(standard deviation function 1)*(standard deviation function 2)</param>
        /// <param name="tau">tau</param>
        /// <returns></returns>
        private double CrossCorrelationNormalized(double[] funct1, double[] funct2, double mean1, double mean2, double stddev, int tau)
        {
            double sum;
            sum = 0;
            if (tau >= 0)
            {
                for (int t = 0; t < (funct2.Length - tau); t++)
                {
                    sum += ((funct1[t] - mean1) * (funct2[t + tau] - mean2));
                }
            }
            else
            {
                tau = Math.Abs(tau);
                for (int t = 0; t < (funct1.Length - tau); t++)
                {
                    sum += ((funct2[t] - mean2) * (funct1[t + tau] - mean1));
                }
            }
            sum /= stddev;
            return sum / (double)funct1.Length;
        }

        public double [] CrossCorrelationNormalized(double[] funct1, double[] funct2, int tau1, int tau2)
        {
            double[] R = new double[tau2 - tau1 +1];

            //Calculate mean and Standarddeviation
            CalcAvgStdv(funct1, out double mean1, out double stddev1);
            CalcAvgStdv(funct2, out double mean2, out double stddev2);
            double stddev = stddev1 * stddev2;

            int cnt = 0;    
            for (int tau = tau1; tau < tau2; tau++)
                {
                    R[cnt] = CrossCorrelationNormalized(funct1, funct2, mean1, mean2, stddev, tau);
                    cnt++;
                }
            
            return R;
        }


        #endregion

        /// <summary>
        /// Normalizes Data_y to +/- 1
        /// </summary>
        public void Normalize(ref double[] y_data)
        {
            double MaxVal = 0;
            foreach (double d in y_data)
            {
                if (Math.Abs(d) > MaxVal)
                { MaxVal= d; }
            }

            for (int i = 0; i < y_data.Length; i++)
            {
                y_data[i] /= MaxVal;
            }
        }

        /// <summary>
        /// Calculates Average and Standard Deviation
        /// </summary>
        /// <param name="values">values</param>
        /// <param name="Avg">Average</param>
        /// <param name="Stdv">Stddev</param>
        public void CalcAvgStdv(double[] values, out double Avg, out double Stdv)
        {
            //Average
            if (values.Length > 0)
            {
                List<double> doubleList = new List<double>(values);
                Avg = doubleList.Average(); // --> Requires Linq

                //Standard Deviation
                double sumOfDerivation = 0;
                foreach (double value in doubleList)
                {
                    sumOfDerivation += (value) * (value);
                }
                double sumOfDerivationAverage = sumOfDerivation / doubleList.Count;
                Stdv = Math.Sqrt(sumOfDerivationAverage - (Avg * Avg));
            }
            else
            {
                Avg = 0;
                Stdv = 0;
            }
        }

    }

}
    

