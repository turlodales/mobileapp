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
        public bool HasProjectIdBackup { get; set; }

        [JsonIgnore]
        public long? ProjectIdBackup { get; set; }

        [JsonIgnore]
        public bool HasTaskIdBackup { get; set; }

        [JsonIgnore]
        public long? TaskIdBackup { get; set; }

        [JsonIgnore]
        public bool HasBillableBackup { get; set; }

        [JsonIgnore]
        public bool BillableBackup { get; set; }

        [JsonIgnore]
        public bool HasStartBackup { get; set; }

        [JsonIgnore]
        public DateTimeOffset StartBackup { get; set; }

        [JsonIgnore]
        public bool HasDurationBackup { get; set; }

        [JsonIgnore]
        public long? DurationBackup { get; set; }

        [JsonIgnore]
        public bool HasDescriptionBackup { get; set; }

        [JsonIgnore]
        public string DescriptionBackup { get; set; }

        [JsonIgnore]
        public bool HasTagIdsBackup { get; set; }

        [JsonIgnore]
        public IList<long> TagIdsBackup { get; } = new List<long>();
    }
}
