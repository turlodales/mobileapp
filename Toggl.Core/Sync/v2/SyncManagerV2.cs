using System;
using System.Reactive.Linq;

namespace Toggl.Core.Sync.V2
{
    public class SyncManagerV2 : ISyncManager
    {
        public bool IsRunningSync => false;

        public IObservable<SyncProgress> ProgressObservable
            => Observable.Return(SyncProgress.Failed);

        public IObservable<Exception> Errors
            => Observable.Empty<Exception>();

        public IObservable<SyncState> CleanUp()
            => Observable.Return(SyncState.Sleep);

        public IObservable<SyncState> ForceFullSync()
            => Observable.Return(SyncState.Sleep);

        public IObservable<SyncState> Freeze()
            => Observable.Return(SyncState.Sleep);

        public IObservable<SyncState> PullTimeEntries()
            => Observable.Return(SyncState.Sleep);

        public IObservable<SyncState> PushSync()
            => Observable.Return(SyncState.Sleep);
    }
}
