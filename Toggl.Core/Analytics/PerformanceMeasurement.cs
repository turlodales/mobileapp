using System;

namespace Toggl.Core.Analytics
{
    public class PerformanceMeasurement
    {
        private readonly DateTimeOffset start;

        public string Name { get; }

        internal PerformanceMeasurement(string name, DateTimeOffset start)
        {
            this.start = start;

            Name = name;
        }

        internal PerformanceResult DurationUntil(DateTimeOffset end)
        {
            return new PerformanceResult(end - start);
        }
    }
}