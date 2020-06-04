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

        public static Uri ForApi(ApiEnvironment environment)
            => forEnvironment(environment, apiPrefix);

        public static Uri ForReports(ApiEnvironment environment)
            => forEnvironment(environment, reportsPrefix);

        public static Uri ForSyncServer(ApiEnvironment environment)
            => new Uri(new Uri("https://http://toggl-sync-staging.appspot.com"), syncApiPrefix); // @todo: the final URL is not known at the moment

        private static Uri forEnvironment(ApiEnvironment environment, string prefix)
        {
            // switch (environment)
            // {
            //     case ApiEnvironment.Staging:
            //         return new Uri(stagingBaseUrl, prefix);
            //     case ApiEnvironment.Production:
            //         return new Uri(productionBaseUrl, prefix);
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(environment), environment, "Unknown api environment.");
            // }

            // @todo: this must be changed back as soon as possible and definitely before
            // this is rolled out to users in production...
            //
            // THIS MUST NOT BE MERGED INTO `DEVELOP`, it is intended just for testing purposes
            // on the `sync-team/sync` branch.
            return new Uri(stagingBaseUrl, prefix);
        }
    }
}
