using System;
using System.Reactive;
using System.Threading.Tasks;
using Toggl.Core.Interactors.Notifications;

namespace Toggl.Core.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<Unit>> UnscheduleAllNotifications()
            => new UnscheduleAllNotificationsInteractor(notificationService);

        public IInteractor<IObservable<Unit>> ScheduleEventNotificationsForNextWeek()
            => new ScheduleEventNotificationsInteractor(timeService, calendarService, userPreferences, notificationService);

        public IInteractor<Task> UpdateEventNotificationsSchedules()
            => new UpdateEventNotificationsSchedulesInteractor(userPreferences, this);
    }
}
