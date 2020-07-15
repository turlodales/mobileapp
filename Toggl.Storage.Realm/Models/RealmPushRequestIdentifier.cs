using Realms;
using Toggl.Storage.Models;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Models
{
    public sealed class RealmPushRequestIdentifier
        : RealmObject,
        IDatabasePushRequestIdentifier,
        IUpdatesFrom<IDatabasePushRequestIdentifier>
    {
        [PrimaryKey]
        public long Id { get; set; }

        public string PushRequestId { get; set; }

        public RealmPushRequestIdentifier()
        {
        }

        public RealmPushRequestIdentifier(IDatabasePushRequestIdentifier entity)
        {
            SetPropertiesFrom(entity, null);
        }

        public void SetPropertiesFrom(IDatabasePushRequestIdentifier entity, RealmDb realm)
        {
            Id = entity.Id;
            PushRequestId = entity.PushRequestId;
        }
    }
}
