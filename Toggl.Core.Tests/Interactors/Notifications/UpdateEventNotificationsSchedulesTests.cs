using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Interactors.Notifications;
using Xunit;

namespace Toggl.Core.Tests.Interactors.Notifications
{
    public class UpdateEventNotificationsSchedulesTests
    {
        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly UpdateEventNotificationsSchedulesInteractor interactor;
            private readonly IInteractorFactory interactorFactory = Substitute.For<IInteractorFactory>();

            public TheExecuteMethod()
            {
                interactor = new UpdateEventNotificationsSchedulesInteractor(UserPreferences, interactorFactory);
            }

            [Fact, LogIfTooSlow]
            public async Task AlwaysUnscheduleExistingEvents()
            {
                await interactor.Execute();

                interactorFactory.Received().UnscheduleAllNotifications();
            }
            
            [Fact, LogIfTooSlow]
            public async Task ReschedulesExistingEventsWhenCalendarNotificationsAreEnabledAndThereAreLinkedCalendars()
            {
                UserPreferences.CalendarNotificationsEnabled.Returns(Observable.Return(true));
                UserPreferences.EnabledCalendars.Returns(Observable.Return(new List<string>() {"1", "2"}));
                
                await interactor.Execute();

                interactorFactory.Received().UnscheduleAllNotifications();
                interactorFactory.Received().ScheduleEventNotificationsForNextWeek();
            }
            
            [Fact, LogIfTooSlow]
            public async Task DoesNotReschedulesEventsWhenCalendarNotificationsAreNotEnabledAndThereAreLinkedCalendars()
            {
                UserPreferences.CalendarNotificationsEnabled.Returns(Observable.Return(false));
                UserPreferences.EnabledCalendars.Returns(Observable.Return(new List<string>() {"1", "2"}));
                
                await interactor.Execute();

                interactorFactory.Received().UnscheduleAllNotifications();
                interactorFactory.DidNotReceive().ScheduleEventNotificationsForNextWeek();
            }
            
            [Fact, LogIfTooSlow]
            public async Task DoesNotReschedulesEventsWhenCalendarNotificationsAreEnabledButThereAreNoLinkedCalendars()
            {
                UserPreferences.CalendarNotificationsEnabled.Returns(Observable.Return(true));
                UserPreferences.EnabledCalendars.Returns(Observable.Return(new List<string>()));
                
                await interactor.Execute();

                interactorFactory.Received().UnscheduleAllNotifications();
                interactorFactory.DidNotReceive().ScheduleEventNotificationsForNextWeek();
            }
            
            [Fact, LogIfTooSlow]
            public async Task DoesNotReschedulesEventsWhenCalendarNotificationsAreNotEnabledAndThereAreNoLinkedCalendars()
            {
                UserPreferences.CalendarNotificationsEnabled.Returns(Observable.Return(false));
                UserPreferences.EnabledCalendars.Returns(Observable.Return(new List<string>()));
                
                await interactor.Execute();

                interactorFactory.Received().UnscheduleAllNotifications();
                interactorFactory.DidNotReceive().ScheduleEventNotificationsForNextWeek();
            }
        }
    }
}