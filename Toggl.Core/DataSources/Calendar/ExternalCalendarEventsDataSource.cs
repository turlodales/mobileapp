using System;
using Toggl.Core.Models.Calendar;
using Toggl.Storage;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.DataSources.Calendar
{
    public class ExternalCalendarEventsDataSource : DataSource<IThreadSafeExternalCalendarEvent, IDatabaseExternalCalendarEvent>
    {
        public ExternalCalendarEventsDataSource(IRepository<IDatabaseExternalCalendarEvent> repository)
            : base(repository)
        {
        }

        protected override IThreadSafeExternalCalendarEvent Convert(IDatabaseExternalCalendarEvent entity)
            => ExternalCalendarEvent.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseExternalCalendarEvent first, IDatabaseExternalCalendarEvent second)
            => ConflictResolutionMode.Ignore;
    }
}
