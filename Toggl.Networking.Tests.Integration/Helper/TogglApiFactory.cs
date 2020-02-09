using System;
using System.Net.Http;
using Toggl.Networking.Network;
using static System.Net.DecompressionMethods;

namespace Toggl.Networking.Tests.Integration.Helper
{
    internal static class TogglApiFactory
    {
        private static readonly LeakyBucket sharedBucket = new LeakyBucket(() => DateTimeOffset.Now, 200, 10);

        public static HttpClient CreateHttpClientForIntegrationTests()
        {
            var httpHandler = new HttpClientHandler { AutomaticDecompression = GZip | Deflate };
            return new HttpClient(httpHandler, true);
        }

        public static IApiClient CreateApiClientForIntegrationTests(UserAgent userAgent)
        {
            var apiClient = new ApiClient(CreateHttpClientForIntegrationTests(), userAgent);
            return new RetryingApiClient(new RateLimitingAwareApiClient(apiClient, sharedBucket));
        }

        public static ITogglApi TogglApiWith(Credentials credentials)
        {
            var apiConfiguration = configurationFor(credentials);
            var apiClient = CreateApiClientForIntegrationTests(apiConfiguration.UserAgent);
            return new TogglApi(apiConfiguration, apiClient);
        }

        private static ApiConfiguration configurationFor(Credentials credentials)
            => new ApiConfiguration(ApiEnvironment.Staging, credentials, Configuration.UserAgent);
    }
}
