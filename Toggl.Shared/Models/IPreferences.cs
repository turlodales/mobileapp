namespace Toggl.Shared.Models
{
    public partial interface IPreferences
    {
        TimeFormat TimeOfDayFormat { get; }

        DateFormat DateFormat { get; }

        DurationFormat DurationFormat { get; }

        bool CollapseTimeEntries { get; }

        bool UseNewSync { get; }
    }
}
