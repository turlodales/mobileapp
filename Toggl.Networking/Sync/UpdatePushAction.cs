using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync
{
    public class UpdatePushAction<TPayload> : PushAction<IMeta>
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Update;

        public TPayload Payload { get; protected set; }

        public UpdatePushAction(TPayload payload)
        {
            Payload = payload;

            Meta = null;
        }
    }
}
