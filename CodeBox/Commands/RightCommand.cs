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
    public class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            return MoveRight(Context, sel);
        }

        internal static Pos MoveRight(IEditorContext ctx, Selection sel)
        {
            var doc = ctx.Buffer.Document;
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col + 1);
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count)
                pos = new Pos(pos.Line + 1, 0);

            sel.SetToRestore(pos);
            return pos;
        }
    }
}
