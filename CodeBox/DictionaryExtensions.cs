using CodeBox.Core;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Json;
using CodeBox.Core.Themes;

namespace CodeBox
{
    internal static class DictionaryExtensions
    {
        public static Color Color(this Dictionary<string, object> dict, string key)
        {
            var str = dict.String(key);
            var col = default(Color);

            if (str != null)
                col = ColorTranslator.FromHtml(str);

            return col;
        }

        public static Adornment Adornments(this Dictionary<string, object> dict)
        {
            var ad = Adornment.None;
            var str = dict.String("adornment") ?? "";
            
            if (str.Equals("line", StringComparison.OrdinalIgnoreCase)) ad = Adornment.Line;

            return ad;
        }

        public static FontStyle FontStyles(this Dictionary<string, object> dict)
        {
            var fs = FontStyle.Regular;

            if (dict.Bool("bold")) fs |= FontStyle.Bold;
            if (dict.Bool("italic")) fs |= FontStyle.Italic;
            if (dict.Bool("underline")) fs |= FontStyle.Underline;
            if (dict.Bool("strikeout")) fs |= FontStyle.Strikeout;

            return fs;
        }

        public static StandardStyle Style(this Dictionary<string, object> dict, string key)
        {
            var str = dict.String(key);
            return StandardStyleConverter.FromString(str);
        }
    }

    internal static class StandardStyleConverter
    {
        private static Dictionary<string, StandardStyle> styles;

        public static StandardStyle FromString(string str)
        {
            if (styles == null)
            {
                styles = new Dictionary<string, StandardStyle>(StringComparer.OrdinalIgnoreCase);

                foreach (var fi in typeof(StandardStyle).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute));
                    var val = (StandardStyle)fi.GetValue(null);
                    var ekey = attr != null ? attr.ToString() : fi.Name;
                    styles.Add(ekey, val);
                }
            }

            var ret = StandardStyle.Default;

            if (str != null)
                styles.TryGetValue(str, out ret);

            return ret;
        }
    }
}
