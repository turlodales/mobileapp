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
        public ITimeEntry BackedUpEntity { get; private set; }

        public PartiallySerializableTimeEntry(ITimeEntry entity, ITimeEntry backedUpEntity)
        {
            Entity = entity;
            BackedUpEntity = backedUpEntity;
        }

        public JObject Serialize()
        {
            var obj = new JObject();

            if (Entity.ProjectId != BackedUpEntity.ProjectId)
                obj.Add(new JProperty("project_id", Entity.ProjectId));

            if (Entity.TaskId != BackedUpEntity.TaskId)
                obj.Add(new JProperty("task_id", Entity.TaskId));

            if (Entity.Billable != BackedUpEntity.Billable)
                obj.Add(new JProperty("billable", Entity.Billable));

            if (Entity.Start != BackedUpEntity.Start)
                obj.Add(new JProperty("start", Entity.Start));

            if (Entity.Duration != BackedUpEntity.Duration)
                obj.Add(new JProperty("duration", Entity.Duration));

            if (Entity.Description != BackedUpEntity.Description)
                obj.Add(new JProperty("description", Entity.Description));

            var entityTags = Entity.TagIds ?? new long[0];
            var backupEntityTags = BackedUpEntity.TagIds ?? new long[0];

            if (!entityTags.SetEquals(backupEntityTags))
            {
                var array = new JArray();
                entityTags.ForEach(tagId => array.Add(tagId));

                obj.Add(new JProperty("tag_ids", array));
            }

            return obj;
        }
    }
}
