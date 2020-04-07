using Toggl.Core.Models;
using static Toggl.Core.Analytics.DateRangeSelectionSource;
using static Toggl.Core.Models.DateRangePeriod;

namespace Toggl.Core.Analytics
{
    public enum DateRangeSelectionSource
    {
        Initial,
        ShortcutToday,
        ShortcutYesterday,
        ShortcutThisWeek,
        ShortcutLastWeek,
        ShortcutThisMonth,
        ShortcutLastMonth,
        ShortcutThisYear,
        ShortcutLastYear,
        Calendar,
        Other,
        Siri,
    }

    public static class DateRangeSelectionSourceExtensions
    {
        public static DateRangeSelectionSource ToDateRangeSelectionSource(this DateRangePeriod period)
        {
            return period switch
            {
                Today => ShortcutToday,
                Yesterday => ShortcutYesterday,
                ThisWeek => ShortcutThisWeek,
                LastWeek => ShortcutLastWeek,
                ThisMonth => ShortcutThisMonth,
                LastMonth => ShortcutLastMonth,
                ThisYear => ShortcutThisYear,
                LastYear => ShortcutLastWeek,
                _ => Other
            };
        }
    }
}
