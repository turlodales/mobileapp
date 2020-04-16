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
        public PushRequest CreateTags(IEnumerable<ITag> tags)
        {
            tags
                .Select(tag => new Tag(tag))
                .Select(tag => new CreatePushAction<Tag>(tag))
                .AddTo(Tags);

            return this;
        }
    }
}
