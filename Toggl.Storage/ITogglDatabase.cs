﻿using System;
using System.Reactive;
using Toggl.Storage.Models;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Storage
{
    public interface ITogglDatabase
    {
        ISingleObjectStorage<IDatabaseUser> User { get; }
        IRepository<IDatabaseClient> Clients { get; }
        IRepository<IDatabaseProject> Projects { get; }
        ISingleObjectStorage<IDatabasePreferences> Preferences { get; }
        IRepository<IDatabaseTag> Tags { get; }
        IRepository<IDatabaseTask> Tasks { get; }
        IRepository<IDatabaseTimeEntry> TimeEntries { get; }
        IRepository<IDatabaseWorkspace> Workspaces { get; }
        IRepository<IDatabaseWorkspaceFeatureCollection> WorkspaceFeatures { get; }
        IRepository<IDatabaseExternalCalendar> ExternalCalendars { get; }
        IRepository<IDatabaseExternalCalendarEvent> ExternalCalendarEvents { get; }
        IIdProvider IdProvider { get; }
        ISinceParameterRepository SinceParameters { get; }
        IPushRequestIdentifierRepository PushRequestIdentifier { get; }
        IObservable<Unit> Clear();
    }
}
