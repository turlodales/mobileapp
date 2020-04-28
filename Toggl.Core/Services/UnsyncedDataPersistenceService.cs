using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Shared;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Services
{
    public class UnsyncedDataPersistenceService : IUnsyncedDataPersistenceService
    {
        private readonly IInteractor<Task<UnsyncedDataDump>> createUnsyncedDataDumpInteractor;
        private readonly Func<string, StreamWriter, Task> writeToFile;

        public UnsyncedDataPersistenceService(IInteractor<Task<UnsyncedDataDump>> createUnsyncedDataDumpInteractor, Func<string, StreamWriter, Task> writeToFile)
        {
            Ensure.Argument.IsNotNull(createUnsyncedDataDumpInteractor, nameof(createUnsyncedDataDumpInteractor));
            Ensure.Argument.IsNotNull(writeToFile, nameof(writeToFile));
            this.createUnsyncedDataDumpInteractor = createUnsyncedDataDumpInteractor;
            this.writeToFile = writeToFile;
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
            var backingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), IUnsyncedDataPersistenceService.UnsyncedDataFileName);
            await using (var writer = File.CreateText(backingFile))
            {
                await writeToFile(serializedUnsyncedDataDump, writer);
            }
        }
    }
}
