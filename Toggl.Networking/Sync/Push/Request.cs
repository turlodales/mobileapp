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
    }
}
