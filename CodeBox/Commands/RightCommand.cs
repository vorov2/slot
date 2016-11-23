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

        internal static Pos MoveRight(IEditorView ctx, Selection sel)
        {
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col + 1);

            for (;;)
            {
                pos = InternalMoveRight(ctx, sel, pos);

                if (ctx.Buffer.Document.Lines[pos.Line].Folding.HasFlag(Folding.FoldingStates.Invisible) && pos.Line < ctx.Buffer.Document.Lines.Count)
                    pos = new Pos(pos.Line + 1, 0);
                else
                    break;
            }

            return pos;
        }

        private static Pos InternalMoveRight(IEditorView ctx, Selection sel, Pos pos)
        {
            var doc = ctx.Buffer.Document;
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count - 1)
                pos = new Pos(pos.Line + 1, 0);
            else if (pos.Col > line.Length)
                pos = new Pos(pos.Line, line.Length);

            sel.SetToRestore(pos);
            return pos;
        }

        internal override bool SupportLimitedMode => true;
    }
}
