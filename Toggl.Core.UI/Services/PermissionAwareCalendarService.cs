using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Core.DataSources;
using Toggl.Core.Models.Calendar;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Services;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.Calendar
{
    public abstract class PermissionAwareCalendarService : ICalendarService
    {
        private readonly IPermissionsChecker permissionsChecker;
        private readonly ITogglDataSource dataSource;

        protected PermissionAwareCalendarService(IPermissionsChecker permissionsChecker, ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(permissionsChecker, nameof(permissionsChecker));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.permissionsChecker = permissionsChecker;
            this.dataSource = dataSource;
        }

        public IObservable<IEnumerable<CalendarItem>> GetEventsForDate(DateTime date)
        {
            var startOfDay = new DateTimeOffset(date.Date);
            var endOfDay = startOfDay.AddDays(1);

            return GetEventsInRange(startOfDay, endOfDay);
        }

        public IObservable<CalendarItem> GetEventWithId(string id)
        {
            if (id.StartsWith(CalendarItem.ExternalEventIdPrefix))
            {
                var syncId = id.Substring(CalendarItem.ExternalEventIdPrefix.Length);
                return dataSource.ExternalCalendarEvents
                    .GetAll((externalEvent) => externalEvent.SyncId == syncId)
                    .Select((events) => events.First())
                    .Select(CalendarItem.From);
            }

            return permissionsChecker
               .CalendarPermissionGranted
               .DeferAndThrowIfPermissionNotGranted(
                   () => Observable.Return(NativeGetCalendarItemWithId(id))
               );
        }

        public IObservable<IEnumerable<CalendarItem>> GetEventsInRange(DateTimeOffset start, DateTimeOffset end)
        {
            var nativeEvents = permissionsChecker.CalendarPermissionGranted
                           .DeferAndReturnDefaultIfPermissionNotGranted(
                               () => Observable.Return(NativeGetEventsInRange(start, end)),
                               () => Observable.Return(Enumerable.Empty<CalendarItem>()));

            var externalEvents = getExternalCalendarEventsInRange(start, end);

            return Observable.CombineLatest(nativeEvents, externalEvents, mergingNativeAndExternalEvents);
        }

        public IObservable<IEnumerable<UserCalendar>> GetUserCalendars()
            => permissionsChecker
                .CalendarPermissionGranted
                .DeferAndThrowIfPermissionNotGranted(
                    () => Observable.Return(NativeGetUserCalendars())
                );

        protected abstract CalendarItem NativeGetCalendarItemWithId(string id);

        protected abstract IEnumerable<UserCalendar> NativeGetUserCalendars();

        protected abstract IEnumerable<CalendarItem> NativeGetEventsInRange(DateTimeOffset start, DateTimeOffset end);

        protected abstract IEnumerable<IThreadSafeExternalCalendarEvent> ResolveDuplicates(IEnumerable<CalendarItem> nativeEvents, IEnumerable<IThreadSafeExternalCalendarEvent> externalEvents);

        private IObservable<IEnumerable<IThreadSafeExternalCalendarEvent>> getExternalCalendarEventsInRange(
            DateTimeOffset start, DateTimeOffset end)
            => dataSource
                .ExternalCalendarEvents.GetAll((calendarEvent) =>
                    calendarEvent.StartTime >= start && calendarEvent.EndTime <= end)
                .Select(events => events.ToList());

        private IEnumerable<CalendarItem> mergingNativeAndExternalEvents(IEnumerable<CalendarItem> nativeEvents, IEnumerable<IThreadSafeExternalCalendarEvent> externalEvents)
        {
            var conflictFreeExternalEvents = ResolveDuplicates(nativeEvents, externalEvents).Select(CalendarItem.From);
            return nativeEvents.Concat(conflictFreeExternalEvents);
        }
    }
}
