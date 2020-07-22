using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.ApiClients.Interfaces
{
    public interface IExternalCalendarsApi
    {
        Task<List<ICalendarIntegration>> GetIntegrations();

        Task<List<IExternalCalendar>> GetCalendars(
            long integrationId,
            string nextPageToken = null,
            long? limit = null);

        Task<List<IExternalCalendarEvent>> GetCalendarEvents(
            long integrationId,
            string calendarId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string nextPageToken = null,
            long? limit = null);
    }
}
