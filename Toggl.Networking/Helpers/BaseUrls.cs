using System;

namespace Toggl.Networking.Helpers
{
    internal static class BaseUrls
    {
        private static Uri productionBaseUrl { get; }
            = new Uri("https://mobile.toggl.com");

        private static Uri stagingBaseUrl { get; }
            = new Uri("https://mobile.toggl.space");

        private const string apiPrefix = "/api/v9/";

        private const string reportsPrefix = "/reports/api/v3/";

        private const string syncApiPrefix = "/";

        private const string integrationsPrefix = "/integrations/api/v1/";

        public static Uri ForApi(ApiEnvironment environment)
            => forEnvironment(environment, apiPrefix);

        public static Uri ForReports(ApiEnvironment environment)
            => forEnvironment(environment, reportsPrefix);

        public static Uri ForSyncServer(ApiEnvironment environment)
            => selectByEnvironment(
                environment,
                staging: new Uri("https://sync.toggl.space"),
                production: new Uri("https://sync.toggl.com"));

        public static Uri ForIntegrations(ApiEnvironment environment)
            => forEnvironment(environment, integrationsPrefix);

        private static Uri forEnvironment(ApiEnvironment environment, string prefix)
            => selectByEnvironment(
                environment,
                staging: new Uri(stagingBaseUrl, prefix),
                production: new Uri(productionBaseUrl, prefix));

        private static Uri selectByEnvironment(ApiEnvironment environment, Uri staging, Uri production)
        {
            switch (environment)
            {
                case ApiEnvironment.Staging:
                    return staging;
                case ApiEnvironment.Production:
                    return production;
                default:
                    throw new ArgumentOutOfRangeException(nameof(environment), environment, "Unknown api environment.");
            }
        }
    }
}
