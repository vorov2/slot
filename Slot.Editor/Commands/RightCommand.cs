using System;
using Slot.Editor.ObjectModel;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.right")]
    public class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveRight(Ed, sel);

        internal static Pos MoveRight(EditorControl ctx, Selection sel)
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

        private static Pos InternalMoveRight(EditorControl ctx, Selection sel, Pos pos)
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
