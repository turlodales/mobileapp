using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toggl.Networking.Models;
using Toggl.Networking.Serialization.Converters;
using Toggl.Networking.Sync.Push.Serialization;
using Toggl.Shared.Models;

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
                    new EntityActionResultConverter<IClient>(),
                    new EntityActionResultConverter<IProject>(),
                    new EntityActionResultConverter<ITag>(),
                    new EntityActionResultConverter<ITask>(),
                    new EntityActionResultConverter<ITimeEntry>(),
                    new EntityActionResultConverter<IWorkspace>(),

                    new ActionResultConverter<IClient, Client>(),
                    new ActionResultConverter<IProject, Project>(),
                    new ActionResultConverter<ITag, Tag>(),
                    new ActionResultConverter<ITask, Task>(),
                    new ActionResultConverter<ITimeEntry, TimeEntry>(),
                    new ActionResultConverter<IWorkspace, Workspace>(),
                    new ActionResultConverter<IUser, User>(),
                    new ActionResultConverter<IPreferences, Preferences>(),

                    new JsonPartialEntityConverter()
                }
            };
        }
    }
}
