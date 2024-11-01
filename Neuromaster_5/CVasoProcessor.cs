using FilteringLib;
using WindControlLib;


namespace Neuromaster_V5
{
    public class CVasoProcessor
    {
        public const double HP_fg = 10;
        public const long CONF_SAMPLING_FREQ = 500;

        public const long t_calc_new_scaling_ms_default = 5000;     //was 3000
        public const long t_max_overload_time_ms_default = 500;	//Max Zeit in Übersteuerung - dann wird Verstärkungsfaktor halbiert


        private readonly CIntegerMovingAverager avg = new(2048);    //debug Vaso
        //CSignalFilter HP;
        //CDoubleMovingAverager davg = new CDoubleMovingAverager(1024);
        MathNet.Filtering.OnlineFilter? HP;

        private readonly CHPFilter_Micromodeler HPM = new();


        public CVasoProcessor()
        {
            Init_VasoProcessor();
        }

        public void Init_VasoProcessor()
        {
            //HP = new CSignalFilter(enumSignalFilterType.HighPass, HP_fg, SampleRate, 2);
            HP = MathNet.Filtering.OnlineFilter.CreateHighpass(MathNet.Filtering.ImpulseResponse.Finite, CONF_SAMPLING_FREQ, 0.2, 1);

            t_max_overload_time_cnts = (ushort)(t_max_overload_time_ms_default * CONF_SAMPLING_FREQ / 1000);
            t_calc_new_scaling_cnts = (ushort)(t_calc_new_scaling_ms_default * CONF_SAMPLING_FREQ / 1000);
        }

        public double ProcessVasoSample(double value)
        {
            //Filtern
            double[] val = [value];
            if (HP is not null)
            {
                val = HP.ProcessSamples(val);
            }
            return val[0];
        }

        private ushort cntsamples_scal = 0;
        private ushort cnt_neg_overload_time = 0;
        private ushort cnt_pos_overload_time = 0;
        private ushort t_max_overload_time_cnts = 0;
        private ushort t_calc_new_scaling_cnts = 0;
        private byte scalingfactor_asPowerof2 = 64;

        private readonly byte Min_ScalingFactor_asPowerof2_default = 6;
        private readonly byte Max_ScalingFactor_asPowerof2_default = 10;

        private short amax, amin, max = 0;


        public short ProcessVasoSampleAutoRange_red(ushort val)
        {
            cntsamples_scal++;
            short sval = (short)(val - 32768);

            //sval = remove_baseline_amplify(val); //Moving AVG + Lowpass filtering + amplification
            //int sval = round_long_to_short((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
            //int sval = ((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
            //sval -32000 ... +32000

            //sval = (Int16)HP.ProcessSample((double)sval);
            sval = HPM.ProcessSample(sval);
            return sval;
        }

        public short ProcessVasoSampleAutoRange_org(ushort val)
        {
            double f;
            cntsamples_scal++;

            short sval = Remove_baseline_amplify(val); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = round_long_to_short((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //int sval = ((remove_baseline_amplify(val))); //Moving AVG + Lowpass filtering + amplification
                                                       //sval -32000 ... +32000

            //Übersteuerungserkennung
            if (cnt_pos_overload_time > 0) cnt_pos_overload_time--; //Zählen wie lange in pos Übersteuerung
            if (cnt_neg_overload_time > 0) cnt_neg_overload_time--; //neg Übersteuerung

            if (sval == short.MaxValue)
            {
                cnt_pos_overload_time += 2; //Increment wenn in pos Übersteuerung
            }
            else if (sval == short.MinValue)
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

                if (max >= short.MaxValue - 2)
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

                //ushort ui = (ushort) avg.Average;
            }

            //reduce max and min by 1%
            f = amax;
            f *= 0.99;
            amax = (short)f;
            f = amin;
            f *= 0.99;
            amin = (short)f;

            //sval = (Int16)HP.ProcessSample((double)sval);
            sval = HPM.ProcessSample(sval);
            return sval;
        }

        private short Remove_baseline_amplify(ushort val)
        {
            avg.Push(val); //unsigned short, Low pass filtering
                           //float f = val;
                           //f = (f - ((unsigned int) (*pmovavg_sum >> MWANZMW))) * (float) scalingfactor;
            long l = val;
            //debug weg
            l -= avg.Average;
            //l = l <<  scalingfactor_asPowerof2;

            return Round_long_to_short(l);
        }

        private static short Round_long_to_short(long l)
        {
            if (l > short.MaxValue)
                return short.MaxValue;
            else if (l < short.MinValue)
                return short.MinValue;
            return (short)l;
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
