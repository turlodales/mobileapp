using System;
using System.Net;
using System.Net.Http;
using Foundation;
using Toggl.Networking;
using Toggl.Networking.Network;

namespace Toggl.iOS.Shared
{
    public class APIHelper
    {
#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        public static ITogglApi GetTogglAPI()
        {
            var apiToken = SharedStorage.Instance.GetApiToken();
            if (apiToken == null)
            {
                return null;
            }

            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            var userAgent = new UserAgent("Daneel", $"{version} SiriExtension");
            var apiConfiguration = new ApiConfiguration(environment, Credentials.WithApiToken(apiToken), userAgent);

            var httpHandler = new NSUrlSessionHandler(NSUrlSessionConfiguration.DefaultSessionConfiguration);
            var httpClient = new HttpClient(httpHandler, true);
            return TogglApiFactory.With(apiConfiguration, httpClient, () => DateTimeOffset.UtcNow);
        }
    }
}
