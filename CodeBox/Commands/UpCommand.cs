using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.up")]
    public class UpCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveUp(View, sel);

        internal static Pos MoveUp(Editor ctx, Selection sel)
        {
            var pos = sel.Caret;
            var lines = ctx.Buffer.Document.Lines;

            do
            {
                pos = InternalMoveUp(ctx, sel, pos);
            } while (!ctx.Folding.IsLineVisible(pos.Line));

            return pos;
        }

        private static Pos InternalMoveUp(Editor ctx, Selection sel, Pos pos)
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
                        var strp = newLn.Stripes - 2;
                        var newCut = newLn.GetCut(strp < 0 ? 0 : strp);
                        if (newCut != newLn.Length)
                        {
                            var nc = newCut + tetra;
                            return new Pos(pos.Line - 1, nc > newLn.Length ? newLn.Length : nc);
                        }
                        else
                            return new Pos(pos.Line - 1, newCut > tetra ? tetra : newCut);
                    }
                }
                else
                {
                    var newStart = stripe > 1 ? ln.GetCut(stripe - 2) + 1 : 0;
                    var newEnd = ln.GetCut(stripe - 1);
                    var nc = newStart + tetra;
                    return new Pos(pos.Line, nc > newEnd ? newEnd : nc);
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
