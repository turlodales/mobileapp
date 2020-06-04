namespace Toggl.Shared.Models
{
    public partial interface IPreferences
    {
        bool ContainsBackup { get; set; }

        TimeFormat TimeOfDayFormatBackup { get; set; }
        DateFormat DateFormatBackup { get; set; }
        DurationFormat DurationFormatBackup { get; set; }
        bool CollapseTimeEntriesBackup { get; set; }
    }
}
