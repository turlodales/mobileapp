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
        public PushRequest UpdatePreferences(IPreferences preferences)
        {
            var networkPreferences = new Preferences(preferences);

            Preferences = new UpdatePushAction<IPreferences>(networkPreferences);

            return this;
        }
    }
}
