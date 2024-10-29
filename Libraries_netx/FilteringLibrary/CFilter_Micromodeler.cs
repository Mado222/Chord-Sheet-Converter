using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilteringLibrary
{
    public class CHPFilter_Micromodeler
    {

        public short ProcessSample(short sval)
        {
            return round_long_to_short(HP_Butter_2nd_int_red(sval));
        }

        private static short round_long_to_short(long l)
        {
            if (l > short.MaxValue)
                return short.MaxValue;
            else if (l < short.MinValue)
                return short.MinValue;
            return (short)l;
        }


        /***********************************************************/
        /************* fg= 1 Hz, Sampling 500Hz ******************/
        /***********************************************************/
        // Scaled for a 16x16:32 Direct form 1 IIR filter with: 
        // Feedback shift = 14
        // Output shift = 14
        // Input has a maximum value of 1, scaled by 2^15
        // Output has a maximum value of 1.4646677313796677, scaled by 2^14

        //8120, -16239, 8120, 32477, -16095// b0 Q13(0.991), b1 Q13(-1.98), b2 Q13(0.991), a1 Q14(1.98), a2 Q14(-0.982)

        /*
                private const Int16 HPi_b0 =8120;
                private const Int16 HPi_b1= - 16239;
                private const Int16 HPi_b2= 8120;
                private const Int16 HPi_a1= 32477;
                private const Int16 HPi_a2= - 16095;

                private const Int16 HPi_Feedback_shift=  14;
                private const Int16 HPi_Output_shift=  14;

                private const Int16 HPi_InputScale_shift=  15;
                private const Int16 HPi_OutputScale_shift=  14;
                */

        /***********************************************************/
        /************* fg= 0.5 Hz, Sampling 500Hz ******************/
        /***********************************************************/
        //8156, -16311, 8156, 32622, -16239

        private const double HPi_b0 = 8156;
        private const double HPi_b1 = -16311;
        private const double HPi_b2 = 8156;
        private const double HPi_a1 = 32622;
        private const double HPi_a2 = -16239;

        private const short HPi_Feedback_shift = 14;
        private const short HPi_Output_shift = 14;

        private const short HPi_InputScale_shift = 15;
        private const short HPi_OutputScale_shift = 14;



        double HPi_x0, HPi_x1, HPi_x2;
        double HPi_y1 = 0;
        double HPi_y2 = 0;

        private long HP_Butter_2nd_int_red(short input)
        {
            //long accumulator;
            double accumulator;
            //int count = 1;

            // Loop for all samples in the input buffer
            //while (count--)
            {
                HPi_x0 = input;
                accumulator = (HPi_x2 * HPi_b2);
                accumulator += (HPi_x1 * HPi_b1);
                accumulator += (HPi_x0 * HPi_b0);
                HPi_x2 = HPi_x1; // Shuffle left history buffer
                HPi_x1 = HPi_x0;
                accumulator += (HPi_y2 * HPi_a2);
                accumulator += (HPi_y1 * HPi_a1);
                HPi_y2 = HPi_y1; // Shuffle right history buffer
                HPi_y1 = (short)((int)accumulator >> HPi_Feedback_shift);
            }
            // Write output
            return HP_ScaleOutput((int)accumulator >> HPi_Output_shift);
        }

        private const long HPi_output_calibration_factor = ((long)1 << HPi_InputScale_shift) / ((long)1 << HPi_OutputScale_shift);

        static long HP_ScaleOutput(long val)
        {
            val *= HPi_output_calibration_factor;
            //val *= 1024;
            //val = val>>HPi_skal;
            return val;
        }
    }
}
