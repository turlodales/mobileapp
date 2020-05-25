using System.Linq;
using System.Collections.Generic;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;
using Toggl.Shared;
using Newtonsoft.Json;

namespace Toggl.Networking.Sync.Push
{
    public sealed class Request
    {
        private string userAgent;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> TimeEntries { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> Projects { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> Clients { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> Tasks { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PushAction> Workspaces { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PushAction Preferences { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PushAction User { get; set; }

        public Request(string userAgent)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(userAgent, nameof(userAgent));
            this.userAgent = userAgent;
        }

        [JsonIgnore]
        public bool IsEmpty
            => (TimeEntries == null || TimeEntries.None())
            && (Tags == null || Tags.None())
            && (Projects == null || Projects.None())
            && (Clients == null || Clients.None())
            && (Tasks == null || Tasks.None())
            && (Workspaces == null || Workspaces.None())
            && Preferences == null
            && User == null;

        public void CreateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            var list = timeEntries.ToList();
            if (list.Count == 0)
                return;

            if (TimeEntries == null)
            {
                TimeEntries = new List<PushAction>();
            }

            list
                .Select(timeEntry => new TimeEntry(timeEntry) { CreatedWith = userAgent })
                .Select(timeEntry => PushAction.Create(timeEntry.Id, timeEntry))
                .AddTo(TimeEntries);
        }

        public void UpdateTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            var list = timeEntries.ToList();
            if (list.Count == 0)
                return;

            TimeEntries ??= new List<PushAction>();

            list
                .Select(te => PushAction.Update<ITimeEntry>(te.Id, te.WorkspaceId, te))
                .AddTo(TimeEntries);
        }

        public void DeleteTimeEntries(IEnumerable<ITimeEntry> timeEntries)
        {
            var list = timeEntries.ToList();
            if (list.Count == 0)
                return;

            TimeEntries ??= new List<PushAction>();

            list
                .Select(timeEntry => new TimeEntry(timeEntry))
                .Select(timeEntry => PushAction.Delete(timeEntry.Id, timeEntry.WorkspaceId))
                .AddTo(TimeEntries);
        }

        public void CreateProjects(IEnumerable<IProject> projects)
        {
            var list = projects.ToList();
            if (list.Count == 0)
                return;

            Projects ??= new List<PushAction>();

            list
                .Select(project => new Project(project))
                .Select(project => PushAction.Create(project.Id, project))
                .AddTo(Projects);
        }

        public void CreateClients(IEnumerable<IClient> clients)
        {
            var list = clients.ToList();
            if (list.Count == 0)
                return;

            Clients ??= new List<PushAction>();

            list
                .Select(client => new Client(client))
                .Select(client => PushAction.Create(client.Id, client))
                .AddTo(Clients);
        }

        public void CreateTags(IEnumerable<ITag> tags)
        {
            var list = tags.ToList();
            if (list.Count == 0)
                return;

            Tags ??= new List<PushAction>();

            list
                .Select(tag => new Tag(tag))
                .Select(tag => PushAction.Create(tag.Id, tag))
                .AddTo(Tags);
        }

        public void CreateWorkspaces(IEnumerable<IWorkspace> workspaces)
        {
            var list = workspaces.ToList();
            if (list.Count == 0)
                return;

            Workspaces ??= new List<PushAction>();

            list
                .Select(ws => new Workspace(ws))
                .Select(ws => PushAction.Create(ws.Id, ws))
                .AddTo(Workspaces);
        }

        public void UpdatePreferences(IPreferences preferences)
        {
            if (preferences == null)
                return;

            var networkPreferences = new Preferences(preferences);
            Preferences = PushAction.UpdateSingleton(networkPreferences);
        }

        public void UpdateUser(IUser user)
        {
            if (user == null)
                return;

            var networkUser = new User(user);
            User = PushAction.UpdateSingleton(networkUser);
        }
    }
}
