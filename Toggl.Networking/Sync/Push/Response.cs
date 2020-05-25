using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    internal class Response : IResponse
    {
        public ImmutableList<IEntityActionResult<IClient>> Clients { get; }
        public ImmutableList<IEntityActionResult<IProject>> Projects { get; }
        public ImmutableList<IEntityActionResult<ITag>> Tags { get; }
        public ImmutableList<IEntityActionResult<ITask>> Tasks { get; }
        public ImmutableList<IEntityActionResult<ITimeEntry>> TimeEntries { get; }
        public ImmutableList<IEntityActionResult<IWorkspace>> Workspaces { get; }
        public IActionResult<IUser> User { get; }
        public IActionResult<IPreferences> Preferences { get; }
        public Response(
            IEnumerable<IEntityActionResult<IClient>> clients,
            IEnumerable<IEntityActionResult<IProject>> projects,
            IEnumerable<IEntityActionResult<ITag>> tags,
            IEnumerable<IEntityActionResult<ITask>> tasks,
            IEnumerable<IEntityActionResult<ITimeEntry>> timeEntries,
            IEnumerable<IEntityActionResult<IWorkspace>> workspaces,
            IActionResult<IUser> user,
            IActionResult<IPreferences> preferences)
        {
            Clients = clients?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<IClient>>.Empty;
            Projects = projects?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<IProject>>.Empty;
            Tags = tags?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<ITag>>.Empty;
            Tasks = tasks?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<ITask>>.Empty;
            TimeEntries = timeEntries?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<ITimeEntry>>.Empty;
            Workspaces = workspaces?.ToList().ToImmutableList() ?? ImmutableList<IEntityActionResult<IWorkspace>>.Empty;
            User = user;
            Preferences = preferences;
        }
    }
}
