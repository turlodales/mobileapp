using System;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using static Toggl.Shared.PropertySyncStatus;

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

        public static (PropertySyncStatus, long?) Resolve(PropertySyncStatus propertyStatus, long? local, long? backup, long? server)
        {
            var original = propertyStatus == InSync ? local : backup;
            var resolvedValue = Merge(original, local, server);
            var resolvedStatus = server == resolvedValue ? InSync : SyncNeeded;
            return (resolvedStatus, resolvedValue);
        }
        public static (PropertySyncStatus, long[]) Resolve(PropertySyncStatus propertyStatus, long[] local, long[] backup, long[] server)
        {
            local ??= Array.Empty<long>();
            backup ??= Array.Empty<long>();
            server ??= Array.Empty<long>();

            var original = propertyStatus == InSync ? local : backup;
            var resolvedValue = Merge(original, local, server);
            var resolvedStatus = server.SetEquals(resolvedValue) ? InSync : SyncNeeded;
            return (resolvedStatus, resolvedValue);
        }

        public static (PropertySyncStatus, T) Resolve<T>(PropertySyncStatus propertyStatus, T local, T backup, T server)
            where T : IEquatable<T>
        {
            var original = propertyStatus == InSync ? local : backup;
            var resolvedValue = Merge(original, local, server);
            bool isClean = server is null
                ? resolvedValue is null
                : server.Equals(resolvedValue);
            var resolvedStatus = isClean ? InSync : SyncNeeded;
            return (resolvedStatus, resolvedValue);
        }
    }
}
