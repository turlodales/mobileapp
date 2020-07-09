using Toggl.Networking.ApiClients;

namespace Toggl.Networking
{
    public interface IUnauthenticatedTogglApi
    {
        IAuthApi Auth { get; }
    }
}
