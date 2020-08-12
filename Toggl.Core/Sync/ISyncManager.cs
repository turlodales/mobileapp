using System;

namespace Toggl.Core.Sync
{
    public interface ISyncManager
    {
        int Version { get; }

        IObservable<SyncProgress> ProgressObservable { get; }
        IObservable<Exception> Errors { get; }

        bool IsRunningSync { get; }

        IObservable<SyncState> PushSync();
        IObservable<SyncState> ForceFullSync();
        IObservable<SyncState> PullTimeEntries();
        IObservable<SyncState> CleanUp();

        IObservable<SyncState> Freeze();
    }
}
