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
        private string wordSep;

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
                Lines[i].State = st;
                lss = state.SectionId;
            }
        }

        private GrammarSection backDelegate;
        private State ParseLine(GrammarSection mys, ref int i, int line)
        {
            var lastNum = -1;
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var grammar = GrammarProvider.GetGrammar(mys.GrammarKey);
            ln.GrammarId = grammar.Id;
            wordSep = grammar.NonWordSymbols ?? Context.Settings.NonWordSymbols;
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
                    Styles.StyleRange(StandardStyle.Bracket, line, i, i);

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

                if (lastNum != -1 && mys.StyleNumbers)
                {
                    var num = IsNumeric(c, last);

                    if (!num)
                    {
                        Styles.StyleRange(StandardStyle.Number, line, lastNum, i - 1);
                        lastNum = -1;
                    }
                    else if (!nonIdent && !num)
                        lastNum = -1;
                }

                if (nonIdent)
                {
                    if (!lastNonIdent && mys.IdentifierStyle != 0 && i - identStart - 1 >= 0)
                    {
                        Styles.StyleRange(
                           mys.FirstIdentifierStyle != StandardStyle.Default && !mys.FoundKeyword ?
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

                if (lastNum == -1 && lastNonIdent && (IsDigit(c) || c == '.' && IsDigit(ln.CharAt(i + 1))))
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
            return IsDigit(c) 
                || c == '.'
                || c == 'e' && IsDigit(last)
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
            return c=='\0'
                || c == ' '
                || c =='\t'
                || wordSep.IndexOf(c) != -1;
        }

        internal List<Line> Lines => Context.Buffer.Document.Lines;

        internal StyleManager Styles => Context.Styles;

        public IEditorContext Context { get; private set; }

        public string GrammarKey { get; set; }

        public GrammarProvider GrammarProvider { get; } = new GrammarProvider();
    }
}
