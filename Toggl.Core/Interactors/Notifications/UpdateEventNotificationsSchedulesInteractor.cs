using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Storage.Settings;

namespace Toggl.Core.Interactors.Notifications
{
    public sealed class UpdateEventNotificationsSchedulesInteractor : IInteractor<Task>
    {
        private readonly IUserPreferences userPreferences;
        private readonly IInteractorFactory interactorFactory;

        public UpdateEventNotificationsSchedulesInteractor(
            IUserPreferences userPreferences,
            IInteractorFactory interactorFactory)
        {
            this.userPreferences = userPreferences;
            this.interactorFactory = interactorFactory;
        }

        public async Task Execute()
        {
            await interactorFactory.UnscheduleAllNotifications().Execute();
            var notificationsAreEnabled = await userPreferences.CalendarNotificationsEnabled.FirstAsync();
            var enabledCalendars = await userPreferences.EnabledCalendars.FirstOrDefaultAsync() ?? new List<string>();
            if (notificationsAreEnabled && enabledCalendars.Count > 0)
            {
                await interactorFactory.ScheduleEventNotificationsForNextWeek().Execute();
            }
        }
    }
}