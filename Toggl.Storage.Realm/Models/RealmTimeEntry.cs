using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using Toggl.Storage.Realm.Sync;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTimeEntry
        : RealmObject, IDatabaseTimeEntry, IPushable, ISyncable<ITimeEntry>
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
            var commonDescription = ContainsBackup
                ? DescriptionBackup
                : Description;

            DescriptionBackup = Description =
                ThreeWayMerge.Merge(commonDescription, Description, timeEntry.Description);

            shouldStayDirty |= timeEntry.Description != Description;

            // ProjectId
            var commonProjectId = ContainsBackup
                ? ProjectIdBackup
                : ProjectId;

            var projectId = ThreeWayMerge.Merge(commonProjectId, ProjectId, timeEntry.ProjectId);

            RealmProject = projectId.HasValue
                ? realm.GetById<RealmProject>(projectId.Value)
                : null;

            shouldStayDirty |= timeEntry.ProjectId != projectId;

            // Billable
            var commonBillable = ContainsBackup
                ? BillableBackup
                : Billable;

            BillableBackup = Billable =
                ThreeWayMerge.Merge(commonBillable, Billable, timeEntry.Billable);

            shouldStayDirty |= timeEntry.Billable != Billable;

            // Start
            var commonStart = ContainsBackup
                ? StartBackup
                : Start;

            Start = ThreeWayMerge.Merge(commonStart, Start, timeEntry.Start);

            shouldStayDirty |= timeEntry.Start != Start;

            // Duration
            var commonDuration = ContainsBackup
                ? DurationBackup
                : Duration;

            Duration = ThreeWayMerge.Merge(commonDuration, Duration, timeEntry.Duration);

            shouldStayDirty |= timeEntry.Duration != Duration;

            // Task
            var commonTaskId = ContainsBackup
                ? TaskIdBackup
                : TaskId;

            var taskId = ThreeWayMerge.Merge(commonTaskId, TaskId, timeEntry.TaskId);

            RealmTask = taskId.HasValue
                ? realm.GetById<RealmTask>(taskId.Value)
                : null;

            shouldStayDirty |= timeEntry.TaskId != taskId;

            // Tag Ids
            var commonTagIds = ContainsBackup
                ? Arrays.NotNullOrEmpty(TagIdsBackup)
                : Arrays.NotNullOrEmpty(TagIds);

            var localTagIds = Arrays.NotNullOrEmpty(TagIds);
            var serverTagIds = Arrays.NotNullOrEmpty(timeEntry.TagIds);

            var tagsIds = ThreeWayMerge.Merge(commonTagIds, localTagIds, serverTagIds);
            shouldStayDirty |= !tagsIds.SetEquals(localTagIds);

            RealmTags.Clear();
            tagsIds
                .Select(tagId => realm.GetById<RealmTag>(tagId))
                .AddTo(RealmTags);

            // the conflict is resolved, the backup is no longer needed until next local change
            ContainsBackup = false;
            LastSyncErrorMessage = null;

            // Update sync status depending on the way the time entry has changed during the 3-way merge
            SyncStatus = wasDirty && shouldStayDirty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
