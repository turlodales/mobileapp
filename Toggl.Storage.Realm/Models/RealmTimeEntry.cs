using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

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

        public void SaveSyncResult(ITimeEntry entity, Realms.Realm realm)
        {
            Id = entity.Id;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.ServerDeletedAt.HasValue;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipProjectFetch = entity?.ProjectId == null || entity.ProjectId == 0;
            RealmProject = skipProjectFetch ? null : realm.All<RealmProject>().SingleOrDefault(x => x.Id == entity.ProjectId || x.OriginalId == entity.ProjectId);
            var skipTaskFetch = RealmProject == null || entity?.TaskId == null || entity.TaskId == 0;
            RealmTask = skipTaskFetch ? null : realm.All<RealmTask>().SingleOrDefault(x => x.Id == entity.TaskId || x.OriginalId == entity.TaskId);
            Billable = entity.Billable;
            Start = entity.Start;
            Duration = entity.Duration;
            Description = entity.Description;

            var tags = entity.TagIds?.Select(id =>
                realm.All<RealmTag>().Single(x => x.Id == id || x.OriginalId == id)) ?? new RealmTag[0];
            RealmTags.Clear();
            tags.ForEach(RealmTags.Add);

            var skipUserFetch = entity?.UserId == null || entity.UserId == 0;
            RealmUser = skipUserFetch ? null : realm.All<RealmUser>().Single(x => x.Id == entity.UserId || x.OriginalId == entity.UserId);

            ContainsBackup = entity.ContainsBackup;
            BillableBackup = entity.BillableBackup;
            DescriptionBackup = entity.DescriptionBackup;
            DurationBackup = entity.DurationBackup;
            ProjectIdBackup = entity.ProjectIdBackup;
            StartBackup = entity.StartBackup;
            TaskIdBackup = entity.TaskIdBackup;
            TagIdsBackup.Clear();

            if (entity.TagIdsBackup != null)
            {
                foreach (var tagId in entity.TagIdsBackup)
                    TagIdsBackup.Add(tagId);
            }
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
