using Realms;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmPreferences
        : RealmObject, IDatabasePreferences, IUpdatable, ISyncable<IPreferences>
    {
        public const long fakeId = 0;

        [Ignored]
        public long Id => fakeId;

        [Ignored]
        public TimeFormat TimeOfDayFormat
        {
            get => TimeFormat.FromLocalizedTimeFormat(TimeOfDayFormatString);
            set => TimeOfDayFormatString = value.Localized;
        }

        public string TimeOfDayFormatString { get; set; }

        [Ignored]
        public DateFormat DateFormat
        {
            get => DateFormat.FromLocalizedDateFormat(DateFormatString);
            set => DateFormatString = value.Localized;
        }

        public string DateFormatString { get; set; }

        [Ignored]
        public DurationFormat DurationFormat
        {
            get => (DurationFormat)DurationFormatInt;
            set => DurationFormatInt = (int)value;
        }

        public int DurationFormatInt { get; set; }

        public bool CollapseTimeEntries { get; set; }

        public bool UseNewSync { get; set; }

        public void PrepareForSyncing()
        {
            SyncStatus = SyncStatus.Syncing;
        }

        public void PushFailed(string errorMessage)
        {
            SyncStatus = SyncStatus.SyncFailed;
            LastSyncErrorMessage = errorMessage;
        }

        public void UpdateSucceeded()
        {
            SyncStatus = SyncStatus.InSync;
            ContainsBackup = false;
        }

        public void SaveSyncResult(IPreferences entity, Realms.Realm realm)
        {
            TimeOfDayFormat = entity.TimeOfDayFormat;
            DateFormat = entity.DateFormat;
            DurationFormat = entity.DurationFormat;
            CollapseTimeEntries = entity.CollapseTimeEntries;
            UseNewSync = entity.UseNewSync;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
        }
    }
}
