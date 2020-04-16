using System.Linq;
using System.Collections.Generic;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public Request CreateProjects(IEnumerable<IProject> projects)
        {
            projects
                .Select(project => new Project(project))
                .Select(project => new CreateAction<Project>(project))
                .AddTo(Projects);

            return this;
        }
    }
}
