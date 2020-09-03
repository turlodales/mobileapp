using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Core.Interactors
{
    public sealed class PullExternalCalendarsInteractor : IInteractor<Task<IEnumerable<IExternalCalendar>>>
    {
        private const int pageSizeLimit = 100;
        private readonly ITogglApi api;
        private readonly ICalendarIntegration integration;

        public PullExternalCalendarsInteractor(ITogglApi api, ICalendarIntegration integration)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(integration, nameof(integration));
            this.api = api;
            this.integration = integration;
        }

        public async Task<IEnumerable<IExternalCalendar>> Execute()
        {
            var calendars = new List<IExternalCalendar>();
            string nextPageToken = null;
            do
            {
                var page = await api.ExternalCalendars.GetCalendars(integration.Id, nextPageToken, pageSizeLimit);
                calendars.AddRange(page.Calendars);
                nextPageToken = page.NextPageToken;
            } while (nextPageToken != null);

            return calendars;
        }
    }
}
