using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal class SelectWordCommand : Command
    {
        internal enum Strategy
        {
            Ws,
            NonWord,
            Word
        }
        
        public override void Execute(EditorContext context, Selection sel)
        {
            var range = SelectWord(context);

            if (range == null)
            {
                var line = context.Document.Lines[sel.Caret.Line];

                if (line.Length > 0)
                    range = new Range(new Pos(line.Index, line.Length - 1), 
                        new Pos(line.Index, line.Length));
            }

            if (range != null)
                context.Document.Selections.Set(Selection.FromRange(range));
        }

        internal static Range SelectWord(EditorContext context)
        {
            var doc = context.Document;
            var seps = context.Settings.WordSeparators;
            var caret = doc.Selections.Main.Caret;
            var line = doc.Lines[caret.Line];

            if (caret.Col == line.Length - 1)
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
                return new Range(new Pos(line.Index, start), new Pos(line.Index, end));
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
                    return pos;
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

                if (strat == Strategy.Word && (nonWord || (!nonWord && hadWs)))
                    return pos;
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
    }
}
