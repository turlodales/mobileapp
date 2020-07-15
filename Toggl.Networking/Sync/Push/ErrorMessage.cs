using Toggl.Shared;

namespace Toggl.Networking.Sync.Push
{
    [Preserve(AllMembers = true)]
    public class ErrorMessage
    {
        public ErrorCode Code { get; set; }
        public string DefaultMessage { get; set; }
    }
}
