using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.DataSources.Interfaces;
using Toggl.Core.Exceptions;
using Toggl.Core.Interactors;
using Toggl.Core.Models.Interfaces;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Models;

namespace Toggl.Core.Sync.V2
{
    public class SyncManagerV2 : ISyncManager
    {
        private readonly object access = new object();
        private readonly ISchedulerProvider schedulerProvider;

        private readonly IAnalyticsService analyticsService;
        private readonly IInteractorFactory interactorFactory;
        private readonly ISubject<SyncProgress> progress;
        private readonly ISubject<Exception> errors;
        private readonly IObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> timeEntriesDataSource;

        private bool isRunningSync = false;
        private bool isFrozen = false;

        public bool IsRunningSync => isRunningSync;
        public int Version => 2;

        public SyncManagerV2(
            IAnalyticsService analyticsService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            IObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> timeEntriesSource)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(timeEntriesSource, nameof(timeEntriesSource));

            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.schedulerProvider = schedulerProvider;
            this.timeEntriesDataSource = timeEntriesSource;

            progress = new BehaviorSubject<SyncProgress>(SyncProgress.Synced);
            errors = new Subject<Exception>();
        }

        public IObservable<SyncProgress> ProgressObservable => progress.AsObservable();
        public IObservable<Exception> Errors => errors.AsObservable();

        public IObservable<SyncState> CleanUp()
            => Observable.Return(SyncState.Sleep);

        public IObservable<SyncState> ForceFullSync()
            => sync()
                .ToObservable(schedulerProvider.DefaultScheduler)
                .SelectValue(SyncState.Sleep);

        public IObservable<SyncState> Freeze()
        {
            lock (access)
            {
                isFrozen = true;
            }

            // the observable should finish once we stop syncing
            return progress.SkipWhile(p => p == SyncProgress.Syncing)
                .FirstAsync()
                .SelectValue(SyncState.Sleep);
        }

        public IObservable<SyncState> PullTimeEntries()
            => ForceFullSync();

        public IObservable<SyncState> PushSync()
            => ForceFullSync();

        private async Task sync()
        {
            lock (access)
            {
                if (isFrozen || isRunningSync)
                    return;

                isRunningSync = true;
            }

            progress.OnNext(SyncProgress.Syncing);

            var syncSequence = new IInteractor<Task>[]
            {
                // interactorFactory.ResolveOutstandingPushRequest(),
                // interactorFactory.PullSync(),
                // interactorFactory.PushSync(),
                // interactorFactory.PullSync(),
            };

            try
            {
                foreach (var interactor in syncSequence)
                {
                    await interactor.Execute();
                    if (isFrozen) break; // abort as soon as possible
                }

                // if the sync manager is frozen, don't update the progress anymore
                progress.OnNext(SyncProgress.Synced);
                analyticsService.SyncCompleted.Track();
                timeEntriesDataSource.ReportChange();
            }
            catch (Exception error)
            {
                processError(error);
            }
            finally
            {
                lock (access)
                {
                    isRunningSync = false;
                }
            }
        }

        private void processError(Exception error)
        {
            // shamelessly copy&pasted from SyncManager V1
            analyticsService.TrackAnonymized(error);
            analyticsService.SyncFailed.Track(error.GetType().FullName, error.Message, error.StackTrace ?? "");

            if (error is NoWorkspaceException
                || error is NoDefaultWorkspaceException)
            {
                errors.OnNext(error);
                progress.OnNext(SyncProgress.Synced);
                return;
            }

            if (error is OfflineException)
            {
                progress.OnNext(SyncProgress.OfflineModeDetected);
                analyticsService.OfflineModeDetected.Track();
            }
            else
            {
                progress.OnNext(SyncProgress.Failed);
            }

            if (error is ClientDeprecatedException
                || error is ApiDeprecatedException
                || error is UnauthorizedException)
            {
                Freeze();
                errors.OnNext(error);
                progress.OnNext(SyncProgress.Failed);
            }
        }
    }
}
