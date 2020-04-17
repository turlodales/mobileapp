using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Toggl.Networking.Sync.Push;
using Toggl.Shared;

namespace Toggl.Networking.Serialization.Converters
{
    [Preserve(AllMembers = true)]
    public class ActionResultConverter<T> : JsonConverter<IActionResult<T>>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override IActionResult<T> ReadJson(
            JsonReader reader,
            Type objectType,
            IActionResult<T> existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var success = jsonObject.Value<bool>("success");
            var result = jsonObject["result"];

            if (success)
            {
                if (result == null)
                {
                    return new SuccessResult<T>();
                }

                var entity = serializer.Deserialize<T>(result.CreateReader());
                return new SuccessPayloadResult<T>(entity);
            }
            else
            {
                var error = serializer.Deserialize<ErrorMessage>(result["error_message"].CreateReader());
                return new ErrorResult<T>(error);
            }
        }

        public override void WriteJson(JsonWriter writer, IActionResult<T> value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException($"Cannot serialize {nameof(IActionResult<T>)}");
        }
    }
}
