using System;
using System.Reactive;
using Toggl.Shared;
using Toggl.Storage.Queries;
using Realms;
using RealmDb = Realms.Realm;
using Toggl.Networking.Sync.Pull;

namespace Toggl.Storage.Realm.Sync
{
    internal sealed class ResetLocalStateQuery : WritePulledDataTransaction, IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;
        private Func<DateTimeOffset> currentTimeProvider;
        private IResponse response;

        public ResetLocalStateQuery(Func<RealmDb> realmProvider, Func<DateTimeOffset> currentTimeProvider, IResponse response)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(currentTimeProvider, nameof(currentTimeProvider));
            Ensure.Argument.IsNotNull(response, nameof(response));

            this.realmProvider = realmProvider;
            this.currentTimeProvider = currentTimeProvider;
            this.response = response;
        }

        public Unit Execute()
        {
            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                prune(realm);
                WriteUsingThreeWayMerge(realm, currentTimeProvider, response);
                transaction.Commit();
            }

            return Unit.Default;
        }

        private void prune(RealmDb realm)
        {
            markAllWorkspacesAsInaccessible(realm);
            removeAllInSync<RealmTag>(realm);
            removeAllInSync<RealmClient>(realm);
            removeAllInSync<RealmTask>(realm);
            removeAllInSync<RealmProject>(realm);
            removeAllInSync<RealmTimeEntry>(realm);
        }

        private void markAllWorkspacesAsInaccessible(RealmDb realm)
        {
            foreach (var ws in realm.All<RealmWorkspace>())
            {
                ws.IsInaccessible = true;
            }
        }

        private void removeAllInSync<T>(RealmDb realm)
            where T : RealmObject, IDatabaseSyncable
        {
            foreach (var model in realm.All<T>())
            {
                if (model.SyncStatus == SyncStatus.InSync)
                {
                    realm.Remove(model);
                }
            }
        }
    }
}
