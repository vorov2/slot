using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos caret)
        {
            return MoveRight(Context, caret);
        }

        internal static Pos MoveRight(IEditorContext ctx, Pos pos)
        {
            var doc = ctx.Buffer.Document;
            pos = new Pos(pos.Line, pos.Col + 1);
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count)
                pos = new Pos(pos.Line + 1, 0);

            return pos;
        }
    }
}
