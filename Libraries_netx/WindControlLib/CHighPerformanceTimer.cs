namespace WindControlLib
{
    using System;
    using System.Diagnostics;

    public class CHighPerformanceDateTime
    {
        private readonly Stopwatch stopwatch;
        private readonly DateTime startDateTime;

        // Constructor
        public CHighPerformanceDateTime()
        {
            stopwatch = Stopwatch.StartNew();
            startDateTime = DateTime.Now;
        }

        // Returns the duration of the timer (in seconds)
        public DateTime Now
        {
            get
            {
                // Calculate elapsed ticks based on the stopwatch's elapsed time in ticks
                long elapsedTicks = stopwatch.ElapsedTicks * 10_000_000 / Stopwatch.Frequency;
                return startDateTime.AddTicks(elapsedTicks);
            }
        }
    }
}
