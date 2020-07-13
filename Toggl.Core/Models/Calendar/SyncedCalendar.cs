using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.Models.Calendar
{
    public sealed class SyncedCalendar : IThreadSafeSyncedCalendar
    {
        public long Id { get; }

        public string SyncId { get; }

        public string Name { get; }

        public SyncedCalendar(long id, string syncId, string name)
        {
            Id = id;
            SyncId = syncId;
            Name = name;
        }

        public SyncedCalendar(IDatabaseSyncedCalendar entity) : this(entity.Id, entity.SyncId, entity.Name)
        {
        }

        public static SyncedCalendar From(IDatabaseSyncedCalendar entity)
            => new SyncedCalendar(entity);
    }
}
