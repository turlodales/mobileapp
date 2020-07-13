using Realms;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Storage.Realm.Models.Calendar
{
    public class RealmSyncedCalendar : RealmObject, IDatabaseSyncedCalendar, IUpdatesFrom<IDatabaseSyncedCalendar>, IModifiableId
    {
        public RealmSyncedCalendar() { }

        public RealmSyncedCalendar(IDatabaseSyncedCalendar entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public string SyncId { get; set; }

        public string Name { get; set; }
        
        public void SetPropertiesFrom(IDatabaseSyncedCalendar entity, Realms.Realm realm)
        {
            SyncId = entity.SyncId;
            Name = entity.Name;
        }

        public void ChangeId(long id)
        {
            Id = id;
        }
    }
}
