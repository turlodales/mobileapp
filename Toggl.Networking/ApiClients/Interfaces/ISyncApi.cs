using System;
using System.Threading.Tasks;

namespace Toggl.Networking.ApiClients
{
    public interface ISyncApi
    {
        Task<Sync.Pull.IResponse> Pull(DateTimeOffset? since);
        Task<Sync.Push.IResponse> Push(Guid id, Sync.Push.Request request);
        Task<Sync.Push.IResponse> OutstandingPush(Guid id);
    }
}
