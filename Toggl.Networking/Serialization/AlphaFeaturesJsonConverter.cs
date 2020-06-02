using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Toggl.Shared;
using Serializer = Newtonsoft.Json.JsonSerializer;

namespace Toggl.Networking.Serialization
{
    [Preserve(AllMembers = true)]
    internal class AlphaFeaturesJsonConverter : JsonConverter<bool>
    {
        private string alphaFeature;
        private bool defaultValue;

        public AlphaFeaturesJsonConverter(string alphaFeature, bool defaultValue)
        {
            this.alphaFeature = alphaFeature;
            this.defaultValue = defaultValue;
        }

        public override bool ReadJson(JsonReader reader, Type objectType, [AllowNull] bool existingValue, bool hasExistingValue, Serializer serializer)
        {
            try
            {
                var array = JArray.Load(reader);
                var token = array.SelectToken($"$[?(@.code == '{alphaFeature}')].enabled");
                var value = token.Value<bool>();
                return value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public override bool CanWrite
            => false;

        public override void WriteJson(JsonWriter writer, bool value, Serializer serializer)
            => throw new NotSupportedException("Serializing alpha features is not suppoted.");
    }
}
