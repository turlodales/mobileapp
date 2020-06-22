using Realms;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmUser
        : RealmObject, IDatabaseUser, IPushable, ISyncable<IUser>
    {

        [Ignored]
        public PropertySyncStatus DefaultWorkspaceIdSyncStatus
        {
            get => (PropertySyncStatus)DefaultWorkspaceIdSyncStatusInt;
            set => DefaultWorkspaceIdSyncStatusInt = (int)value;
        }

        public int DefaultWorkspaceIdSyncStatusInt { get; set; }

        [Ignored]
        public PropertySyncStatus BeginningOfWeekSyncStatus
        {
            get => (PropertySyncStatus)BeginningOfWeekSyncStatusInt;
            set => BeginningOfWeekSyncStatusInt = (int)value;
        }

        public int BeginningOfWeekSyncStatusInt { get; set; }

        public long? DefaultWorkspaceIdBackup { get; set; }

        //Realm doesn't support enums
        [Ignored]
        public BeginningOfWeek BeginningOfWeekBackup
        {
            get => (BeginningOfWeek)BeginningOfWeekIntBackup;
            set => BeginningOfWeekIntBackup = (int)value;
        }

        public int BeginningOfWeekIntBackup { get; set; }
    }
}
