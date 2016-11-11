using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveRight(Context, sel);

        internal static Pos MoveRight(IEditorContext ctx, Selection sel)
        {
            var doc = ctx.Buffer.Document;
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col + 1);
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count - 1)
                pos = new Pos(pos.Line + 1, 0);
            else if (pos.Col > line.Length)
                pos = new Pos(pos.Line, line.Length);

            sel.SetToRestore(pos);
            return pos;
        }
    }
}
