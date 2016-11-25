using CodeBox.Affinity;
using CodeBox.Core;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CodeBox.Drawing;
using Json;

namespace CodeBox.Styling
{
    using MAP = Dictionary<string, object>;

    public static class ThemeReader
    {
        public static void Read(string source, ThemeComponent comp)
        {
            ColorExtensions.Clean();
            var json = new Json.JsonParser(source) { SkipNulls = true };
            var list = json.Parse() as List<object>;

            if (list != null)
                ReadStyles(list, comp);
        }

        private static void ReadStyles(List<object> styles, ThemeComponent coll)
        {
            foreach (var o in styles)
            {
                var dict = o as MAP;

                if (dict != null)
                {
                    var styleKey = dict.String("key");

                    if (styleKey != null)
                        ReadStyle(coll, styleKey, dict);
                }
            }
        }

        private static void ReadStyle(ThemeComponent coll, string styleKey, MAP dict)
        {
            if (dict == null)
                return;

            var style = coll.GetStyle(StandardStyleConverter.FromString(styleKey));
            style.ForeColor = dict.Color("color");
            style.BackColor = dict.Color("backColor");
            style.FontStyle = dict.FontStyles();
            var ms = style as MarginStyle;

            if (ms != null)
            {
                ms.ActiveBackColor = dict.Color("activeBackColor");
                ms.ActiveForeColor = dict.Color("activeColor");
            }
            else
            {
                var ps = style as PopupStyle;

                if (ps != null)
                {
                    ps.HoverColor = dict.Color("hoverColor");
                    ps.SelectedColor = dict.Color("selectedColor");
                    ps.BorderColor = dict.Color("borderColor");
                }
            }
        }
    }
}
