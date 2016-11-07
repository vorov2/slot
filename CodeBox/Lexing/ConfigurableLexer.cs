using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public sealed class ConfigurableLexer : IStylingProvider
    {
        public ConfigurableLexer()
        {
            
        }

        public void Style(IEditorContext context, Range range)
        {
            Context = context;
            GrammarProvider.GetGrammar(GrammarKey).Sections.Reset();
            Parse(new State(0, GrammarKey), range);
        }

        private int lastKw = 0;

        private void Parse(State state, Range rng)
        {
            Console.WriteLine($"Start parse from {rng.Start}");
            backDelegate = null;
            lastKw = 0;
            var lss = 0;

            for (var i = rng.Start.Line; i < rng.End.Line + 1; i++)
            {
                var grm = GrammarProvider.GetGrammar(state.GrammarKey);
                Styles.ClearStyles(i);
                var col = 0;
                var sect = grm.Sections[lss];
                sect.FoundKeyword = false;
                state = ParseLine(sect, ref col, i);
                grm = GrammarProvider.GetGrammar(state.GrammarKey);

                if (col <= Lines[i].Length - 1)
                    state = ParseLine(grm.Sections[state.SectionId], ref col, i);

                var st = state.GrammarKey != GrammarKey || state.SectionId != 0 ? 1 : 0;
                Lines[i].State = st;// lss != st ? lss : st;
                lss = state.SectionId;
            }
        }

        private GrammarSection backDelegate;
        private State ParseLine(GrammarSection mys, ref int i, int line)
        {
            var lastNum = i;
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var grammar = GrammarProvider.GetGrammar(mys.GrammarKey);
            var backm = mys.Id == 0 && backDelegate != null ? backDelegate : mys;
            var last = '\0';
            var term = '\0';
            var lastNonIdent = true;

            for (; i < ln.Length + 1; i++)
            {
                var c = ln.CharAt(i);
                var nonIdent = IsNonIdent(c);
                var kres = lastNonIdent || mys.Keywords.Offset != -1 ? mys.Keywords.Match(c) : -1;

                if (mys.StyleBrackets && IsBracket(c))
                    Styles.StyleRange((int)StandardStyle.Bracket, line, i, i);

                if (kres > 0 && IsNonIdent(ln.CharAt(i + 1)))
                {
                    lastKw = (kres >> 16) & 0xFFFF;
                    Styles.StyleRange(kres & 0xFFFF, line, i - mys.Keywords.Offset, i);
                    identStart = i + 1;
                    mys.FoundKeyword = true;
                    mys.Keywords.Reset();
                }
                else if (kres < 0)
                    mys.Keywords.Reset();

                if (nonIdent)
                {
                    if (lastNum != -1 && mys.StyleNumbers && !IsNumeric(c, last))
                    {
                        Styles.StyleRange((int)StandardStyle.Number, line, lastNum, i - 1);
                        lastNum = -1;
                    }
                    else if (!lastNonIdent && mys.IdentifierStyle != 0 && i - identStart - 1 >= 0)
                    {
                        Styles.StyleRange(
                           mys.FirstIdentifierStyle != 0 && !mys.FoundKeyword ?
                                mys.FirstIdentifierStyle : mys.IdentifierStyle, line, identStart, i - 1);
                        mys.FoundKeyword = true;
                    }

                    identStart = i + 1;
                }

                var sect = mys.Sections.Match(c);

                if (sect != null 
                    && (sect.Start == null || 
                    (!Overlap(mys.Sections, ln, i, sect) && (sect.Start.Length > 1
                        || IsNonIdent(sect.Start.First()) || IsNonIdent(last)))))
                {
                    if (mys.Style != 0)
                        Styles.StyleRange(mys.Style, line, start, ln.Length);

                    i++;

                    if (sect.ExternalGrammarKey != null)
                    {
                        backDelegate = sect;
                        sect = GrammarProvider.GetGrammar(sect.ExternalGrammarKey).Sections[0];
                    }
                    reparse:
                    var ret = ParseLine(sect, ref i, line);

                    if (i >= ln.Length - 1)
                        return ret;
                    else if (ret.SectionId != mys.Id || ret.GrammarKey != grammar.Key)
                    {
                        sect = ret.GrammarKey != grammar.Key
                            ? GrammarProvider.GetGrammar(ret.GrammarKey).Sections[ret.SectionId]
                            : grammar.Sections[ret.SectionId];
                        goto reparse;
                    }
                }
                else if (backm.End != null && backm.End.Match(c) == MatchResult.Hit
                    && (backm.EscapeChar == '\0' || backm.EscapeChar != last))
                {
                    mys.FoundKeyword = false;

                    if (backm.Style != 0)
                    {
                        var off1 = backm.DontStyleCompletely ? 0 : backm.Start != null ? backm.Start.Length : 0;
                        Styles.StyleRange(backm.Style, line, start - off1, i - (backm.DontStyleCompletely ? backm.End.Offset : 0));
                    }

                    if (backm.DontStyleCompletely)
                    {
                        i -= backm.End.Offset;
                        i = i < 0 ? 0 : i;
                    }

                    if (lastKw != 0)
                    {
                        sect = grammar.Sections[mys.ParentId].Sections.MatchByKeyword(lastKw);
                        if (sect != null)
                        {
                            lastKw = 0;

                            if (sect.ExternalGrammarKey != null)
                            {
                                backDelegate = sect;
                                sect = GrammarProvider.GetGrammar(sect.ExternalGrammarKey).Sections[0];
                            }

                            return Fetch(sect.Id, sect);
                        }
                    }

                    return Fetch(0, backm);
                }

                if (!IsWhiteSpace(c))
                    term = c;

                if (lastNum == -1 && IsDigit(c) && lastNonIdent)
                    lastNum = i;
                else if (IsWhiteSpace(c))
                    lastNum = -1;

                last = c;
                lastNonIdent = nonIdent;
            }

            var singleLineContinue = !mys.Multiline && mys.ContinuationChar != '\0' && mys.ContinuationChar == term;

            if (mys.Style != 0 && (mys.Multiline || mys.End == null))
            {
                var off = !mys.DontStyleCompletely ? mys.Start.Length : 0;
                Styles.StyleRange(mys.Style, line, start - off, ln.Length);
            }

            if (!mys.Multiline && !singleLineContinue)
                mys.FoundKeyword = false;

            if (mys.End != null && mys.Multiline || mys.End == null && singleLineContinue)
                return Fetch(mys.Id, mys);
            else
                return Fetch(0, mys);
        }

        private bool IsBracket(char c)
        {
            return c == ')' || c == '(' || c == '['
                || c == ']' || c == '{' || c == '}';
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsNumeric(char c, char last)
        {
            return c == '.'
                || c == '+' && (last == 'e' || last == 'E')
                || c == '-' && (last == 'e' || last == 'E');
        }

        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t';
        }

        private State Fetch(int sectionId, GrammarSection par)
        {
            return sectionId != 0 ? new State(sectionId, par.GrammarKey) :
                new State(par.ParentId, par.GrammarKey);
        }

        private bool Overlap(IEnumerable<GrammarSection> seq, Line ln, int col, GrammarSection sect)
        {
            return seq.TryMatch(ln, col + 1, sect) != null;
        }

        private bool IsNonIdent(char c)
        {
            return c=='\0'||c == ' '||c =='\t'||"`~!@#$%^&*()-=+[{]}\\|;:\",.<>/?".IndexOf(c) != -1;
        }

        internal List<Line> Lines => Context.Buffer.Document.Lines;

        internal StyleManager Styles => Context.Styles;

        public IEditorContext Context { get; private set; }

        public string GrammarKey { get; set; }

        public GrammarProvider GrammarProvider { get; } = new GrammarProvider();
    }

    public sealed class Grammar
    {
        public Grammar(string key)
        {
            Key = key;
        }

        public string Key { get; }
        
        public GrammarSection AddSection(GrammarSection section)
        {
            section.GrammarKey = Key;
            Sections.Add(section);
            if (section.Id != 0)
                Sections[section.ParentId].Sections.Add(section);
            return section;
        }

        internal List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }

    public sealed class GrammarSection
    {
        internal bool FoundKeyword { get; set; }

        public byte Id { get; set; }

        public string GrammarKey { get; set; }

        public string ExternalGrammarKey { get; set; }

        public byte ParentId { get; set; }

        public StringTable Keywords { get; } = new StringTable();

        public SectionSequence Start { get; set; }

        public SectionSequence End { get; set; }

        public bool DontStyleCompletely { get; set; }

        public bool Multiline { get; set; }

        public int Style { get; set; }

        public int IdentifierStyle { get; set; }

        public int FirstIdentifierStyle { get; set; }

        public bool StyleNumbers { get; set; }

        public bool StyleBrackets { get; set; }

        public int NonIdentifierStyle { get; set; }

        public int StartKeyword { get; set; }

        public char ContinuationChar { get; set; }

        public char EscapeChar { get; set; }

        public List<GrammarSection> Sections { get; } = new List<GrammarSection>();
    }

    public static class EnumerableExtensions
    {
        public static GrammarSection TryMatch(this IEnumerable<GrammarSection> sections, Line ln, int col, GrammarSection except)
        {
            foreach (var sect in sections)
            {
                if (sect == except || sect.Start == null)
                    continue;

                var res = sect.Start.TryMatch(ln.CharAt(col + 0), 0);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 1), 1);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 2), 2);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 3), 3);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 4), 4);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 5), 5);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 6), 6);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 7), 7);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 8), 8);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;

                res = sect.Start.TryMatch(ln.CharAt(col + 9), 9);
                if (res == MatchResult.Hit)
                    return sect;
                else if (res == MatchResult.Fail)
                    continue;
            }

            return null;
        }

        public static GrammarSection MatchByKeyword(this IEnumerable<GrammarSection> sections, int key)
        {
            foreach (var sect in sections)
            {
                if (sect.StartKeyword == key)
                    return sect;
            }

            return null;
        }

        public static GrammarSection Match(this IEnumerable<GrammarSection> sections, char c)
        {
            var res = MatchResult.Fail;
            var match = default(GrammarSection);
            
            foreach (var sect in sections)
            {
                if (sect.Start == null)
                    continue;

                res = sect.Start.Match(c);

                if (res == MatchResult.Hit && match == null)
                    match = sect;
                else if (res == MatchResult.Hit 
                    && sect.Start.Offset > match.Start.Offset)
                    match = sect;
            }

            return match;
        }

        public static void Reset(this IEnumerable<GrammarSection> sections)
        {
            foreach (var sect in sections)
                if (sect.Start != null)
                    sect.Start.Reset();
        }
    }

    public struct State
    {
        public State(int sectionId, string grammarKey)
        {
            SectionId = sectionId;
            GrammarKey = grammarKey;
        }

        public readonly int SectionId;

        public readonly string GrammarKey;
    }

    public sealed class GrammarProvider
    {
        private readonly Dictionary<string, Grammar> grammars = new Dictionary<string, Grammar>();

        public void RegisterGrammar(Grammar grammar)
        {
            grammars.Remove(grammar.Key);
            grammars.Add(grammar.Key, grammar);
        }

        public Grammar GetGrammar(string key)
        {
            Grammar grammar;

            if (!grammars.TryGetValue(key, out grammar))
                throw new CodeBoxException($"Grammar '{key}' not found!");

            return grammar;
        }
    }
}
