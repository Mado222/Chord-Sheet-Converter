using FilteringLibrary;
using WindControlLib;


namespace Neuromaster_V5
{
    public class CVasoProcessor
    {
        public const double HP_fg = 10;
        public const long CONF_SAMPLING_FREQ = 500;

        public const long t_calc_new_scaling_ms_default = 5000;     //was 3000
        public const long t_max_overload_time_ms_default = 500;	//Max Zeit in Übersteuerung - dann wird Verstärkungsfaktor halbiert


        CIntegerMovingAverager avg = new CIntegerMovingAverager(2048);    //debug Vaso
        //CSignalFilter HP;
        //CDoubleMovingAverager davg = new CDoubleMovingAverager(1024);
        MathNet.Filtering.OnlineFilter HP;

        CHPFilter_Micromodeler HPM = new CHPFilter_Micromodeler();


        public CVasoProcessor()
        {
            Init_VasoProcessor();
        }

        public void Init_VasoProcessor()
        {
            //HP = new CSignalFilter(enumSignalFilterType.HighPass, HP_fg, SampleRate, 2);
            HP = MathNet.Filtering.OnlineFilter.CreateHighpass(MathNet.Filtering.ImpulseResponse.Finite, CONF_SAMPLING_FREQ, 0.2, 1);

            t_max_overload_time_cnts = (UInt16) (t_max_overload_time_ms_default * CONF_SAMPLING_FREQ / (long)1000);
            t_calc_new_scaling_cnts = (UInt16) (t_calc_new_scaling_ms_default * CONF_SAMPLING_FREQ / (long)1000);
        }

        public double ProcessVasoSample(double value)
        {
            //Filtern
            double[] val = new double[1];
            val[0] = value;

            val= HP.ProcessSamples(val);
            return val[0];
            //return value - davg.Push(value);
            //return Denoise.ProcessSample(value);
        }

        private UInt16 cntsamples_scal = 0;
        private UInt16 cnt_neg_overload_time = 0;
        private UInt16 cnt_pos_overload_time = 0;
        private UInt16 t_max_overload_time_cnts= 0;
        private UInt16 t_calc_new_scaling_cnts= 0;
        private byte scalingfactor_asPowerof2 = 64;

        private byte Min_ScalingFactor_asPowerof2_default = 6;
        private byte Max_ScalingFactor_asPowerof2_default = 10;

        private Int16 amax, amin, max = 0;


        public Int16 ProcessVasoSampleAutoRange_red(UInt16 val)
        {
            double f;
            cntsamples_scal++;
            Int16 sval = (Int16) (val -32768) ;

            //sval = remove_baseline_amplify(val); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = round_long_to_short((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = ((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //sval -32000 ... +32000

            //sval = (Int16)HP.ProcessSample((double)sval);
            sval = (Int16)HPM.ProcessSample(sval);
            return sval;
        }

        public Int16 ProcessVasoSampleAutoRange_org(UInt16 val)
        {
            double f;
            cntsamples_scal++;

            Int16 sval = remove_baseline_amplify(val); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = round_long_to_short((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = ((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //sval -32000 ... +32000
                                                                   
            //Übersteuerungserkennung
            if (cnt_pos_overload_time > 0) cnt_pos_overload_time--; //Zählen wie lange in pos Übersteuerung
            if (cnt_neg_overload_time > 0) cnt_neg_overload_time--; //neg Übersteuerung

            if (sval == Int16.MaxValue)
            {
                cnt_pos_overload_time += 2; //Increment wenn in pos Übersteuerung
            }
            else if (sval == Int16.MinValue)
            {
                cnt_neg_overload_time += 2; //Increment wenn in neg Überstuerung
            }

            if (cnt_neg_overload_time + cnt_pos_overload_time > t_max_overload_time_cnts)
            {
                cnt_neg_overload_time = 0;
                cnt_pos_overload_time = 0;
                scalingfactor_asPowerof2 /= 2;
                Adapt_ScalingFactor();

                cntsamples_scal = 0; //"Normale" Anpassung zurücksetzen
            }
            //End Übersteuerungserkennung

            //Verstärkung regelmässig anpassen
            if (sval > amax) amax = sval;
            if (sval < amin) amin = sval;
            if (cntsamples_scal > t_calc_new_scaling_cnts)
            {
                //Set Amplification
                max = Math.Abs(amax);

                if (amin < short.MinValue + 1) amin++; //+ 1 das Min = -32768 und Max = 32767

                if (Math.Abs(amin) > max) max = Math.Abs(amin); 

                if (max >= Int16.MaxValue - 2)
                    scalingfactor_asPowerof2 -= 1;
                else if (max < 2000)
                    scalingfactor_asPowerof2 += 3;
                else if (max < 5000)
                    scalingfactor_asPowerof2 += 2;
                else if (max < 10000)
                    scalingfactor_asPowerof2 += 1;

                Adapt_ScalingFactor();
                cntsamples_scal = 0;
                amax = 0;
                amin = 0;

                UInt16 ui = (UInt16) avg.Average;
            }

            //reduce max and min by 1%
            f = amax;
            f *= 0.99;
            amax = (Int16)f;
            f = amin;
            f *= 0.99;
            amin = (Int16)f;

            //sval = (Int16)HP.ProcessSample((double)sval);
            sval = (Int16) HPM.ProcessSample(sval);
            return sval;
        }

        private Int16 remove_baseline_amplify(UInt16 val)
        {
            avg.Push(val); //unsigned short, Low pass filtering
                                   //float f = val;
                                   //f = (f - ((unsigned int) (*pmovavg_sum >> MWANZMW))) * (float) scalingfactor;
            long l = val;
            //debug weg
            l -= (long)avg.Average;
            //l = l <<  scalingfactor_asPowerof2;

            return round_long_to_short(l);
        }

        private Int16 round_long_to_short(long l)
        {
            if (l > Int16.MaxValue)
                return Int16.MaxValue;
            else if (l < Int16.MinValue)
                return Int16.MinValue;
            return (Int16)l;
        }

        private void Adapt_ScalingFactor()
        {
            if (scalingfactor_asPowerof2 < Min_ScalingFactor_asPowerof2_default)
                scalingfactor_asPowerof2 = Min_ScalingFactor_asPowerof2_default;
            else if (scalingfactor_asPowerof2 > Max_ScalingFactor_asPowerof2_default)
                scalingfactor_asPowerof2 = Max_ScalingFactor_asPowerof2_default;
        }


    }
}
