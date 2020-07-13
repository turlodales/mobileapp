using System;
using Toggl.Core.Models.Calendar;
using Toggl.Storage;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.DataSources.Calendar
{
    public class SyncedCalendarsDataSource : DataSource<IThreadSafeSyncedCalendar, IDatabaseSyncedCalendar>
    {
        public SyncedCalendarsDataSource(IRepository<IDatabaseSyncedCalendar> repository)
            : base(repository)
        {
        }

        protected override IThreadSafeSyncedCalendar Convert(IDatabaseSyncedCalendar entity)
            => SyncedCalendar.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseSyncedCalendar first, IDatabaseSyncedCalendar second)
            => ConflictResolutionMode.Ignore;
    }
}
