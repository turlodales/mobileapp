using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface ISyncedCalendarEventsPage
    {
        List<ISyncedCalendarEvent> events { get; }
        string nextPageToken { get; }
    }
}
