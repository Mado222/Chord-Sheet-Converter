namespace FilteringLib
{
    public class CHP_2nd_Butterworth_float
    {
        //https://www.micromodeler.com/dsp/

        public CHP_2nd_Butterworth_float(EnFilterParams filterParams, EnFilterForm filterForm)
        {
            this.FilterParams = filterParams;
            this.FilterForm = filterForm;
            InitFilter();
        }

        public enum EnFilterParams
        {
            HP_fg_0o5_100Hz
        }

        public enum EnFilterForm
        {
            Form1,
            Form2
        }


        public EnFilterParams FilterParams { get; private set; }
        public EnFilterForm FilterForm { get; private set; }

        private double HPi_x0, HPi_x1, HPi_x2;  //Form1
        private double HPi_w0, HPi_w1, HPi_w2;  //Form2
        private double HPi_y1 = 0;
        private double HPi_y2 = 0;
        private double[] HP_coefficients = [];

        double HPi_b0;
        double HPi_b1;
        double HPi_b2;
        double HPi_a1;
        double HPi_a2;



        /***********************************************************/
        /************* fg= 0,5 Hz, Sampling 100Hz ******************/
        /***********************************************************/
        /*
Generated code is based on the following filter design:
<micro.DSP.FilterDocument sampleFrequency="#100" arithmetic="float" biquads="Direct1" classname="HP" inputMax="#1" inputShift="#15" >
  <micro.DSP.IirButterworthFilter N="#2" bandType="h" w1="#0.005" w2="#0.4999" stopbandRipple="#undefined" passbandRipple="#undefined" transitionRatio="#undefined" >
    <micro.DSP.FilterStructure coefficientBits="#0" variableBits="#0" accumulatorBits="#0" biquads="Direct1" >
      <micro.DSP.FilterSection form="Direct1" historyType="WriteBack" accumulatorBits="#0" variableBits="#0" coefficientBits="#0" />
    </micro.DSP.FilterStructure>
    <micro.DSP.PoleOrZeroContainer >
      <micro.DSP.PoleOrZero i="#0.021728161744398348" r="#0.9777891201575177" isPoint="#true" isPole="#true" isZero="#false" symmetry="c" N="#1" cascade="#0" />
      <micro.DSP.PoleOrZero i="#0" r="#1" isPoint="#true" isPole="#false" isZero="#true" symmetry="r" N="#1" cascade="#0" />
      <micro.DSP.PoleOrZero i="#0" r="#1" isPoint="#true" isPole="#false" isZero="#true" symmetry="r" N="#1" cascade="#0" />
    </micro.DSP.PoleOrZeroContainer>
    <micro.DSP.GenericC.CodeGenerator generateTestCases="#false" />
    <micro.DSP.GainControl magnitude="#1" frequency="#0.498046875" peak="#true" />
  </micro.DSP.IirButterworthFilter>
</micro.DSP.FilterDocument>*/

        //Gilt für Form1 und 2
        private static readonly double[] HP_coefficients_HP_fg_0o5_100Hz = [
            0.9780304792065595, -1.956060958413119, 0.9780304792065595, 1.9555782403150355, -0.9565436765112033 // b0, b1, b2, a1, a2
        ];

        private void InitFilter()
        {
            // Scaled for floating point
            switch (FilterParams)
            {
                case EnFilterParams.HP_fg_0o5_100Hz:
                    HP_coefficients = HP_coefficients_HP_fg_0o5_100Hz;
                    break;
            }

            HPi_b0 = HP_coefficients[0];
            HPi_b1 = HP_coefficients[1];
            HPi_b2 = HP_coefficients[2];
            HPi_a1 = HP_coefficients[3];
            HPi_a2 = HP_coefficients[4];
        }

        public double ProcessSample(double sval)
        {
            switch (FilterForm)
            {
                case EnFilterForm.Form1:
                    return HP_Butter_2nd_Form1(sval);
                case EnFilterForm.Form2:
                    return HP_Butter_2nd_Form2(sval);
            }
            return HP_Butter_2nd_Form1(sval);
        }

        private double HP_Butter_2nd_Form1(double input)
        {
            double accumulator;

            HPi_x0 = input;
            accumulator = HPi_x2 * HPi_b2;
            accumulator += HPi_x1 * HPi_b1;
            accumulator += HPi_x0 * HPi_b0;
            HPi_x2 = HPi_x1; // Shuffle left history buffer
            HPi_x1 = HPi_x0;
            accumulator += HPi_y2 * HPi_a2;
            accumulator += HPi_y1 * HPi_a1;
            HPi_y2 = HPi_y1; // Shuffle right history buffer
            HPi_y1 = accumulator;

            // Write output
            return accumulator;
        }

        private double HP_Butter_2nd_Form2(double input)
        {
            double accumulator;

            HPi_x0 = input;

            // Run feedback part of filter
            accumulator = HPi_w2 * HPi_a2;
            accumulator += HPi_w1 * HPi_a1;
            accumulator += HPi_x0;

            HPi_w0 = accumulator;

            // Run feedforward part of filter
            accumulator = HPi_w0 * HPi_b0;
            accumulator += HPi_w1 * HPi_b1;
            accumulator += HPi_w2 * HPi_b2;

            HPi_w2 = HPi_w1;        // Shuffle history buffer
            HPi_w1 = HPi_w0;

            // Write output
            return accumulator;
        }

    }
}
