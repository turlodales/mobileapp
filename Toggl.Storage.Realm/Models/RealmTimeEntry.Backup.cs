using Realms;
using System;
using System.Collections.Generic;
using Toggl.Shared;
using Toggl.Storage.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTimeEntry : RealmObject, IDatabaseTimeEntry
    {
        public bool IsDeletedBackup { get; set; }
        public bool BillableBackup { get; set; }
        public DateTimeOffset StartBackup { get; set; }
        public long? DurationBackup { get; set; }
        public string DescriptionBackup { get; set; }

        public long? WorkspaceIdBackup { get; set; }
        public long? ProjectIdBackup { get; set; }
        public long? TaskIdBackup { get; set; }
        public IList<long> TagIdsBackup { get; }

        [Ignored]
        public PropertySyncStatus IsDeletedSyncStatus
        {
            get => (PropertySyncStatus)IsDeletedSyncStatusInt;
            set => IsDeletedSyncStatusInt = (int)value;
        }

        public int IsDeletedSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus BillableSyncStatus
        {
            get => (PropertySyncStatus)BillableSyncStatusInt;
            set => BillableSyncStatusInt = (int)value;
        }

        public int BillableSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus StartSyncStatus
        {
            get => (PropertySyncStatus)StartSyncStatusInt;
            set => StartSyncStatusInt = (int)value;
        }

        public int StartSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus DurationSyncStatus
        {
            get => (PropertySyncStatus)DurationSyncStatusInt;
            set => DurationSyncStatusInt = (int)value;
        }

        public int DurationSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus DescriptionSyncStatus
        {
            get => (PropertySyncStatus)DescriptionSyncStatusInt;
            set => DescriptionSyncStatusInt = (int)value;
        }

        public int DescriptionSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus WorkspaceIdSyncStatus
        {
            get => (PropertySyncStatus)WorkspaceIdSyncStatusInt;
            set => WorkspaceIdSyncStatusInt = (int)value;
        }

        public int WorkspaceIdSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus ProjectIdSyncStatus
        {
            get => (PropertySyncStatus)ProjectIdSyncStatusInt;
            set => ProjectIdSyncStatusInt = (int)value;
        }

        public int ProjectIdSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus TaskIdSyncStatus
        {
            get => (PropertySyncStatus)TaskIdSyncStatusInt;
            set => TaskIdSyncStatusInt = (int)value;
        }

        public int TaskIdSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus TagIdsSyncStatus
        {
            get => (PropertySyncStatus)TagIdsSyncStatusInt;
            set => TagIdsSyncStatusInt = (int)value;
        }

        public int TagIdsSyncStatusInt { get; set; }
    }
}
