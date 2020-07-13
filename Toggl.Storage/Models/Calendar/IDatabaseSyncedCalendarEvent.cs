using System;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Storage.Models.Calendar
{
    public interface IDatabaseSyncedCalendarEvent : ISyncedCalendarEvent, IDatabaseModel
    {
        IDatabaseSyncedCalendar Calendar { get; }
    }
}
