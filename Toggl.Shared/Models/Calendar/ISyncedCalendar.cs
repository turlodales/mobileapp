using System;
namespace Toggl.Shared.Models.Calendar
{
    public interface ISyncedCalendar
    {
        string SyncId { get; }
        string Name { get; }
    }
}
