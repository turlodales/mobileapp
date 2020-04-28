using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.DataSources;
using Toggl.Core.DataSources.Interfaces;
using Toggl.Core.Models.Interfaces;
using Toggl.Networking.Sync;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage;
using static Toggl.Storage.SyncStatus;
using static Toggl.Networking.Sync.Push.ActionType;
using Toggl.Networking.Sync.Push;
using Toggl.Core.Interactors;
using Toggl.Shared.Extensions;

namespace Toggl.Core.Interactors
{
    public class PreparePushRequestInteractor : IInteractor<Task<Request>>
    {
        private readonly ITogglDataSource dataSource;

        public PreparePushRequestInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public async Task<Request> Execute()
        {
            var pushRequest = new Request();

            await dataSource.Clients
                .GetAll(isDirty)
                .Select(entitiesToCreate)
                .Do(pushRequest.CreateClients);

            await dataSource.Projects
                .GetAll(isDirty)
                .Select(entitiesToCreate)
                .Do(pushRequest.CreateProjects);

            await dataSource.Tags
                .GetAll(isDirty)
                .Select(entitiesToCreate)
                .Do(pushRequest.CreateTags);

            var timeEntries = await dataSource
                .TimeEntries
                .GetAll(isDirty);

            timeEntries
                .Where(te => !isLocalDelete(te))
                .DistributedExecute(actionType,
                    (Create, pushRequest.CreateTimeEntries),
                    (Update, pushRequest.UpdateTimeEntries),
                    (Delete, pushRequest.DeleteTimeEntries));

            await dataSource.Preferences
                .Current
                .Where(isDirty)
                .Do(pushRequest.UpdatePreferences);

            await dataSource.User
                .Current
                .Where(isDirty)
                .Do(pushRequest.UpdateUser);

            return pushRequest;
        }

        private IEnumerable<T> entitiesToCreate<T>(IEnumerable<T> entities) where T : IIdentifiable
            => entities.Where(entity => entity.Id < 0);

        private bool isDirty(IDatabaseSyncable entity)
            => entity.SyncStatus == SyncNeeded;

        private bool isLocalDelete<T>(T entity) where T : IDatabaseSyncable, IIdentifiable
            => entity.IsDeleted && entity.Id < 0;

        private ActionType actionType<T>(T entity) where T : IIdentifiable, IDatabaseSyncable
            => entity.IsDeleted
            ? Delete
            : entity.Id >= 0 ? Update : Create;
    }
}
