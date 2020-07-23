using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmProject
        : RealmObject, IDatabaseProject, IPushable, ISyncable<IProject>
    {
        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        public bool Active { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public string Color { get; set; }

        public bool? Billable { get; set; }

        public bool? Template { get; set; }

        public bool? AutoEstimates { get; set; }

        public long? EstimatedHours { get; set; }

        public double? Rate { get; set; }

        public string Currency { get; set; }

        public int? ActualHours { get; set; }

        public RealmWorkspace RealmWorkspace { get; set; }

        public long WorkspaceId => RealmWorkspace?.Id ?? 0;

        public IDatabaseWorkspace Workspace => RealmWorkspace;

        public RealmClient RealmClient { get; set; }

        public long? ClientId => RealmClient?.Id;

        public IDatabaseClient Client => RealmClient;

        [Backlink(nameof(RealmTask.RealmProject))]
        public IQueryable<RealmTask> RealmTasks { get; }

        public IEnumerable<IDatabaseTask> Tasks => RealmTasks;

        public bool IsInaccessible => Workspace.IsInaccessible;

        public void SaveSyncResult(IProject entity, Realms.Realm realm)
        {
            Id = entity.Id;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.ServerDeletedAt.HasValue;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipClientFetch = entity?.ClientId == null || entity.ClientId == 0;
            RealmClient = skipClientFetch ? null : realm.All<RealmClient>().Single(x => x.Id == entity.ClientId || x.OriginalId == entity.ClientId);
            Name = entity.Name;
            IsPrivate = entity.IsPrivate;
            Active = entity.Active;
            Color = entity.Color;
            Billable = entity.Billable;
            Template = entity.Template;
            AutoEstimates = entity.AutoEstimates;
            EstimatedHours = entity.EstimatedHours;
            Rate = entity.Rate;
            Currency = entity.Currency;
            ActualHours = entity.ActualHours;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
