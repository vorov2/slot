using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.right")]
    public class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveRight(View, sel);

        internal static Pos MoveRight(Editor ctx, Selection sel)
        {
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col + 1);

            for (;;)
            {
                pos = InternalMoveRight(ctx, sel, pos);

                if (!ctx.Folding.IsLineVisible(pos.Line) && pos.Line < ctx.Buffer.Document.Lines.Count)
                    pos = new Pos(pos.Line + 1, 0);
                else
                    break;
            }

            return pos;
        }

        private static Pos InternalMoveRight(Editor ctx, Selection sel, Pos pos)
        {
            var doc = ctx.Buffer.Document;
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count - 1)
                pos = new Pos(pos.Line + 1, 0);
            else if (pos.Col > line.Length)
                pos = new Pos(pos.Line, line.Length);

            sel.SetToRestore(ctx.Document.Lines[pos.Line].GetStripeCol(pos.Col));
            return pos;
        }

        internal override bool SupportLimitedMode => true;
    }
}
