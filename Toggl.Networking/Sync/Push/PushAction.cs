using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Toggl.Networking.Sync.Push.Serialization;
using Toggl.Shared;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    public class PushAction
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ActionType Type { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Payload { get; }

        private PushAction(ActionType type, object meta, object payload)
        {
            Type = type;
            Meta = meta;
            Payload = payload;
        }

        public static PushAction Create<TEntity>(long localId, TEntity payload)
            => new PushAction(ActionType.Create, new CreateMeta(localId), payload);

        public static PushAction Update<TEntity>(long id, long workspaceId, TEntity payload)
            => new PushAction(
                ActionType.Update,
                new ModificationMeta(id, workspaceId),
                PartiallySerializableEntityFactory.Create(payload));

        public static PushAction UpdateSingleton(object payload)
            => new PushAction(ActionType.Update, null, payload);

        public static PushAction Delete(long id, long workspaceId)
            => new PushAction(ActionType.Delete, new ModificationMeta(id, workspaceId), null);
    }
}