using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Storage;
using Toggl.Storage.Models;

namespace Toggl.Core.Tests.Mocks
{
    public sealed class MockTimeEntry : IThreadSafeTimeEntry
    {
        IDatabaseTask IDatabaseTimeEntry.Task => Task;

        IDatabaseUser IDatabaseTimeEntry.User => User;

        IDatabaseProject IDatabaseTimeEntry.Project => Project;

        IDatabaseWorkspace IDatabaseTimeEntry.Workspace => Workspace;

        IEnumerable<IDatabaseTag> IDatabaseTimeEntry.Tags => Tags;

        public long WorkspaceId { get; set; }

        public long? ProjectId { get; set; }

        public long? TaskId { get; set; }

        public bool Billable { get; set; }

        public DateTimeOffset Start { get; set; }

        public long? Duration { get; set; }

        public string Description { get; set; }

        public IEnumerable<long> TagIds { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public long UserId { get; set; }

        public long Id { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public IThreadSafeTask Task { get; set; }

        public IThreadSafeUser User { get; }

        public IThreadSafeProject Project { get; set; }

        public IThreadSafeWorkspace Workspace { get; set; }

        public IEnumerable<IThreadSafeTag> Tags { get; set; }

        public bool IsInaccessible => Workspace.IsInaccessible;

        [JsonIgnore]
        public bool IsDeletedBackup { get; set; }

        [JsonIgnore]
        public long? WorkspaceIdBackup { get; set; }

        [JsonIgnore]
        public long? ProjectIdBackup { get; set; }

        [JsonIgnore]
        public long? TaskIdBackup { get; set; }

        [JsonIgnore]
        public bool BillableBackup { get; set; }

        [JsonIgnore]
        public DateTimeOffset StartBackup { get; set; }

        [JsonIgnore]
        public long? DurationBackup { get; set; }

        [JsonIgnore]
        public string DescriptionBackup { get; set; }

        [JsonIgnore]
        public IList<long> TagIdsBackup { get; } = new List<long>();

        [JsonIgnore]
        public PropertySyncStatus IsDeletedSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus ProjectIdSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus WorkspaceIdSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus TaskIdSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus BillableSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus StartSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DurationSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus DescriptionSyncStatus { get; set; }

        [JsonIgnore]
        public PropertySyncStatus TagIdsSyncStatus { get; set; }

        public MockTimeEntry()
        {
            Tags = Tags ?? Array.Empty<IThreadSafeTag>();
            TagIds = Tags?.Select(tag => tag.Id);
        }

        public MockTimeEntry(
            long id,
            IThreadSafeWorkspace workspace,
            DateTimeOffset? start = null,
            long? duration = null,
            IThreadSafeProject project = null,
            IThreadSafeTask task = null,
            IEnumerable<IThreadSafeTag> tags = null,
            SyncStatus syncStatus = SyncStatus.InSync
        ) : this()
        {
            Id = id;
            Workspace = workspace;
            WorkspaceId = workspace.Id;
            Start = start ?? default;
            Duration = duration;
            Project = project;
            ProjectId = project?.Id;
            Task = task;
            TaskId = task?.Id;
            Tags = tags;
            TagIds = tags?.Select(tag => tag.Id);
            SyncStatus = syncStatus;
        }

        public MockTimeEntry(IThreadSafeTimeEntry entity)
        {
            Id = entity.Id;
            WorkspaceId = entity.WorkspaceId;
            ProjectId = entity.ProjectId;
            TaskId = entity.TaskId;
            Billable = entity.Billable;
            Start = entity.Start;
            Duration = entity.Duration;
            Description = entity.Description;
            TagIds = entity.TagIds;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            UserId = entity.UserId;
        }
    }
}
