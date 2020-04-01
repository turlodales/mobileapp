using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.DataSources;
using Toggl.Core.Models;
using Toggl.Shared;
using Toggl.Storage;

namespace Toggl.Core.Interactors
{
    public class CreateUnsyncedDataDumpInteractor : IInteractor<Task<UnsyncedDataDump>>
    {
        private readonly ITogglDataSource dataSource;

        public CreateUnsyncedDataDumpInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            this.dataSource = dataSource;
        }

        public async Task<UnsyncedDataDump> Execute()
        {
            var clients = await dataSource.Clients.GetAll(notSynced);
            var projects = await dataSource.Projects.GetAll(notSynced);
            var tags = await dataSource.Tags.GetAll(notSynced);
            var timeEntries = await dataSource.TimeEntries.GetAll(notSynced);

            return new UnsyncedDataDump(timeEntries, tags, projects, clients);
        }
        
        private bool notSynced(IDatabaseSyncable p) 
            => p.SyncStatus != SyncStatus.InSync;
    }
}
