using Realms;
using System;
using System.Linq;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTask
        : RealmObject, IDatabaseTask, IPushable, ISyncable<ITask>
    {
        public string Name { get; set; }

        public long EstimatedSeconds { get; set; }

        public bool Active { get; set; }

        public DateTimeOffset At { get; set; }

        public long TrackedSeconds { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public RealmProject RealmProject { get; set; }

        public long ProjectId => RealmProject?.Id ?? 0;

        public IDatabaseProject Project => RealmProject;

        public RealmUser RealmUser { get; set; }

        public long? UserId => RealmUser?.Id;

        public IDatabaseUser User => RealmUser;

        public bool IsInaccessible => Workspace.IsInaccessible;

        public void SaveSyncResult(ITask entity, Realms.Realm realm)
        {
            Id = entity.Id;
            At = entity.At;
            IsDeleted = false;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
            Name = entity.Name;
            var skipProjectFetch = entity?.ProjectId == null || entity.ProjectId == 0;
            RealmProject = skipProjectFetch ? null : realm.All<RealmProject>().Single(x => x.Id == entity.ProjectId || x.OriginalId == entity.ProjectId);
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipUserFetch = entity?.UserId == null || entity.UserId == 0;
            RealmUser = skipUserFetch ? null : realm.All<RealmUser>().Single(x => x.Id == entity.UserId || x.OriginalId == entity.UserId);
            EstimatedSeconds = entity.EstimatedSeconds;
            Active = entity.Active;
            TrackedSeconds = entity.TrackedSeconds;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
