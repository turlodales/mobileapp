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

            response.ClientResults.Should().HaveCount(3);
            response.ClientResults[0].Result.Success.Should().BeTrue();
            response.ClientResults[1].Result.Success.Should().BeTrue();
            response.ClientResults[2].Result.Success.Should().BeTrue();

            response.ProjectResults.Should().HaveCount(3);
            response.ProjectResults[0].Result.Success.Should().BeTrue();
            response.ProjectResults[1].Result.Success.Should().BeTrue();
            response.ProjectResults[2].Result.Success.Should().BeTrue();

            response.TagResults.Should().HaveCount(3);
            response.TagResults[0].Result.Success.Should().BeTrue();
            response.TagResults[1].Result.Success.Should().BeTrue();
            response.TagResults[2].Result.Success.Should().BeTrue();

            response.TaskResults.Should().HaveCount(3);
            response.TaskResults[0].Result.Success.Should().BeTrue();
            response.TaskResults[1].Result.Success.Should().BeTrue();
            response.TaskResults[2].Result.Success.Should().BeTrue();

            response.TimeEntryResults.Should().HaveCount(3);
            response.TimeEntryResults[0].Result.Success.Should().BeTrue();
            response.TimeEntryResults[1].Result.Success.Should().BeTrue();
            response.TimeEntryResults[2].Result.Success.Should().BeTrue();

            response.WorkspaceResults.Should().HaveCount(2);
            response.WorkspaceResults[0].Result.Success.Should().BeTrue();
            response.WorkspaceResults[1].Result.Success.Should().BeTrue();

            response.UserResult.Success.Should().BeTrue();
            response.PreferencesResult.Success.Should().BeTrue();
        }
    }
}
