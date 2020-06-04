namespace Toggl.Shared.Models
{
    public partial interface IUser 
    {
        bool ContainsBackup { get; set; }

        long? DefaultWorkspaceIdBackup { get; set; }
        BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }
}
