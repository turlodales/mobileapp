using System;
using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Queries;

namespace Toggl.Core.Interactors
{
    internal class PushSyncInteractor : IInteractor<Task>
    {
        private readonly ITogglApi api;
        private readonly IPushRequestIdentifierRepository pushRequestIdentifier;
        private readonly IInteractorFactory interactorFactory;
        private readonly IQueryFactory queryFactory;

        public PushSyncInteractor(
            ITogglApi api,
            IPushRequestIdentifierRepository pushRequestIdentifier,
            IInteractorFactory interactorFactory,
            IQueryFactory queryFactory)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(pushRequestIdentifier, nameof(pushRequestIdentifier));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));

            this.api = api;
            this.pushRequestIdentifier = pushRequestIdentifier;
            this.interactorFactory = interactorFactory;
            this.queryFactory = queryFactory;
        }

        public async Task Execute()
        {
            var request = await interactorFactory.PreparePushRequest().Execute();
            if (request.IsEmpty) return;

            var id = Guid.NewGuid();
            if (pushRequestIdentifier.TryGet(out _))
            {
                throw new InvalidOperationException(
                    "The push interactor can be executed only when"
                    + " there is no other outstanding push request.");
            }

            try
            {
                pushRequestIdentifier.Set(id);
                var response = await api.SyncApi.Push(id, request);
                queryFactory.ProcessPushResult(response).Execute();
                pushRequestIdentifier.Clear();
            }
            catch (BadRequestException e)
            {
                // the request was rejected by the server during the validation
                // phase, there's no need to try to get this request later
                // (it wouldn't hurt us, we would get back 404 anyway, but we can
                // save that one HTTP request anyway)
                pushRequestIdentifier.Clear();
                throw e;
            }
            catch (OfflineException ex) when (ex.HasTimeouted)
            {
                // When the API request timeouts, it's not necessarily a problem.
                // We simply need to resolve the outstanding push request and therefore
                // we can swallow the exception and just keep the outstanding push
                // request's ID for later.
            }
        }
    }
}
