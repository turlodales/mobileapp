using System.Collections.Immutable;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Pull
{
    public interface IResponse
    {
        long ServerTime { get; }
        ImmutableList<IClient> Clients { get; }
        ImmutableList<IProject> Projects { get; }
        ImmutableList<ITag> Tags { get; }
        ImmutableList<ITask> Tasks { get; }
        ImmutableList<ITimeEntry> TimeEntries { get; }
        ImmutableList<IWorkspace> Workspaces { get; }
        ImmutableList<IWorkspaceFeatureCollection> WorkspaceFeatures { get; }
        IUser User { get; }
        IPreferences Preferences { get; }
    }
}
