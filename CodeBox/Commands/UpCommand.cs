using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;

namespace CodeBox.Commands
{
    public class UpCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveUp(Context, sel);

        internal static Pos MoveUp(IEditorContext ctx, Selection sel)
        {
            var pos = sel.Caret;

            do
            {
                pos = InternalMoveUp(ctx, sel, pos);
            } while (ctx.Buffer.Document.Lines[pos.Line].Folding.Has(FoldingStates.Invisible));

            return pos;
        }

        private static Pos InternalMoveUp(IEditorContext ctx, Selection sel, Pos pos)
        {
            var doc = ctx.Buffer.Document;

            if (ctx.WordWrap)
            {
                var ln = doc.Lines[pos.Line];
                var stripe = ln.GetStripe(pos.Col);
                var tetra = ln.GetStripeCol(pos.Col, stripe);
                tetra = tetra > sel.RestoreCaretCol ? tetra : sel.RestoreCaretCol;

                if (stripe == 0)
                {
                    if (pos.Line == 0)
                        return pos;
                    else
                    {
                        var newLn = doc.Lines[pos.Line - 1];
                        var newCut = newLn.GetCut(newLn.Stripes - 2);
                        if (newCut != newLn.Length)
                            return new Pos(pos.Line - 1, newCut + tetra > newLn.Length ? newLn.Length : newCut + tetra);
                        else
                            return new Pos(pos.Line - 1, newCut > tetra ? tetra : newCut);
                    }
                }
                else
                {
                    var newStart = stripe > 1 ? ln.GetCut(stripe - 2) + 1 : 0;
                    var newEnd = ln.GetCut(stripe - 1);
                    return new Pos(pos.Line, newStart + tetra > newEnd ? newEnd : newStart + tetra);
                }
            }
            else if (pos.Line > 0)
            {
                var ln = doc.Lines[pos.Line - 1];
                var col = pos.Col > sel.RestoreCaretCol ? pos.Col : sel.RestoreCaretCol;
                return new Pos(pos.Line - 1, ln.Length < col ? ln.Length : col);
            }
            else
                return pos;
        }
    }
}
