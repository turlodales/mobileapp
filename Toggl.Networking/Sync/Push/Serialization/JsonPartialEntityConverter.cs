using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Toggl.Shared;

namespace Toggl.Networking.Sync.Push.Serialization
{
    [Preserve(AllMembers = true)]
    internal class JsonPartialEntityConverter : JsonConverter<IPartiallySerializableEntity>
    {
        public override IPartiallySerializableEntity ReadJson(
            JsonReader reader,
            Type objectType,
            IPartiallySerializableEntity existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, IPartiallySerializableEntity serializableEntity, JsonSerializer serializer)
        {
            var payload = serializableEntity.Serialize();
            payload.WriteTo(writer);
        }
    }
}
