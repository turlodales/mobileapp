using System;
using System.Collections.Generic;
using System.Reactive;
using FluentAssertions;
using FluentAssertions.Common;
using NSubstitute;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models.Calendar;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Tests.Interactors.ExternalCalendars
{
    public sealed class SyncExternalCalendarsInteractorTests
    {
        public abstract class SyncExternalCalendarsInteractorTestBase : BaseInteractorTests
        {
            public DateTimeOffset LastSynced = new DateTimeOffset(2020, 7, 26, 15, 22, 0, TimeSpan.Zero);
            public DateTimeOffset Now = new DateTimeOffset(2020, 7, 27, 15, 22, 0, TimeSpan.Zero);
            public void Prepare()
            {
                TimeService.Now().Returns(Now);
                LastTimeUsageStorage.LastTimeExternalCalendarsSynced.Returns(LastSynced);
            }
        }

        public sealed class TheConstructor : SyncExternalCalendarsInteractorTestBase
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useInteractorFactory, bool useTimeService, bool useLastTimeUsageStorage)
            {
                var theInteractorFactory = useInteractorFactory ? InteractorFactory : null;
                var theTimeService = useTimeService ? TimeService : null;
                var theLastTimeUsageStorage = useLastTimeUsageStorage ? LastTimeUsageStorage : null;

                Action tryingToConstructWithNull = () =>
                    new SyncExternalCalendarsInteractor(theInteractorFactory, theTimeService, theLastTimeUsageStorage);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : SyncExternalCalendarsInteractorTestBase
        {
            public sealed class WhenItShouldNotExecute : SyncExternalCalendarsInteractorTestBase
            {
                public WhenItShouldNotExecute()
                {
                    LastSynced = new DateTimeOffset(2020, 7, 27, 0, 22, 0, TimeSpan.Zero);
                    Now = new DateTimeOffset(2020, 7, 27, 15, 22, 0, TimeSpan.Zero);
                }

                [Fact, LogIfTooSlow]
                public async Task ItDoesNotExecute()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    var outcome = await interactor.Execute();

                    outcome.Should().Be(SyncOutcome.NoData);

                    await interactorFactory
                        .PullCalendarIntegrations()
                        .DidNotReceive()
                        .Execute();

                    await interactorFactory
                        .PullExternalCalendars(Arg.Any<ICalendarIntegration>())
                        .DidNotReceive()
                        .Execute();

                    await interactorFactory
                        .PullExternalCalendarEvents(Arg.Any<ICalendarIntegration>(), Arg.Any<IExternalCalendar>())
                        .DidNotReceive()
                        .Execute();

                    LastTimeUsageStorage
                        .DidNotReceive()
                        .SetLastTimeExternalCalendarsSynced(Arg.Any<DateTimeOffset>());
            }
        }

            public sealed class WhenNewDataIsAvailable : SyncExternalCalendarsInteractorTestBase
            {
                #region MockData

                private List<ICalendarIntegration> integrations = new List<ICalendarIntegration>
                {
                    new CalendarIntegration
                    {
                        Id = 42,
                    },
                    new CalendarIntegration
                    {
                        Id = 1337,
                    },
                };

                private List<IExternalCalendar> calendars_0 = new List<IExternalCalendar>
                {
                    new ExternalCalendar
                    {
                        SyncId = "42-0",
                        Name = "Memes",
                    },
                    new ExternalCalendar
                    {
                        SyncId = "42-1",
                        Name = "Work",
                    },
                };

                private List<IExternalCalendar> calendars_1 = new List<IExternalCalendar>
                {
                    new ExternalCalendar
                    {
                        SyncId = "1337-0",
                        Name = "Potato",
                    },
                    new ExternalCalendar
                    {
                        SyncId = "1337-1",
                        Name = "Personal",
                    },
                };

                private List<IExternalCalendarEvent> events_0_0 = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "42-1-0",
                        ICalId = "42-1-0",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                    new ExternalCalendarEvent
                    {
                        SyncId = "42-1-1",
                        ICalId = "42-1-1",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    }
                };

                private List<IExternalCalendarEvent> events_0_1 = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "42-2-0",
                        ICalId = "42-2-0",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                    new ExternalCalendarEvent
                    {
                        SyncId = "42-2-1",
                        ICalId = "42-2-1",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    }
                };

                private List<IExternalCalendarEvent> events_1_0 = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "1337-1-0",
                        ICalId = "1337-1-0",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                    new ExternalCalendarEvent
                    {
                        SyncId = "1337-1-1",
                        ICalId = "1337-1-1",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    }
                };

                private List<IExternalCalendarEvent> events_1_1 = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "1337-2-0",
                        ICalId = "1337-2-0",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                    new ExternalCalendarEvent
                    {
                        SyncId = "1337-2-1",
                        ICalId = "1337-2-1",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    }
                };

                #endregion

                [Fact, LogIfTooSlow]
                public async Task ItPersistsThePulledData()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();
                    interactorFactory.PullCalendarIntegrations().Execute().Returns(integrations);
                    interactorFactory.PullExternalCalendars(Arg.Is(integrations[0])).Execute().Returns(calendars_0);
                    interactorFactory.PullExternalCalendars(Arg.Is(integrations[1])).Execute().Returns(calendars_1);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[0]), Arg.Is(calendars_0[0])).Execute().Returns(events_0_0);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[0]), Arg.Is(calendars_0[1])).Execute().Returns(events_0_1);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[1]), Arg.Is(calendars_1[0])).Execute().Returns(events_1_0);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[1]), Arg.Is(calendars_1[1])).Execute().Returns(events_1_1);

                    interactorFactory
                        .PersistExternalCalendarsData(
                            Arg.Any<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>()).Execute()
                        .Returns(Unit.Default);

                    var expected = new Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>
                    {
                        { calendars_0[0], events_0_0 },
                        { calendars_0[1], events_0_1 },
                        { calendars_1[0], events_1_0 },
                        { calendars_1[1], events_1_1 },
                    };

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    var outcome = await interactor.Execute();

                    outcome.Should().Be(SyncOutcome.NewData);
                    interactorFactory
                        .Received()
                        .PersistExternalCalendarsData(Arg.Is<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>(
                            actual => actual.Count == expected.Count &&
                                      actual[calendars_0[0]].IsSameOrEqualTo(expected[calendars_0[0]]) &&
                                      actual[calendars_0[1]].IsSameOrEqualTo(expected[calendars_0[1]]) &&
                                      actual[calendars_1[0]].IsSameOrEqualTo(expected[calendars_1[0]]) &&
                                      actual[calendars_1[1]].IsSameOrEqualTo(expected[calendars_1[1]])
                        )).Execute();
                }

                [Fact, LogIfTooSlow]
                public async Task UpdatesTheLastTimeSinceStorage()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();
                    interactorFactory.PullCalendarIntegrations().Execute().Returns(integrations);
                    interactorFactory.PullExternalCalendars(Arg.Is(integrations[0])).Execute().Returns(calendars_0);
                    interactorFactory.PullExternalCalendars(Arg.Is(integrations[1])).Execute().Returns(calendars_1);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[0]), Arg.Is(calendars_0[0])).Execute().Returns(events_0_0);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[0]), Arg.Is(calendars_0[1])).Execute().Returns(events_0_1);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[1]), Arg.Is(calendars_1[0])).Execute().Returns(events_1_0);
                    interactorFactory.PullExternalCalendarEvents(Arg.Is(integrations[1]), Arg.Is(calendars_1[1])).Execute().Returns(events_1_1);

                    interactorFactory
                        .PersistExternalCalendarsData(
                            Arg.Any<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>()).Execute()
                        .Returns(Unit.Default);

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    await interactor.Execute();

                    LastTimeUsageStorage.Received().SetLastTimeExternalCalendarsSynced(Arg.Is(Now));
                }
            }

            public sealed class WhenNoDataIsAvailable : SyncExternalCalendarsInteractorTestBase
            {
                #region MockData

                private List<ICalendarIntegration> integrations = new List<ICalendarIntegration>();
                private List<IExternalCalendar> calendars = new List<IExternalCalendar>();
                private List<IExternalCalendarEvent> events = new List<IExternalCalendarEvent>();

                #endregion

                [Fact, LogIfTooSlow]
                public async Task ItClearsTheDatabase()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();
                    interactorFactory.PullCalendarIntegrations().Execute().Returns(integrations);
                    interactorFactory.PullExternalCalendars(Arg.Any<ICalendarIntegration>()).Execute().Returns(calendars);
                    interactorFactory.PullExternalCalendarEvents(Arg.Any<ICalendarIntegration>(), Arg.Any<IExternalCalendar>()).Execute().Returns(events);

                    interactorFactory
                        .PersistExternalCalendarsData(
                            Arg.Any<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>()).Execute()
                        .Returns(Unit.Default);

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    var outcome = await interactor.Execute();

                    outcome.Should().Be(SyncOutcome.NoData);
                    interactorFactory
                        .Received()
                        .PersistExternalCalendarsData(Arg.Is<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>(
                            actual => actual.None()
                        )).Execute();
                }
            }

            public sealed class WhenTheApiFails : SyncExternalCalendarsInteractorTestBase
            {
                #region MockData

                private List<ICalendarIntegration> integrations = new List<ICalendarIntegration>
                {
                    new CalendarIntegration
                    {
                        Id = 42,
                    },
                };

                private List<IExternalCalendar> calendars = new List<IExternalCalendar>
                {
                    new ExternalCalendar
                    {
                        SyncId = "42-0",
                        Name = "Memes",
                    },
                };

                private List<IExternalCalendarEvent> events = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "42-1",
                        ICalId = "42-1",
                        Title = "Title",
                        StartTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 24, 12, 12, 12, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                };

                #endregion

                [Fact, LogIfTooSlow]
                public async Task ItDoesNotTouchTheDatabase()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();
                    interactorFactory.PullCalendarIntegrations().Execute().Returns(integrations);
                    interactorFactory.PullExternalCalendars(Arg.Any<ICalendarIntegration>()).Execute().Returns(calendars);
                    interactorFactory.PullExternalCalendarEvents(Arg.Any<ICalendarIntegration>(), Arg.Any<IExternalCalendar>())
                        .Execute()
                        .ReturnsThrowingTaskOf(new Exception("Something failed"));

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    var outcome = await interactor.Execute();

                    outcome.Should().Be(SyncOutcome.Failed);

                    interactorFactory
                        .DidNotReceive()
                        .PersistExternalCalendarsData(Arg.Any<Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>>()).Execute();
                }

                [Fact, LogIfTooSlow]
                public async Task ItDoesNotTouchTheLastTimeUsageStorage()
                {
                    Prepare();

                    var interactorFactory = Substitute.For<IInteractorFactory>();
                    interactorFactory.PullCalendarIntegrations().Execute().Returns(integrations);
                    interactorFactory.PullExternalCalendars(Arg.Any<ICalendarIntegration>()).Execute().Returns(calendars);
                    interactorFactory.PullExternalCalendarEvents(Arg.Any<ICalendarIntegration>(), Arg.Any<IExternalCalendar>())
                        .Execute()
                        .ReturnsThrowingTaskOf(new Exception("Something failed"));

                    var interactor = new SyncExternalCalendarsInteractor(interactorFactory, TimeService, LastTimeUsageStorage);
                    await interactor.Execute();

                    LastTimeUsageStorage
                        .DidNotReceive()
                        .SetLastTimeExternalCalendarsSynced(Arg.Any<DateTimeOffset>());
                }
            }
        }
    }
}
