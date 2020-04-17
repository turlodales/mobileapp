using System;
using FluentAssertions;
using Toggl.Networking.Models;
using Toggl.Networking.Sync.Push;
using Toggl.Networking.Serialization;
using Xunit;

namespace Toggl.Networking.Tests.Sync.Push
{
    public sealed class UpdateActionResultTests
    {
        string validSuccessJson = @"
        {
            ""type"": ""update"",
            ""meta"": {""id"": ""123""},
            ""payload"": {
                ""success"": true,
                ""result"": {
                    ""id"": 456,
                    ""name"": ""Tag A"",
                    ""workspace_id"": 789
                }
            }
        }";

        string validErrorJson = @"
        {
            ""type"": ""update"",
            ""meta"": {""id"": 123},
            ""payload"": {
                ""success"": false,
                ""result"": {
                    ""error_message"": {
                        ""code"": 42,
                        ""default_message"": ""Cannot update the tag.""
                    }
                }
            }
        }";

        public static object[][] InvalidJsons = {
            new[] {
                @"
                {
                    ""type"": ""update"",
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": 456,
                            ""name"": ""Tag A"",
                            ""workspace_id"": 789
                        }
                    }
                }",
            },
            new[] {
                @"
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": 123},
                    ""payload"": {
                        ""success"": true,
                        ""result"": {
                            ""id"": ""not an int64"",
                            ""name"": ""Tag A"",
                            ""workspace_id"": 789
                        }
                    }
                }",
            },
            new[] {
                @"
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": 123},
                    ""payload"": { ""success"": true }
                }",
            },
            new[] {
                @"
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": 123},
                    ""payload"": { ""success"": false }
                }",
            },
            new[] {
                @"
                {
                    ""type"": ""update"",
                    ""meta"": {""id"": 123},
                    ""payload"": {
                        ""success"": false,
                        ""result"": {
                            ""code"": 42,
                            ""default_message"": ""Cannot update the tag.""
                        }
                    }
                }",
            },
        };

        [Fact, LogIfTooSlow]
        public void CanDeserializeValidSuccessActionResult()
        {
            var serializer = new JsonSerializer();

            var result = serializer.Deserialize<IEntityActionResult<Tag>>(validSuccessJson);

            result.Should().NotBeNull();
            result.Should().BeOfType<UpdateActionResult<Tag>>();
            result.Id.Should().Be(123);

            result.Result.Should().BeOfType<SuccessPayloadResult<Tag>>();
            var success = result.Result as SuccessPayloadResult<Tag>;

            success.Payload.Should().NotBeNull();
            success.Payload.Id.Should().Be(456);
            success.Payload.WorkspaceId.Should().Be(789);
            success.Payload.Name.Should().Be("Tag A");
        }

        [Fact, LogIfTooSlow]
        public void CanDeserializeValidErrorActionResult()
        {
            var serializer = new JsonSerializer();

            var result = serializer.Deserialize<IEntityActionResult<Tag>>(validErrorJson);

            result.Should().NotBeNull();
            result.Should().BeOfType<UpdateActionResult<Tag>>();
            result.Id.Should().Be(123);

            result.Result.Should().BeOfType<ErrorResult<Tag>>();
            var error = result.Result as ErrorResult<Tag>;

            error.ErrorMessage.Code.Should().Be(42);
            error.ErrorMessage.DefaultMessage.Should().Be("Cannot update the tag.");
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(InvalidJsons))]
        public void ThrowsForInvalidResults(string invalidJson)
        {
            var serializer = new JsonSerializer();

            Action deserialization = () => serializer.Deserialize<IEntityActionResult<Tag>>(invalidJson);

            deserialization.Should().Throw<DeserializationException>();
        }
    }
}
