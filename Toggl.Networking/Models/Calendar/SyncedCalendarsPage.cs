using System.Collections.Generic;
using Newtonsoft.Json;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class SyncedCalendarsPage : ISyncedCalendarsPage
    {
        [JsonConverter(typeof(ConcreteListTypeConverter<SyncedCalendar, ISyncedCalendar>))]
        public List<ISyncedCalendar> calendars { get; set; }

        public string nextPageToken { get; set; }
    }
}
