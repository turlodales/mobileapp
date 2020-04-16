using Newtonsoft.Json;

namespace Toggl.Networking.Sync.Push
{
    public sealed class CreateMeta : IMeta
    {
        public CreateMeta(long clientId)
        {
            ClientId = clientId.ToString();
        }

        [JsonProperty("client_assigned_id")]
        public string ClientId { get; private set; }
    }
}
