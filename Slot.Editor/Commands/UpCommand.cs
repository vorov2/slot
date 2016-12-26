using System;
using Slot.Editor.ObjectModel;
using Slot.Editor.Folding;
using Slot.Editor.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;
using Slot.Core.Settings;
using Slot.Core;
using Slot.Core.ViewModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.up")]
    public class UpCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveUp(View, sel);

        internal static Pos MoveUp(EditorControl ctx, Selection sel)
        {
            var pos = sel.Caret;
            var lines = ctx.Buffer.Document.Lines;

            do
            {
                pos = InternalMoveUp(ctx, sel, pos);
            } while (!ctx.Folding.IsLineVisible(pos.Line));

            return pos;
        }

        private static Pos InternalMoveUp(EditorControl ctx, Selection sel, Pos pos)
        {
            var doc = ctx.Buffer.Document;

            if (ctx.WordWrap)
            {
                var ln = doc.Lines[pos.Line];
                var stripe = ln.GetStripe(pos.Col);
                var tetra = ln.GetStripeCol(pos.Col, stripe);
                tetra = tetra > sel.RestoreCaretCol ? tetra : sel.RestoreCaretCol;
                var set = ctx.EditorSettings;
                var shift = set.WrappingIndent == WrappingIndent.Same ? ln.Indent
                    : set.WrappingIndent == WrappingIndent.Indent ? ln.Indent /*+ set.IndentSize*/ : 0;

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
                            shift = strp <= 0 ? 0 : shift;
                            var nc = newCut + tetra - shift;
                            return new Pos(pos.Line - 1, nc > newLn.Length ? newLn.Length : nc);
                        }
                        else
                            return new Pos(pos.Line - 1, newCut > tetra ? tetra : newCut);
                    }
                }
                else
                {
                    var newStart = stripe > 1 ? ln.GetCut(stripe - 2) : 0;
                    var newEnd = ln.GetCut(stripe - 1) - 1;
                    shift = stripe - 1 == 0 ? 0 : shift;
                    var nc = newStart + tetra - shift;
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
