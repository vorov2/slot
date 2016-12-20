using System;
using System.Linq;
using System.Collections.Generic;

namespace Json
{
    public static class DictionaryExtensions
    {
        public static object Object(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res;
        }

        public static string String(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null ? res.ToString() : null;
        }

        public static int Int(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null && res is double ? (int)(double)res : 0;
        }

        public static double Double(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null && res is double ? (double)res : 0d;
        }

        public static bool Bool(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null && res is bool ? (bool)res
                : res != null ? res.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                : false;
        }

        public static T Enum<T>(this Dictionary<string, object> dict, string key) where T : struct
        {
            var str = dict.String(key);
            T res;
            System.Enum.TryParse<T>(str, true, out res);
            return res;
        }

        public static char Char(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null ? res.ToString()[0] : '\0';
        }

        public static List<T> List<T>(this Dictionary<string, object> dict, string key)
        {
            var list = Object(dict, key) as List<object>;

            if (list == null)
                return null;

            if (typeof(T) == typeof(int))
                return list.OfType<double>().Cast<T>().ToList();
            else
                return list.OfType<T>().ToList();
        }
    }
}
