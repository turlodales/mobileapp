using System;

namespace Toggl.Core.Analytics
{
    internal class PerformanceResult
    {
        private static TimeSpan[] thresholds =
        {
            TimeSpan.FromSeconds(0.10),
            TimeSpan.FromSeconds(0.25),
            TimeSpan.FromSeconds(0.50),
            TimeSpan.FromSeconds(0.75),
            TimeSpan.FromSeconds(1.00),
            TimeSpan.FromSeconds(2.00),
            TimeSpan.FromSeconds(3.00),
            TimeSpan.FromSeconds(5.00),
            TimeSpan.FromSeconds(10.00),
            TimeSpan.FromSeconds(30.00),
            TimeSpan.FromSeconds(60.00),
        };

        private readonly TimeSpan duration;

        public PerformanceResult(TimeSpan duration)
        {
            this.duration = duration;
        }

        public override string ToString()
        {
            foreach (var upperBound in thresholds)
            {
                if (duration < upperBound)
                    return upperBound.TotalSeconds < 1
                        ? $"< {upperBound.TotalMilliseconds} ms"
                        : $"< {upperBound.TotalSeconds} s";
            }

            return $"{thresholds[thresholds.Length - 1].TotalSeconds}+ s";
        }
    }
}