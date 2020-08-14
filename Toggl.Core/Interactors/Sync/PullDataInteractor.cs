using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Exceptions;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage;
using Toggl.Storage.Queries;

namespace Toggl.Core.Interactors
{
    public class PullDataInteractor : IInteractor<Task>
    {
        private ITogglApi api;
        private ISinceParameterRepository sinceRepository;
        private IQueryFactory queryFactory;
        private IInteractorFactory interactorFactory;

        public PullDataInteractor(
            ITogglApi api,
            IInteractorFactory interactorFactory,
            ISinceParameterRepository sinceRepository,
            IQueryFactory queryFactory)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(sinceRepository, nameof(sinceRepository));
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.api = api;
            this.sinceRepository = sinceRepository;
            this.interactorFactory = interactorFactory;
            this.queryFactory = queryFactory;
        }

        public async Task Execute()
        {
            long? serverTime = null;

            try
            {
                serverTime = await pull();
            }
            catch (BadRequestException e)
            {
                serverTime = await pullAndResetLocalState();
            }

            await handlePotentialNoWorkspaceScenario();

            if (serverTime.HasValue)
            {
                var nextSince = DateTimeOffset.FromUnixTimeSeconds(serverTime.Value);
                sinceRepository.Set<ITimeEntry>(nextSince);
            }
        }

        private async Task<long> pull()
        {
            var since = sinceRepository.Get<ITimeEntry>();
            var response = await api.SyncApi.Pull(since);
            queryFactory.ProcessPullResult(response).Execute();
            return response.ServerTime;
        }

        private async Task<long> pullAndResetLocalState()
        {
            var response = await api.SyncApi.Pull(null);
            queryFactory.ResetLocalState(response).Execute();
            return response.ServerTime;
        }

        private async Task handlePotentialNoWorkspaceScenario()
        {
            var currentUser = await interactorFactory
                .GetCurrentUser()
                .Execute();

            // NOTE: The interactor returns only accessible workspaces.
            var workspaces = (await interactorFactory
                .GetAllWorkspaces() 
                .Execute())
                .ToList();

            if (currentUser.DefaultWorkspaceId != null)
            {
                var defaultWorkspaceStillAccessible = workspaces.Any(ws => ws.Id == currentUser.DefaultWorkspaceId);

                if (defaultWorkspaceStillAccessible)
                    return;
            }

            if (workspaces.Count() == 1)
            {
                var newDefaultWorkspaceId = workspaces.Single().Id;
                interactorFactory.SetDefaultWorkspace(newDefaultWorkspaceId);
                return;
            }

            throw new NoDefaultWorkspaceException();
        }
    }
}
