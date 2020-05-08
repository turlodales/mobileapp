using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Shared;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Services
{
    public class UnsyncedDataPersistenceService : IUnsyncedDataPersistenceService
    {
        private readonly IAnalyticsService analyticsService;
        private readonly Func<string, StreamWriter, Task> writeToFile;
        private readonly IInteractor<Task<UnsyncedDataDump>> createUnsyncedDataDumpInteractor;

        public UnsyncedDataPersistenceService(
            IAnalyticsService analyticsService,
            Func<string, StreamWriter, Task> writeToFile,
            IInteractor<Task<UnsyncedDataDump>> createUnsyncedDataDumpInteractor)
        {
            Ensure.Argument.IsNotNull(writeToFile, nameof(writeToFile));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(createUnsyncedDataDumpInteractor, nameof(createUnsyncedDataDumpInteractor));

            this.writeToFile = writeToFile;
            this.analyticsService = analyticsService;
            this.createUnsyncedDataDumpInteractor = createUnsyncedDataDumpInteractor;
        }

        public async Task PersistUnsyncedData()
        {
            var unsyncedDataDump = await createUnsyncedDataDumpInteractor.Execute();
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            var serializedUnsyncedDataDump = JsonConvert.SerializeObject(unsyncedDataDump, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            var backingFile = IUnsyncedDataPersistenceService.UnsyncedDataFilePath;
            await using var writer = File.CreateText(backingFile);
            await writeToFile(serializedUnsyncedDataDump, writer);

            analyticsService.UnsyncedDataDumped.Track(
                unsyncedDataDump.TimeEntries.Count,
                unsyncedDataDump.Projects.Count,
                unsyncedDataDump.Clients.Count,
                unsyncedDataDump.Tags.Count
            );            
        }
    }
}
