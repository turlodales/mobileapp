using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Toggl.Networking.Sync.Push.Serialization;

namespace Toggl.Networking.Sync.Push
{
    internal class UpdateAction<TEntity> : Action<IMeta>
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Update;

        [JsonConverter(typeof(JsonPartialEntityConverter))]
        public IPartiallySerializableEntity Payload { get; protected set; }

        public UpdateAction(TEntity entity)
        {
            Payload = PartiallySerializableEntityFactory.Create(entity);
            Meta = null;
        }
    }
}
