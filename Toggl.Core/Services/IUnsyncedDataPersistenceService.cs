using System.Threading.Tasks;

namespace Toggl.Core.Services
{
    public interface IUnsyncedDataPersistenceService
    {
        protected const string UnsyncedDataFileName = "unsynced-migration-dump.json";
        Task PersistUnsyncedData();
    }
}
