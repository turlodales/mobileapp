using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Calendar;
using Toggl.Core.DataSources;
using Toggl.Core.Models.Calendar;
using Toggl.Core.UI.Services;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Models.Calendar;
using Xunit;

namespace Toggl.Core.Tests.UI.Services
{
    public sealed class PermissionAwareCalendarServiceTests
    {
        public sealed class TestCalendarService : PermissionAwareCalendarService
        {
            public IEnumerable<UserCalendar> NativeCalendars;
            public IEnumerable<CalendarItem> NativeCalendarEvents;

            public TestCalendarService(
                IPermissionsChecker permissionsChecker,
                ITogglDataSource dataSource,
                IEnumerable<UserCalendar> nativeCalendars,
                IEnumerable<CalendarItem> nativeCalendarEvents)
                : base(permissionsChecker, dataSource)
            {
                NativeCalendars = nativeCalendars;
                NativeCalendarEvents = nativeCalendarEvents;
            }

            protected override CalendarItem NativeGetCalendarItemWithId(string id)
                => NativeCalendarEvents.First((e) => e.Id == id);

            protected override IEnumerable<CalendarItem> NativeGetEventsInRange(DateTimeOffset start, DateTimeOffset end)
                => NativeCalendarEvents;

            protected override IEnumerable<UserCalendar> NativeGetUserCalendars()
                => NativeCalendars;

            protected override IEnumerable<IThreadSafeSyncedCalendarEvent> ResolveDuplicates(
                IEnumerable<CalendarItem> nativeEvents,
                IEnumerable<IThreadSafeSyncedCalendarEvent> syncedEvents)
                => syncedEvents.Where((syncedEvent) => nativeEvents.None((nativeEvent) => syncedEvent.SyncId == nativeEvent.SyncId));
        }

        public sealed class MergingCalendarItemsFromMultipleSources
        {
            private IEnumerable<UserCalendar> nativeCalendars;
            private IEnumerable<CalendarItem> nativeCalendarEvents;

            private IThreadSafeSyncedCalendar syncedCalendar;
            private IEnumerable<IThreadSafeSyncedCalendarEvent> syncedCalendarEvents;

            public MergingCalendarItemsFromMultipleSources()
            {
                nativeCalendars = new List<UserCalendar> { };

                nativeCalendarEvents = new List<CalendarItem>
                {
                    new CalendarItem(
                        "0",
                        "SyncId-0",
                        CalendarItemSource.Calendar,
                        new DateTimeOffset(2020, 7, 14, 9, 0, 0, TimeSpan.Zero),
                        TimeSpan.FromMinutes(30),
                        "Event 0",
                        CalendarIconKind.Event
                    ),
                    new CalendarItem(
                        "1",
                        "SyncId-1",
                        CalendarItemSource.Calendar,
                        new DateTimeOffset(2020, 7, 14, 10, 0, 0, TimeSpan.Zero),
                        TimeSpan.FromMinutes(30),
                        "Event 1",
                        CalendarIconKind.Event
                    ),
                };

                syncedCalendar = new SyncedCalendar(0, "0", "Calendar");

                syncedCalendarEvents = new List<IThreadSafeSyncedCalendarEvent>
                {
                    new SyncedCalendarEvent(
                        42,
                        "SyncId-1",
                        "ICalId-1",
                        "Event 1",
                        new DateTimeOffset(2020, 7, 14, 10, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2020, 7, 14, 10, 30, 0, TimeSpan.Zero),
                        new DateTimeOffset(2020, 7, 14, 10, 0, 0, TimeSpan.Zero),
                        "",
                        "",
                        0,
                        syncedCalendar
                    ),
                    new SyncedCalendarEvent(
                        1337,
                        "SyncId-2",
                        "ICalId-2",
                        "Event 2",
                        new DateTimeOffset(2020, 7, 14, 11, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2020, 7, 14, 11, 30, 0, TimeSpan.Zero),
                        new DateTimeOffset(2020, 7, 14, 11, 0, 0, TimeSpan.Zero),
                        "",
                        "",
                        0,
                        syncedCalendar
                    )
                };
            }

            [Fact]
            public async void RemovesDuplicateItems()
            {
                var permissionsChecker = Substitute.For<IPermissionsChecker>();
                permissionsChecker.CalendarPermissionGranted.Returns(Observable.Return(true));

                var dataSource = Substitute.For<ITogglDataSource>();
                dataSource.SyncedCalendarEvents.GetAll(Arg.Any<Func<IDatabaseSyncedCalendarEvent, bool>>()).Returns(Observable.Return(syncedCalendarEvents));

                var start = new DateTimeOffset(2020, 7, 14, 0, 0, 0, TimeSpan.Zero);
                var end = new DateTimeOffset(2020, 7, 15, 0, 0, 0, TimeSpan.Zero);

                var calendarService = new TestCalendarService(permissionsChecker, dataSource, nativeCalendars, nativeCalendarEvents);
                var calendarItems = (await calendarService.GetEventsInRange(start, end)).ToArray();

                calendarItems.Length.Should().Be(3);
                calendarItems[0].Id.Should().Be("0");
                calendarItems[0].SyncId.Should().Be("SyncId-0");
                calendarItems[1].Id.Should().Be("1");
                calendarItems[1].SyncId.Should().Be("SyncId-1");
                calendarItems[2].Id.Should().Be("SyncedEvent-1337");
                calendarItems[2].SyncId.Should().Be("SyncId-2");
            }

            [Fact]
            public async void GetCalendarEventById()
            {
                var permissionsChecker = Substitute.For<IPermissionsChecker>();
                permissionsChecker.CalendarPermissionGranted.Returns(Observable.Return(true));

                var dataSource = Substitute.For<ITogglDataSource>();
                dataSource.SyncedCalendarEvents.GetAll(Arg.Any<Func<IDatabaseSyncedCalendarEvent, bool>>()).Returns(Observable.Return(syncedCalendarEvents.Skip(1)));

                var calendarService = new TestCalendarService(permissionsChecker, dataSource, nativeCalendars, nativeCalendarEvents);

                var nativeCalendarItem = await calendarService.GetEventWithId("0");
                nativeCalendarItem.Id.Should().Be("0");
                nativeCalendarItem.SyncId.Should().Be("SyncId-0");

                var syncedCalendarItem = await calendarService.GetEventWithId("SyncedEvent-1337");
                syncedCalendarItem.Id.Should().Be("SyncedEvent-1337");
                syncedCalendarItem.SyncId.Should().Be("SyncId-2");
            }
        }
    }
}
