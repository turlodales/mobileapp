using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Toggl.Networking.Sync.Push;
using Toggl.Shared;

namespace Toggl.Networking.Serialization.Converters
{
    [Preserve(AllMembers = true)]
    public class EntityActionResultConverter<T> : JsonConverter<IEntityActionResult<T>>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override IEntityActionResult<T> ReadJson(
            JsonReader reader,
            Type objectType,
            IEntityActionResult<T> existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var type = jsonObject.Value<string>("type");
            var meta = jsonObject["meta"];
            var payload = jsonObject["payload"];
            var result = serializer.Deserialize<IActionResult<T>>(payload.CreateReader());

            switch (type)
            {
                case "create":
                    {
                        var clientId = meta.Value<string>("client_assigned_id");
                        var id = long.Parse(clientId);

                        if (result is SuccessResult<T>)
                        {
                            throw new MissingFieldException("The result of the `create` action must contain the created entity in the case of a success.");
                        }

                        return new CreateActionResult<T>(id, result);
                    }

                case "update":
                    {
                        var id = meta.Value<long>("id");
                        return new UpdateActionResult<T>(id, result);
                    }

                case "delete":
                    {
                        var id = meta.Value<long>("id");
                        return new DeleteActionResult<T>(id, result);
                    }

                default:
                    throw new ArgumentOutOfRangeException($"The action result type '{type}' is not supported.");
            }
        }

        public override void WriteJson(JsonWriter writer, IEntityActionResult<T> value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException($"Cannot serialize {nameof(IEntityActionResult<T>)}");
        }
    }
}
