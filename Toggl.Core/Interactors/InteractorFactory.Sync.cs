using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Core.Interactors.Sync;
using Toggl.Core.Models;
using Toggl.Networking.Sync.Push;

namespace Toggl.Core.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IEnumerable<SyncFailureItem>>> GetItemsThatFailedToSync()
            => new GetItemsThatFailedToSyncInteractor(dataSource);

        public IInteractor<IObservable<bool>> HasFinishedSyncBefore()
            => new HasFinsihedSyncBeforeInteractor(dataSource);

        public IInteractor<IObservable<SyncOutcome>> RunBackgroundSync()
            => new RunBackgroundSyncInteractor(syncManager, analyticsService);

        public IInteractor<IObservable<bool>> ContainsPlaceholders()
            => new ContainsPlaceholdersInteractor(dataSource);

        public IInteractor<IObservable<SyncOutcome>> RunPushNotificationInitiatedSyncInForeground()
            => new RunSyncInteractor(
                syncManager,
                analyticsService,
                PushNotificationSyncSourceState.Foreground);

        public IInteractor<IObservable<SyncOutcome>> RunPushNotificationInitiatedSyncInBackground()
            => new RunSyncInteractor(
                syncManager,
                analyticsService,
                PushNotificationSyncSourceState.Background);

        public IInteractor<Task<UnsyncedDataDump>> CreateUnsyncedDataDump()
            => new CreateUnsyncedDataDumpInteractor(dataSource);

        public IInteractor<Task<Request>> PreparePushRequest()
            => new PreparePushRequestInteractor(userAgent.ToString(), dataSource);

        public IInteractor<System.Threading.Tasks.Task> ResolveOutstandingPushRequest()
            => new ResolveOutstandingPushRequestInteractor(api.SyncApi, database.PushRequestIdentifier, queryFactory);

        public IInteractor<System.Threading.Tasks.Task> PushSync()
            => new PushSyncInteractor(api.SyncApi, database.PushRequestIdentifier, this, queryFactory);

        public IInteractor<System.Threading.Tasks.Task> PullSync()
            => new PullDataInteractor(api, this, database.SinceParameters, queryFactory);

        public IInteractor<System.Threading.Tasks.Task> CleanUp()
            => new CleanUpInteractor(timeService, dataSource, analyticsService);
    }
}
