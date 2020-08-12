using Toggl.Networking.Sync.Pull;
using Toggl.Networking.Serialization;
using Toggl.Shared;
using Xunit;
using FluentAssertions;

namespace Toggl.Networking.Tests.Sync.Pull
{
    public sealed class ResponseTests
    {
        string json = @"
        {
            ""server_time"": 1588666300,
            ""clients"": [
                {
                    ""id"": 456,
                    ""name"": ""Client A"",
                    ""workspace_id"": 789
                }
            ],
            ""projects"": [
                {
                    ""id"": 456,
                    ""name"": ""Project A"",
                    ""workspace_id"": 789
                }
            ],
            ""tags"": [
                {
                    ""id"": 456,
                    ""name"": ""Tag A"",
                    ""workspace_id"": 789
                }
            ],
            ""tasks"": [
                {
                    ""id"": 456,
                    ""name"": ""Task A"",
                    ""workspace_id"": 789,
                    ""project_id"": 123,
                }
            ],
            ""time_entries"": [
                {
                    ""id"": 456,
                    ""description"": ""Time Entry A"",
                    ""workspace_id"": 789
                }
            ],
            ""workspaces"": [
                {
                    ""id"": 456,
                    ""name"": ""Workspace A""
                }
            ],
            ""user"": {
                ""fullname"": ""User A""
            },
            ""preferences"": {
                ""duration_format"": ""classic""
            }
        }";

        [Fact, LogIfTooSlow]
        public void CanDeserializeACompleteResponse()
        {
            var serializer = new JsonSerializer();

            IResponse response = serializer.Deserialize<Response>(json);

            response.ServerTime.Should().Be(1588666300);

            response.Clients.Should().HaveCount(1);
            response.Clients[0].Name.Should().Be("Client A");

            response.Projects.Should().HaveCount(1);
            response.Projects[0].Name.Should().Be("Project A");

            response.Tags.Should().HaveCount(1);
            response.Tags[0].Name.Should().Be("Tag A");

            response.Tasks.Should().HaveCount(1);
            response.Tasks[0].Name.Should().Be("Task A");

            response.TimeEntries.Should().HaveCount(1);
            response.TimeEntries[0].Description.Should().Be("Time Entry A");

            response.Workspaces.Should().HaveCount(1);
            response.Workspaces[0].Name.Should().Be("Workspace A");

            response.User.Fullname.Should().Be("User A");
            response.Preferences.DurationFormat.Should().Be(DurationFormat.Classic);
        }

        [Fact, LogIfTooSlow]
        public void CanHandleAnMinimalResponse()
        {
            var serializer = new JsonSerializer();

            IResponse response = serializer.Deserialize<Response>("{\"server_time\":1588666300}");

            response.ServerTime.Should().Be(1588666300);
            response.Clients.Should().HaveCount(0);
            response.Projects.Should().HaveCount(0);
            response.Tags.Should().HaveCount(0);
            response.Tasks.Should().HaveCount(0);
            response.TimeEntries.Should().HaveCount(0);
            response.Workspaces.Should().HaveCount(0);
            response.User.Should().BeNull();
            response.Preferences.Should().BeNull();
        }
    }
}
