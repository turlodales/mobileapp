using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.Extensions;
using Toggl.Core.Models;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Settings;

namespace Toggl.Core.Interactors
{
    public sealed class SyncExternalCalendarsInteractor : IInteractor<Task<SyncOutcome>>
    {
        private readonly IInteractorFactory interactorFactory;
        private readonly ITimeService timeService;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;

        public SyncExternalCalendarsInteractor(IInteractorFactory interactorFactory, ITimeService timeService, ILastTimeUsageStorage lastTimeUsageStorage)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            this.interactorFactory = interactorFactory;
            this.timeService = timeService;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
        }

        public async Task<SyncOutcome> Execute()
        {
            if (!shouldExecute())
                return SyncOutcome.NoData;

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
            lastTimeUsageStorage.SetLastTimeExternalCalendarsSynced(timeService.Now());
            return calendarData.Any() ? SyncOutcome.NewData : SyncOutcome.NoData;
        }

        private bool shouldExecute()
        {
            var now = timeService.Now();
            var lastSynced = lastTimeUsageStorage.LastTimeExternalCalendarsSynced;
            return lastSynced?.Date != now.Date;
        }
    }
}
