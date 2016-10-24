using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal class DownCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos pos)
        {
            return MoveDown(Context, pos);
        }

        internal static Pos MoveDown(IEditorContext ctx, Pos pos)
        {
            var doc = ctx.Buffer.Document;

            if (ctx.Buffer.WordWrap)
            {
                var ln = doc.Lines[pos.Line];
                var stripe = ln.GetStripe(pos.Col);
                var tetra = ln.GetStripeCol(pos.Col, stripe);

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
                    var newStart = ln.GetCut(stripe) + 1;
                    var newEnd = ln.GetCut(stripe + 1);
                    return new Pos(pos.Line, newStart + tetra > newEnd ? newEnd : newStart + tetra);
                }

            }
            else if (pos.Line + 1 < doc.Lines.Count)
            {
                var ln = doc.Lines[pos.Line + 1];
                return new Pos(pos.Line + 1, ln.Length < pos.Col ? ln.Length : pos.Col);
            }
            else
                return pos;
        }
    }
}
