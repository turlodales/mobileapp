using System.Linq;
using System.Collections.Generic;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public Request CreateClients(IEnumerable<IClient> clients)
        {
            clients
                .Select(client => new Client(client))
                .Select(client => new CreateAction<Client>(client))
                .AddTo(Clients);

            return this;
        }
    }
}
