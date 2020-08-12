using System;
using System.Linq;
using System.Reactive;
using Toggl.Shared;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Models;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Sync
{
    public class MigrateBackToOldSyncingQuery : IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;

        public MigrateBackToOldSyncingQuery(Func<RealmDb> realmProvider)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));

            this.realmProvider = realmProvider;
        }

        public Unit Execute()
        {
            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                foreach (var outstandingPushRequest in realm.All<RealmPushRequestIdentifier>())
                    realm.Remove(outstandingPushRequest);

                foreach (var preferences in realm.All<RealmPreferences>().Where(p => p.SyncStatusInt == (int)SyncStatus.Syncing))
                    preferences.SyncStatus = SyncStatus.SyncNeeded;

                foreach (var user in realm.All<RealmUser>().Where(u => u.SyncStatusInt == (int)SyncStatus.Syncing))
                    user.SyncStatus = SyncStatus.SyncNeeded;

                foreach (var timeEntry in realm.All<RealmTimeEntry>().Where(te => te.SyncStatusInt == (int)SyncStatus.Syncing))
                    timeEntry.SyncStatus = SyncStatus.SyncNeeded;

                transaction.Commit();
            }

            return Unit.Default;
        }
    }
}
