using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Sync.States.CleanUp;
using Toggl.Shared;

namespace Toggl.Core.Interactors.Sync
{
    public sealed class CleanUpInteractor : IInteractor<Task>
    {
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;

        public CleanUpInteractor(
            ITimeService timeService,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.timeService = timeService;
            this.dataSource = dataSource;
            this.analyticsService = analyticsService;
        }

        public async Task Execute()
        {
            var deleteOlderEntries = new DeleteOldTimeEntriesState(timeService, dataSource.TimeEntries);
            var deleteUnsnecessaryProjectPlaceholders = new DeleteUnnecessaryProjectPlaceholdersState(dataSource.Projects, dataSource.TimeEntries);

            var checkForInaccessibleWorkspaces = new CheckForInaccessibleWorkspacesState(dataSource);

            var deleteInaccessibleTimeEntries = new DeleteInaccessibleTimeEntriesState(dataSource.TimeEntries);
            var deleteInaccessibleTags = new DeleteNonReferencedInaccessibleTagsState(dataSource.Tags, dataSource.TimeEntries);
            var deleteInaccessibleTasks = new DeleteNonReferencedInaccessibleTasksState(dataSource.Tasks, dataSource.TimeEntries);
            var deleteInaccessibleProjects = new DeleteNonReferencedInaccessibleProjectsState(dataSource.Projects, dataSource.Tasks, dataSource.TimeEntries);
            var deleteInaccessibleClients = new DeleteNonReferencedInaccessibleClientsState(dataSource.Clients, dataSource.Projects);
            var deleteInaccessibleWorkspaces = new DeleteNonReferencedInaccessibleWorkspacesState(
                dataSource.Workspaces,
                dataSource.TimeEntries,
                dataSource.Projects,
                dataSource.Tasks,
                dataSource.Clients,
                dataSource.Tags);

            var trackInaccesssibleDataAfterCleanUp = new TrackInaccessibleDataAfterCleanUpState(dataSource, analyticsService);
            var trackInaccesssibleWorkspacesAfterCleanUp = new TrackInaccessibleWorkspacesAfterCleanUpState(dataSource, analyticsService);

            await deleteOlderEntries.Start().SingleAsync();
            await deleteUnsnecessaryProjectPlaceholders.Start().SingleAsync();

            var transition = await checkForInaccessibleWorkspaces.Start().SingleAsync();
            while (transition.Result == checkForInaccessibleWorkspaces.FoundInaccessibleWorkspaces)
            {
                await deleteInaccessibleTimeEntries.Start().SingleAsync();
                await deleteInaccessibleTags.Start().SingleAsync();
                await deleteInaccessibleTasks.Start().SingleAsync();
                await deleteInaccessibleProjects.Start().SingleAsync();
                await deleteInaccessibleClients.Start().SingleAsync();
                await trackInaccesssibleDataAfterCleanUp.Start().SingleAsync();

                await deleteInaccessibleWorkspaces.Start().SingleAsync();
                await trackInaccesssibleWorkspacesAfterCleanUp.Start().SingleAsync();

                transition = await checkForInaccessibleWorkspaces.Start().SingleAsync();
            }
        }
    }
}
