using Realms;
using System;
using System.Linq;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmClient
        : RealmObject, IDatabaseClient, IPushable, ISyncable<IClient>
    {
        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public bool IsInaccessible => Workspace.IsInaccessible;

        public void PushFailed(string message)
        {
            LastSyncErrorMessage = message;
            SyncStatus = SyncStatus.SyncFailed;
        }

        public void SaveSyncResult(IClient entity, Realms.Realm realm)
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
    }
}
