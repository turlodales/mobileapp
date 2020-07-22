using System.Collections.Generic;
using Newtonsoft.Json;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class ExternalCalendarEventsPage : IExternalCalendarEventsPage
    {
        [JsonConverter(typeof(ConcreteListTypeConverter<ExternalCalendarEvent, IExternalCalendarEvent>))]
        public List<IExternalCalendarEvent> events { get; set; }

        public string nextPageToken { get; set; }
    }
}
