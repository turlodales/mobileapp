using System;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.DataSources;
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

        public PullDataInteractor(
            ITogglApi api,
            ISinceParameterRepository sinceRepository,
            IQueryFactory queryFactory)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(sinceRepository, nameof(sinceRepository));
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));

            this.api = api;
            this.sinceRepository = sinceRepository;
            this.queryFactory = queryFactory;
        }

        public async Task Execute()
        {
            var since = sinceRepository.Get<ITimeEntry>();

            var response = await api.SyncApi.Pull(since);
            queryFactory.ProcessPullResult(response).Execute();

            sinceRepository.Set<ITimeEntry>(DateTimeOffset.FromUnixTimeSeconds(response.ServerTime));
        }
    }
}
