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
            pushRequestIdentifier.Set(id);

            try
            {
                var response = await api.SyncApi.Push(id, request);
                queryFactory.ProcessPushResult(response).Execute();
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
                await interactorFactory.ResolveOutstandingPushRequest().Execute();
            }

            pushRequestIdentifier.Clear();
        }
    }
}
