using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Tagada
{
    public static class CachedTypes
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> TypeProperties = new ConcurrentDictionary<string, PropertyInfo[]>();

        public static PropertyInfo[] GetTypeProperties(Type type)
        {
            return TypeProperties.GetOrAdd(type.FullName, typeFullName => type.GetProperties());
        }
    }
}
