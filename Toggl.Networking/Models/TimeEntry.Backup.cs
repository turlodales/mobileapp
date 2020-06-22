using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class TimeEntry : ITimeEntry
    {
        [JsonIgnore]
        public PropertySyncStatus ProjectIdSyncStatus { get; set; }

        [JsonIgnore]
        public long? ProjectIdBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus WorkspaceIdSyncStatus { get; set; }

        [JsonIgnore]
        public long? WorkspaceIdBackup { get; set; }

        [JsonIgnore]
        public bool IsDeletedBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus IsDeletedSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus TaskIdSyncStatus { get; set; }

        [JsonIgnore]
        public long? TaskIdBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus BillableSyncStatus { get; set; }

        [JsonIgnore]
        public bool BillableBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus StartSyncStatus { get; set; }

        [JsonIgnore]
        public DateTimeOffset StartBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DurationSyncStatus { get; set; }

        [JsonIgnore]
        public long? DurationBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DescriptionSyncStatus { get; set; }

        [JsonIgnore]
        public string DescriptionBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus TagIdsSyncStatus { get; set; }

        [JsonIgnore]
        public IList<long> TagIdsBackup { get; } = new List<long>();
    }
}
