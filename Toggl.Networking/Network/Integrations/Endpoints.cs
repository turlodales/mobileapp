using System;
using Toggl.Networking.Helpers;

namespace Toggl.Networking.Network.Integrations
{
    internal sealed class Endpoints
    {
        private readonly Uri baseUrl;

        public CalendarEndpoints Calendars => new CalendarEndpoints(baseUrl);

        public Endpoints(ApiEnvironment environment)
        {
            baseUrl = BaseUrls.ForReports(environment);
        }
    }
}
