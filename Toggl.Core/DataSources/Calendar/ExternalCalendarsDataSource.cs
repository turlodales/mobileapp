using System;
using Toggl.Core.Models.Calendar;
using Toggl.Storage;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Core.DataSources.Calendar
{
    public class ExternalCalendarsDataSource : DataSource<IThreadSafeExternalCalendar, IDatabaseExternalCalendar>
    {
        public ExternalCalendarsDataSource(IRepository<IDatabaseExternalCalendar> repository)
            : base(repository)
        {
        }

        protected override IThreadSafeExternalCalendar Convert(IDatabaseExternalCalendar entity)
            => ExternalCalendar.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseExternalCalendar first, IDatabaseExternalCalendar second)
            => ConflictResolutionMode.Ignore;
    }
}
