using Realms;
using System;
using System.Linq;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTag
        : RealmObject, IDatabaseTag, IPushable, ISyncable<ITag>
    {
        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public bool IsInaccessible => Workspace.IsInaccessible;

        public void SaveSyncResult(ITag entity, Realms.Realm realm)
        {
            Id = entity.Id;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.ServerDeletedAt.HasValue;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            Name = entity.Name;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
