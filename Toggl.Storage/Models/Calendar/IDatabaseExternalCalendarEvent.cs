using System;
using Toggl.Shared.Models;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Storage.Models.Calendar
{
    public interface IDatabaseExternalCalendarEvent : IExternalCalendarEvent, IIdentifiable, IDatabaseModel
    {
        long CalendarId { get; }
        IDatabaseExternalCalendar Calendar { get; }
    }
}
