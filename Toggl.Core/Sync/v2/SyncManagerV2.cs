using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Exceptions;
using Toggl.Core.Interactors;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;

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
        private readonly ITogglDataSource dataSource;

        private bool syncQueued = false;

        private bool isFrozen = false;

        public bool IsRunningSync { get; private set; }
        public int Version => 2;

        public SyncManagerV2(
            IAnalyticsService analyticsService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.schedulerProvider = schedulerProvider;
            this.dataSource = dataSource;

            progress = new BehaviorSubject<SyncProgress>(SyncProgress.Synced);
            errors = new Subject<Exception>();
        }

        public IObservable<SyncProgress> ProgressObservable => progress.AsObservable();
        public IObservable<Exception> Errors => errors.AsObservable();

        public IObservable<SyncState> CleanUp()
            => ForceFullSync();

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
                if (isFrozen)
                    return;

                syncQueued = true;

                if (IsRunningSync)
                    return;

                IsRunningSync = true;
            }

            try
            {
                progress.OnNext(SyncProgress.Syncing);

                var performanceMeasurement = analyticsService.StartNewSyncPerformanceMeasurement();
                while (shouldKeepSyncing())
                {
                    await syncOnce();
                }

                progress.OnNext(SyncProgress.Synced);
                analyticsService.SyncCompleted.Track();
                analyticsService.StopAndTrack(performanceMeasurement); // we only want to track how long syncing takes in the successful case
                reportDataChanged();
            }
            catch (Exception error)
            {
                processError(error);
            }
            finally
            {
                lock (access)
                {
                    IsRunningSync = false;
                    syncQueued = false;
                }
            }
        }

        private async Task syncOnce()
        {
            foreach (var interactor in syncSequence())
            {
                await interactor.Execute();
                if (isFrozen)
                    break; // abort as soon as possible
            }

            await checkForNoWorkspacesScenario();
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

        private bool shouldKeepSyncing()
        {
            lock (access)
            {
                var wasQueued = syncQueued;
                syncQueued = false;
                return wasQueued;
            }
        }

        private IEnumerable<IInteractor<Task>> syncSequence()
        {
            yield return interactorFactory.ResolveOutstandingPushRequest();
            yield return interactorFactory.PullSync();
            yield return interactorFactory.PushSync();
            yield return interactorFactory.ResolveOutstandingPushRequest();
            yield return interactorFactory.PullSync();
            yield return interactorFactory.CleanUp();
        }

        private void reportDataChanged()
        {
            dataSource.TimeEntries.ReportChange();
            dataSource.Workspaces.ReportChange();
            dataSource.Preferences.ReportChange();
            dataSource.User.ReportChange();
        }

        private async Task checkForNoWorkspacesScenario()
        {
            // NOTE: The interactor returns only accessible workspaces.
            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();

            if (workspaces.None())
                throw new NoWorkspaceException();
        }
    }
}
