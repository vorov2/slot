using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core;
using Slot.Core.Settings;
using Slot.Core.ViewModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.down")]
    public class DownCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveDown(Ed, sel);

        internal static Pos MoveDown(EditorControl ctx, Selection sel)
        {
            var pos = sel.Caret;

            do
            {
                pos = InternalMoveDown(ctx, sel, pos);
            } while (!ctx.Folding.IsLineVisible(pos.Line));

            return pos;
        }

        private static Pos InternalMoveDown(EditorControl ctx, Selection sel, Pos pos)
        {
            var doc = ctx.Buffer.Document;

            if (sel.RestoreCaretCol == 0 && pos.Col != 0)
                sel.SetToRestore(doc.Lines[pos.Line].GetStripeCol(pos.Col));

            if (ctx.WordWrap)
            {
                var ln = doc.Lines[pos.Line];
                var stripe = ln.GetStripe(pos.Col);
                var tetra = ln.GetStripeCol(pos.Col, stripe);
                tetra = tetra > sel.RestoreCaretCol ? tetra : sel.RestoreCaretCol;
                var set = ctx.EditorSettings;

                if (stripe == ln.Stripes - 1)
                {
                    if (pos.Line == doc.Lines.Count - 1)
                        return pos;
                    else
                    {
                        var newLn = doc.Lines[pos.Line + 1];
                        var newCut = newLn.GetCut(0);
                        return new Pos(pos.Line + 1, newCut > tetra ? tetra : newCut);
                    }
                }
                else
                {
                    var shift = set.WrappingIndent == WrappingIndent.Same ? ln.Indent
                        : set.WrappingIndent == WrappingIndent.Indent ? ln.Indent /*+ set.IndentSize*/ : 0;
                    var newStart = ln.GetCut(stripe);// + 1;
                    var newEnd = ln.GetCut(stripe + 1) - 1;
                    var nc = newStart + tetra - shift;
                    return new Pos(pos.Line, nc > newEnd ? newEnd : nc);
                }

            }
            else if (pos.Line + 1 < doc.Lines.Count)
            {
                var ln = doc.Lines[pos.Line + 1];
                var col = pos.Col > sel.RestoreCaretCol ? pos.Col : sel.RestoreCaretCol;
                return new Pos(pos.Line + 1, ln.Length < col ? ln.Length : col);
            }
            else
                return pos;
        }
    }
}
