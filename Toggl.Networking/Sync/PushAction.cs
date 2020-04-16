using Newtonsoft.Json;
using System;

namespace Toggl.Networking.Sync
{
    public class PushAction<TMeta> : IPushAction
        where TMeta : IMeta
    {
        public virtual ActionType Type { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TMeta Meta { get; protected set; }
    }
}
