using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Core.DataSources;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Core.Interactors
{
    public sealed class ObserveCurrentPlanInteractor : IInteractor<IObservable<Plan>>
    {
        private readonly ITogglDataSource dataSource;
        private readonly InteractorFactory interactorFactory;

        public ObserveCurrentPlanInteractor(
            ITogglDataSource dataSource,
            InteractorFactory interactorFactory)
        {
            this.dataSource = dataSource;
            this.interactorFactory = interactorFactory;
        }

        public IObservable<Plan> Execute()
            => dataSource.User.Current
                .DistinctUntilChanged(user => user.DefaultWorkspaceId)
                .SelectMany(_ => interactorFactory.GetDefaultWorkspace().Execute())
                .SelectMany(workspace => interactorFactory.GetWorkspaceFeaturesById(workspace.Id).Execute())
                .Select(planFromFeatures);

        private Plan planFromFeatures(IThreadSafeWorkspaceFeatureCollection features)
        {
            var isAtLeastStarter = features.Features
                .Any(feature => feature.FeatureId == WorkspaceFeatureId.Pro && feature.Enabled);

            return isAtLeastStarter ? Plan.Starter : Plan.Free;
        }
    }
}
