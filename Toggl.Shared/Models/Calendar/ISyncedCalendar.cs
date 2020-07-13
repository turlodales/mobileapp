using System;
namespace Toggl.Shared.Models.Calendar
{
    public interface ISyncedCalendar : IIdentifiable
    {
        string SyncId { get; }
        string Name { get; }
    }
}
