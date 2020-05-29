using System.Reactive;

namespace Toggl.Storage.Queries
{
    public interface IQueryFactory
    {
        IQuery<Unit> ProcessPullResult(Networking.Sync.Pull.IResponse response);
        IQuery<Unit> ProcessPushResult(Networking.Sync.Push.IResponse response);
    }
}
