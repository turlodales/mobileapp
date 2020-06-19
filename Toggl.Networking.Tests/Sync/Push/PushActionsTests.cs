using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Networking.Network;
using Toggl.Networking.Serialization;
using Toggl.Networking.Sync.Push;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Xunit;
using Request = Toggl.Networking.Sync.Push.Request;

namespace Toggl.Networking.Tests.Sync.Push
{
    public sealed class PushActionsTests
    {
        private const string userAgent = "Test/1.0";
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
            public long? ProjectId { get; set; }
            public long? TaskId { get; }
            public bool Billable { get; } = false;
            public DateTimeOffset Start { get; } = new DateTimeOffset(2020, 03, 22, 6, 24, 7, TimeSpan.Zero);
            public long? Duration { get; } = 9;
            public string Description { get; set; }
            public IEnumerable<long> TagIds { get; } = Array.Empty<long>();
            public long UserId { get; } = 0;
            public DateTimeOffset? ServerDeletedAt { get; }
            public DateTimeOffset At { get; } = DateTimeOffset.Now;

            #region Backup properties
            public bool HasProjectIdBackup { get; set; }
            public long? ProjectIdBackup { get; set; }
            public bool HasTaskIdBackup { get; set; }
            public long? TaskIdBackup { get; set; }
            public bool HasBillableBackup { get; set; }
            public bool BillableBackup { get; set; }
            public bool HasStartBackup { get; set; }
            public DateTimeOffset StartBackup { get; set; }
            public bool HasDurationBackup { get; set; }
            public long? DurationBackup { get; set; }
            public bool HasDescriptionBackup { get; set; }
            public string DescriptionBackup { get; set; }
            public bool HasTagIdsBackup { get; set; }
            public IList<long> TagIdsBackup { get; set; }
            #endregion

            public MockTimeEntry(long id, long wid, string description)
            {
                Id = id;
                WorkspaceId = wid;
                DescriptionBackup = Description = description;
                DurationBackup = Duration;
                ProjectIdBackup = ProjectId;
                TaskIdBackup = TaskId;
                BillableBackup = Billable;
                StartBackup = Start;
                TagIdsBackup = TagIds.ToList();
            }
        }

        [Theory, LogIfTooSlow]
        [InlineData(-1, 2, "Tag")]
        [InlineData(-12, 23, "Tag B")]
        [InlineData(-54345435, 12345, "Tag C")]
        public void CreatePushActionSerializesCorrectly(long id, long wid, string name)
        {
            var tag = new MockTag(id, wid, name);
            var request = new Request(userAgent);
            request.CreateTags(tag.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("tags[0].type").Should().Be("create");
            json.GetLong("tags[0].meta.client_assigned_id").Should().Be(id);
            json.GetLong("tags[0].payload.id").Should().Be(id);
            json.GetLong("tags[0].payload.workspace_id").Should().Be(wid);
            json.GetString("tags[0].payload.name").Should().Be(name);
        }

        [Theory, LogIfTooSlow]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   \t  \n  ")]
        public void ThrowsIfUserAgentIsNullOrEmpty(string userAgent)
        {
            Action createRequest = () => new Request(userAgent);
            createRequest.Should().Throw<ArgumentException>();
        }

        [Fact, LogIfTooSlow]
        public void CorrectlyCreatesRequestWithMultipleCreateActions()
        {
            var tags = new[]
            {
                new MockTag(1, 1, "Tag A"),
                new MockTag(2, 4, "Tag B")
            };
            var request = new Request(userAgent);
            request.CreateTags(tags);

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
            timeEntry.Description = "changed description";
            timeEntry.ProjectId = 987654321;

            var request = new Request(userAgent);
            request.UpdateTimeEntries(timeEntry.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("time_entries[0].type").Should().Be("update");
            json.GetLong("time_entries[0].meta.id").Should().Be(id);
            json.GetLong("time_entries[0].meta.workspace_id").Should().Be(wid);
            json.GetLong("time_entries[0].payload.project_id").Should().Be(timeEntry.ProjectId);
            json.GetString("time_entries[0].payload.description").Should().Be(timeEntry.Description);

            json.Get("time_entries[0].payload.task_id").Should().BeNull();
            json.Get("time_entries[0].payload.billable").Should().BeNull();
            json.Get("time_entries[0].payload.start").Should().BeNull();
            json.Get("time_entries[0].payload.duration").Should().BeNull();
        }

        [Fact, LogIfTooSlow]
        public void DeletePushActionSerializesCorrectly()
        {
            var timeEntry = new MockTimeEntry(154, 12, "Playing HL2 EP3");
            var request = new Request(userAgent);
            request.DeleteTimeEntries(timeEntry.Yield());

            var json = serializer.SerializeRoundtrip(request);

            json.GetString("time_entries[0].type").Should().Be("delete");
            json.Get("time_entries[0].payload").Should().BeNull();
            json.GetLong("time_entries[0].meta.id").Should().Be(timeEntry.Id);
            json.GetLong("time_entries[0].meta.workspace_id").Should().Be(timeEntry.WorkspaceId);
        }
    }
}
