using System.Linq;
using Realms;
using Toggl.Shared.Models;

namespace Toggl.Storage.Realm.Extensions
{
    internal static class RealmExtensions
    {
        public static T GetById<T>(this Realms.Realm realm, long id)
            where T : RealmObject, IIdentifiable
            => realm.All<T>().SingleOrDefault(x => x.Id == id);
    }
}
