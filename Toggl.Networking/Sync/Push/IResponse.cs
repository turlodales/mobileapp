using System.Linq;
using System.Collections.Generic;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push
{
    public interface IResponse
    {
        List<IEntityActionResult<IClient>> ClientResults { get; }
        List<IEntityActionResult<IProject>> ProjectResults { get; }
        List<IEntityActionResult<ITag>> TagResults { get; }
        List<IEntityActionResult<ITask>> TaskResults { get; }
        List<IEntityActionResult<ITimeEntry>> TimeEntryResults { get; }
        List<IEntityActionResult<IWorkspace>> WorkspaceResults { get; }
        IActionResult<IUser> UserResult { get; }
        IActionResult<IPreferences> PreferencesResult { get; }
    }
}
