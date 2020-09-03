using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Tests.Generators;
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
            // We are disabling the external calendars sync for the rebranding release
            // since it's still untested
            public sealed class NeverExecutes : SyncExternalCalendarsInteractorTestBase
            {
                public NeverExecutes()
                {
                    LastSynced = new DateTimeOffset(2020, 7, 27, 0, 22, 0, TimeSpan.Zero);
                    Now = new DateTimeOffset(2020, 7, 30, 15, 22, 0, TimeSpan.Zero);
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
        }
    }
}
