using System.Linq;
using System.Collections.Generic;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Networking.Models;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    internal class Response : IResponse
    {
        public IEnumerable<IEntityActionResult<Client>> Clients { get; set; }
        public IEnumerable<IEntityActionResult<Project>> Projects { get; set; }
        public IEnumerable<IEntityActionResult<Tag>> Tags { get; set; }
        public IEnumerable<IEntityActionResult<Task>> Tasks { get; set; }
        public IEnumerable<IEntityActionResult<TimeEntry>> TimeEntries { get; set; }
        public IEnumerable<IEntityActionResult<Workspace>> Workspaces { get; set; }
        public IActionResult<User> User { get; set; }
        public IActionResult<Preferences> Preferences { get; set; }

        public List<IEntityActionResult<IClient>> ClientResults => Clients.ToList<IEntityActionResult<IClient>>();
        public List<IEntityActionResult<IProject>> ProjectResults => Projects.ToList<IEntityActionResult<IProject>>();
        public List<IEntityActionResult<ITag>> TagResults => Tags.ToList<IEntityActionResult<ITag>>();
        public List<IEntityActionResult<ITask>> TaskResults => Tasks.ToList<IEntityActionResult<ITask>>();
        public List<IEntityActionResult<ITimeEntry>> TimeEntryResults => TimeEntries.ToList<IEntityActionResult<ITimeEntry>>();
        public List<IEntityActionResult<IWorkspace>> WorkspaceResults => Workspaces.ToList<IEntityActionResult<IWorkspace>>();
        public IActionResult<IUser> UserResult => (IActionResult<IUser>)User;
        public IActionResult<IPreferences> PreferencesResult => (IActionResult<IPreferences>)Preferences;
    }
}
