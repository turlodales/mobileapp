namespace Toggl.Networking.Sync
{
    public sealed class DeleteMeta : IMeta
    {
        public DeleteMeta(long id, long workspaceId)
        {
            Id = id;
            WorkspaceId = workspaceId;
        }

        public long Id { get; set; }
        public long WorkspaceId { get; set; }
    }
}
