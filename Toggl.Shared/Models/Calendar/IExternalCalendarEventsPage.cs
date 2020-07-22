using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface IExternalCalendarEventsPage
    {
        List<IExternalCalendarEvent> events { get; }
        string nextPageToken { get; }
    }
}
