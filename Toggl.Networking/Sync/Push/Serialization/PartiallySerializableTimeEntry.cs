using Newtonsoft.Json.Linq;
using System;
using Toggl.Networking.Models;
using Toggl.Shared.Extensions;
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

            if (Entity.ProjectId != Entity.ProjectIdBackup)
                obj.Add(new JProperty("project_id", Entity.ProjectId));

            if (Entity.TaskId != Entity.TaskIdBackup)
                obj.Add(new JProperty("task_id", Entity.TaskId));

            if (Entity.Billable != Entity.BillableBackup)
                obj.Add(new JProperty("billable", Entity.Billable));

            if (Entity.Start != Entity.StartBackup)
                obj.Add(new JProperty("start", Entity.Start));

            if (Entity.Duration != Entity.DurationBackup)
                obj.Add(new JProperty("duration", Entity.Duration));

            if (Entity.Description != Entity.DescriptionBackup)
                obj.Add(new JProperty("description", Entity.Description));

            var entityTags = Entity.TagIds ?? new long[0];
            var backupEntityTags = Entity.TagIdsBackup ?? new long[0];

            if (!entityTags.SetEquals(backupEntityTags))
                obj.Add(new JProperty("tag_ids", new JArray(entityTags)));

            return obj;
        }
    }
}
