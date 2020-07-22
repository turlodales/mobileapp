using System;
using Toggl.Shared.Models;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Storage.Models.Calendar
{
    public interface IDatabaseSyncedCalendar : ISyncedCalendar, IIdentifiable, IDatabaseModel
    {
    }
}
