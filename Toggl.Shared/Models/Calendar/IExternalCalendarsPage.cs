using System.Collections.Generic;

namespace Toggl.Shared.Models.Calendar
{
    public interface IExternalCalendarsPage
    {
        List<IExternalCalendar> Calendars { get; }
        string NextPageToken { get; }
    }
}
