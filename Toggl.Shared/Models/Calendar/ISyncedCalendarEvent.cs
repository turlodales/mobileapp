using System;
namespace Toggl.Shared.Models.Calendar
{
    public interface ISyncedCalendarEvent : IIdentifiable
    {
        string SyncId { get; }
        string ICalId { get; }
        string Title { get; }
        long CalendarId { get; }

        DateTimeOffset StartTime { get; }
        DateTimeOffset EndTime { get; }
        DateTimeOffset Updated { get; }

        string BackgroundColor { get; }
        string ForegroundColor { get; }
    }
}
