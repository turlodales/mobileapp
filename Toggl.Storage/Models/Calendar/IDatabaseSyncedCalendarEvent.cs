using System;
using Toggl.Shared.Models;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Storage.Models.Calendar
{
    public interface IDatabaseSyncedCalendarEvent : ISyncedCalendarEvent, IIdentifiable, IDatabaseModel
    {
        long CalendarId { get; }
        IDatabaseSyncedCalendar Calendar { get; }
    }
}
