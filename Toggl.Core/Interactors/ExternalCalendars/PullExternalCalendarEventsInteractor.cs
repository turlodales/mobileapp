using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Core.Extensions;
using Toggl.Networking;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Core.Interactors
{
    public sealed class PullExternalCalendarEventsInteractor : IInteractor<Task<IEnumerable<IExternalCalendarEvent>>>
    {
        private const int daysInThePast = 67;
        private const int daysInTheFuture = 7;
        private const int pageSizeLimit = 5000;

        private readonly ITogglApi api;
        private readonly ICalendarIntegration integration;
        private readonly IExternalCalendar calendar;
        private readonly DateTimeOffset startDate;
        private readonly DateTimeOffset endDate;

        public PullExternalCalendarEventsInteractor(ITogglApi api, ITimeService timeService, ICalendarIntegration integration, IExternalCalendar calendar)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(integration, nameof(integration));
            Ensure.Argument.IsNotNull(calendar, nameof(calendar));
            this.api = api;
            this.integration = integration;
            this.calendar = calendar;
            startDate = timeService.Now().AddDays(-daysInThePast);
            endDate = timeService.Now().AddDays(daysInTheFuture);
        }

        public async Task<IEnumerable<IExternalCalendarEvent>> Execute()
        {
            var events = new List<IExternalCalendarEvent>();
            string nextPageToken = null;
            do
            {
                var page = await api.ExternalCalendars.GetCalendarEvents(integration.Id, calendar.SyncId, startDate, endDate, nextPageToken, pageSizeLimit);
                events.AddRange(page.Events);
                nextPageToken = page.NextPageToken;
            } while (nextPageToken != null);

            return events;
        }
    }
}
