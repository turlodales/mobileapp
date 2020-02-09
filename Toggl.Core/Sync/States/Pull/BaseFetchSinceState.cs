using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage;
using static Toggl.Core.Helper.Constants;

namespace Toggl.Core.Sync.States.Pull
{
    public abstract class BaseFetchSinceState : ISyncState
    {
        private readonly ISinceParameterRepository since;
        private readonly ITimeService timeService;

        public StateResult<IFetchObservables> Done { get; } = new StateResult<IFetchObservables>();

        protected ITogglApi Api { get; }

        public BaseFetchSinceState(
            ITogglApi api,
            ISinceParameterRepository since,
            ITimeService timeService)
        {
            this.since = since;
            this.timeService = timeService;

            Api = api;
        }

        public IObservable<ITransition> Start()
            => Observable.Create<ITransition>(observer =>
            {
                var observables = Fetch();

                observer.CompleteWith(Done.Transition(observables));
                return () => { };
            });

        protected abstract IFetchObservables Fetch();

        protected Task<List<ITimeEntry>> FetchTwoMonthsOfTimeEntries()
            => Api.TimeEntries.GetAll(
                start: timeService.CurrentDateTime.AddMonths(-FetchTimeEntriesForMonths),
                end: timeService.CurrentDateTime.AddDays(TimeEntriesEndDateInclusiveExtraDaysCount));

        protected IObservable<List<T>> FetchRecentIfPossible<T>(
            Func<DateTimeOffset, Task<List<T>>> getAllSince,
            Func<Task<List<T>>> getAll)
            where T : ILastChangedDatable
        {
            var threshold = since.Get<T>();
            var task = threshold.HasValue && IsWithinLimit(threshold.Value)
                ? getAllSince(threshold.Value)
                : getAll();
            return task.ToObservable();
        }

        protected bool IsWithinLimit(DateTimeOffset threshold)
            => threshold > timeService.CurrentDateTime.AddMonths(-SinceDateLimitMonths);
    }
}
