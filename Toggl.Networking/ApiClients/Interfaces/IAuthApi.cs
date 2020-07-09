using System.Threading.Tasks;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.ApiClients
{
    public interface IAuthApi
    {
        Task<ISamlConfig> GetSamlConfig(Email email);
    }
}
