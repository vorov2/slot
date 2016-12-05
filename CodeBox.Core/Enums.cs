using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Core
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
    }
}
