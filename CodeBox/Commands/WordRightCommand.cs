using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal sealed class WordRightCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return WordRight(context, caret);
        }

        internal static Pos WordRight(EditorContext context, Pos caret)
        {
            var line = context.Document.Lines[caret.Line];

            if (caret.Col < line.Length - 1)
            {
                var seps = context.Settings.WordSeparators;
                var c = line.CharAt(caret.Col);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundRight(seps, line, caret.Col, strat);
                return new Pos(caret.Line, pos);
            }
            else
                return RightCommand.MoveRight(context, caret);
        }
    }
}
