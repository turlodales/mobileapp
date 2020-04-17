using Toggl.Networking.Models;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push
{
    public sealed partial class Request
    {
        public void UpdateUser(IUser user)
        {
            var networkUser = new User(user);
            User = new UpdateAction<IUser>(networkUser);
        }
    }
}
