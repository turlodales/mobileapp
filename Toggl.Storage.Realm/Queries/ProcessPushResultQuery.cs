using System;
using System.Linq;
using System.Reactive;
using Toggl.Networking.Sync.Push;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm.Queries
{
    internal class ProcessPushResultQuery : IQuery<Unit>
    {
        private readonly IResponse response;
        private readonly Func<Realms.Realm> realmProvider;

        public ProcessPushResultQuery(Func<Realms.Realm> realmProvider, IResponse response)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(response));
            Ensure.Argument.IsNotNull(response, nameof(response));

            this.realmProvider = realmProvider;
            this.response = response;
        }

        public Unit Execute()
        {
            var realm = realmProvider();
            using (var transaction = realm.BeginWrite())
            {
                processSingletonResult<IPreferences, RealmPreferences>(response.Preferences, realm);
                processSingletonResult<IUser, RealmUser>(response.User, realm);

                foreach (var res in response.Workspaces)
                    processResult<IWorkspace, RealmWorkspace>(res, realm);

                foreach (var res in response.Tags)
                    processResult<ITag, RealmTag>(res, realm);

                foreach (var res in response.Clients)
                    processResult<IClient, RealmClient>(res, realm);

                foreach (var res in response.Tasks)
                    processResult<ITask, RealmTask>(res, realm);

                foreach (var res in response.Projects)
                    processResult<IProject, RealmProject>(res, realm);

                foreach (var res in response.TimeEntries)
                    processResult<ITimeEntry, RealmTimeEntry>(res, realm);

                transaction.Commit();
            }

            return Unit.Default;
        }

        private void processSingletonResult<TEntity, TRealmEntity>(IActionResult<TEntity> result, Realms.Realm realm)
            where TRealmEntity : Realms.RealmObject, IPushable
        {
            if (result?.Success == false && result is ErrorResult<TEntity> error)
            {
                var entity = realm.All<TRealmEntity>().Single();
                entity.PushFailed(error.ErrorMessage.DefaultMessage);
            }
        }

        private void processResult<TEntity, TRealmEntity>(IEntityActionResult<TEntity> entityResult, Realms.Realm realm)
            where TRealmEntity : Realms.RealmObject, IPushable, IIdentifiable, ISyncable<TEntity>
        {
            var entity = realm.GetById<TRealmEntity>(entityResult.Id);

            // It is possible that the entity was deleted locally since the push.
            // In that case, simply ignore this result.
            if (entity == null)
                return;

            if (entityResult.Result.Success == false && entityResult.Result is ErrorResult<TEntity> error)
            {
                switch (error.ErrorMessage.Code)
                {
                    case ErrorCode.Skipped:
                        break;
                    default:
                        entity.PushFailed(error.ErrorMessage.DefaultMessage);
                        break;
                }
            }
            else if (entityResult is CreateActionResult<TEntity> create
                && create.Result.Success
                && create.Result is SuccessPayloadResult<TEntity> success)
            {
                entity.SaveSyncResult(success.Payload, realm);
            }
        }
    }
}
