using CodeBox.Affinity;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeBox.Lexing
{
    public static class GrammarReader
    {
        public static Grammar Read(string source)
        {
            var json = new Json.JsonParser(source) { SkipNulls = true };
            var dict = json.Parse() as Dictionary<string, object>;
            var tmp = default(string);

            if (dict != null)
            {
                var sectionMap = new Dictionary<string, Tuple<string, string, GrammarSection>>();
                var grammar = new Grammar
                {
                    Key = dict.String("key"),
                    BracketSymbols = dict.String("brackets"),
                    NonWordSymbols = dict.String("delimeters"),
                    CommentMask = dict.String("commentMask"),
                    NumberLiteral = (tmp = dict.String("numbers")) != null ? new NumberLiteral(tmp) :null,
                    IndentProviderKey = dict.String("indentProvider")
                };

                var rt = ProcessSection(grammar.Key, dict);
                sectionMap.Add("root", rt);
                grammar.Sections.Add(rt.Item3);
                var sections = dict.Object("sections") as List<object>;

                foreach (var o in sections)
                {
                    var nd = o as Dictionary<string, object>;
                    var tup = ProcessSection(grammar.Key, nd);
                    tup.Item3.Id = grammar.Sections.Count;
                    grammar.Sections.Add(tup.Item3) ;

                    var key = tup.Item1 ?? "root";

                    if (sectionMap.ContainsKey(key))
                        throw new CodeBoxException($"Duplicate section key '{tup.Item1}' in grammar '{grammar.Key}'.");

                    sectionMap.Add(key, tup);
                }

                foreach (var tup in sectionMap.Values)
                {
                    if (tup.Item1 == null)
                        continue;

                    var parentKey = tup.Item2 ?? "root";
                    Tuple<string, string, GrammarSection> parent = null;

                    if (!sectionMap.TryGetValue(parentKey, out parent))
                        throw new CodeBoxException($"A parent section with key '{parentKey}' not found in grammar '{grammar.Key}'.");

                    tup.Item3.ParentId = parent.Item3.Id;
                    parent.Item3.Sections.Add(tup.Item3);
                }

                return grammar;
            }

            return null;
        }

        private static Tuple<string, string, GrammarSection> ProcessSection(string grammar, Dictionary<string, object> dict)
        {
            if (dict == null)
                return null;

            string tmp;
            var ignoreCase = dict.Bool("ignoreCase");

            var sect = new GrammarSection
            {
                GrammarKey = grammar,
                ExternalGrammarKey = dict.String("grammar"),
                IgnoreCase = ignoreCase,
                ContextChars = dict.String("context"),
                ContinuationChar = dict.Char("contination"),
                EscapeChar = dict.Char("escape"),
                TerminatorChar = dict.Char("terminator"),
                Multiline = dict.Bool("multiline"),
                DontStyleCompletely = dict.Bool("dontStyleCompletely"),
                StyleBrackets = dict.Bool("styleBrackets"),
                StyleNumbers = dict.Bool("styleNumbers"),
                Style = dict.Style("style"),
                IdentifierStyle = dict.Style("identifierStyle"),
                ContextIdentifierStyle = dict.Style("contextIdentifierStyle"),
                Start = (tmp = dict.String("start")) != null ? new SectionSequence(tmp, !ignoreCase) : null,
                End = (tmp = dict.String("end")) != null ? new SectionSequence(tmp, !ignoreCase) : null,
                Keywords = ProcessKeywords(ignoreCase, dict.Object("keywords") as List<object>)
            };

            return Tuple.Create(dict.String("key"), dict.String("parent"), sect);
        }

        private static StringTable ProcessKeywords(bool ignoreCase, List<object> keywords)
        {
            var keys = new StringTable(ignoreCase);

            if (keywords != null)
            {
                foreach (var o in keywords)
                {
                    var dict = o as Dictionary<string, object>;

                    if (dict == null)
                        continue;

                    var style = dict.Style("style");
                    var words = dict.String("words");

                    if (words != null)
                        keys.AddRange(words, (int)style);
                }
            }

            return keys;
        }
    }

    internal static class DictionaryExtensions
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

        public static bool Bool(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null && res is bool ? (bool)res
                : res != null ? res.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                : false;
        }

        public static char Char(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null ? res.ToString()[0] : '\0';
        }

        private static Dictionary<string, StandardStyle> styles;
        public static StandardStyle Style(this Dictionary<string, object> dict, string key)
        {
            if (styles == null)
            {
                styles = new Dictionary<string, StandardStyle>(Json.DictionaryComparer.Instance);

                foreach (var fi in typeof(StandardStyle).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute));
                    var val = (StandardStyle)fi.GetValue(null);
                    var ekey = attr != null ? attr.ToString() : fi.Name;
                    styles.Add(ekey, val);
                }
            }

            var str = String(dict, key);
            var ret = StandardStyle.Default;

            if (str != null)
                styles.TryGetValue(str, out ret);

            return ret;
        }
    }
}
