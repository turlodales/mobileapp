using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toggl.Core.Tests.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<T> WhereMatchesGenericType<T>(this IEnumerable<T> collection, Type typeToMatch)
        {
            foreach (var item in collection)
            {
                var type = item.GetType();

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeToMatch)
                    yield return item;
            }
        }
    }
}
