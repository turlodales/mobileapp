﻿using System;
using Toggl.Core.Extensions;
using Toggl.Core.Models.Calendar;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Storage;
using ColorHelper = Toggl.Core.Helper.Colors;

namespace Toggl.Core.Calendar
{
    public struct CalendarItem
    {
        public static string ExternalEventIdPrefix = "ExternalEvent-";

        public string Id { get; }

        public string SyncId { get; }

        public CalendarItemSource Source { get; }

        public TimeSpan? Duration { get; }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset? EndTime => StartTime + Duration;

        public string Description { get; }

        public string Project { get; }

        public string Task { get; }

        public string Client { get; }

        public string Color { get; }

        public long? TimeEntryId { get; }

        public string CalendarId { get; }

        public CalendarIconKind IconKind { get; }

        public CalendarItem(
            string id,
            string syncId,
            CalendarItemSource source,
            DateTimeOffset startTime,
            TimeSpan? duration,
            string description,
            CalendarIconKind iconKind,
            string color = ColorHelper.NoProject,
            long? timeEntryId = null,
            string calendarId = "",
            string project = "",
            string task = "",
            string client = "")
        {
            Id = id;
            SyncId = syncId;
            Source = source;
            StartTime = startTime;
            Duration = duration;
            Description = string.IsNullOrEmpty(description) ? Resources.NoDescription : description;
            Color = ColorHelper.IsValidHexColor(color) ? color : ColorHelper.NoProject;
            TimeEntryId = timeEntryId;
            CalendarId = calendarId;
            IconKind = iconKind;
            Project = project;
            Task = task;
            Client = client;
        }

        private CalendarItem(IThreadSafeTimeEntry timeEntry)
            : this(
                timeEntry.Id.ToString(),
                null,
                CalendarItemSource.TimeEntry,
                timeEntry.Start,
                timeEntry.Duration.HasValue ? TimeSpan.FromSeconds(timeEntry.Duration.Value) : null as TimeSpan?,
                timeEntry.Description,
                CalendarIconKind.None,
                timeEntry.Project?.Color ?? ColorHelper.NoProject,
                timeEntry.Id,
                project: timeEntry.Project?.Name,
                task: timeEntry.Task?.Name,
                client: timeEntry.Project?.Client?.Name)
        {
            switch (timeEntry.SyncStatus)
            {
                case SyncStatus.SyncNeeded:
                    IconKind = CalendarIconKind.Unsynced;
                    break;
                case SyncStatus.SyncFailed:
                    IconKind = CalendarIconKind.Unsyncable;
                    break;
                default:
                    break;
            }
        }

        private CalendarItem(IThreadSafeExternalCalendarEvent calendarEvent)
            : this(
                  $"{ExternalEventIdPrefix}{calendarEvent.Id}",
                  calendarEvent.SyncId,
                  CalendarItemSource.Calendar,
                  calendarEvent.StartTime,
                  calendarEvent.Duration(),
                  calendarEvent.Title,
                  CalendarIconKind.Event,
                  calendarEvent.BackgroundColor)
        {
        }

        public static CalendarItem From(IThreadSafeTimeEntry timeEntry)
            => new CalendarItem(timeEntry);

        public static CalendarItem From(IThreadSafeExternalCalendarEvent calendarEvent)
            => new CalendarItem(calendarEvent);

        public CalendarItem WithStartTime(DateTimeOffset startTime)
            => new CalendarItem(
                this.Id,
                this.SyncId,
                this.Source,
                startTime,
                this.Duration,
                this.Description,
                this.IconKind,
                this.Color,
                this.TimeEntryId,
                this.CalendarId,
                this.Project,
                this.Task,
                this.Client);

        public CalendarItem WithDuration(TimeSpan? duration)
            => new CalendarItem(
                this.Id,
                this.SyncId,
                this.Source,
                this.StartTime,
                duration,
                this.Description,
                this.IconKind,
                this.Color,
                this.TimeEntryId,
                this.CalendarId,
                this.Project,
                this.Task,
                this.Client);
    }
}
