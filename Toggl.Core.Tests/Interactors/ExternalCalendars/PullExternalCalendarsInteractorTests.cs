using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Core.Tests.Interactors.ExternalCalendars
{
    public sealed class PullExternalCalendarsInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useApi, bool hasIntegration)
            {
                var theApi = useApi ? Api : null;
                var theIntegration = hasIntegration ? Substitute.For<ICalendarIntegration>() : null;

                Action tryingToConstructWithNull = () =>
                    new PullExternalCalendarsInteractor(theApi, theIntegration);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            public sealed class WhenItHasASinglePage : BaseInteractorTests
            {
                private ICalendarIntegration integration = new CalendarIntegration
                {
                    Id = 42,
                };

                private IExternalCalendarsPage page = new ExternalCalendarsPage
                {
                    Calendars = new List<IExternalCalendar>
                    {
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-0",
                            Name = "Memes",
                        },
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-1",
                            Name = "Personal",
                        },
                    },
                    NextPageToken = null,
                };

                [Fact, LogIfTooSlow]
                public async Task ItReturnsTheExpectedCalendars()
                {
                    Api.ExternalCalendars.GetCalendars(integration.Id, Arg.Any<string>(), Arg.Any<long?>()).Returns(page);

                    var interactor = new PullExternalCalendarsInteractor(Api, integration);

                    var actual = await interactor.Execute();
                    actual.Should().BeEquivalentTo(page.Calendars);
                }
            }

            public sealed class WhenItHasMultiplePages : BaseInteractorTests
            {
                private ICalendarIntegration integration = new CalendarIntegration
                {
                    Id = 42,
                };

                private IExternalCalendarsPage firstPage = new ExternalCalendarsPage
                {
                    Calendars = new List<IExternalCalendar>
                    {
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-0",
                            Name = "Memes",
                        },
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-1",
                            Name = "Personal",
                        },
                    },
                    NextPageToken = "next_page_token",
                };

                private IExternalCalendarsPage secondPage = new ExternalCalendarsPage
                {
                    Calendars = new List<IExternalCalendar>
                    {
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-2",
                            Name = "Work",
                        },
                    },
                    NextPageToken = null,
                };

                [Fact, LogIfTooSlow]
                public async Task ItReturnsTheExpectedCalendars()
                {
                    Api.ExternalCalendars.GetCalendars(integration.Id, null, Arg.Any<long?>()).Returns(firstPage);
                    Api.ExternalCalendars.GetCalendars(integration.Id, "next_page_token", Arg.Any<long?>()).Returns(secondPage);

                    var interactor = new PullExternalCalendarsInteractor(Api, integration);
                    var actual = await interactor.Execute();
                    var expected = firstPage.Calendars.Concat(secondPage.Calendars);
                    actual.Should().BeEquivalentTo(expected);
                }
            }

            public sealed class WhenItFailsImmediately : BaseInteractorTests
            {
                private ICalendarIntegration integration = new CalendarIntegration
                {
                    Id = 42,
                };

                [Fact, LogIfTooSlow]
                public async Task ItThrows()
                {
                    Api.ExternalCalendars.GetCalendars(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long?>())
                        .ReturnsThrowingTaskOf(new Exception("Something bad happened"));
                    var interactor = new PullExternalCalendarsInteractor(Api, integration);

                    Action tryingToExecute = () =>
                        interactor.Execute().Wait();

                    tryingToExecute.Should().Throw<Exception>();
                }
            }

            public sealed class WhenItFailsAfterSomePageWasFetched : BaseInteractorTests
            {
                private ICalendarIntegration integration = new CalendarIntegration
                {
                    Id = 42,
                };

                private IExternalCalendarsPage page = new ExternalCalendarsPage
                {
                    Calendars = new List<IExternalCalendar>
                    {
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-0",
                            Name = "Memes",
                        },
                        new ExternalCalendar
                        {
                            SyncId = "Calendar-1",
                            Name = "Personal",
                        },
                    },
                    NextPageToken = "next_page_token",
                };

                [Fact, LogIfTooSlow]
                public async Task ItThrows()
                {
                    Api.ExternalCalendars.GetCalendars(integration.Id, null, Arg.Any<long?>()).Returns(page);
                    Api.ExternalCalendars.GetCalendars(integration.Id, "next_page_token", Arg.Any<long?>())
                        .ReturnsThrowingTaskOf(new Exception("Something bad happened"));
                    var interactor = new PullExternalCalendarsInteractor(Api, integration);

                    Action tryingToExecute = () =>
                        interactor.Execute().Wait();

                    tryingToExecute.Should().Throw<Exception>();
                }
            }
        }
    }
}
