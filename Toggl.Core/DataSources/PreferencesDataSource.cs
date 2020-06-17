using System;
using System.Reactive.Linq;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Sync.ConflictResolution;
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
            if (!preferencesDb.ContainsBackup)
            {
                preferences.ContainsBackup = true;

                preferences.TimeOfDayFormatBackup = preferencesDb.TimeOfDayFormat;
                preferences.DateFormatBackup = preferencesDb.DateFormat;
                preferences.DurationFormatBackup = preferencesDb.DurationFormat;
                preferences.CollapseTimeEntriesBackup = preferencesDb.CollapseTimeEntries;
            }
        }
    }
}
