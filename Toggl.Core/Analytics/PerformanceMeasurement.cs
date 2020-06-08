using System;
using System.Diagnostics;

namespace Toggl.Core.Analytics
{
    public class PerformanceMeasurement
    {
        private readonly Stopwatch stopwatch;

        public string Name { get; }

        internal PerformanceMeasurement(string name)
        {
            Name = name;
            stopwatch = Stopwatch.StartNew();
        }

        internal PerformanceResult Complete()
        {
            stopwatch.Stop();
            return new PerformanceResult(stopwatch.Elapsed);
        }
    }
}
