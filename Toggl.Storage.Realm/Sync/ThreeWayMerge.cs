using System;

namespace Toggl.Storage.Realm.Sync
{
    public static class ThreeWayMerge
    {
        public static T Merge<T>(T original, T local, T server) where T : IEquatable<T>
        {
            if (local.Equals(original))
                return server;

            if (server.Equals(original))
                return local;

            return server;
        }

        public static T Merge<T>(T original, T local, T server, Func<T, T, bool> equal)
        {
            if (equal(local, original))
                return server;

            if (equal(server, original))
                return local;

            return server;
        }
    }
}
