using System.Linq;
using Toggl.Networking.Sync.Push;
using Toggl.Networking.Serialization;
using Toggl.Shared.Models;
using Xunit;
using FluentAssertions;

namespace Toggl.Networking.Tests.Sync.Push
{
    public sealed class ResponseTests
    {
        string json = @"
        {
            ""clients"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Client A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Client A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""delete"",
                    ""meta"": {""id"": ""789""},
                    ""payload"": { ""success"": true }
                }
            ],
            ""projects"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Project A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Project A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""delete"",
                    ""meta"": {""id"": ""789""},
                    ""payload"": { ""success"": true }
                }
            ],
            ""tags"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Tag A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Tag A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""delete"",
                    ""meta"": {""id"": ""789""},
                    ""payload"": { ""success"": true }
                }
            ],
            ""tasks"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Task A"",
                            ""workspace_id"": 789,
                            ""project_id"": 123,
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Task A"",
                            ""workspace_id"": 789,
                            ""project_id"": 123,
                        }
                    }
                },
                {
                    ""type"": ""delete"",
                    ""meta"": {""id"": ""789""},
                    ""payload"": { ""success"": true }
                }
            ],
            ""time_entries"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""description"": ""Time Entry A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""description"": ""Time Entry A"",
                            ""workspace_id"": 789
                        }
                    }
                },
                {
                    ""type"": ""delete"",
                    ""meta"": {""id"": ""789""},
                    ""payload"": { ""success"": true }
                }
            ],
            ""workspaces"": [
                {
                    ""type"": ""create"",
                    ""meta"": {""client_assigned_id"": ""-123""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Workspace A""
                        }
                    }
                },
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": ""456""},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Workspace A""
                        }
                    }
                }
            ],
            ""user"": {
                ""success"": true,
                ""result"": {
                    ""fullname"": ""User A""
                }
            },
            ""preferences"": {
                ""success"": true,
                ""result"": {
                    ""duration_format"": ""classic""
                }
            }
        }";

        [Fact, LogIfTooSlow]
        public void CanDeserializeACompleteResponse()
        {
            var serializer = new JsonSerializer();

            IResponse response = serializer.Deserialize<Response>(json);

            response.Clients.Should().HaveCount(3);
            response.Clients[0].Result.Success.Should().BeTrue();
            response.Clients[1].Result.Success.Should().BeTrue();
            response.Clients[2].Result.Success.Should().BeTrue();

            response.Projects.Should().HaveCount(3);
            response.Projects[0].Result.Success.Should().BeTrue();
            response.Projects[1].Result.Success.Should().BeTrue();
            response.Projects[2].Result.Success.Should().BeTrue();

            response.Tags.Should().HaveCount(3);
            response.Tags[0].Result.Success.Should().BeTrue();
            response.Tags[1].Result.Success.Should().BeTrue();
            response.Tags[2].Result.Success.Should().BeTrue();

            response.Tasks.Should().HaveCount(3);
            response.Tasks[0].Result.Success.Should().BeTrue();
            response.Tasks[1].Result.Success.Should().BeTrue();
            response.Tasks[2].Result.Success.Should().BeTrue();

            response.TimeEntries.Should().HaveCount(3);
            response.TimeEntries[0].Result.Success.Should().BeTrue();
            response.TimeEntries[1].Result.Success.Should().BeTrue();
            response.TimeEntries[2].Result.Success.Should().BeTrue();

            response.Workspaces.Should().HaveCount(2);
            response.Workspaces[0].Result.Success.Should().BeTrue();
            response.Workspaces[1].Result.Success.Should().BeTrue();

            response.User.Success.Should().BeTrue();
            response.Preferences.Success.Should().BeTrue();
        }
    }
}
