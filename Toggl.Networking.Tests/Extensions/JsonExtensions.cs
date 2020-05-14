using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Toggl.Networking.Serialization;
using Toggl.Shared.Extensions;
using Xunit;

namespace Toggl.Networking.Tests
{
    internal static class JsonExtensions
    {
        public static JObject SerializeRoundtrip(this JsonSerializer serializer, object data)
        {
            var json = serializer.Serialize(data, SerializationReason.Post);
            return JObject.Parse(json);
        }

        public static JToken Get(this JObject jObject, string path)
            => jObject.SelectToken(path);

        public static string GetString(this JObject jObject, string path)
            => jObject.SelectToken(path).ToString();

        public static long GetLong(this JObject jObject, string path)
            => long.Parse(jObject.SelectToken(path).ToString());

        public static bool ContainsProperty(this JObject jObject, string property)
            => jObject.Property(property) != null;
    }
}
