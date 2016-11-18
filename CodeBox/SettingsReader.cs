using CodeBox.Affinity;
using CodeBox.Core;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace CodeBox
{
    using MAP = Dictionary<string, object>;

    public static class SettingsReader
    {
        public static void Read(string source, IEditorContext ctx)
        {
            var set = ctx.Settings;
            var json = new Json.JsonParser(source) { SkipNulls = true };
            var dict = json.Parse() as MAP;

            if (dict != null)
            {
                set.Font = CreateFont(dict);
                set.LinePadding = dict.Double("editor.linePadding");
                set.ShowWhitespace = dict.Bool("editor.showWhitespace");
                set.ShowEol = dict.Bool("editor.showEol");
                set.ShowLineLength = dict.Bool("editor.showLineLength");
                set.CurrentLineIndicator = dict.Bool("editor.currentLineIndicator");
                set.MatchBrackets = dict.Bool("editor.matchBrackets");
                set.UseTabs = dict.Bool("editor.useTabs");
                set.WordWrap = dict.Bool("editor.wordWrap");
                set.NonWordSymbols = dict.String("editor.delimeters");
                set.BracketSymbols = dict.String("editor.brackets");
                set.IndentSize = dict.Int("editor.indentSize");
                set.WordWrapColumn = dict.Int("editor.wordWrapColumn");
                set.Eol = GetLineEndings(dict);

                var lst = dict.Object("editor.longLineIndicators") as List<object>;

                if (lst != null)
                {
                    foreach (var s in lst)
                        if (s is int)
                            set.LongLineIndicators.Add((int)s);
                }
            }
        }

        private static Eol GetLineEndings(MAP dic)
        {
            var str = dic.String("editor.lineEndings");
            return string.Equals(str, "lf", StringComparison.OrdinalIgnoreCase) ? Eol.Lf
                : string.Equals(str, "cr", StringComparison.OrdinalIgnoreCase) ? Eol.Cr
                : string.Equals(str, "crlf", StringComparison.OrdinalIgnoreCase) ? Eol.CrLf
                : Eol.Auto;
        }

        private static Font CreateFont(MAP dict)
        {
            const int DEFSIZE = 11;
            var fn = dict.String("editor.font");
            var fs = dict.Int("editor.fontSize");
            fs = fs == 0 ? DEFSIZE : fs;
            var ret = string.IsNullOrWhiteSpace(fn)
                ? new Font(FontFamily.GenericMonospace, fs)
                : new Font(fn, fs);
            return ret.Name != ret.OriginalFontName ?
                new Font(FontFamily.GenericMonospace, fs) : ret;
        }
    }
}
