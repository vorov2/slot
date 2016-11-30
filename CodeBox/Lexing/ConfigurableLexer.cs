using System;
using System.Collections.Generic;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Lexing
{
    [Export(typeof(IComponent))]
    [ComponentData("styler.default")]
    public sealed class ConfigurableLexer : IStylerComponent
    {
        private char contextChar;

        public ConfigurableLexer()
        {

        }

        public void Style(IExecutionContext context, Range range)
        {
            var ctx = (Editor)context;

            if (ctx.Buffer.GrammarKey == null)
                return;

            Editor = ctx;
            ComponentCatalog.Instance.Grammars().GetGrammar(Editor.Buffer.GrammarKey).Sections.Reset();
            Parse(new State(0, Editor.Buffer.GrammarKey, false), range);
        }

        private void Parse(State state, Range rng)
        {
            Console.WriteLine($"Start parse from {rng.Start}");
            var lss = 0;
            contextChar = '\0';
            var len = Editor.Buffer.Document.Lines.Count;

            for (var i = rng.Start.Line; i < len; i++)
            {
                var line = Lines[i];
                Editor.AffinityManager.ClearAssociations(i);
                var grm = ComponentCatalog.Instance.Grammars().GetGrammar(state.GrammarKey);
                Styles.ClearStyles(i);
                var col = 0;
                var sect = grm.Sections[lss];
                sect.Sections.ResetSelective();
                state = ParseLine(sect, ref col, i);
                grm = ComponentCatalog.Instance.Grammars().GetGrammar(state.GrammarKey);
                var st = (sect.Id != 0 || sect.GrammarKey != Editor.Buffer.GrammarKey) && sect.Multiline ? 2 : StateToBit(state);

                while (col <= line.Length)
                {
                    grm = ComponentCatalog.Instance.Grammars().GetGrammar(state.GrammarKey);
                    sect = grm.Sections[state.SectionId];
                    sect.Sections.ResetSelective();
                    state = ParseLine(sect, ref col, i);
                    st = st == 2 ? 1 : StateToBit(state);
                }

                line.State = st > 1 ? 1 : st;
                lss = state.SectionId;

                if (i >= rng.End.Line && st == 0 && (i == len - 1 || Lines[i + 1].State == 0))
                    break;
            }
        }

        private int StateToBit(State state)
        {
            return state.GrammarKey != Editor.Buffer.GrammarKey || state.SectionId != 0 || state.MatchAnyState ? 1 : 0;
        }

        private State ParseLine(GrammarSection mys, ref int i, int line)
        {
            var start = i;
            var identStart = i;
            var ln = Lines[line];
            var grammar = ComponentCatalog.Instance.Grammars().GetGrammar(mys.GrammarKey);
            Editor.AffinityManager.Associate(line, i, grammar.GlobalId);
            var wordSep = grammar.NonWordSymbols ?? Editor.Settings.NonWordSymbols;
            var backm = mys.Id == 0 && mys.BackDelegate != null ? mys.BackDelegate : mys;
            var last = '\0';
            var term = '\0';
            var lastNonIdent = true;
            var lineStart = i == 0;

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

                    if (mys.ContextChars == null || mys.ContextChars.IndexOf(contextChar) != -1)
                    {
                        style = kres;
                        contextChar = '\0';
                        if (mys.ContextChars != null)
                            Editor.CallTips.BindCallTip(
                                "<b>ToolTip header</b><br><i>Subheader for this unique tip</i><br><br>&lt;<keyword>script</keyword><keywordspecial> type</keywordspecial>=<string>\"text/csharp\"</string>&gt;<br>This is a context keyword which is only recognized in a particular context such as (&gt;) or any other different context symbol.", 
                                new Pos(line, i - mys.Keywords.Offset), new Pos(line, i));
                    }

                    Styles.StyleRange(style, line, i - mys.Keywords.Offset, i);
                    identStart = i + 1;
                    mys.Keywords.Reset();
                }
                else if (kres < 0)
                    mys.Keywords.Reset();

                var num = false;

                if (kres < 0 && mys.StyleNumbers && grammar.NumberLiteral != null)
                {
                    var mc = grammar.NumberLiteral.MatchCount;
                    num = grammar.NumberLiteral.Match(c, last);

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
                               && mys.ContextChars != null && mys.ContextChars.IndexOf(contextChar) != -1 ?
                                    mys.ContextIdentifierStyle : mys.IdentifierStyle, line, identStart, i - 1);
                            contextChar = '\0';
                        }
                    }

                    identStart = i + 1;
                }

                if (!ws && nonIdent && c!= '\0')
                    contextChar = c;

                var sect = kres >= 0 || num ? null : mys.Sections.Match(c);

                if (sect != null 
                    && (sect.Start == null || 
                    (!Overlap(mys.Sections, ln, i, sect) && (!sect.OnLineStartOnly || lineStart)
                        && (sect.Start.Length > 1 || IsNonIdent(sect.Start.First(), wordSep) || IsNonIdent(last, wordSep)))))
                {
                    if (i < ln.Length - 1 && sect.TerminatorChar == ln.CharAt(i + 1))
                    {
                        i++;
                        sect.Start.Reset();
                        continue;
                    }

                    if (mys.Style != 0)
                    {
                        var off1 = backm.DontStyleCompletely || mys.Fallback ? 0 : backm.Start != null ? backm.Start.Length : 0;
                        Styles.StyleRange(mys.Style, line, start - off1, i - (sect.Start == null ? 0 : sect.Start.Length));
                        mys.Fallback = false;
                    }

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
                                    nln = Editor.Buffer.Document.Lines[lineIndex];
                                }

                            } while (shift >= 0);

                            newCol = Math.Abs(shift) + 1;
                        }
                        else
                            newCol = i - sect.Start.MatchCount + 1;

                        newCol = newCol < 0 ? 0 : newCol;
                        sect.Start.Disabled = true;
                        var tys = mys;
                        for (var ni = lineIndex; ni < line + 1; ni++)
                        {
                            var tst = ParseLine(tys, ref newCol, ni);
                            tys = grammar.Sections[tst.SectionId];
                            
                            if (newCol >= Lines[ni].Length)
                                newCol = 0;
                            else
                                ni--;
                        }
                        sect.Start.Disabled = false;
                        sect.Start.Reset();
                        i = oldi;
                    }

                    i++;

                    if (sect.ExternalGrammarKey != null)
                    {
                        var bd = sect;
                        sect = ComponentCatalog.Instance.Grammars().GetGrammar(sect.ExternalGrammarKey).Sections[0];
                        sect.BackDelegate = bd;
                    }

                    return Fetch(sect.Id, sect);
                }
                else if (!num && backm.End != null && backm.End.Match(c) == MatchResult.Hit
                    && (backm.EscapeChar == '\0' || backm.EscapeChar != last || (i >= 1 && backm.EscapeChar == ln.CharAt(i - 2))))
                {
                    if (i < ln.Length - 1 && backm.TerminatorEndChar == ln.CharAt(i + 1))
                    {
                        i++;
                        backm.End.Reset();
                        continue;
                    }

                    if (backm.Style != 0)
                    {
                        var off1 = backm.DontStyleCompletely || backm.Fallback ? 0 : backm.Start != null ? backm.Start.Length : 0;
                        Styles.StyleRange(backm.Style, line, start - off1, i - (backm.DontStyleCompletely ? backm.End.Offset : 0));
                        backm.Fallback = false;
                    }

                    i++;
                    var parId = mys.ParentId;

                    if (backm == mys.BackDelegate)
                    {
                        mys.BackDelegate = null;
                        parId = backm.ParentId;

                        if (parId != 0)
                            ComponentCatalog.Instance.Grammars().GetGrammar(backm.GrammarKey).Sections[parId].Fallback = true;
                    }
                    else if (parId != 0)
                        grammar.Sections[parId].Fallback = true;

                    if (backm.DontStyleCompletely)
                    {
                        i -= backm.End.MatchCount;
                        i = i < 0 ? 0 : i;
                    }

                    return Fetch(parId, backm);
                }

                if (!ws)
                {
                    term = c;
                    lineStart = false;
                }

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

        private State Fetch(int sectionId, GrammarSection sect, bool matchAnyState = false)
        {
            return new State(sectionId, sect.GrammarKey, matchAnyState);
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

        internal List<Line> Lines => Editor.Buffer.Document.Lines;

        internal StyleManager Styles => Editor.Styles;

        public Editor Editor { get; private set; }
    }
}
