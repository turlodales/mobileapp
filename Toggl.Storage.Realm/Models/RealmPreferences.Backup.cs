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
        public bool ContainsBackup { get; set; }

        [Ignored]
        public TimeFormat TimeOfDayFormatBackup
        {
            get => TimeOfDayFormatStringBackup != null
                ? TimeFormat.FromLocalizedTimeFormat(TimeOfDayFormatStringBackup)
                : default;
            set => TimeOfDayFormatStringBackup = value.Localized;
        }

        public string TimeOfDayFormatStringBackup { get; set; }

        [Ignored]
        public DateFormat DateFormatBackup
        {
            get => TimeOfDayFormatStringBackup != null
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
    }
}
