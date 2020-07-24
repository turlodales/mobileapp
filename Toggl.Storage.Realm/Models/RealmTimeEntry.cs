using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using static Toggl.Storage.Realm.Sync.ThreeWayMerge;
using static Toggl.Shared.PropertySyncStatus;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTimeEntry
        : RealmObject, IDatabaseTimeEntry, IUpdatable, ISyncable<ITimeEntry>
    {
        public bool Billable { get; set; }

        public DateTimeOffset Start { get; set; }

        public long? Duration { get; set; }

        public string Description { get; set; }

        public IList<RealmTag> RealmTags { get; }

        public IEnumerable<long> TagIds => RealmTags?.Select(tag => tag.Id).ToList();

        public IEnumerable<IDatabaseTag> Tags => RealmTags;

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public RealmProject RealmProject { get; set; }

        public long? ProjectId => RealmProject?.Id;

        public IDatabaseProject Project => RealmProject;

        public RealmTask RealmTask { get; set; }

        public long? TaskId => RealmTask?.Id;

        public IDatabaseTask Task => RealmTask;

        public RealmUser RealmUser { get; set; }

        public long UserId => RealmUser?.Id ?? 0;

        public IDatabaseUser User => RealmUser;

        public bool IsInaccessible => Workspace.IsInaccessible;

        public void SaveSyncResult(ITimeEntry timeEntry, Realms.Realm realm)
        {
            var wasDirty = SyncStatus == SyncStatus.SyncNeeded;

            // Simplest part - these properties should always overwrite the local state
            Id = timeEntry.Id;
            At = timeEntry.At;
            ServerDeletedAt = timeEntry.ServerDeletedAt;
            RealmUser = realm.GetById<RealmUser>(timeEntry.UserId);

            // Simple types - three way merge
            (IsDeletedSyncStatus, IsDeleted) = Resolve(IsDeletedSyncStatus, IsDeleted, IsDeletedBackup, timeEntry.ServerDeletedAt.HasValue);
            (DescriptionSyncStatus, Description) = Resolve(DescriptionSyncStatus, Description, DescriptionBackup, timeEntry.Description);
            (BillableSyncStatus, Billable) = Resolve(BillableSyncStatus, Billable, BillableBackup, timeEntry.Billable);
            (StartSyncStatus, Start) = Resolve(StartSyncStatus, Start, StartBackup, timeEntry.Start);
            (DurationSyncStatus, Duration) = Resolve(DurationSyncStatus, Duration, DurationBackup, timeEntry.Duration);

            // Relationships - three way merge
            // Workspaces
            var (workspaceStatus, workspaceId) = Resolve(WorkspaceIdSyncStatus, WorkspaceId, WorkspaceIdBackup ?? WorkspaceId, timeEntry.WorkspaceId);
            WorkspaceIdSyncStatus = workspaceStatus;
            RealmWorkspace = realm.GetById<RealmWorkspace>(workspaceId);

            // Project
            var (projectStatus, projectId) = Resolve(ProjectIdSyncStatus, ProjectId, ProjectIdBackup, timeEntry.ProjectId);
            ProjectIdSyncStatus = projectStatus;
            RealmProject = projectId.HasValue ? realm.GetById<RealmProject>(projectId.Value) : null;

            // Task
            var (taskStatus, taskId) = Resolve(TaskIdSyncStatus, TaskId, TaskIdBackup, timeEntry.TaskId);
            TaskIdSyncStatus = taskStatus;
            RealmTask = taskId.HasValue ? realm.GetById<RealmTask>(taskId.Value) : null;

            // Tags
            var (tagsStatus, tagIds) = Resolve(TagIdsSyncStatus, TagIds?.ToArray(), TagIdsBackup?.ToArray(), timeEntry.TagIds?.ToArray());
            TagIdsSyncStatus = tagsStatus;
            RealmTags.Clear();
            tagIds.Select(tagId => realm.GetById<RealmTag>(tagId)).AddTo(RealmTags);

            LastSyncErrorMessage = null;

            SyncStatus = wasDirty && hasAtLeastOneDirtyProperty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        public void PrepareForSyncing()
        {
            SyncStatus = SyncStatus.Syncing;
            changePropertiesSyncStatus(from: SyncNeeded, to: Syncing);
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
            changePropertiesSyncStatus(from: Syncing, to: SyncNeeded);
        }

        public void UpdateSucceeded()
        {
            if (SyncStatus != SyncStatus.SyncNeeded)
                SyncStatus = SyncStatus.InSync;

            changePropertiesSyncStatus(from: Syncing, to: InSync);
        }

        private void changePropertiesSyncStatus(PropertySyncStatus from, PropertySyncStatus to)
        {
            if (IsDeletedSyncStatus == from)
                IsDeletedSyncStatus = to;

            if (WorkspaceIdSyncStatus == from)
                WorkspaceIdSyncStatus = to;

            if (ProjectIdSyncStatus == from)
                ProjectIdSyncStatus = to;

            if (TaskIdSyncStatus == from)
                TaskIdSyncStatus = to;

            if (BillableSyncStatus == from)
                BillableSyncStatus = to;

            if (StartSyncStatus == from)
                StartSyncStatus = to;

            if (DurationSyncStatus == from)
                DurationSyncStatus = to;

            if (DescriptionSyncStatus == from)
                DescriptionSyncStatus = to;

            if (TagIdsSyncStatus == from)
                TagIdsSyncStatus = to;
        }

        private bool hasAtLeastOneDirtyProperty
            => IsDeletedSyncStatus == SyncNeeded
                || WorkspaceIdSyncStatus == SyncNeeded
                || ProjectIdSyncStatus == SyncNeeded
                || TaskIdSyncStatus == SyncNeeded
                || BillableSyncStatus == SyncNeeded
                || StartSyncStatus == SyncNeeded
                || DurationSyncStatus == SyncNeeded
                || DescriptionSyncStatus == SyncNeeded
                || TagIdsSyncStatus == SyncNeeded;
    }
}
