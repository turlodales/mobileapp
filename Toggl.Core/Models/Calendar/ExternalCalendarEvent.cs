using System;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.Models.Calendar
{
    public sealed class ExternalCalendarEvent : IThreadSafeExternalCalendarEvent
    {
        public long Id { get; }

        public string SyncId { get; }

        public string ICalId { get; }

        public string Title { get; }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset EndTime { get; }

        public DateTimeOffset Updated { get; }

        public string BackgroundColor { get; }

        public string ForegroundColor { get; }

        public long CalendarId { get; }

        public IThreadSafeExternalCalendar Calendar { get; }

        IDatabaseExternalCalendar IDatabaseExternalCalendarEvent.Calendar => Calendar;

        public ExternalCalendarEvent(
            long id,
            string syncId,
            string iCalId,
            string title,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            DateTimeOffset updated,
            string backgroundColor,
            string foregroundColor,
            long calendarId,
            IThreadSafeExternalCalendar calendar)
        {
            Id = id;
            SyncId = syncId;
            ICalId = iCalId;
            Title = title;
            StartTime = startTime;
            EndTime = endTime;
            Updated = Updated;
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
            CalendarId = calendarId;
            Calendar = calendar;
        }

        public ExternalCalendarEvent(IDatabaseExternalCalendarEvent entity)
            : this(entity.Id,
                  entity.SyncId,
                  entity.ICalId,
                  entity.Title,
                  entity.StartTime,
                  entity.EndTime,
                  entity.Updated,
                  entity.BackgroundColor,
                  entity.ForegroundColor,
                  entity.CalendarId,
                  ExternalCalendar.From(entity.Calendar))
        {
        }

        public static ExternalCalendarEvent From(IDatabaseExternalCalendarEvent entity)
            => new ExternalCalendarEvent(entity);
    }
}
