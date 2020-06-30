using System;
using System.Net.Http;
using Toggl.Networking.ApiClients;
using Toggl.Networking.ApiClients.Interfaces;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;
using Toggl.Shared;

namespace Toggl.Networking
{
    internal sealed class UnauthenticatedTogglApi : IUnauthenticatedTogglApi
    {
        public UnauthenticatedTogglApi(ApiConfiguration configuration, IApiClient apiClient)
        {
            Ensure.Argument.IsNotNull(configuration, nameof(configuration));

            var serializer = new JsonSerializer();
            var endpoints = new Endpoints(configuration.Environment);

            Auth = new AuthApi(endpoints, apiClient, serializer);
        }

        public IAuthApi Auth { get; }
    }

    public static class UnauthenticatedTogglApiFactory
    {
        public static IUnauthenticatedTogglApi With(ApiConfiguration configuration, HttpClient httpClient)
        {
            var apiClient = new ApiClient(httpClient, configuration.UserAgent);
            return new UnauthenticatedTogglApi(configuration, apiClient);
        }
    }
}
