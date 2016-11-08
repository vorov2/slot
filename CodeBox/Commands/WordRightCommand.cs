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
    public sealed class WordRightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => WordRight(Context, sel);

        internal static Pos WordRight(IEditorContext ctx, Selection sel)
        {
            var caret = sel.Caret;
            var line = ctx.Buffer.Document.Lines[caret.Line];

            if (caret.Col < line.Length - 1)
            {
                var seps = ctx.AffinityManager.GetNonWordSymbols(caret);
                var c = line.CharAt(caret.Col);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundRight(seps, line, caret.Col, strat);
                return new Pos(caret.Line, pos);
            }
            else
                return RightCommand.MoveRight(ctx, sel);
        }
    }
}
