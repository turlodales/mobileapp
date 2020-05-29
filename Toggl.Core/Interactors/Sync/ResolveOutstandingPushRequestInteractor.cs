using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Networking.ApiClients;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Queries;

namespace Toggl.Core.Interactors
{
    internal class ResolveOutstandingPushRequestInteractor : IInteractor<Task>
    {
        private readonly ISyncApi syncApi;
        private readonly IPushRequestIdentifierRepository pushRequestIdentifier;
        private readonly IQueryFactory queryFactory;

        public ResolveOutstandingPushRequestInteractor(
            ISyncApi syncApi,
            IPushRequestIdentifierRepository pushRequestIdentifier,
            IQueryFactory queryFactory)
        {
            Ensure.Argument.IsNotNull(syncApi, nameof(syncApi));
            Ensure.Argument.IsNotNull(pushRequestIdentifier, nameof(pushRequestIdentifier));
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));

            this.syncApi = syncApi;
            this.pushRequestIdentifier = pushRequestIdentifier;
            this.queryFactory = queryFactory;
        }

        public async Task Execute()
        {
            while (pushRequestIdentifier.TryGet(out var id))
            {
                try
                {
                    var response = await syncApi.OutstandingPush(id);
                    queryFactory.ProcessPushResult(response).Execute();
                    pushRequestIdentifier.Clear();
                }
                catch (NotFoundException)
                {
                    // there is no record of this ID on the server, we're not getting
                    // any data from the server now or in the future, we can forget
                    // about this very request ID
                    pushRequestIdentifier.Clear();
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
