using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Toggl.Networking.Network;
using Xunit;

namespace Toggl.Networking.Tests.Network
{
    public sealed class SyncApiEndpointsTests
    {
        static SyncApiEndpoints endpoints = new SyncApiEndpoints(new Uri("https://mock.toggl.com/"));

        public sealed class ThePullMethod
        {
            [Fact, LogIfTooSlow]
            public void WithoutSince()
            {
                var endpoint = endpoints.Pull();

                endpoint.Method.Should().Be(HttpMethod.Get);
                endpoint.Url.PathAndQuery.Should().Be("/pull");
            }

            [Theory, LogIfTooSlow]
            [InlineData(1588825871L)]
            [InlineData(1588847878L)]
            [InlineData(1588845214L)]
            public void WithSince(long sinceTimestamp)
            {
                var since = DateTimeOffset.FromUnixTimeSeconds(sinceTimestamp);

                var endpoint = endpoints.Pull(since);

                endpoint.Method.Should().Be(HttpMethod.Get);
                endpoint.Url.PathAndQuery.Should().Be($"/pull?since={sinceTimestamp}");
            }
        }

        public abstract class BaseGuidUsingTest
        {
            public static IEnumerable<object[]> Uuids = new[] {
                new object[] { "ab6745c0-50ed-488d-9e96-0039e095df54" },
                new object[] { "02d672b8-2a40-4902-8068-b85515e08eb6" },
                new object[] { "82a7e5e2-d165-4c41-8aae-e08045f8fcd3" }
            };
        }

        public sealed class ThePushMethod : BaseGuidUsingTest
        {
            [Theory, LogIfTooSlow]
            [MemberData(nameof(Uuids))]
            public void CorrectlyFormsTheURLWithAGuid(string uuid)
            {
                var endpoint = endpoints.Push(Guid.Parse(uuid));

                endpoint.Method.Should().Be(HttpMethod.Post);
                endpoint.Url.PathAndQuery.Should().Be($"/push/{uuid}");
            }
        }

        public sealed class TheOutstandingPushMethod : BaseGuidUsingTest
        {
            [Theory, LogIfTooSlow]
            [MemberData(nameof(Uuids))]
            public void CorrectlyFormsTheURLWithAGuid(string uuid)
            {
                var endpoint = endpoints.OutstandingPush(Guid.Parse(uuid));

                endpoint.Method.Should().Be(HttpMethod.Get);
                endpoint.Url.PathAndQuery.Should().Be($"/push/{uuid}");
            }
        }
    }
}
