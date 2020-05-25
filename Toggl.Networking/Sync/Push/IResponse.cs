using System.Collections.Immutable;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push
{
    public interface IResponse
    {
        ImmutableList<IEntityActionResult<IClient>> Clients { get; }
        ImmutableList<IEntityActionResult<IProject>> Projects { get; }
        ImmutableList<IEntityActionResult<ITag>> Tags { get; }
        ImmutableList<IEntityActionResult<ITask>> Tasks { get; }
        ImmutableList<IEntityActionResult<ITimeEntry>> TimeEntries { get; }
        ImmutableList<IEntityActionResult<IWorkspace>> Workspaces { get; }
        IActionResult<IUser> User { get; }
        IActionResult<IPreferences> Preferences { get; }
    }
}
