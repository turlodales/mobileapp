using Newtonsoft.Json.Linq;

namespace Toggl.Networking.Sync.Push.Serialization
{
    internal interface IPartiallySerializableEntity
    {
        JObject Serialize();
    }
}
