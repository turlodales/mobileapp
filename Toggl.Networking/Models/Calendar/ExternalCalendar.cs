using Newtonsoft.Json;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class ExternalCalendar : IExternalCalendar
    {
        [JsonProperty("id")]
        public string SyncId { get; set; }

        public string Name { get; set; }

        public ExternalCalendar() { }

        public ExternalCalendar(IExternalCalendar entity)
        {
            SyncId = entity.SyncId;
            Name = entity.Name;
        }
    }
}
