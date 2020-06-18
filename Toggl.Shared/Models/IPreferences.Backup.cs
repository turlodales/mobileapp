namespace Toggl.Shared.Models
{
    public partial interface IPreferences
    {
        bool HasTimeOfDayFormatBackup { get; set; }
        TimeFormat TimeOfDayFormatBackup { get; set; }

        bool HasDateFormatBackup { get; set; }
        DateFormat DateFormatBackup { get; set; }

        bool HasDurationFormatBackup { get; set; }
        DurationFormat DurationFormatBackup { get; set; }

        bool HasCollapseTimeEntriesBackup { get; set; }
        bool CollapseTimeEntriesBackup { get; set; }
    }
}
