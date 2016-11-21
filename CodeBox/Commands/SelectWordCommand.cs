using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.selectword", "esw")]
    public class SelectWordCommand : EditorCommand
    {
        internal enum Strategy
        {
            Ws,
            NonWord,
            Word
        }

        protected override ActionResults Execute(Selection sel)
        {
            if (sel.Caret != View.Caret)
                return Clean;

            var caret = sel.Caret;
            var range = SelectWord(View, caret);
            
            if (range == null)
            {
                var line = Document.Lines[sel.Caret.Line];

                if (line.Length > 0)
                    range = new Range(new Pos(sel.Caret.Line, line.Length - 1), 
                        new Pos(sel.Caret.Line, line.Length));
            }

            if (range != null)
            {
                Buffer.Selections.Remove(sel);
                Select(range);
            }

            return Clean | Scroll;
        }

        protected virtual void Select(Range range) => Buffer.Selections.Set(Selection.FromRange(range));

        internal static Range SelectWord(IEditorView ctx, Pos caret)
        {
            var doc = ctx.Buffer.Document;
            var line = doc.Lines[caret.Line];
            var seps = ctx.AffinityManager.GetAffinity(caret).GetNonWordSymbols(ctx);

            if (caret.Col >= line.Length - 1)
                return null;
            else
            {
                var caretColMin = caret.Col > 0 ? caret.Col - 1 : 0;
                var c = line.CharAt(caretColMin);
                var strat = GetStrategy(seps, c);
                var start = FindBoundLeft(seps, line, caretColMin, strat);
                var end = FindBoundRight(seps, line, caret.Col, strat);

                if (start < 0) start = 0;

                var fc = GetStrategy(seps, line[start].Char);
                var lc = GetStrategy(seps, line[end - 1].Char);

                if (fc != strat)
                    start++;
                if (lc != strat)
                    end--;

                if (start < 0) start = 0;
                if (end > line.Length) end = line.Length;
                return new Range(new Pos(caret.Line, start), new Pos(caret.Line, end));
            }
        }

        internal static Strategy GetStrategy(string seps, char c)
        {
            return char.IsWhiteSpace(c) || c == '\t' ? Strategy.Ws
                    : seps.IndexOf(c) != -1 ? Strategy.NonWord
                    : Strategy.Word;
        }

        internal static int FindBoundLeft(string seps, Line line, int start, Strategy strat)
        {
            var c = '\0';
            var pos = start;

            do
            {
                c = line.CharAt(pos);
                var nonWord = seps.IndexOf(c) != -1;
                var ws = char.IsWhiteSpace(c) || c == '\t';

                if (strat == Strategy.Word && (nonWord || ws))
                    return pos;
                else if (strat == Strategy.Ws && !ws)
                {
                    if (nonWord)
                        return pos;
                    else
                        strat = Strategy.Word;
                }
                else if (strat == Strategy.NonWord && !nonWord)
                    return pos;

                pos--;
            }
            while (pos > 0);

            return pos;
        }

        internal static int FindBoundRight(string seps, Line line, int start, Strategy strat)
        {
            var c = '\0';
            var pos = start;
            var hadWs = false;

            do
            {
                c = line.CharAt(pos);
                var nonWord = seps.IndexOf(c) != -1;
                var ws = char.IsWhiteSpace(c) || c == '\t';
                var nextWs = char.IsWhiteSpace(line.CharAt(pos + 1));

                if (strat == Strategy.Word && (nonWord || (!nonWord && hadWs && !nextWs)))
                    return pos == line.Length - 1 ? line.Length : pos;
                else if (strat == Strategy.Ws && !ws)
                    return pos;
                else if (strat == Strategy.NonWord && ((!nonWord && !ws) || (nonWord && hadWs)))
                    return pos;

                if (ws) hadWs = true;
                pos++;
            }
            while (pos < line.Length);

            return pos;
        }

        public override bool SupportLimitedMode => true;
    }
}
