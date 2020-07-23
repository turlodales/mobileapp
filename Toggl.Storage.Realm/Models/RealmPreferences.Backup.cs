using Realms;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmPreferences
        : RealmObject, IDatabasePreferences, IPushable, ISyncable<IPreferences>
    {
        public string TimeOfDayFormatStringBackup { get; set; }

        [Ignored]
        public TimeFormat TimeOfDayFormatBackup
        {
            get => TimeOfDayFormatStringBackup != null
                ? TimeFormat.FromLocalizedTimeFormat(TimeOfDayFormatStringBackup)
                : default;
            set => TimeOfDayFormatStringBackup = value.Localized;
        }

        [Ignored]
        public DateFormat DateFormatBackup
        {
            get => DateFormatStringBackup != null
                ? DateFormat.FromLocalizedDateFormat(DateFormatStringBackup)
                : default;
            set => DateFormatStringBackup = value.Localized;
        }

        public string DateFormatStringBackup { get; set; }

        [Ignored]
        public DurationFormat DurationFormatBackup
        {
            get => (DurationFormat)DurationFormatIntBackup;
            set => DurationFormatIntBackup = (int)value;
        }

        public int DurationFormatIntBackup { get; set; }

        public bool CollapseTimeEntriesBackup { get; set; }

        [Ignored]
        public PropertySyncStatus TimeOfDayFormatSyncStatus
        {
            get => (PropertySyncStatus)TimeOfDayFormatSyncStatusInt;
            set => TimeOfDayFormatSyncStatusInt = (int)value;
        }

        public int TimeOfDayFormatSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus DateFormatSyncStatus
        {
            get => (PropertySyncStatus)DateFormatSyncStatusInt;
            set => DateFormatSyncStatusInt = (int)value;
        }

        public int DateFormatSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus DurationFormatSyncStatus
        {
            get => (PropertySyncStatus)DurationFormatSyncStatusInt;
            set => DurationFormatSyncStatusInt = (int)value;
        }

        public int DurationFormatSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus CollapseTimeEntriesSyncStatus
        {
            get => (PropertySyncStatus)CollapseTimeEntriesSyncStatusInt;
            set => CollapseTimeEntriesSyncStatusInt = (int)value;
        }

        public int CollapseTimeEntriesSyncStatusInt { get; set; }
    }
}