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
        public bool ContainsBackup { get; set; }

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
