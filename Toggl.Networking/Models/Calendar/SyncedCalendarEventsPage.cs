using System.Collections.Generic;
using Newtonsoft.Json;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class SyncedCalendarEventsPage : ISyncedCalendarEventsPage
    {
        [JsonConverter(typeof(ConcreteListTypeConverter<SyncedCalendarEvent, ISyncedCalendarEvent>))]
        public List<ISyncedCalendarEvent> events { get; set; }

        public string nextPageToken { get; set; }
    }
}
