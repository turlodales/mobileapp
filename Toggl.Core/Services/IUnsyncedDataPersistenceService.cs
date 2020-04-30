using System;
using System.IO;
using System.Threading.Tasks;

namespace Toggl.Core.Services
{
    public interface IUnsyncedDataPersistenceService
    {
        static readonly string UnsyncedDataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "unsynced-migration-dump.json");
        Task PersistUnsyncedData();
    }
}
