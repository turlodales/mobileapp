using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Toggl.Networking.Sync
{
    public class DeletePushAction : PushAction<DeleteMeta>
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public override ActionType Type => ActionType.Delete;

        public DeletePushAction(long id, long workspaceId)
        {
            Meta = new DeleteMeta(id, workspaceId);
        }
    }
}
