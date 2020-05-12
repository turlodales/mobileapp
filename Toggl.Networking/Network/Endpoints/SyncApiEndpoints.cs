using System;

namespace Toggl.Networking.Network
{
    internal struct SyncApiEndpoints
    {
        private readonly Uri baseUrl;

        public SyncApiEndpoints(Uri baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public Endpoint Pull(DateTimeOffset? since = null) =>
            since.HasValue
                ? Endpoint.Get(baseUrl, $"pull?since={since.Value.ToUnixTimeSeconds()}")
                : Endpoint.Get(baseUrl, "pull");

        public Endpoint Push(Guid id) => Endpoint.Post(baseUrl, $"push/{id}");

        public Endpoint OutstandingPush(Guid id) => Endpoint.Get(baseUrl, $"push/{id}");
    }
}
