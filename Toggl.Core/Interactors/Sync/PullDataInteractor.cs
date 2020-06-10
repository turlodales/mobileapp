using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.DataSources;
using Toggl.Core.Exceptions;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
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
            var since = sinceRepository.Get<ITimeEntry>();

            var response = await api.SyncApi.Pull(since);
            queryFactory.ProcessPullResult(response).Execute();

            await handlePotentialNoWorkspaceScenario();

            sinceRepository.Set<ITimeEntry>(DateTimeOffset.FromUnixTimeSeconds(response.ServerTime));
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
