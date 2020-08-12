using Toggl.Shared;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    public sealed class ModificationMeta
    {
        public ModificationMeta(long id, long workspaceId)
        {
            Id = id;
            WorkspaceId = workspaceId;
        }

        public long Id { get; }
        public long WorkspaceId { get; }
    }
}
