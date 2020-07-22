using System.Collections.Generic;
using Newtonsoft.Json;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class ExternalCalendarsPage : IExternalCalendarsPage
    {
        [JsonConverter(typeof(ConcreteListTypeConverter<ExternalCalendar, IExternalCalendar>))]
        public List<IExternalCalendar> calendars { get; set; }

        public string nextPageToken { get; set; }
    }
}
