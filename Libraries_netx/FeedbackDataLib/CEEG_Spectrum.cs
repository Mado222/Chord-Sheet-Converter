using Math_Net_nuget;
using System;
using WindControlLib;
using System.Collections.Generic;

/*
 * Es sollte ja nun die FFT Analyse im PC gemacht werden und wir benötigen folgende Berechnungen online (während der Messung).
 * A. BÄNDER (Amplituden)
 * Delta 1-3
 * Theta 4-8
 * Alpha 8-12
 * SMR 12-15
 * Low Beta 15-18
 * Beta 18-23
 * High Beta 23-30
 * Artefakte 51-58
 * 
 * B. FFT - Balkendiagramm
 * zwischen 1 und 30 Hz:
 * ingesamt 30 Balken in 1 Hz -Schritten*/

namespace FeedbackDataLib
{
    /// <summary>
    /// Class to calculate EEG spectrum
    /// </summary>
    public class CEEG_Spectrum
    {
        public class CEEG_FrequencyRanges : CFrequencyRange
        {
            public CEEG_FrequencyRanges(double f_Low, double f_High, double Calibration_factor) : base("NoName", f_Low, f_High, Calibration_factor)
            {
            }

            public CEEG_FrequencyRanges(string Name,double f_Low, double f_High, double Calibration_factor) : base(Name, f_Low, f_High, Calibration_factor)
            {
            }

            public override void Add(double f, double value)
            {
                base.Add(f, Math.Pow(value, 2));
            }
            /// <summary>
            /// Value
            /// </summary>
            public new double value
            {
                get { return Math.Sqrt(_value) * Calibration_factor; }
            }
        }

        private CFFT_MathNet FFT_MathNet;
        public List<CEEG_FrequencyRanges> EEG_Bands;

        public DateTime dtLastFFT { get; set; } = DateTime.Now;

        private double _Hanninng_Percentage = 0;
        /// <summary>
        /// Gets or sets the hanninng percentage.
        /// </summary>
        public double Hanninng_Percentage
        {
            get { return _Hanninng_Percentage; }
            set { _Hanninng_Percentage = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CEEG_Spectrum"/> class.
        /// </summary>
        public CEEG_Spectrum(CEEG_FrequencyRanges[] Frequ_Ranges)
        {
            Init_CEEG_Spectrum(Frequ_Ranges);
        }

        private void Init_CEEG_Spectrum(in CEEG_FrequencyRanges[] Frequ_Ranges)
        {
            FFT_MathNet = new CFFT_MathNet();
            EEG_Bands = [];
            for (int i = 0; i< Frequ_Ranges.Length; i++)
            {
                EEG_Bands.Add(Frequ_Ranges[i]);
            }
        }

        /// <summary>
        /// Processes the eeg.
        /// </summary>
        public void Process_Spectrum(double[] samples, double Sample_time_ms)
        {
            FFT_MathNet.Data_x = new double[] { 0, (double)Sample_time_ms / 1000 }; //2 Werte genügen
            FFT_MathNet.Data_y = samples;

            if (Hanninng_Percentage != 0)
            {
                FFT_MathNet.Hanning_Percent(Hanninng_Percentage, 0);
            }

            FFT_MathNet.FFT();

            //Reset bands
            foreach (CEEG_FrequencyRanges cf in EEG_Bands) cf?.Reset();
            double[] fftFrequ = FFT_MathNet.fftFrequ;
            double[] fftRMS = FFT_MathNet.fftAmplitudeRMS; //Power [s^2/Hz]

            //Skip DC
            fftRMS[0] = 0;
            FFT_MathNet.SetDCZero();

            double f, y;

            for (int i = 0; i < fftRMS.Length; i++)
            {
                f = fftFrequ[i];
                y = fftRMS[i]; //Power [s^2/Hz]

                foreach (CEEG_FrequencyRanges cf in EEG_Bands)
                    cf?.Add(f, y);
            }
            dtLastFFT = DateTime.Now;
        }

        /// <summary>
        /// Bildet den Effektivwert aller Spektralanteile im 1Hz Bereich 0..1, 1..2, ...
        /// Brechnet Spektrum neu, wenn letzte Brechnung (ProcessSprectrum) länger als 2s her ist
        /// </summary>
        /// <param name="samples">data</param>
        /// <param name="Sample_time_ms">The sample time ms.</param>
        /// <returns>
        /// Ueff von 0 ... 29Hz
        /// </returns>
        public double[]? GetEEGSpectrum_1Hz_Steps(double[] samples, double Sample_time_ms)
        {
            if (dtLastFFT + new TimeSpan(0, 0, 2) < DateTime.Now)
                Process_Spectrum (samples, Sample_time_ms);
            
            if (FFT_MathNet.fftFrequ != null)
            {
                double[] arr = new double[30];

                double[] fftFrequ = FFT_MathNet.fftFrequ;
                double[] fftRMS = FFT_MathNet.fftAmplitudeRMS; //- RMS!! not Power [s^2/Hz]

                double f, y;
                int cnt_Steps = 0;

                for (int i = 0; i < fftRMS.Length; i++)
                {
                    f = fftFrequ[i];
                    y = fftRMS[i]; //- RMS!! not Power [s^2/Hz]

                    if (f >= cnt_Steps + 1)
                    {
                        cnt_Steps++;
                        if (cnt_Steps >= arr.Length)
                            break;
                    }

                    arr[cnt_Steps] += Math.Pow(y, 2);
                }

                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = Math.Sqrt(arr[i]);
                }
                return arr;
            }
            return null;
        }
    }
}
