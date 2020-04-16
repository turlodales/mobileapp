using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync
{
    public sealed partial class PushRequest
    {
        public PushRequest CreateClients(IEnumerable<IClient> clients)
        {
            clients
                .Select(client => new Client(client))
                .Select(client => new CreatePushAction<Client>(client))
                .AddTo(Clients);

            return this;
        }
    }
}
