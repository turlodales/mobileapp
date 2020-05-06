using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Networking.Models;

namespace Toggl.Networking.Sync.Pull
{
    [Preserve(AllMembers = true)]
    internal class Response : IResponse
    {
        public long ServerTime { get; }
        public ImmutableList<IClient> Clients { get; }
        public ImmutableList<IProject> Projects { get; }
        public ImmutableList<ITag> Tags { get; }
        public ImmutableList<ITask> Tasks { get; }
        public ImmutableList<ITimeEntry> TimeEntries { get; }
        public ImmutableList<IWorkspace> Workspaces { get; }
        public IUser User { get; }
        public IPreferences Preferences { get; }

        public Response(
            long serverTime,
            IEnumerable<Client> clients,
            IEnumerable<Project> projects,
            IEnumerable<Tag> tags,
            IEnumerable<Task> tasks,
            IEnumerable<TimeEntry> timeEntries,
            IEnumerable<Workspace> workspaces,
            User user,
            Preferences preferences)
        {
            ServerTime = serverTime;
            Clients = clients?.ToList<IClient>().ToImmutableList() ?? ImmutableList<IClient>.Empty;
            Projects = projects?.ToList<IProject>().ToImmutableList() ?? ImmutableList<IProject>.Empty;
            Tags = tags?.ToList<ITag>().ToImmutableList() ?? ImmutableList<ITag>.Empty;
            Tasks = tasks?.ToList<ITask>().ToImmutableList() ?? ImmutableList<ITask>.Empty;
            TimeEntries = timeEntries?.ToList<ITimeEntry>().ToImmutableList() ?? ImmutableList<ITimeEntry>.Empty;
            Workspaces = workspaces?.ToList<IWorkspace>().ToImmutableList() ?? ImmutableList<IWorkspace>.Empty;
            User = user;
            Preferences = preferences;
        }
    }
}
