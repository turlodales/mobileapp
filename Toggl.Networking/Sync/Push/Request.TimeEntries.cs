using System.Linq;
using System.Collections.Generic;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public Request CreateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new CreateAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);

            return this;
        }

        public Request UpdateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new UpdateAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);

            return this;
        }

        public Request DeleteTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new DeleteAction(timeEntry.Id, timeEntry.WorkspaceId))
                .AddTo(TimeEntries);

            return this;
        }
    }
}
