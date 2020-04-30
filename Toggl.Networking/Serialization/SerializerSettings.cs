using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toggl.Networking.Models;
using Toggl.Networking.Serialization.Converters;
using Toggl.Networking.Sync.Push.Serialization;

namespace Toggl.Networking.Serialization
{
    internal static class SerializerSettings
    {
        public static JsonSerializerSettings For<TContractResolver>(TContractResolver contractResolver)
            where TContractResolver : DefaultContractResolver
        {
            contractResolver.NamingStrategy = new SnakeCaseNamingStrategy();

            return new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                DateFormatString = @"yyyy-MM-dd\THH:mm:ssK",
                Converters = new List<JsonConverter>
                {
                    new EntityActionResultConverter<Client>(),
                    new EntityActionResultConverter<Project>(),
                    new EntityActionResultConverter<Tag>(),
                    new EntityActionResultConverter<Task>(),
                    new EntityActionResultConverter<TimeEntry>(),
                    new EntityActionResultConverter<Workspace>(),

                    new ActionResultConverter<Client>(),
                    new ActionResultConverter<Project>(),
                    new ActionResultConverter<Tag>(),
                    new ActionResultConverter<Task>(),
                    new ActionResultConverter<TimeEntry>(),
                    new ActionResultConverter<Workspace>(),
                    new ActionResultConverter<User>(),
                    new ActionResultConverter<Preferences>(),

                    new JsonPartialEntityConverter()
                }
            };
        }
    }
}
