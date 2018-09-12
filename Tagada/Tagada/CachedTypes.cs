using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tagada
{
    public static class CachedTypes
    {
        private static readonly Dictionary<string, PropertyInfo[]> TypeProperties = new Dictionary<string, PropertyInfo[]>();

        public static PropertyInfo[] GetTypeProperties(Type type)
        {
            if (TypeProperties.ContainsKey(type.FullName))
            {
                return TypeProperties[type.FullName];
            }

            var properties = type.GetProperties();
            TypeProperties.Add(type.FullName, properties);
            return properties;
        }
    }
}
