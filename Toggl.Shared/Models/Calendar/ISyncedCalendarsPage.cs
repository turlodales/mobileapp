using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface ISyncedCalendarsPage
    {
        List<ISyncedCalendar> calendars { get; }
        string nextPageToken { get; }
    }
}
