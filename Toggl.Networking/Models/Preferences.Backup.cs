using Newtonsoft.Json;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class Preferences : IPreferences
    {
        [JsonIgnore]
        public bool HasTimeOfDayFormatBackup { get; set; }

        [JsonIgnore]
        public TimeFormat TimeOfDayFormatBackup { get; set; }

        [JsonIgnore]
        public bool HasDateFormatBackup { get; set; }

        [JsonIgnore]
        public DateFormat DateFormatBackup { get; set; }

        [JsonIgnore]
        public bool HasDurationFormatBackup { get; set; }

        [JsonIgnore]
        public DurationFormat DurationFormatBackup { get; set; }

        [JsonIgnore]
        public bool HasCollapseTimeEntriesBackup { get; set; }

        [JsonIgnore]
        public bool CollapseTimeEntriesBackup { get; set; }
    }
}
