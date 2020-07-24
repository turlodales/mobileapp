namespace Toggl.Shared.Models
{
    public partial interface IUser 
    {
        PropertySyncStatus DefaultWorkspaceIdSyncStatus { get; set; }
        long? DefaultWorkspaceIdBackup { get; set; }

        PropertySyncStatus BeginningOfWeekSyncStatus { get; set; }
        BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }
}
