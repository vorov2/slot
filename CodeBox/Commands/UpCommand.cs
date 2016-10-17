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
    internal class UpCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return MoveUp(context, caret);
        }
        
        internal static Pos MoveUp(EditorContext context, Pos pos)
        {
            var doc = context.Document;

            if (context.Settings.WordWrap)
            {
                var ln = doc.Lines[pos.Line];
                var stripe = ln.GetStripe(pos.Col);
                var tetra = ln.GetStripeCol(pos.Col, stripe);

                if (stripe == 0)
                {
                    if (pos.Line == 0)
                        return pos;
                    else
                    {
                        var newLn = doc.Lines[pos.Line - 1];
                        var newCut = newLn.GetCut(newLn.Stripes - 1);
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
                return new Pos(pos.Line - 1, ln.Length < pos.Col ? ln.Length : pos.Col);
            }
            else
                return pos;
        }
    }
}
