using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace CyberChan.Extensions
{
    internal static class ReflectionPropertyAccessor
    {
        private static readonly ConcurrentDictionary<(Type Type, string PropertyName), PropertyInfo> PropertyCache = new();

        public static T? GetPropertyValue<T>(object obj, string propertyName) where T : class
        {
            if (obj is null)
                return null;

            var key = (Type: obj.GetType(), PropertyName: propertyName);

            if (!PropertyCache.TryGetValue(key, out var property))
            {
                property = key.Type.GetProperty(key.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                    PropertyCache.TryAdd(key, property);
            }

            return property?.GetValue(obj) as T;
        }
    }
}
