using System;
using Newtonsoft.Json;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class ExternalCalendarEvent : IExternalCalendarEvent
    {
        [JsonProperty("id")]
        public string SyncId { get; set; }

        [JsonProperty("ical_uid")]
        public string ICalId { get; set; }

        public string Title { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public DateTimeOffset Updated { get; set; }

        public string BackgroundColor { get; set; }

        public string ForegroundColor { get; set; }

        public ExternalCalendarEvent() { }

        public ExternalCalendarEvent(IExternalCalendarEvent entity)
        {
            SyncId = entity.SyncId;
            ICalId = entity.ICalId;
            Title = entity.Title;
            StartTime = entity.StartTime;
            EndTime = entity.EndTime;
            Updated = entity.Updated;
            BackgroundColor = entity.BackgroundColor;
            ForegroundColor = entity.ForegroundColor;
        }
    }
}
