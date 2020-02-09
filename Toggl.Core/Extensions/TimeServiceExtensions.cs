using System;

namespace Toggl.Core.Extensions
{
    public static class TimeServiceExtensions
    {
        public static DateTimeOffset Now(this ITimeService timeService)
            => timeService.CurrentDateTime;
    }
}
