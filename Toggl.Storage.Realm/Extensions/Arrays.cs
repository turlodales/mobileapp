using System;
using System.Collections.Generic;
using System.Linq;

namespace Toggl.Storage.Realm.Extensions
{
    public static class Arrays
    {
        public static T[] NotNullOrEmpty<T>(IEnumerable<T> collection)
            => collection?.ToArray() ?? Array.Empty<T>();
    }
}
