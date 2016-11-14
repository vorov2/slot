using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using CodeBox.Styling;

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
            Parse(new State(0, GrammarKey, false), range);
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
                var sect = grm.GetSection(lss);
                sect.Sections.ResetSelective();
                state = ParseLine(sect, ref col, i, pst);
                grm = GrammarProvider.GetGrammar(state.GrammarKey);

                while (col <= line.Length - 1)
                {
                    grm = GrammarProvider.GetGrammar(state.GrammarKey);
                    sect = grm.GetSection(state.SectionId);
                    sect.Sections.ResetSelective();
                    state = ParseLine(sect, ref col, i, pst);
                }

                var st = state.GrammarKey != GrammarKey || state.SectionId != 0 || state.MatchAnyState ? 1 : 0;
                line.State = st;
                lss = state.SectionId;
            }
        }

        private State ParseLine(GrammarSection mys, ref int i, int line, ParseState pst)
        {
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var grammar = GrammarProvider.GetGrammar(mys.GrammarKey);
            Context.AffinityManager.Associate(line, i > 0 ? i + 0 : 0, grammar.GlobalId);
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
                        style = kres;
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

                if (mys.StyleNumbers && grammar.NumberLiteral != null)
                {
                    var mc = grammar.NumberLiteral.MatchCount;
                    var num = grammar.NumberLiteral.Match(c, last);

                    if (!num && mc > 0 && nonIdent)
                        Styles.StyleRange(StandardStyle.Number, line, i - mc, i - 1);
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

                    if (sect.DontStyleCompletely)
                    {
                        var oldi = i;
                        var lineIndex = line;
                        var newCol = i;

                        if (sect.Start.MatchCount > ln.Length)
                        {
                            var shift = sect.Start.MatchCount;
                            var nln = ln;

                            do
                            {
                                shift -= nln.Length;

                                if (nln.Length == 0)
                                    shift--;

                                if (shift >= 0)
                                {
                                    lineIndex--;
                                    nln = Context.Buffer.Document.Lines[lineIndex];
                                }

                            } while (shift >= 0);

                            newCol = Math.Abs(shift) + 1;
                        }
                        else
                            newCol = i - sect.Start.MatchCount + 1;

                        sect.Start.Disabled = true;
                        var tys = mys;
                        for (var ni = lineIndex; ni < line + 1; ni++)
                        {
                            var tst = ParseLine(tys, ref newCol, ni, pst);
                            tys = grammar.Sections[tst.SectionId];
                            newCol = 0;
                        }
                        sect.Start.Disabled = false;
                        sect.Start.Reset();
                        i = oldi;
                    }

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
                    else if (ret.SectionId != mys.Id || ret.GrammarKey != grammar.GrammarKey)
                    {
                        sect = ret.GrammarKey != grammar.GrammarKey
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
                        && (backm.EscapeChar == '\0' || backm.EscapeChar != last || (i >= 1 && backm.EscapeChar == ln.CharAt(i - 2))))
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
                        i -= backm.End.MatchCount;
                        i = i < 0 ? 0 : i;
                    }

                    return Fetch(mys.ParentId, backm);
                }

                if (!ws)
                    term = c;

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
                return Fetch(0, mys, mys.Sections.MatchAnyState());
        }

        private bool IsBracket(Grammar grammar, char c)
        {
            return grammar.BracketSymbols.IndexOf(c) != -1;
        }

        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t';
        }

        private State Fetch(int sectionId, GrammarSection par, bool matchAnyState = false)
        {
            return new State(sectionId, par.GrammarKey, matchAnyState);
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
}
