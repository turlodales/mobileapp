using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync
{
    public sealed partial class PushRequest
    {
        public PushRequest CreateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new CreatePushAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);

            return this;
        }

        public PushRequest UpdateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new UpdatePushAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);

            return this;
        }

        public PushRequest DeleteTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new DeletePushAction(timeEntry.Id, timeEntry.WorkspaceId))
                .AddTo(TimeEntries);

            return this;
        }
    }
}
