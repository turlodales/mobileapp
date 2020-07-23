using Newtonsoft.Json;
using Toggl.Shared;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    public sealed class CreateMeta
    {
        public CreateMeta(long clientId)
        {
            ClientId = clientId.ToString();
        }

        [JsonProperty("client_assigned_id")]
        public string ClientId { get; private set; }
    }
}
