using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Core.Analytics;
using Toggl.Core.Extensions;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Sync.ConflictResolution;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage;
using Toggl.Storage.Models;

namespace Toggl.Core.DataSources
{
    internal sealed class TimeEntriesDataSource : ObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry>, ITimeEntriesSource
    {
        private readonly IAnalyticsService analyticsService;
        private readonly IRepository<IDatabaseTimeEntry> timeEntriesRepository;

        private readonly Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode> alwaysCreate
            = (a, b) => ConflictResolutionMode.Create;

        private readonly Subject<IThreadSafeTimeEntry> timeEntryStartedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> timeEntryStoppedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> suggestionStartedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> timeEntryContinuedSubject = new Subject<IThreadSafeTimeEntry>();

        public IObservable<IThreadSafeTimeEntry> TimeEntryStarted { get; }
        public IObservable<IThreadSafeTimeEntry> TimeEntryStopped { get; }
        public IObservable<IThreadSafeTimeEntry> SuggestionStarted { get; }
        public IObservable<IThreadSafeTimeEntry> TimeEntryContinued { get; }

        public IObservable<bool> IsEmpty { get; }

        private ISchedulerProvider schedulerProvider;

        public IObservable<IThreadSafeTimeEntry> CurrentlyRunningTimeEntry { get; }

        protected override IRivalsResolver<IDatabaseTimeEntry> RivalsResolver { get; }

        public TimeEntriesDataSource(
            IRepository<IDatabaseTimeEntry> timeEntriesRepository,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            ISchedulerProvider schedulerProvider)
            : base(timeEntriesRepository, schedulerProvider)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(timeEntriesRepository, nameof(timeEntriesRepository));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.schedulerProvider = schedulerProvider;

            this.timeEntriesRepository = timeEntriesRepository;
            this.analyticsService = analyticsService;

            RivalsResolver = new TimeEntryRivalsResolver(timeService);

            CurrentlyRunningTimeEntry =
                ItemsChanged
                    .StartWith(Unit.Default)
                    .SelectMany(_ => getCurrentlyRunningTimeEntry())
                    .ConnectedReplay();

            IsEmpty =
                ItemsChanged
                    .StartWith(Unit.Default)
                    .SelectMany(_ => GetAll(te => te.IsDeleted == false))
                    .Select(timeEntries => timeEntries.None());

            RivalsResolver = new TimeEntryRivalsResolver(timeService);

            TimeEntryStarted = timeEntryStartedSubject.AsObservable();
            TimeEntryStopped = timeEntryStoppedSubject.AsObservable();
            SuggestionStarted = suggestionStartedSubject.AsObservable();
            TimeEntryContinued = timeEntryContinuedSubject.AsObservable();
        }

        public override IObservable<IThreadSafeTimeEntry> Create(IThreadSafeTimeEntry entity)
            => timeEntriesRepository.UpdateWithConflictResolution(entity.Id, entity, alwaysCreate, RivalsResolver)
                .ToThreadSafeResult(Convert)
                .Flatten()
                .OfType<CreateResult<IThreadSafeTimeEntry>>()
                .FirstAsync()
                .Do(ReportChange)
                .Select(result => result.Entity);

        public void OnTimeEntryStopped(IThreadSafeTimeEntry timeEntry)
        {
            timeEntryStoppedSubject.OnNext(timeEntry);
        }

        public override IObservable<IThreadSafeTimeEntry> Update(IThreadSafeTimeEntry timeEntry)
            => timeEntriesRepository.GetById(timeEntry.Id)
                .Do(timeEntryDb => backupTimeEntry(timeEntryDb, timeEntry))
                .SelectMany(_ => base.Update(timeEntry));

        public override IObservable<IEnumerable<IConflictResolutionResult<IThreadSafeTimeEntry>>> BatchUpdate(IEnumerable<IThreadSafeTimeEntry> entities)
        {
            var timeEntries = entities.ToList();
            return backupLocal(timeEntries)
                .SingleAsync()
                .SelectMany(_ => base.BatchUpdate(timeEntries))
                .SingleAsync();
        }

        private IObservable<Unit> backupLocal(IEnumerable<IThreadSafeTimeEntry> timeEntries)
        {
            var ids = timeEntries.Select(te => te.Id).ToArray();
            return timeEntriesRepository.GetByIds(ids)
                .SingleAsync()
                .SelectMany(CommonFunctions.Identity)
                .Select(Convert)
                .ToDictionary(te => te.Id)
                .Do(localTimeEntries =>
                {
                    foreach (var remote in timeEntries)
                    {
                        if (localTimeEntries.TryGetValue(remote.Id, out var local))
                            backupTimeEntry(local, remote);
                    }
                })
                .SelectUnit();
        }

        private void backupTimeEntry(IDatabaseTimeEntry timeEntryDb, IThreadSafeTimeEntry timeEntry)
        {
            if (timeEntryDb.IsDeletedSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.IsDeletedSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.IsDeletedBackup = timeEntryDb.IsDeleted;
            }

            if (timeEntryDb.BillableSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.BillableSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.BillableBackup = timeEntryDb.Billable;
            }

            if (timeEntryDb.DescriptionSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.DescriptionSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.DescriptionBackup = timeEntryDb.Description;
            }

            if (timeEntryDb.DurationSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.DurationSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.DurationBackup = timeEntryDb.Duration;
            }

            if (timeEntryDb.WorkspaceIdSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.WorkspaceIdSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.WorkspaceIdBackup = timeEntryDb.WorkspaceId;
            }

            if (timeEntryDb.ProjectIdSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.ProjectIdSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.ProjectIdBackup = timeEntryDb.ProjectId;
            }

            if (timeEntryDb.StartSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.StartSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.StartBackup = timeEntryDb.Start;
            }

            if (timeEntryDb.TaskIdSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.TaskIdSyncStatus = PropertySyncStatus.SyncNeeded;
                timeEntry.TaskIdBackup = timeEntryDb.TaskId;
            }

            if (timeEntryDb.TagIdsSyncStatus == PropertySyncStatus.InSync)
            {
                timeEntry.TagIdsSyncStatus = PropertySyncStatus.SyncNeeded;

                timeEntry.TagIdsBackup.Clear();
                timeEntryDb.TagIds.AddTo(timeEntry.TagIdsBackup);
            }
        }

        public void OnTimeEntryStarted(IThreadSafeTimeEntry timeEntry, TimeEntryStartOrigin origin)
        {
            switch (origin)
            {
                case TimeEntryStartOrigin.ContinueMostRecent:
                    timeEntryContinuedSubject.OnNext(timeEntry);
                    break;

                case TimeEntryStartOrigin.Manual:
                case TimeEntryStartOrigin.Timer:
                    timeEntryStartedSubject.OnNext(timeEntry);
                    break;

                case TimeEntryStartOrigin.Suggestion:
                    suggestionStartedSubject.OnNext(timeEntry);
                    break;
            }
        }

        protected override IThreadSafeTimeEntry Convert(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseTimeEntry first, IDatabaseTimeEntry second)
            => Resolver.ForTimeEntries.Resolve(first, second);

        private IObservable<IThreadSafeTimeEntry> getCurrentlyRunningTimeEntry()
            => stopMultipleRunningTimeEntries()
                .SelectMany(_ => getAllRunning())
                .Flatten()
                .SingleOrDefaultAsync();

        private IObservable<Unit> stopMultipleRunningTimeEntries()
            => getAllRunning()
                .Where(list => list.Count() > 1)
                .SelectMany(BatchUpdate)
                .Track(analyticsService.TwoRunningTimeEntriesInconsistencyFixed)
                .ToList()
                .SelectUnit()
                .DefaultIfEmpty(Unit.Default);

        private IObservable<IEnumerable<IThreadSafeTimeEntry>> getAllRunning()
            => GetAll(te => te.IsDeleted == false && te.Duration == null);
    }
}
