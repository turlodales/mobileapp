using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Helpers;
using Toggl.Networking.Models;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.ApiClients
{
    internal sealed class AuthApi : IAuthApi
    {
        private readonly AuthEndpoints endpoints;
        private readonly IApiClient apiClient;
        private readonly IJsonSerializer serializer;

        public AuthApi(Endpoints endpoints, IApiClient apiClient, IJsonSerializer serializer)
        {
            this.endpoints = endpoints.Auth;
            this.apiClient = apiClient;
            this.serializer = serializer;
        }

        public async Task<ISamlConfig> GetSamlConfig(Email email)
        {
            var endpoint = endpoints.Get;
            var url = new UriBuilder(endpoint.Url);
            url.Query = $"email={Uri.EscapeDataString(email.ToString())}&client=mobile";

            var request = new Request("", url.Uri, Enumerable.Empty<HttpHeader>(), endpoint.Method);
            var response = await apiClient.Send(request).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                return serializer.Deserialize<SamlConfig>(response.RawData);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (response.RawData.ToUpper().Contains("SSO"))
                {
                    throw new SamlNotConfiguredException(request, response);
                }
            }

            throw ApiExceptions.For(request, response);
        }
    }
}