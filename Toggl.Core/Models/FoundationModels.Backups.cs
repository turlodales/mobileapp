using System;
using System.Collections.Generic;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;

namespace Toggl.Core.Models
{
    internal partial class TimeEntry : IThreadSafeTimeEntry
    {
        public bool IsDeletedBackup { get; set; }
        public PropertySyncStatus IsDeletedSyncStatus { get; set; }

        public long? TaskIdBackup { get; set; }
        public PropertySyncStatus TaskIdSyncStatus { get; set; }

        public bool BillableBackup { get; set; }
        public PropertySyncStatus BillableSyncStatus { get; set; }

        public DateTimeOffset StartBackup { get; set; }
        public PropertySyncStatus StartSyncStatus { get; set; }
        
        public long? DurationBackup { get; set; }
        public PropertySyncStatus DurationSyncStatus { get; set; }
         
        public string DescriptionBackup { get; set; }
        public PropertySyncStatus DescriptionSyncStatus { get; set; }
        
        public IList<long> TagIdsBackup { get; } = new List<long>();
        public PropertySyncStatus TagIdsSyncStatus { get; set; }

        public long? ProjectIdBackup { get; set; }
        public PropertySyncStatus ProjectIdSyncStatus { get; set; }

        public long? WorkspaceIdBackup { get; set; }
        public PropertySyncStatus WorkspaceIdSyncStatus { get; set; }
    }

    internal partial class User : IThreadSafeUser
    {
        public PropertySyncStatus DefaultWorkspaceIdSyncStatus { get; set; }

        public long? DefaultWorkspaceIdBackup { get; set; }

        public PropertySyncStatus BeginningOfWeekSyncStatus { get; set; }
        public BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }

    internal partial class Preferences : IThreadSafePreferences
    {
        public PropertySyncStatus TimeOfDayFormatSyncStatus { get; set; }
        public PropertySyncStatus DateFormatSyncStatus { get; set; }
        public PropertySyncStatus DurationFormatSyncStatus { get; set; }
        public PropertySyncStatus CollapseTimeEntriesSyncStatus { get; set; }

        public TimeFormat TimeOfDayFormatBackup { get; set; }

        public DateFormat DateFormatBackup { get; set; }

        public DurationFormat DurationFormatBackup { get; set; }

        public bool CollapseTimeEntriesBackup { get; set; }
    }
}
