using Toggl.Networking.Models;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public void UpdatePreferences(IPreferences preferences)
        {
            var networkPreferences = new Preferences(preferences);
            Preferences = new UpdateAction<IPreferences>(networkPreferences);
        }
    }
}
