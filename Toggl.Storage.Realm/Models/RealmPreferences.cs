using System.Linq;
using Realms;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;
using static Toggl.Shared.PropertySyncStatus;

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
            get => TimeOfDayFormatString != null
                ? TimeFormat.FromLocalizedTimeFormat(TimeOfDayFormatString)
                : TimeFormat.TwelveHoursFormat;
            set => TimeOfDayFormatString = value.Localized;
        }

        public string TimeOfDayFormatString { get; set; }

        [Ignored]
        public DateFormat DateFormat
        {
            get => DateFormatString != null
                ? DateFormat.FromLocalizedDateFormat(DateFormatString)
                : DateFormat.ValidDateFormats.First();
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
            changePropertiesSyncStatus(from: SyncNeeded, to: Syncing);
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
            changePropertiesSyncStatus(from: Syncing, to: SyncNeeded);
        }

        public void UpdateSucceeded()
        {
            if (SyncStatus != SyncStatus.SyncNeeded)
                SyncStatus = SyncStatus.InSync;

            changePropertiesSyncStatus(from: Syncing, to: InSync);
        }

        public void SaveSyncResult(IPreferences entity, Realms.Realm realm)
        {
            TimeOfDayFormat = entity.TimeOfDayFormat;
            DateFormat = entity.DateFormat;
            DurationFormat = entity.DurationFormat;
            CollapseTimeEntries = entity.CollapseTimeEntries;
            UseNewSync = entity.UseNewSync;
            SyncStatus = SyncStatus.InSync;
            TimeOfDayFormatSyncStatus = InSync;
            DateFormatSyncStatus = InSync;
            DurationFormatSyncStatus = InSync;
            CollapseTimeEntriesSyncStatus = InSync;
            LastSyncErrorMessage = null;
        }

        private void changePropertiesSyncStatus(PropertySyncStatus from, PropertySyncStatus to)
        {
            if (TimeOfDayFormatSyncStatus == from)
                TimeOfDayFormatSyncStatus = to;

            if (DateFormatSyncStatus == from)
                DateFormatSyncStatus = to;

            if (DurationFormatSyncStatus == from)
                DurationFormatSyncStatus = to;

            if (CollapseTimeEntriesSyncStatus == from)
                CollapseTimeEntriesSyncStatus = to;
        }
    }
}
