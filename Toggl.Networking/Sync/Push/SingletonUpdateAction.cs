using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Toggl.Networking.Sync.Push
{
    public class SingletonUpdateAction<TPayload> : Action<IMeta>
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Update;

        public TPayload Payload { get; protected set; }

        public SingletonUpdateAction(TPayload payload)
        {
            Payload = payload;

            Meta = null;
        }
    }
}
