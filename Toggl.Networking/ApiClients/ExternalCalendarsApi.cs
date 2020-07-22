using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Networking.ApiClients.Interfaces;
using Toggl.Networking.Models.Calendar;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.ApiClients
{
    internal class ExternalCalendarsApi : BaseApi, IExternalCalendarsApi
    {
        private readonly CalendarEndpoints endpoints;

        public ExternalCalendarsApi(Endpoints endpoints, IApiClient apiClient, IJsonSerializer serializer, Credentials credidentials)
            : base(apiClient, serializer, credidentials, endpoints.LoggedIn)
        {
            this.endpoints = endpoints.IntegrationsEndpoints.Calendars;
        }

        public Task<List<ICalendarIntegration>> GetIntegrations()
            => SendRequest<CalendarIntegration, ICalendarIntegration>(endpoints.GetIntegrations, AuthHeader);

        public Task<List<IExternalCalendar>> GetCalendars(
            long integrationId,
            string nextPageToken = null,
            long? limit = null)
            => SendRequest<ExternalCalendar, IExternalCalendar>(endpoints.GetAllCalendars(integrationId, nextPageToken, limit), AuthHeader);

        public Task<List<IExternalCalendarEvent>> GetCalendarEvents(
            long integrationId,
            string calendarId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string nextPageToken = null,
            long? limit = null)
            => SendRequest<ExternalCalendarEvent, IExternalCalendarEvent>(endpoints.GetAllCalendarEvents(integrationId, calendarId, startDate, endDate, nextPageToken, limit), AuthHeader);
    }
}
