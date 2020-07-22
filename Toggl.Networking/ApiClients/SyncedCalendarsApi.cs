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
    internal class SyncedCalendarsApi : BaseApi, ISyncedCalendarsApi
    {
        private readonly CalendarEndpoints endpoints;

        public SyncedCalendarsApi(Endpoints endpoints, IApiClient apiClient, IJsonSerializer serializer, Credentials credidentials)
            : base(apiClient, serializer, credidentials, endpoints.LoggedIn)
        {
            this.endpoints = endpoints.IntegrationsEndpoints.Calendars;
        }

        public Task<List<ICalendarIntegration>> GetIntegrations()
            => SendRequest<CalendarIntegration, ICalendarIntegration>(endpoints.GetIntegrations, AuthHeader);

        public Task<List<ISyncedCalendar>> GetSyncedCalendars(
            long integrationId,
            string nextPageToken = null,
            long? limit = null)
            => SendRequest<SyncedCalendar, ISyncedCalendar>(endpoints.GetAllCalendars(integrationId, nextPageToken, limit), AuthHeader);

        public Task<List<ISyncedCalendarEvent>> GetSyncedCalendarEvents(
            long integrationId,
            string calendarId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string nextPageToken = null,
            long? limit = null)
            => SendRequest<SyncedCalendarEvent, ISyncedCalendarEvent>(endpoints.GetAllCalendarEvents(integrationId, calendarId, startDate, endDate, nextPageToken, limit), AuthHeader);
    }
}
