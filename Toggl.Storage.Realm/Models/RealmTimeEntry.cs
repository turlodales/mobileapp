using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using static Toggl.Storage.Realm.Sync.BackupHelper;
using static Toggl.Storage.Realm.Sync.ThreeWayMerge;

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
            var shouldStayDirty = false;

            Id = timeEntry.Id;
            At = timeEntry.At;
            ServerDeletedAt = timeEntry.ServerDeletedAt;
            IsDeleted = timeEntry.ServerDeletedAt.HasValue;
            RealmUser = realm.GetById<RealmUser>(timeEntry.UserId);
            RealmWorkspace = realm.GetById<RealmWorkspace>(timeEntry.WorkspaceId);

            // Description
            var commonDescription = HasDescriptionBackup
                ? DescriptionBackup
                : Description;

            Description = Merge(commonDescription, Description, timeEntry.Description);

            shouldStayDirty |= ClearBackupIf(
                timeEntry.Description != Description,
                () => HasDescriptionBackup = false);

            // ProjectId
            var commonProjectId = HasProjectIdBackup 
                ? ProjectIdBackup
                : ProjectId;

            var projectId = Merge(commonProjectId, ProjectId, timeEntry.ProjectId);

            RealmProject = projectId.HasValue
                ? realm.GetById<RealmProject>(projectId.Value)
                : null;

            shouldStayDirty |= !ClearBackupIf(
                timeEntry.ProjectId == projectId,
                () => HasProjectIdBackup = false);

            // Billable
            var commonBillable = HasBillableBackup
                ? BillableBackup
                : Billable;

            Billable = Merge(commonBillable, Billable, timeEntry.Billable);

            shouldStayDirty |= !ClearBackupIf(
                timeEntry.Billable == Billable,
                () => HasBillableBackup = false);

            // Start
            var commonStart = HasStartBackup
                ? StartBackup
                : Start;

            Start = Merge(commonStart, Start, timeEntry.Start);

            shouldStayDirty |= !ClearBackupIf(
                timeEntry.Start == Start,
                () => HasStartBackup = false);

            // Duration
            var commonDuration = HasDurationBackup
                ? DurationBackup
                : Duration;

            Duration = Merge(commonDuration, Duration, timeEntry.Duration);

            shouldStayDirty |= !ClearBackupIf(
                timeEntry.Duration == Duration,
                () => HasDurationBackup = false);

            // Task
            var commonTaskId = HasTaskIdBackup
                ? TaskIdBackup
                : TaskId;

            var taskId = Merge(commonTaskId, TaskId, timeEntry.TaskId);

            RealmTask = taskId.HasValue
                ? realm.GetById<RealmTask>(taskId.Value)
                : null;

            shouldStayDirty |= !ClearBackupIf(
                timeEntry.TaskId == taskId,
                () => HasTaskIdBackup = false);

            // Tag Ids
            var commonTagIds = HasTagIdsBackup
                ? Arrays.NotNullOrEmpty(TagIdsBackup)
                : Arrays.NotNullOrEmpty(TagIds);

            var localTagIds = Arrays.NotNullOrEmpty(TagIds);
            var serverTagIds = Arrays.NotNullOrEmpty(timeEntry.TagIds);

            var tagsIds = Merge(commonTagIds, localTagIds, serverTagIds);
            shouldStayDirty |= !ClearBackupIf(
                tagsIds.SetEquals(localTagIds),
                () => HasTagIdsBackup = false);

            RealmTags.Clear();
            tagsIds
                .Select(tagId => realm.GetById<RealmTag>(tagId))
                .AddTo(RealmTags);

            LastSyncErrorMessage = null;

            // Update sync status depending on the way the time entry has changed during the 3-way merge
            SyncStatus = wasDirty && shouldStayDirty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        public void PrepareForSyncing()
        {
            SyncStatus = SyncStatus.Syncing;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }

        public void UpdateSucceeded()
        {
            if (SyncStatus != SyncStatus.SyncNeeded)
                SyncStatus = SyncStatus.InSync;
        }
    }
}
