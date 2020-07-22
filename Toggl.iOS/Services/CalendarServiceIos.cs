using EventKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Core.Calendar;
using Toggl.Core.DataSources;
using Toggl.Core.Models.Calendar;
using Toggl.Core.UI.Services;
using Toggl.iOS.Extensions;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.iOS.Services
{
    public sealed class CalendarServiceIos : PermissionAwareCalendarService
    {
        private EKEventStore eventStore = new EKEventStore();

        public CalendarServiceIos(IPermissionsChecker permissionsChecker, ITogglDataSource dataSource)
            : base(permissionsChecker, dataSource)
        {
        }

        protected override IEnumerable<CalendarItem> NativeGetEventsInRange(DateTimeOffset start, DateTimeOffset end)
        {
            var calendars = eventStore.GetCalendars(EKEntityType.Event);

            var predicate = eventStore
                .PredicateForEvents(start.ToNSDate(), end.ToNSDate(), calendars);

            var calendarItems = eventStore
                .EventsMatching(predicate)
                ?.Where(isValidEvent)
                ?.Select(calendarItemFromEvent)
                ?? new CalendarItem[0];

            return calendarItems;
        }

        protected override IEnumerable<UserCalendar> NativeGetUserCalendars()
            => eventStore
                .GetCalendars(EKEntityType.Event)
                .Select(userCalendarFromEKCalendar);

        protected override CalendarItem NativeGetCalendarItemWithId(string id)
        {
            var calendarEvent = eventStore.EventFromIdentifier(id);
            if (calendarEvent == null)
                throw new InvalidOperationException("An invalid calendar Id was provided");

            var calendarItem = calendarItemFromEvent(calendarEvent);
            return calendarItem;
        }

        protected override IEnumerable<IThreadSafeExternalCalendarEvent> ResolveDuplicates(
            IEnumerable<CalendarItem> nativeEvents,
            IEnumerable<IThreadSafeExternalCalendarEvent> externalEvents)
            => externalEvents.Where((externalEvent) => nativeEvents.None((nativeEvent) => externalEvent.ICalId == nativeEvent.SyncId));

        private UserCalendar userCalendarFromEKCalendar(EKCalendar calendar)
            => new UserCalendar(
                calendar.CalendarIdentifier,
                calendar.Title,
                calendar.Source.Title);

        private CalendarItem calendarItemFromEvent(EKEvent ev)
        {
            var startDate = ev.StartDate.ToDateTimeOffset();
            var endDate = ev.EndDate.ToDateTimeOffset();
            var duration = endDate - startDate;

            return new CalendarItem(
                ev.EventIdentifier,
                ev.CalendarItemExternalIdentifier,
                CalendarItemSource.Calendar,
                startDate,
                duration,
                ev.Title,
                CalendarIconKind.Event,
                ev.Calendar.CGColor.ToHexColor(),
                calendarId: ev.Calendar.CalendarIdentifier
            );
        }

        private bool isValidEvent(EKEvent ev)
            => !ev.AllDay;
    }
}
