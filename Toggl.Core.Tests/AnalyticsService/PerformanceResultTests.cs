using System;
using Xunit;
using Toggl.Core.Analytics;
using FluentAssertions;

namespace Toggl.Core.Tests
{
    public class PerformanceResultTests
    {
        [Theory, LogIfTooSlow]
        [InlineData(0.01, "< 100 ms")]
        [InlineData(0.05, "< 100 ms")]
        [InlineData(0.10, "< 250 ms")]
        [InlineData(0.20, "< 250 ms")]
        [InlineData(0.25, "< 500 ms")]
        [InlineData(0.40, "< 500 ms")]
        [InlineData(0.50, "< 750 ms")]
        [InlineData(0.60, "< 750 ms")]
        [InlineData(0.75, "< 1 s")]
        [InlineData(1.00, "< 2 s")]
        [InlineData(1.50, "< 2 s")]
        [InlineData(2.00, "< 3 s")]
        [InlineData(2.50, "< 3 s")]
        [InlineData(3.00, "< 5 s")]
        [InlineData(4.00, "< 5 s")]
        [InlineData(5.00, "< 10 s")]
        [InlineData(7.50, "< 10 s")]
        [InlineData(10.00, "< 30 s")]
        [InlineData(20.00, "< 30 s")]
        [InlineData(30.00, "< 60 s")]
        [InlineData(45.00, "< 60 s")]
        [InlineData(60.00, "60+ s")]
        [InlineData(1000.00, "60+ s")]
        public void ReturnsCorrectClassification(double durationInSeconds, string expectedClassification)
        {
            var result = new PerformanceResult(TimeSpan.FromSeconds(durationInSeconds));

            result.ToString().Should().Be(expectedClassification);
        }
    }
}