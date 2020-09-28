using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Queries;

namespace Toggl.Core.Interactors
{
    public sealed class PersistExternalCalendarsDataInteractor : IInteractor<Unit>
    {
        private readonly IQueryFactory queryFactory;
        private readonly Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>> calendarData;

        public PersistExternalCalendarsDataInteractor(
            IQueryFactory queryFactory,
            Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>> calendarData)
        {
            Ensure.Argument.IsNotNull(queryFactory, nameof(queryFactory));
            Ensure.Argument.IsNotNull(calendarData, nameof(calendarData));
            this.queryFactory = queryFactory;
            this.calendarData = calendarData;
        }

        public Unit Execute()
            => queryFactory.PersistExternalCalendarsData(calendarData).Execute();
    }
}
