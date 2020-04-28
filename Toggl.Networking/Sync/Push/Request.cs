using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public List<IAction> TimeEntries { get; set; } = new List<IAction>();
        public List<IAction> Tags { get; set; } = new List<IAction>();
        public List<IAction> Projects { get; set; } = new List<IAction>();
        public List<IAction> Clients { get; set; } = new List<IAction>();
        public List<IAction> Tasks { get; set; } = new List<IAction>();
        public List<IAction> Workspaces { get; set; } = new List<IAction>();

        public UpdateAction<IPreferences> Preferences { get; set; }
        public UpdateAction<IUser> User { get; set; }

        public void CreateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new CreateAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);
        }

        public void UpdateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new UpdateAction<TimeEntry>(timeEntry))
                .AddTo(TimeEntries);
        }

        public void DeleteTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            timeEntries
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => new DeleteAction(timeEntry.Id, timeEntry.WorkspaceId))
                .AddTo(TimeEntries);
        }

        public void CreateProjects(IEnumerable<IProject> projects)
        {
            projects
                .Select(project => new Project(project))
                .Select(project => new CreateAction<Project>(project))
                .AddTo(Projects);
        }

        public void CreateClients(IEnumerable<IClient> clients)
        {
            clients
                .Select(client => new Client(client))
                .Select(client => new CreateAction<Client>(client))
                .AddTo(Clients);
        }

        public void CreateTags(IEnumerable<ITag> tags)
        {
            tags
                .Select(tag => new Tag(tag))
                .Select(tag => new CreateAction<Tag>(tag))
                .AddTo(Tags);
        }

        public void UpdatePreferences(IPreferences preferences)
        {
            var networkPreferences = new Preferences(preferences);
            Preferences = new UpdateAction<IPreferences>(networkPreferences);
        }

        public void UpdateUser(IUser user)
        {
            var networkUser = new User(user);
            User = new UpdateAction<IUser>(networkUser);
        }
    }
}
