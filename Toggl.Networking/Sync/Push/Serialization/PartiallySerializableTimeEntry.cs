using Newtonsoft.Json.Linq;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push.Serialization
{
    internal class PartiallySerializableTimeEntry : IPartiallySerializableEntity
    {
        public ITimeEntry Entity { get; private set; }

        public PartiallySerializableTimeEntry(ITimeEntry entity)
        {
            Entity = entity;
        }

        public JObject Serialize()
        {
            var obj = new JObject();

            if (Entity.WorkspaceIdSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("workspace_id", Entity.WorkspaceId));

            if (Entity.ProjectIdSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("project_id", Entity.ProjectId));

            if (Entity.TaskIdSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("task_id", Entity.TaskId));

            if (Entity.BillableSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("billable", Entity.Billable));

            if (Entity.StartSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("start", Entity.Start));

            if (Entity.DurationSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("duration", Entity.Duration));

            if (Entity.DescriptionSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("description", Entity.Description));

            if (Entity.TagIdsSyncStatus != PropertySyncStatus.InSync)
                obj.Add(new JProperty("tag_ids", Entity.TagIds ?? new long[0]));

            return obj;
        }
    }
}
