using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.Models.Calendar
{
    public sealed class ExternalCalendar : IThreadSafeExternalCalendar
    {
        public long Id { get; }

        public string SyncId { get; }

        public string Name { get; }

        public ExternalCalendar(long id, string syncId, string name)
        {
            Id = id;
            SyncId = syncId;
            Name = name;
        }

        public ExternalCalendar(IDatabaseExternalCalendar entity) : this(entity.Id, entity.SyncId, entity.Name)
        {
        }

        public static ExternalCalendar From(IDatabaseExternalCalendar entity)
            => new ExternalCalendar(entity);
    }
}
