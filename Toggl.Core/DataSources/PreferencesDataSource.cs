using System;
using System.Reactive.Linq;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Sync.ConflictResolution;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Models;

namespace Toggl.Core.DataSources
{
    internal sealed class PreferencesDataSource
        : SingletonDataSource<IThreadSafePreferences, IDatabasePreferences>
    {
        private ISingleObjectStorage<IDatabasePreferences> storage;

        public PreferencesDataSource(ISingleObjectStorage<IDatabasePreferences> storage)
            : base(storage, Preferences.DefaultPreferences)
        {
            this.storage = storage;
        }

        protected override IThreadSafePreferences Convert(IDatabasePreferences entity)
            => Preferences.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabasePreferences first, IDatabasePreferences second)
            => Resolver.ForPreferences.Resolve(first, second);

        public override IObservable<IThreadSafePreferences> Update(IThreadSafePreferences entity)
            => storage.Single()
            .Do(preferencesDb => backupPreferences(preferencesDb, entity))
            .SelectMany(_ => base.Update(entity));

        private void backupPreferences(IDatabasePreferences preferencesDb, IThreadSafePreferences preferences)
        {
            if (preferencesDb.TimeOfDayFormatSyncStatus == PropertySyncStatus.InSync)
            {
                preferences.TimeOfDayFormatSyncStatus = PropertySyncStatus.SyncNeeded;
                preferences.TimeOfDayFormatBackup = preferencesDb.TimeOfDayFormat;
            }

            if (preferencesDb.DateFormatSyncStatus == PropertySyncStatus.InSync)
            {
                preferences.DateFormatSyncStatus = PropertySyncStatus.SyncNeeded;
                preferences.DateFormatBackup = preferencesDb.DateFormat;
            }

            if (preferencesDb.DurationFormatSyncStatus == PropertySyncStatus.InSync)
            {
                preferences.DurationFormatSyncStatus = PropertySyncStatus.SyncNeeded;
                preferences.DurationFormatBackup = preferencesDb.DurationFormat;
            }

            if (preferencesDb.CollapseTimeEntriesSyncStatus == PropertySyncStatus.InSync)
            {
                preferences.CollapseTimeEntriesSyncStatus = PropertySyncStatus.SyncNeeded;
                preferences.CollapseTimeEntriesBackup = preferencesDb.CollapseTimeEntries;
            }
        }
    }
}
