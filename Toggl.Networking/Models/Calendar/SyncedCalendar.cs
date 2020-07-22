using Newtonsoft.Json;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.Models.Calendar
{
    [Preserve(AllMembers = true)]
    internal sealed class SyncedCalendar : ISyncedCalendar
    {
        [JsonProperty("id")]
        public string SyncId { get; set; }

        public string Name { get; set; }

        public SyncedCalendar() { }

        public SyncedCalendar(ISyncedCalendar entity)
        {
            SyncId = entity.SyncId;
            Name = entity.Name;
        }
    }
}
