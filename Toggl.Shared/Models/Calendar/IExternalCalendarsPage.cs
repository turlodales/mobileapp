using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface IExternalCalendarsPage
    {
        List<IExternalCalendar> calendars { get; }
        string nextPageToken { get; }
    }
}
