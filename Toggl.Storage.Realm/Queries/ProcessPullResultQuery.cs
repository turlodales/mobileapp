using System;
using System.Reactive;
using Toggl.Networking.Sync.Pull;
using Toggl.Shared;
using Toggl.Storage.Queries;

namespace Toggl.Storage.Realm.Sync
{
    internal sealed class ProcessPullResultQuery : WritePulledDataTransaction, IQuery<Unit>
    {
        private Func<Realms.Realm> realmProvider;
        private Func<DateTimeOffset> currentTimeProvider;
        private IResponse response;

        public ProcessPullResultQuery(Func<Realms.Realm> realmProvider, Func<DateTimeOffset> currentTimeProvider, IResponse response)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(response, nameof(response));
            Ensure.Argument.IsNotNull(currentTimeProvider, nameof(currentTimeProvider));

            this.realmProvider = realmProvider;
            this.response = response;
            this.currentTimeProvider = currentTimeProvider;
        }
        public Unit Execute()
        {
            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                WriteUsingThreeWayMerge(realm, currentTimeProvider, response);
                transaction.Commit();
            }

            return Unit.Default;
        }
    }
}
