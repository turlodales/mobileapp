namespace Toggl.Shared.Models
{
    public partial interface IUser 
    {
        bool HasDefaultWorkspaceIdBackup { get; set; }
        long? DefaultWorkspaceIdBackup { get; set; }

        bool HasBeginningOfWeekBackup { get; set; }
        BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }
}
