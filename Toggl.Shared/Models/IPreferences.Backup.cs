namespace Toggl.Shared.Models
{
    public partial interface IPreferences
    {
        PropertySyncStatus TimeOfDayFormatSyncStatus { get; set; }
        TimeFormat TimeOfDayFormatBackup { get; set; }

        PropertySyncStatus DateFormatSyncStatus { get; set; }
        DateFormat DateFormatBackup { get; set; }

        PropertySyncStatus DurationFormatSyncStatus { get; set; }
        DurationFormat DurationFormatBackup { get; set; }

        PropertySyncStatus CollapseTimeEntriesSyncStatus { get; set; }
        bool CollapseTimeEntriesBackup { get; set; }
    }
}
