using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Queries;
using Xunit;

namespace Toggl.Core.Tests.Interactors.ExternalCalendars
{
    public sealed class PersistExternalCalendarsDataInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useQueryFactory, bool hasCalendarData)
            {
                IQueryFactory theQueryFactory = useQueryFactory ? QueryFactory : null;
                var theCalendarData = hasCalendarData ? new Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>() : null;

                Action tryingToConstructWithNull = () =>
                    new PersistExternalCalendarsDataInteractor(theQueryFactory, theCalendarData);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public void PersistsTheData()
            {
                var queryFactory = Substitute.For<IQueryFactory>();
                var calendarData = new Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>>();
                var interactor = new PersistExternalCalendarsDataInteractor(queryFactory, calendarData);
                var _ = interactor.Execute();
                queryFactory.Received(1).PersistExternalCalendarsData(Arg.Is(calendarData));
            }
        }
    }
}
