using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Toggl.Networking.Sync.Push
{
    public class DeleteAction : Action<DeleteMeta>
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Delete;

        public DeleteAction(long id, long workspaceId)
        {
            Meta = new DeleteMeta(id, workspaceId);
        }
    }
}
