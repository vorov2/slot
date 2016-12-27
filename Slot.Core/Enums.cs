using System;
using System.Collections.Generic;
using System.Reflection;

namespace Slot.Core
{
    public static class Enums
    {
        public static IEnumerable<string> GetDisplayNames<T>() where T : struct
        {
            foreach (var fi in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute));
                yield return attr != null ? attr.ToString() : fi.Name;
            }
        }

        public static string GetDisplayName<T>(this T en) where T : struct
        {
            var fi = typeof(T).GetField(en.ToString(), BindingFlags.Public | BindingFlags.Static);
            var attr = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute)) as FieldNameAttribute;
            return attr != null ? attr.ToString() : fi.Name;
        }
    }
}
