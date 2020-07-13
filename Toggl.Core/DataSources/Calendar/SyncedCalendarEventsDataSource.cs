using System;
using Toggl.Core.Models.Calendar;
using Toggl.Storage;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.DataSources.Calendar
{
    public class SyncedCalendarEventsDataSource : DataSource<IThreadSafeSyncedCalendarEvent, IDatabaseSyncedCalendarEvent>
    {
        public SyncedCalendarEventsDataSource(IRepository<IDatabaseSyncedCalendarEvent> repository)
            : base(repository)
        {
        }

        protected override IThreadSafeSyncedCalendarEvent Convert(IDatabaseSyncedCalendarEvent entity)
            => SyncedCalendarEvent.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseSyncedCalendarEvent first, IDatabaseSyncedCalendarEvent second)
            => ConflictResolutionMode.Ignore;
    }
}
