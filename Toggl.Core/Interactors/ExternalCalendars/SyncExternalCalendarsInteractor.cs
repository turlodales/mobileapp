using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.Models;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Core.Interactors
{
    public sealed class SyncExternalCalendarsInteractor : IInteractor<Task<SyncOutcome>>
    {
        private readonly IInteractorFactory interactorFactory;

        public SyncExternalCalendarsInteractor(IInteractorFactory interactorFactory)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            this.interactorFactory = interactorFactory;
        }

        public async Task<SyncOutcome> Execute()
        {
            var calendarData = new Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>();

            try
            {
                var integrations = await interactorFactory.PullCalendarIntegrations().Execute();

                foreach (var integration in integrations)
                {
                    var calendars = await interactorFactory.PullExternalCalendars(integration).Execute();
                    foreach (var calendar in calendars)
                    {
                        var events = await interactorFactory.PullExternalCalendarEvents(integration, calendar)
                            .Execute();
                        calendarData[calendar] = events;
                    }
                }
            }
            catch
            {
                return SyncOutcome.Failed;
            }

            interactorFactory.PersistExternalCalendarsData(calendarData).Execute();
            return calendarData.Any() ? SyncOutcome.NewData : SyncOutcome.NoData;
        }
    }
}
