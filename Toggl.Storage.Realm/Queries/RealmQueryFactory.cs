using System;
using System.Reactive;
using Toggl.Shared;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Sync;

namespace Toggl.Storage.Realm.Queries
{
    public sealed class RealmQueryFactory : IQueryFactory
    {
        private readonly Func<Realms.Realm> realmProvider;
        private readonly Func<DateTimeOffset> currentTimeProvider;

        public RealmQueryFactory(Func<Realms.Realm> realmProvider, Func<DateTimeOffset> currentTimeProvider)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(currentTimeProvider, nameof(currentTimeProvider));

            this.realmProvider = realmProvider;
            this.currentTimeProvider = currentTimeProvider;
        }

        public IQuery<Unit> ProcessPullResult(Networking.Sync.Pull.IResponse response)
            => new ProcessPullResultQuery(realmProvider, currentTimeProvider, response);

        public IQuery<Unit> ProcessPushResult(Networking.Sync.Push.IResponse response)
            => new ProcessPushResultQuery(realmProvider, response);

        public IQuery<Unit> MarkEntitiesAsSyncing(Networking.Sync.Push.Request request)
            => new MarkEntitiesAsSyncingQuery(realmProvider, request);

        public IQuery<Unit> MigrateBackToOldSyncing()
            => new MigrateBackToOldSyncingQuery(realmProvider);
    }
}
