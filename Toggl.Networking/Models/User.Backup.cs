using Newtonsoft.Json;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class User : IUser
    {
        [JsonIgnore]
        public PropertySyncStatus DefaultWorkspaceIdSyncStatus { get; set; }

        [JsonIgnore]
        public long? DefaultWorkspaceIdBackup { get; set; }

        [JsonIgnore]
        public PropertySyncStatus BeginningOfWeekSyncStatus { get; set; }

        [JsonIgnore]
        public BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }
}
