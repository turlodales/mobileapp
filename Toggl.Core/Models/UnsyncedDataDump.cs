using System.Collections.Generic;
using System.Collections.Immutable;
using Toggl.Core.Models.Interfaces;

namespace Toggl.Core.Models
{
    public class UnsyncedDataDump
    {
        public IImmutableList<IThreadSafeTimeEntry> TimeEntries { get; }
        public IImmutableList<IThreadSafeTag> Tags { get; }
        public IImmutableList<IThreadSafeProject> Projects { get; }
        public IImmutableList<IThreadSafeClient> Clients { get; }

        public UnsyncedDataDump(
            IEnumerable<IThreadSafeTimeEntry> timeEntries,
            IEnumerable<IThreadSafeTag> tags,
            IEnumerable<IThreadSafeProject> projects,
            IEnumerable<IThreadSafeClient> clients)
        {
            TimeEntries = timeEntries.ToImmutableList() ?? ImmutableList<IThreadSafeTimeEntry>.Empty;
            Tags = tags.ToImmutableList() ?? ImmutableList<IThreadSafeTag>.Empty;
            Projects = projects.ToImmutableList() ?? ImmutableList<IThreadSafeProject>.Empty;
            Clients = clients.ToImmutableList() ?? ImmutableList<IThreadSafeClient>.Empty;
        }
    }
}
