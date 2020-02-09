using System.Net.Http;
using Toggl.Core.Extensions;
using Toggl.Networking;
using Toggl.Networking.Network;
using Toggl.Shared;

namespace Toggl.Core.Login
{
    public sealed class ApiFactory : IApiFactory
    {
        public UserAgent UserAgent { get; }

        public ApiEnvironment Environment { get; }

        private readonly HttpClient httpClient;

        public ApiFactory(
            ApiEnvironment apiEnvironment,
            UserAgent userAgent,
            HttpClient httpClient)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(httpClient, nameof(httpClient));

            UserAgent = userAgent;
            Environment = apiEnvironment;

            this.httpClient = httpClient;
        }

        public ITogglApi CreateApiWith(Credentials credentials, ITimeService timeService)
        {
            var configuration = new ApiConfiguration(Environment, credentials, UserAgent);
            return TogglApiFactory.With(configuration, httpClient, timeService.Now);
        }
    }
}
