using Newtonsoft.Json;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Storage;

namespace Toggl.Core.Tests.Mocks
{
    public sealed class MockPreferences : IThreadSafePreferences
    {
        public TimeFormat TimeOfDayFormat { get; set; }

        public DateFormat DateFormat { get; set; }

        public DurationFormat DurationFormat { get; set; }

        public bool CollapseTimeEntries { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public long Id { get; set; }

        public bool UseNewSync { get; set; }

        [JsonIgnore]
        public PropertySyncStatus TimeOfDayFormatSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DateFormatSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DurationFormatSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus CollapseTimeEntriesSyncStatus { get; set; }

        [JsonIgnore]
        public TimeFormat TimeOfDayFormatBackup { get; set; }

        [JsonIgnore]
        public DateFormat DateFormatBackup { get; set; }

        [JsonIgnore]
        public DurationFormat DurationFormatBackup { get; set; }

        [JsonIgnore]
        public bool CollapseTimeEntriesBackup { get; set; }
    }
}
