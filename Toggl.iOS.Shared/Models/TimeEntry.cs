using System;
using System.Collections.Generic;
using Toggl.Shared;
using Toggl.Shared.Models;

namespace Toggl.iOS.Shared.Models
{
    public class TimeEntry : ITimeEntry
    {
        public long WorkspaceId { get; }
        public long? ProjectId { get; }
        public long? TaskId { get; }
        public bool Billable { get; }
        public DateTimeOffset Start { get; }
        public long? Duration { get; }
        public string Description { get; }
        public IEnumerable<long> TagIds { get; }
        public long UserId { get; }
        public long Id { get; }
        public DateTimeOffset? ServerDeletedAt { get; }
        public DateTimeOffset At { get; }

        public bool IsDeletedBackup { get; set; }
        public long? WorkspaceIdBackup { get; set; }
        public long? ProjectIdBackup { get; set; }
        public long? TaskIdBackup { get; set; }
        public bool BillableBackup { get; set; }
        public DateTimeOffset StartBackup { get; set; }
        public long? DurationBackup { get; set; }
        public string DescriptionBackup { get; set; }
        public IList<long> TagIdsBackup => throw new NotImplementedException();

        public PropertySyncStatus IsDeletedSyncStatus { get; set; }
        public PropertySyncStatus WorkspaceIdSyncStatus { get; set; }
        public PropertySyncStatus ProjectIdSyncStatus { get; set; }
        public PropertySyncStatus TaskIdSyncStatus { get; set; }
        public PropertySyncStatus BillableSyncStatus { get; set; }
        public PropertySyncStatus StartSyncStatus { get; set; }
        public PropertySyncStatus DurationSyncStatus { get; set; }
        public PropertySyncStatus DescriptionSyncStatus { get; set; }
        public PropertySyncStatus TagIdsSyncStatus { get; set; }

        public TimeEntry(long workspaceId, long? projectId, long? taskId, bool billable, DateTimeOffset start, long? duration,
                         string description, IEnumerable<long> tagIds, long userId, long id, DateTimeOffset? serverDeletedAt, DateTimeOffset at)
        {
            WorkspaceId = workspaceId;
            ProjectId = projectId;
            TaskId = taskId;
            Billable = billable;
            Start = start;
            Duration = duration;
            Description = description;
            TagIds = tagIds;
            UserId = userId;
            Id = id;
            ServerDeletedAt = serverDeletedAt;
            At = at;
        }

        public static TimeEntry From(ITimeEntry entry)
        {
            return new TimeEntry(
                entry.WorkspaceId,
                entry.ProjectId,
                entry.TaskId,
                entry.Billable,
                entry.Start,
                entry.Duration,
                entry.Description,
                entry.TagIds ?? new List<long>(),
                entry.UserId,
                entry.Id,
                entry.ServerDeletedAt,
                entry.At
            );
        }

        public TimeEntry With(long duration)
        {
            return new TimeEntry(
                this.WorkspaceId,
                this.ProjectId,
                this.TaskId,
                this.Billable,
                this.Start,
                duration,
                this.Description,
                this.TagIds,
                this.UserId,
                this.Id,
                this.ServerDeletedAt,
                this.At
            );
        }
    }
}
