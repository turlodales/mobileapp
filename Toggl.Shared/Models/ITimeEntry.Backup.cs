using System;
using System.Collections.Generic;

namespace Toggl.Shared.Models
{
    public partial interface ITimeEntry
    {
        bool IsDeletedBackup { get; set; }
        PropertySyncStatus IsDeletedSyncStatus { get; set; }

        long? WorkspaceIdBackup { get; set; }
        PropertySyncStatus WorkspaceIdSyncStatus { get; set; }

        long? ProjectIdBackup { get; set; }
        PropertySyncStatus ProjectIdSyncStatus { get; set; }

        long? TaskIdBackup { get; set; }
        PropertySyncStatus TaskIdSyncStatus { get; set; }

        bool BillableBackup { get; set; }
        PropertySyncStatus BillableSyncStatus { get; set; }

        DateTimeOffset StartBackup { get; set; }
        PropertySyncStatus StartSyncStatus { get; set; }

        long? DurationBackup { get; set; }
        PropertySyncStatus DurationSyncStatus { get; set; }

        string DescriptionBackup { get; set; }
        PropertySyncStatus DescriptionSyncStatus { get; set; }

        IList<long> TagIdsBackup { get; }
        PropertySyncStatus TagIdsSyncStatus { get; set; }
    }
}
