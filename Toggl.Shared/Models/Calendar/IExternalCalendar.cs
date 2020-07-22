using System;
namespace Toggl.Shared.Models.Calendar
{
    public interface IExternalCalendar
    {
        string SyncId { get; }
        string Name { get; }
    }
}
