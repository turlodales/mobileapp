using System;
using System.Threading.Tasks;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;

namespace Toggl.Networking.ApiClients
{
    internal sealed class SyncApi : BaseApi, ISyncApi
    {
        private readonly SyncApiEndpoints endpoints;
        private readonly IApiClient apiClient;
        private readonly IJsonSerializer serializer;

        public SyncApi(Endpoints endpoints, IApiClient apiClient, IJsonSerializer serializer, Credentials credentials)
            : base(apiClient, serializer, credentials, endpoints.LoggedIn)
        {
            this.endpoints = endpoints.SyncServerEndpoints;
            this.apiClient = apiClient;
            this.serializer = serializer;
        }

        public async Task<Sync.Pull.IResponse> Pull(DateTimeOffset? since)
            => await SendRequest<Sync.Pull.Response>(endpoints.Pull(since), AuthHeader);

        public async Task<Sync.Push.IResponse> Push(Guid id, Sync.Push.Request request)
        {
            var body = serializer.Serialize(request, SerializationReason.Post);
            return await SendRequest<Sync.Push.Response>(endpoints.Push(id), AuthHeader, body);
        }

        public async Task<Sync.Push.IResponse> OutstandingPush(Guid id)
            => await SendRequest<Sync.Push.Response>(endpoints.OutstandingPush(id), AuthHeader);
    }
}
