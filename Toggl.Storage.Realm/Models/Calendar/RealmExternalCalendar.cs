using Realms;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Storage.Realm.Models.Calendar
{
    public class RealmExternalCalendar : RealmObject, IDatabaseExternalCalendar, IUpdatesFrom<IDatabaseExternalCalendar>, IModifiableId
    {
        public RealmExternalCalendar() { }

        public RealmExternalCalendar(IDatabaseExternalCalendar entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public RealmExternalCalendar(long id, IExternalCalendar entity, Realms.Realm realm)
        {
            Id = id;
            SetPropertiesFrom(entity, realm);
        }

        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public string SyncId { get; set; }

        public string Name { get; set; }

        public void SetPropertiesFrom(IDatabaseExternalCalendar entity, Realms.Realm realm)
        {
            SyncId = entity.SyncId;
            Name = entity.Name;
        }

        public void SetPropertiesFrom(IExternalCalendar entity, Realms.Realm realm)
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
