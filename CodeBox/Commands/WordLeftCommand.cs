using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    public sealed class WordLeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos caret)
        {
            return WordLeft(Context, caret);
        }

        internal static Pos WordLeft(IEditorContext ctx, Pos caret)
        {
            var line = ctx.Buffer.Document.Lines[caret.Line];

            if (caret.Col > 0)
            {
                var seps = ctx.Settings.WordSeparators;
                var c = line.CharAt(caret.Col - 1);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundLeft(seps, line, caret.Col - 1, strat);

                if (Math.Abs(pos - caret.Col) > 1 && pos > 0)
                    pos++;

                return new Pos(caret.Line, pos);
            }
            else
                return LeftCommand.MoveLeft(ctx.Buffer.Document, caret);
        }
    }
}
