using System.Linq;
using System.Collections.Generic;
using Toggl.Networking.Models;
using Toggl.Shared.Models;
using Toggl.Shared.Extensions;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public Request CreateTags(IEnumerable<ITag> tags)
        {
            tags
                .Select(tag => new Tag(tag))
                .Select(tag => new CreateAction<Tag>(tag))
                .AddTo(Tags);

            return this;
        }
    }
}
