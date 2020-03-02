using System;
using System.Collections.Generic;
using Toggl.Core.Analytics;
using Toggl.Shared;
using static Toggl.Core.Helper.Constants;

namespace Toggl.Core.UI.Parameters
{
    public sealed class StartTimeEntryParameters
    {
        private static readonly TimeSpan defaultManualModeDuration = TimeSpan.FromMinutes(DefaultTimeEntryDurationForManualModeInMinutes);

        public DateTimeOffset StartTime { get; }

        public string PlaceholderText { get; }

        public string EntryDescription { get; }

        public TimeSpan? Duration { get; }

        public long? WorkspaceId { get; }

        public long? ProjectId { get; }

        public long? TaskId { get; }

        public IEnumerable<long> TagIds { get; }

        public TimeEntryStartOrigin? Origin { get; }

        public StartTimeEntryParameters(
            DateTimeOffset startTime,
            string placeholderText,
            TimeSpan? duration,
            long? workspaceId,
            string entryDescription = "",
            long? projectId = null,
            long? taskId = null,
            IEnumerable<long> tagIds = null,
            TimeEntryStartOrigin? origin = null)
        {
            StartTime = startTime;
            PlaceholderText = placeholderText;
            Duration = duration;
            WorkspaceId = workspaceId;
            EntryDescription = entryDescription;
            ProjectId = projectId;
            TaskId = taskId;
            TagIds = tagIds;
            Origin = origin;
        }

        public static StartTimeEntryParameters ForCalendarTapAndDrag(DateTimeOffset startTime)
            => new StartTimeEntryParameters(
                startTime: startTime.Subtract(defaultManualModeDuration),
                placeholderText: Resources.ManualTimeEntryPlaceholder,
                duration: defaultManualModeDuration,
                workspaceId: null,
                origin: TimeEntryStartOrigin.CalendarTapAndDrag);

        public static StartTimeEntryParameters ForManualMode(DateTimeOffset now, bool fromLongPress)
            => new StartTimeEntryParameters(
                startTime: now.Subtract(defaultManualModeDuration),
                placeholderText: Resources.ManualTimeEntryPlaceholder,
                duration: defaultManualModeDuration,
                workspaceId: null,
                origin: fromLongPress ? TimeEntryStartOrigin.ManualLongPress : TimeEntryStartOrigin.Manual);

        public static StartTimeEntryParameters ForTimerMode(DateTimeOffset now, bool fromLongPress)
            => new StartTimeEntryParameters(
                startTime: now,
                placeholderText: Resources.StartTimeEntryPlaceholder,
                duration: null,
                workspaceId: null,
                origin: fromLongPress ? TimeEntryStartOrigin.TimerLongPress : TimeEntryStartOrigin.Timer);
    }
}
