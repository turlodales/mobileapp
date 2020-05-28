using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Toggl.Networking.Serialization;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class TimeEntry : ITimeEntry
    {
        [JsonIgnore]
        public bool ContainsBackup { get; set; }

        [JsonIgnore]
        public long? ProjectIdBackup { get; set; }

        [JsonIgnore]
        public long? TaskIdBackup { get; set; }

        [JsonIgnore]
        public bool BillableBackup { get; set; }

        [JsonIgnore]
        public DateTimeOffset StartBackup { get; set; }

        [JsonIgnore]
        public long? DurationBackup { get; set; }

        [JsonIgnore]
        public string DescriptionBackup { get; set; }

        [JsonIgnore]
        public IList<long> TagIdsBackup { get; } = new List<long>();
    }
}
