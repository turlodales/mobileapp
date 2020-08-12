using System;
using System.Linq;
using Toggl.Core.Extensions;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.UI.Transformations;
using Toggl.Core.UI.ViewModels.MainLog;
using Toggl.Shared;
using Toggl.Storage;

namespace Toggl.Core.UI.Extensions
{
    public static class TimeEntryExtensions
    {
        public static TimeEntryLogItemViewModel ToViewModel(
            this IThreadSafeTimeEntry timeEntry,
            GroupId groupId,
            LogItemVisualizationIntent visualizationIntent,
            DurationFormat durationFormat,
            int indexInLog,
            int dayInLog,
            int daysInThePast)
            => new TimeEntryLogItemViewModel(
                groupId,
                new[] { timeEntry.Id },
                visualizationIntent,
                timeEntry.Billable,
                timeEntry.Project?.Active ?? true,
                timeEntry.Description,
                timeEntry.Duration.HasValue
                    ? DurationAndFormatToString.Convert(
                        TimeSpan.FromSeconds(timeEntry.Duration.Value), durationFormat)
                    : string.Empty,
                timeEntry.Project?.DisplayName(),
                timeEntry.Project?.DisplayColor(),
                timeEntry.Project?.Client?.Name,
                timeEntry.Task?.Name,
                timeEntry.TagIds.Any(),
                timeEntry.SyncStatus == SyncStatus.SyncNeeded
                    || timeEntry.SyncStatus == SyncStatus.Syncing,
                timeEntry.SyncStatus != SyncStatus.SyncFailed,
                timeEntry.IsInaccessible,
                indexInLog,
                dayInLog,
                daysInThePast,
                timeEntry.Project?.IsPlaceholder() ?? false,
                timeEntry.Task?.IsPlaceholder() ?? false);
    }
}
