using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Networking.Sync.Push;
using Toggl.Shared;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Sync
{
    public class MarkEntitiesAsSyncingQuery : IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;
        private readonly Request request;

        public MarkEntitiesAsSyncingQuery(Func<RealmDb> realmProvider, Request request)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(request, nameof(request));

            this.realmProvider = realmProvider;
            this.request = request;
        }

        public Unit Execute()
        {
            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                processSingleton<RealmPreferences>(realm, request.Preferences);
                processSingleton<RealmUser>(realm, request.User);
                processTimeEntries(realm, request.TimeEntries);

                transaction.Commit();
            }

            return Unit.Default;
        }

        private void processSingleton<T>(RealmDb realm, PushAction action)
            where T : RealmObject, IUpdatable
        {
            if (action != null)
            {
                var entity = realm.All<T>().Single();
                entity.PrepareForSyncing();
            }
        }

        private void processTimeEntries(RealmDb realm, IEnumerable<PushAction> actions)
        {
            if (actions == null)
                return;

            foreach (var action in actions) 
            {
                if (action.Type == ActionType.Update || action.Type == ActionType.Delete)
                {
                    var meta = (ModificationMeta)action.Meta;
                    var timeEntry = realm.GetById<RealmTimeEntry>(meta.Id);
                    timeEntry?.PrepareForSyncing();
                }
            }
        }
    }
}
