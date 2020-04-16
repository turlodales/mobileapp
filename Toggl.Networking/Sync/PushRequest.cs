using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync
{
    public sealed partial class PushRequest
    {
        public List<IPushAction> TimeEntries { get; set; } = new List<IPushAction>();
        public List<IPushAction> Tags { get; set; } = new List<IPushAction>();
        public List<IPushAction> Projects { get; set; } = new List<IPushAction>();
        public List<IPushAction> Clients { get; set; } = new List<IPushAction>();
        public List<IPushAction> Tasks { get; set; } = new List<IPushAction>();
        public List<IPushAction> Workspaces { get; set; } = new List<IPushAction>();

        public UpdatePushAction<IPreferences> Preferences { get; set; }
        public UpdatePushAction<IUser> User { get; set; }
    }
}
