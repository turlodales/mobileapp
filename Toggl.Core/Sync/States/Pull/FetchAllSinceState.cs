using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using Toggl.Core.Extensions;
using Toggl.Networking;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage;


namespace Toggl.Core.Sync.States.Pull
{
    internal sealed class FetchAllSinceState : BaseFetchSinceState
    {
        public FetchAllSinceState(
            ITogglApi api,
            ISinceParameterRepository since,
            ITimeService timeService)
            : base(api, since, timeService)
        {
        }

        protected override IFetchObservables Fetch()
        {
            var workspaces = Api.Workspaces.GetAll().ToObservable().ConnectedReplay();
            var user = Api.User.Get().ToObservable().ConnectedReplay();
            var features = Api.WorkspaceFeatures.GetAll().ToObservable().ConnectedReplay();
            var preferences = Api.Preferences.Get().ToObservable().ConnectedReplay();
            var tags = FetchRecentIfPossible(Api.Tags.GetAllSince, Api.Tags.GetAll).ConnectedReplay();
            var clients = FetchRecentIfPossible(Api.Clients.GetAllSince, Api.Clients.GetAll).ConnectedReplay();
            var projects = FetchRecentIfPossible(Api.Projects.GetAllSince, Api.Projects.GetAll).ConnectedReplay();
            var timeEntries = FetchRecentIfPossible(Api.TimeEntries.GetAllSince, FetchTwoMonthsOfTimeEntries).ConnectedReplay();
            var tasks = FetchRecentIfPossible(Api.Tasks.GetAllSince, Api.Tasks.GetAll).ConnectedReplay();

            return new FetchObservables(workspaces, features, user, clients, projects, timeEntries, tags, tasks, preferences);
        }
    }
}
