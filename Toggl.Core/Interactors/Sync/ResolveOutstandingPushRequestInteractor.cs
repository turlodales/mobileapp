using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Queries;

namespace Toggl.Core.Interactors
{
    internal class ResolveOutstandingPushRequestInteractor : IInteractor<Task>
    {
        private readonly ITogglApi api;
        private readonly ITogglDatabase database;
        private readonly IQueryFactory queryFactory;

        public ResolveOutstandingPushRequestInteractor(
            ITogglApi api,
            ITogglDatabase database,
            IQueryFactory queryFactory)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(database, nameof(database));
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));

            this.api = api;
            this.database = database;
            this.queryFactory = queryFactory;
        }

        public async Task Execute()
        {
            while (database.PushRequestIdentifier.TryGet(out var id))
            {
                try
                {
                    var response = await api.SyncApi.OutstandingPush(id);
                    queryFactory.ProcessPushResult(response).Execute();
                    database.PushRequestIdentifier.Clear();
                }
                catch (NotFoundException)
                {
                    // there is no record of this ID on the server, we're not getting
                    // any data from the server now or in the future, we can forget
                    // about this very request ID
                    database.PushRequestIdentifier.Clear();
                }
                catch (OfflineException e) when (e.HasTimeouted)
                {
                    // In the case when we wait so long that the request timeouts,
                    // we need to reconnect to the server and wait until the GET
                    // request succeeds.

                    // All other exceptions aren't caught and they will have to
                    // be handled by the caller of this interactor.
                }
            }
        }
    }
}
