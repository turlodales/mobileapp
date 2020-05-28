using System.Reactive;
using Toggl.Networking.Sync.Push;

namespace Toggl.Storage.Queries
{
    public interface IQueryFactory
    {
        IQuery<Unit> ProcessPushResult(IResponse response);
    }
}
