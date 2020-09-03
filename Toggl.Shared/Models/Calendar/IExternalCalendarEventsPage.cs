using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface IExternalCalendarEventsPage
    {
        List<IExternalCalendarEvent> Events { get; }
        string NextPageToken { get; }
    }
}
