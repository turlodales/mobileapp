using Newtonsoft.Json;
using System;

namespace Toggl.Networking.Sync.Push
{
    public class Action<TMeta> : IAction
        where TMeta : IMeta
    {
        public virtual ActionType Type { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TMeta Meta { get; protected set; }
    }
}
