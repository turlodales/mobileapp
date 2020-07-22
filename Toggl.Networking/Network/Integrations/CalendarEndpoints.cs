using System;
using System.Web;

namespace Toggl.Networking.Network
{
    internal struct CalendarEndpoints
    {
        private readonly Uri baseUrl;

        public CalendarEndpoints(Uri baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public Endpoint GetIntegrations => Endpoint.Get(baseUrl, "/calendar");

        public Endpoint GetAllCalendars(long integrationId, string nextPageToken, long? limit)
        {
            var path = $"/calendar/{integrationId}/calendars";

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["next_page_token"] = nextPageToken;
            queryParams["limit"] = limit.ToString();
            path += $"?{queryParams}";

            return Endpoint.Get(baseUrl, path);
        }

        public Endpoint GetAllCalendarEvents(
            long integrationId,
            string calendarId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string nextPageToken,
            long? limit)
        {
            var path = $"/calendar/{integrationId}/calendars/{calendarId}/events?start_date=start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["next_page_token"] = nextPageToken;
            queryParams["limit"] = limit.ToString();
            path += $"?{queryParams}";

            return Endpoint.Get(baseUrl, path);
        }
    }
}
