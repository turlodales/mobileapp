using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Core.Tests.Interactors.ExternalCalendars
{
    public class PullExternalCalendarEventsInteractorTests
    {
        public abstract class PullExternalCalendarEventsInteractorBaseTest : BaseInteractorTests
        {
            protected ICalendarIntegration Integration = new CalendarIntegration
            {
                Id = 42,
            };

            protected IExternalCalendar Calendar = new ExternalCalendar
            {
                SyncId = "Calendar-0",
                Name = "Memes",
            };

            public PullExternalCalendarEventsInteractorBaseTest()
            {
                TimeService.Now().Returns(new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero));
            }
        }

        public sealed class TheConstructor : PullExternalCalendarEventsInteractorBaseTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useApi, bool useTimeService, bool hasIntegration,
                bool hasCalendar)
            {
                var theApi = useApi ? Api : null;
                var theTimeService = useTimeService ? TimeService : null;
                var theIntegration = hasIntegration ? Integration : null;
                var theCalendar = hasCalendar ? Calendar : null;

                Action tryingToConstructWithNull = () =>
                    new PullExternalCalendarEventsInteractor(theApi, theTimeService, theIntegration, theCalendar);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : PullExternalCalendarEventsInteractorBaseTest
        {
            public sealed class WhenItHasASinglePage : PullExternalCalendarEventsInteractorBaseTest
            {
                private IExternalCalendarEventsPage page = new ExternalCalendarEventsPage
                {
                    Events = new List<IExternalCalendarEvent>
                    {
                        new ExternalCalendarEvent
                        {
                            SyncId = "0",
                            ICalId = "0",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                        new ExternalCalendarEvent
                        {
                            SyncId = "1",
                            ICalId = "1",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                    },
                    NextPageToken = null,
                };

                [Fact, LogIfTooSlow]
                public async Task ItReturnsTheExpectedEvents()
                {
                    Api.ExternalCalendars.GetCalendarEvents(
                        Integration.Id,
                        Calendar.SyncId,
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<string>(), Arg.Any<long?>()).Returns(page);

                    var interactor = new PullExternalCalendarEventsInteractor(Api, TimeService, Integration, Calendar);

                    var actual = await interactor.Execute();
                    actual.Should().BeEquivalentTo(page.Events);
                }
            }

            public sealed class WhenItHasMultiplePages : PullExternalCalendarEventsInteractorBaseTest
            {
                private IExternalCalendarEventsPage firstPage = new ExternalCalendarEventsPage
                {
                    Events = new List<IExternalCalendarEvent>
                    {
                        new ExternalCalendarEvent
                        {
                            SyncId = "0",
                            ICalId = "0",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                        new ExternalCalendarEvent
                        {
                            SyncId = "1",
                            ICalId = "1",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                    },
                    NextPageToken = "next_page_token",
                };

                private IExternalCalendarEventsPage secondPage = new ExternalCalendarEventsPage
                {
                    Events = new List<IExternalCalendarEvent>
                    {
                        new ExternalCalendarEvent
                        {
                            SyncId = "3",
                            ICalId = "0",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                    },
                    NextPageToken = null,
                };

                [Fact, LogIfTooSlow]
                public async Task ItReturnsTheExpectedEvents()
                {
                    Api.ExternalCalendars.GetCalendarEvents(
                        Integration.Id,
                        Calendar.SyncId,
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<DateTimeOffset>(),
                        null, Arg.Any<long?>()).Returns(firstPage);

                    Api.ExternalCalendars.GetCalendarEvents(
                        Integration.Id,
                        Calendar.SyncId,
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<DateTimeOffset>(),
                        "next_page_token", Arg.Any<long?>()).Returns(secondPage);

                    var interactor = new PullExternalCalendarEventsInteractor(Api, TimeService, Integration, Calendar);

                    var actual = await interactor.Execute();
                    var expected = firstPage.Events.Concat(secondPage.Events);
                    actual.Should().BeEquivalentTo(expected);
                }
            }

            public sealed class WhenItFailsImmediately : PullExternalCalendarEventsInteractorBaseTest
            {
                [Fact, LogIfTooSlow]
                public async Task ItThrows()
                {
                    Api.ExternalCalendars.GetCalendarEvents(
                            Integration.Id,
                            Calendar.SyncId,
                            Arg.Any<DateTimeOffset>(),
                            Arg.Any<DateTimeOffset>(),
                            null, Arg.Any<long?>())
                        .ReturnsThrowingTaskOf(new Exception("Something bad happened"));
                    var interactor = new PullExternalCalendarEventsInteractor(Api, TimeService, Integration, Calendar);

                    Action tryingToExecute = () =>
                        interactor.Execute().Wait();

                    tryingToExecute.Should().Throw<Exception>();
                }
            }

            public sealed class WhenItFailsAfterSomePageWasFetched : PullExternalCalendarEventsInteractorBaseTest
            {
                private IExternalCalendarEventsPage page = new ExternalCalendarEventsPage
                {
                    Events = new List<IExternalCalendarEvent>
                    {
                        new ExternalCalendarEvent
                        {
                            SyncId = "0",
                            ICalId = "0",
                            Title = "Memes Meeting",
                            StartTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            EndTime = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            Updated = new DateTimeOffset(2020, 7, 24, 15, 46, 0, TimeSpan.Zero),
                            BackgroundColor = "#ffffff",
                            ForegroundColor = "#000000",
                        },
                    },
                    NextPageToken = "next_page_token",
                };

                [Fact, LogIfTooSlow]
                public async Task ItThrows()
                {
                    Api.ExternalCalendars.GetCalendarEvents(
                        Integration.Id,
                        Calendar.SyncId,
                        Arg.Any<DateTimeOffset>(),
                        Arg.Any<DateTimeOffset>(),
                        null, Arg.Any<long?>()).Returns(page);

                    Api.ExternalCalendars.GetCalendarEvents(
                            Integration.Id,
                            Calendar.SyncId,
                            Arg.Any<DateTimeOffset>(),
                            Arg.Any<DateTimeOffset>(),
                            null, Arg.Any<long?>())
                        .ReturnsThrowingTaskOf(new Exception("Something bad happened"));
                    var interactor = new PullExternalCalendarEventsInteractor(Api, TimeService, Integration, Calendar);

                    Action tryingToExecute = () =>
                        interactor.Execute().Wait();

                    tryingToExecute.Should().Throw<Exception>();
                }
            }
        }
    }
}
