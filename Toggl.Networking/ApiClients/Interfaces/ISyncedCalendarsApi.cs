using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Networking.ApiClients.Interfaces
{
    public interface ISyncedCalendarsApi
    {
        Task<List<ICalendarIntegration>> GetIntegrations();

        Task<List<ISyncedCalendar>> GetSyncedCalendars(
            long integrationId,
            string nextPageToken = null,
            long? limit = null);

        Task<List<ISyncedCalendarEvent>> GetSyncedCalendarEvents(
            long integrationId,
            string calendarId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string nextPageToken = null,
            long? limit = null);
    }
}
