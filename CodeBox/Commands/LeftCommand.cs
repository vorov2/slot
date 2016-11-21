using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.left", "ecl")]
    public class LeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveLeft(Document, sel);

        internal static Pos MoveLeft(Document doc, Selection sel)
        {
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col - 1);

            for (;;)
            {
                pos = InternalMoveLeft(doc, sel, pos);

                if (doc.Lines[pos.Line].Folding.Has(FoldingStates.Invisible) && pos.Line > 0)
                    pos = new Pos(pos.Line - 1, doc.Lines[pos.Line - 1].Length);
                else
                    break;
            }

            return pos;
        }

        private static Pos InternalMoveLeft(Document doc, Selection sel, Pos pos)
        {
            if (pos.Col < 0 && pos.Line > 0)
            {
                var line = doc.Lines[pos.Line - 1];
                pos = new Pos(pos.Line - 1, line.Length);
            }
            else if (pos.Col < 0)
                pos = new Pos(pos.Line, 0);

            sel.SetToRestore(pos);
            return pos;
        }

        internal override bool SupportLimitedMode => true;
    }
}
