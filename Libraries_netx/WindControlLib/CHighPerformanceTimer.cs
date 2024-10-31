using System;
using System.Runtime.InteropServices;

namespace WindControlLib
{
    public class CHighPerformanceDateTime
    {
        //QueryPerformanceCounter, returns the current processor's amount of ticks.
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        //This function returns the performance frequency: the number of performance counter values per second. 
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private readonly long startTime;
        private readonly long freq;
        private readonly DateTime _dt;

        // Constructor
        public CHighPerformanceDateTime()
        {
            startTime = 0;
            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Exception();
            }
            _dt = DateTime.Now;
            QueryPerformanceCounter(out startTime);
        }

        // Returns the duration of the timer (in seconds)
        public DateTime Now
        {
            get
            {
                QueryPerformanceCounter(out long ticksNow);
                long ticks = (long)((ticksNow - startTime) / (double)freq * 10000000);
                return _dt.AddTicks(ticks); ;
            }
        }
    }
}
