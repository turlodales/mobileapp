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
        public PushRequest CreateProjects(IEnumerable<IProject> projects)
        {
            projects
                .Select(project => new Project(project))
                .Select(project => new CreatePushAction<Project>(project))
                .AddTo(Projects);

            return this;
        }
    }
}
