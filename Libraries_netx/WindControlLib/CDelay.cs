using System;
using System.Threading;
#if !NET6_0_OR_GREATER
using System.Windows.Forms;
#endif

namespace WindControlLib
{
    public class CDelay
    {
        static readonly CHighPerformanceDateTime hptimer = new();
        public static void Delay_ms(int delayms)
        {
            DateTime dt = hptimer.Now + new TimeSpan(0, 0, 0, 0, delayms);
            while (hptimer.Now < dt)
            {
                Thread.Sleep(1);
            }
        }
        public static void Delay_ms_DoEvents(int delayms)
        {
            DateTime dt = hptimer.Now + new TimeSpan(0, 0, 0, 0, delayms);
            while (hptimer.Now < dt)
            {
#if !NET6_0_OR_GREATER
                Application.DoEvents();
#endif
            }
        }

    }
}
