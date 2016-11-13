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

        private void Parse(State state, Range rng)
        {
            Console.WriteLine($"Start parse from {rng.Start}");
            var lss = 0;
            var pst = new ParseState();

            for (var i = rng.Start.Line; i < rng.End.Line + 1; i++)
            {
                var line = Lines[i];
                Context.AffinityManager.ClearAssociations(i);
                var grm = GrammarProvider.GetGrammar(state.GrammarKey);
                Styles.ClearStyles(i);
                var col = 0;
                var sect = grm.Sections[lss];
                sect.Sections.Reset();
                state = ParseLine(sect, ref col, i, pst);
                grm = GrammarProvider.GetGrammar(state.GrammarKey);

                while (col <= line.Length - 1)
                {
                    grm = GrammarProvider.GetGrammar(state.GrammarKey);
                    sect = grm.Sections[state.SectionId];
                    sect.Sections.Reset();
                    state = ParseLine(sect, ref col, i, pst);
                }

                var st = state.GrammarKey != GrammarKey || state.SectionId != 0 ? 1 : 0;
                line.State = st;
                lss = state.SectionId;
            }
        }

        private State ParseLine(GrammarSection mys, ref int i, int line, ParseState pst)
        {
            var lastNum = -1;
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var grammar = GrammarProvider.GetGrammar(mys.GrammarKey);
            Context.AffinityManager.Associate(line, i > 0 ? i + 0 : 0, grammar.Id);
            var wordSep = grammar.NonWordSymbols ?? Context.Settings.NonWordSymbols;
            var backm = mys.Id == 0 && pst.BackDelegate != null ? pst.BackDelegate : mys;
            var last = '\0';
            var term = '\0';
            var lastNonIdent = true;

            for (; i < ln.Length + 1; i++)
            {
                var c = ln.CharAt(i);
                var nonIdent = IsNonIdent(c, wordSep);
                var ws = IsWhiteSpace(c);
                var kres = lastNonIdent || mys.Keywords.Offset != -1 ? mys.Keywords.Match(c) : -1;

                if (mys.StyleBrackets && IsBracket(grammar, c))
                    Styles.StyleRange(StandardStyle.Bracket, line, i, i);

                if (kres > 0 && IsNonIdent(ln.CharAt(i + 1), wordSep))
                {
                    var style = (int)mys.IdentifierStyle;

                    if (mys.ContextChars == null || mys.ContextChars.IndexOf(pst.Context) != -1)
                    {
                        style = kres & 0xFFFF;
                        pst.LastKeyword = (kres >> 16) & 0xFFFF;
                        pst.Context = '\0';
                        if (mys.ContextChars != null)
                            Context.CallTips.BindCallTip(
                                "<b>ToolTip header</b><br><i>Subheader for this unique tip</i><br><br>&lt;<keyword>script</keyword><keywordspecial> type</keywordspecial>=<string>\"text/csharp\"</string>&gt;<br>This is a context keyword which is only recognized in a particular context such as (&gt;) or any other different context symbol.", 
                                new Pos(line, i - mys.Keywords.Offset), new Pos(line, i));
                    }

                    Styles.StyleRange(style, line, i - mys.Keywords.Offset, i);
                    identStart = i + 1;
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
                    if (!lastNonIdent && i - identStart - 1 >= 0)
                    {
                        if (mys.IdentifierStyle != 0)
                        {
                            Styles.StyleRange(
                               mys.ContextIdentifierStyle != StandardStyle.Default
                               && mys.ContextChars != null && mys.ContextChars.IndexOf(pst.Context) != -1 ?
                                    mys.ContextIdentifierStyle : mys.IdentifierStyle, line, identStart, i - 1);
                            pst.Context = '\0';
                        }
                    }

                    identStart = i + 1;
                }

                if (!ws && nonIdent)
                    pst.Context = c;

                var sect = mys.Sections.Match(c);

                if (sect != null 
                    && (sect.Start == null || 
                    (!Overlap(mys.Sections, ln, i, sect) && (sect.Start.Length > 1
                        || IsNonIdent(sect.Start.First(), wordSep) || IsNonIdent(last, wordSep)))))
                {
                    if (mys.Style != 0)
                        Styles.StyleRange(mys.Style, line, start, ln.Length);

                    i++;
                    mys.Sections.Reset();

                    if (sect.ExternalGrammarKey != null)
                    {
                        pst.BackDelegate = sect;
                        sect = GrammarProvider.GetGrammar(sect.ExternalGrammarKey).Sections[0];
                    }
                    reparse:
                    var ret = ParseLine(sect, ref i, line, pst);

                    if (i >= ln.Length - 1)
                        return ret;
                    else if (ret.SectionId != mys.Id || ret.GrammarKey != grammar.Key)
                    {
                        sect = ret.GrammarKey != grammar.Key
                            ? GrammarProvider.GetGrammar(ret.GrammarKey).Sections[ret.SectionId]
                            : grammar.Sections[ret.SectionId];
                        goto reparse;
                    }
                    else
                    {
                        i--;
                        lastNonIdent = true;
                        identStart = i + 1;
                    }
                }
                else if (backm.End != null && backm.End.Match(c) == MatchResult.Hit
                        && (backm.EscapeChar == '\0' || backm.EscapeChar != last))
                {
                    if (backm.Style != 0)
                    {
                        var off1 = backm.DontStyleCompletely ? 0 : backm.Start != null ? backm.Start.Length : 0;
                        Styles.StyleRange(backm.Style, line, start - off1, i - (backm.DontStyleCompletely ? backm.End.Offset : 0));
                    }

                    i++;
                    
                    if (backm == pst.BackDelegate)
                        pst.BackDelegate = null;

                    if (backm.DontStyleCompletely)
                    {
                        i -= backm.End.Offset;
                        i = i < 0 ? 0 : i;
                    }

                    if (pst.LastKeyword != 0)
                    {
                        sect = grammar.Sections[mys.ParentId].Sections.MatchByKeyword(pst.LastKeyword);
                        if (sect != null)
                        {
                            pst.LastKeyword = 0;

                            if (sect.ExternalGrammarKey != null)
                            {
                                pst.BackDelegate = sect;
                                sect = GrammarProvider.GetGrammar(sect.ExternalGrammarKey).Sections[0];
                            }

                            return Fetch(sect.Id, sect);
                        }
                    }

                    return Fetch(0, backm);
                }

                if (!ws)
                    term = c;
                
                if (lastNum == -1 && lastNonIdent && (IsDigit(c) || c == '.' && IsDigit(ln.CharAt(i + 1))))
                    lastNum = i;
                else if (ws)
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

            if (mys.End != null && mys.Multiline || mys.End == null && singleLineContinue)
                return Fetch(mys.Id, mys);
            else
                return Fetch(0, mys);
        }

        private bool IsBracket(Grammar grammar, char c)
        {
            return grammar.BracketSymbols.IndexOf(c) != -1;
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

        private bool IsNonIdent(char c, string wordSep)
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

    internal sealed class ParseState
    {
        public char Context;
        public int LastKeyword;
        public GrammarSection BackDelegate;
    }
}
