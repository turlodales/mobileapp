using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using Toggl.Networking.Serialization;
using Toggl.Networking.Serialization.Converters;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class Preferences : IPreferences
    {
        private const string newSyncClient = "mobile_sync_client";

        [JsonProperty("timeofday_format")]
        [JsonConverter(typeof(TimeFormatConverter))]
        public TimeFormat TimeOfDayFormat { get; set; }

        [JsonConverter(typeof(DateFormatConverter))]
        public DateFormat DateFormat { get; set; }

        [JsonConverter(typeof(StringEnumConverter), true)]
        public DurationFormat DurationFormat { get; set; }

        [JsonProperty("CollapseTimeEntries")]
        public bool CollapseTimeEntries { get; set; }

        [JsonProperty("alpha_features")]
        [JsonConverter(typeof(AlphaFeaturesJsonConverter), newSyncClient, false)]
        [IgnoreWhenPosting]
        public bool UseNewSync { get; set; }
    }
}
