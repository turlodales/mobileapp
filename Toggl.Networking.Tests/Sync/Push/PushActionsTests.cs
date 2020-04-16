using FluentAssertions;
using System;
using System.Collections.Generic;
using Toggl.Networking.Serialization;
using Toggl.Networking.Sync.Push;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Xunit;

namespace Toggl.Networking.Tests.Sync.Push
{
    public sealed class PushActionsTests
    {
        private JsonSerializer serializer => new JsonSerializer();

        public class MockTag : ITag
        {
            public long Id { get; private set; }
            public long WorkspaceId { get; private set; }
            public string Name { get; private set; }

            public DateTimeOffset? ServerDeletedAt => null;
            public DateTimeOffset At => DateTimeOffset.Now;

            public MockTag(long id, long wid, string name)
            {
                Id = id;
                WorkspaceId = wid;
                Name = name;
            }
        }

        public class MockTimeEntry : ITimeEntry
        {
            public long Id { get; }
            public long WorkspaceId { get; }
            public long? ProjectId { get; }
            public long? TaskId { get; }
            public bool Billable { get; } = false;
            public DateTimeOffset Start { get; } = new DateTimeOffset(2020, 03, 22, 6, 24, 7, TimeSpan.Zero);
            public long? Duration { get; } = 9;
            public string Description { get; }
            public IEnumerable<long> TagIds { get; } = Array.Empty<long>();
            public long UserId { get; } = 0;
            public DateTimeOffset? ServerDeletedAt { get; }
            public DateTimeOffset At { get; } = DateTimeOffset.Now;

            public MockTimeEntry(long id, long wid, string description)
            {
                Id = id;
                WorkspaceId = wid;
                Description = description;
            }
        }

        [Theory, LogIfTooSlow]
        [InlineData(-1, 2, "Tag")]
        [InlineData(-12, 23, "Tag B")]
        [InlineData(-54345435, 12345, "Tag C")]
        public void CreatePushActionSerializesCorrectly(long id, long wid, string name)
        {
            var tag = new MockTag(id, wid, name);
            var request = new Request().CreateTags(tag.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("tags[0].type").Should().Be("create");
            json.GetLong("tags[0].meta.client_assigned_id").Should().Be(id);
            json.GetLong("tags[0].payload.id").Should().Be(id);
            json.GetLong("tags[0].payload.workspace_id").Should().Be(wid);
            json.GetString("tags[0].payload.name").Should().Be(name);
        }

        [Fact, LogIfTooSlow]
        public void CorrectlyCreatesRequestWithMultipleCreateActions()
        {
            var tags = new[]
            {
                new MockTag(1, 1, "Tag A"),
                new MockTag(2, 4, "Tag B")
            };
            var request = new Request().CreateTags(tags);

            var json = serializer.SerializeRoundtrip(request);

            for (int i = 0; i <= 1; i++)
            {
                var tag = tags[i];
                json.GetString($"tags[{i}].type").Should().Be("create");
                json.GetLong($"tags[{i}].meta.client_assigned_id").Should().Be(tag.Id);
                json.GetLong($"tags[{i}].payload.id").Should().Be(tag.Id);
                json.GetLong($"tags[{i}].payload.workspace_id").Should().Be(tag.WorkspaceId);
                json.GetString($"tags[{i}].payload.name").Should().Be(tag.Name);
            }
        }

        [Theory, LogIfTooSlow]
        [InlineData(-1, 2, "Some time entry")]
        [InlineData(-12, 23, "Random work")]
        [InlineData(-54345435, 12345, "Playing HL2 EP3")]
        public void UpdatePushActionSerializesCorrectly(long id, long wid, string description)
        {
            var timeEntry = new MockTimeEntry(id, wid, description);
            var request = new Request().UpdateTimeEntries(timeEntry.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("time_entries[0].type").Should().Be("update");
            json.Get("time_entries[0].meta").Should().BeNull();
            json.GetLong("time_entries[0].payload.id").Should().Be(id);
            json.GetLong("time_entries[0].payload.workspace_id").Should().Be(wid);
            json.GetString("time_entries[0].payload.description").Should().Be(description);
        }

        [Fact, LogIfTooSlow]
        public void DeletePushActionSerializesCorrectly()
        {
            var timeEntry = new MockTimeEntry(154, 12, "Playing HL2 EP3");
            var request = new Request().DeleteTimeEntries(timeEntry.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("time_entries[0].type").Should().Be("delete");
            json.Get("time_entries[0].payload").Should().BeNull();
            json.GetLong("time_entries[0].meta.id").Should().Be(timeEntry.Id);
            json.GetLong("time_entries[0].meta.workspace_id").Should().Be(timeEntry.WorkspaceId);
        }
    }
}
