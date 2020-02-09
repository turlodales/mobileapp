using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources.Interfaces;
using Toggl.Core.Extensions;
using Toggl.Core.Models.Interfaces;
using Toggl.Networking.ApiClients.Interfaces;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage;
using static Toggl.Core.Sync.PushSyncOperation;

namespace Toggl.Core.Sync.States.Push
{
    internal sealed class DeleteEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        : BasePushEntityState<TThreadsafeModel>
        where TModel : IIdentifiable
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : class, TDatabaseModel, IThreadSafeModel
    {
        private readonly IDeletingApiClient<TModel> api;

        private readonly IDataSource<TThreadsafeModel, TDatabaseModel> dataSource;

        public StateResult Done { get; } = new StateResult();

        public DeleteEntityState(
            IDeletingApiClient<TModel> api,
            IAnalyticsService analyticsService,
            IDataSource<TThreadsafeModel, TDatabaseModel> dataSource)
            : base(analyticsService)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.api = api;
            this.dataSource = dataSource;
        }

        public override IObservable<ITransition> Start(TThreadsafeModel entity)
            => delete(entity)
                .SelectMany(_ => dataSource.Delete(entity.Id))
                .Track(AnalyticsService.EntitySynced, Delete, entity.GetSafeTypeName())
                .Track(AnalyticsService.EntitySyncStatus, entity.GetSafeTypeName(), $"{Delete}:Success")
                .Select(_ => Done.Transition())
                .Catch(Fail(entity, Delete));

        private IObservable<Unit> delete(TModel entity)
            => entity == null
                ? Observable.Throw<Unit>(new ArgumentNullException(nameof(entity)))
                : api.Delete(entity).ToObservable();
    }
}
