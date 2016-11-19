using CodeBox.Affinity;
using CodeBox.Core;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace CodeBox.Styling
{
    using Drawing;
    using MAP = Dictionary<string, object>;

    public static class StylesReader
    {
        public static void Read(string source, IEditorContext ctx)
        {
            ColorExtensions.Clean();
            var styles = ctx.Styles;
            var settings = ctx.Settings;
            var json = new Json.JsonParser(source) { SkipNulls = true };
            var dict = json.Parse() as MAP;

            if (dict != null)
            {
                ReadStyles(dict.Object("styles") as List<object>, styles);
                ReadLineNumbersStyles(dict.Object("lineNumbers") as MAP, settings);
                ReadFoldingStyles(dict.Object("folding") as MAP, settings);
                ReadScrollBarStyles(dict.Object("scrollBars") as MAP, settings);
                ReadPopupStyles(dict.Object("popup") as MAP, settings);
                settings.CurrentLineIndicatorColor = dict.Color("currentLineColor");
                settings.CaretColor = dict.Color("caretColor");
            }
        }

        private static void ReadLineNumbersStyles(MAP dict, EditorSettings set)
        {
            set.LineNumbersForeColor = dict.Color("color");
            set.LineNumbersBackColor = dict.Color("backColor");
            set.LineNumbersCurrentForeColor = dict.Color("currentColor");
            set.LineNumbersCurrentBackColor = dict.Color("currentBackColor");
        }

        private static void ReadFoldingStyles(MAP dict, EditorSettings set)
        {
            set.FoldingForeColor = dict.Color("color");
            set.FoldingBackColor = dict.Color("backColor");
            set.FoldingActiveForeColor = dict.Color("activeColor");
        }

        private static void ReadScrollBarStyles(MAP dict, EditorSettings set)
        {
            set.ScrollForeColor = dict.Color("color");
            set.ScrollBackColor = dict.Color("backColor");
            set.ScrollActiveForeColor = dict.Color("activeColor");
        }

        private static void ReadPopupStyles(MAP dict, EditorSettings set)
        {
            set.PopupForeColor = dict.Color("color");
            set.PopupBackColor = dict.Color("backColor");
            set.PopupBorderColor = dict.Color("borderColor");
            set.PopupHoverColor = dict.Color("hoverColor");
            set.PopupSelectedColor = dict.Color("selectedColor");
        }

        private static void ReadStyles(List<object> styles, StyleManager man)
        {
            if (styles == null)
                return;

            foreach (var o in styles)
            {
                var dict = o as MAP;

                if (dict != null)
                {
                    var styleKey = dict.String("key");

                    if (styleKey != null)
                        ReadStyle(man.GetStyle(StandardStyleConverter.FromString(styleKey)), dict);
                }
            }
        }

        private static void ReadStyle(Style style, MAP dict)
        {
            if (dict == null)
                return;

            style.ForeColor = dict.Color("color");
            style.BackColor = dict.Color("backColor");
            style.FontStyle = dict.FontStyles();
        }
    }
}
