using System;
using Toggl.Shared.Extensions;

namespace Toggl.Storage.Realm.Sync
{
    public static class ThreeWayMerge
    {
        public static long? Merge(long? original, long? local, long? server)
        {
            if (local == original)
                return server;

            if (server == original)
                return local;

            return server;
        }

        public static long[] Merge(long[] original, long[] local, long[] server)
        {
            original ??= Array.Empty<long>();
            server ??= Array.Empty<long>();

            if (local.SetEquals(original))
                return server;

            if (server.SetEquals(original))
                return local;

            return server;
        }

        public static T Merge<T>(T original, T local, T server) where T : IEquatable<T>
        {
            if (local is null)
                return server;

            if (server is null)
                return local;

            if (local.Equals(original))
                return server;

            if (server.Equals(original))
                return local;

            return server;
        }
    }
}
