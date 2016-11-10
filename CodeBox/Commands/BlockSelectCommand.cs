using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | SingleRun)]
    public sealed class BlockSelectCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            DoSelection(arg.Pos, arg.Location);
            return ActionResults.Clean;
        }

        private void DoSelection(Pos p, Point loc)
        {
            var start = Buffer.Selections[Buffer.Selections.Count - 1].Start;
            var pline = p.Line;
            var tetra = (loc.X - Context.Info.TextLeft) / Context.Info.CharWidth;
            tetra = tetra < 0 ? 0 : tetra;
            var lines = Document.Lines;

            if (lines[pline].Length == 0)
                return;

            var tabSize = Context.Settings.TabSize;
            var startTetra = lines[start.Line].GetTetras(start.Col, tabSize);
            var maxLen = p.Col;
            Buffer.Selections.Clear();

            if (start > p)
            {
                for (var i = start.Line; i > pline - 1; i--)
                {
                    var ln = lines[i];
                    var lnt = ln.GetTetras(tabSize);
                    var min = tetra > startTetra ? startTetra : tetra;

                    if (lnt < min)
                        continue;

                    var startCol = lnt < startTetra ? lnt : startTetra;
                    var endCol = tetra > startCol && tetra > lnt ? lnt : tetra;

                    var sel = new Selection(
                        new Pos(i, ln.GetColumnForTetra(startCol, tabSize)),
                        new Pos(i, ln.GetColumnForTetra(endCol, tabSize)));

                    AddSelection(i, start, p, sel);
                }
            }
            else
            {
                for (var i = start.Line; i < pline + 1; i++)
                {
                    var ln = lines[i];
                    var lnt = ln.GetTetras(tabSize);

                    if (lnt < startTetra)
                        continue;

                    var endCol = tetra > lnt ? lnt : tetra;

                    var sel = new Selection(
                        new Pos(i, ln.GetColumnForTetra(startTetra, tabSize)),
                        new Pos(i, ln.GetColumnForTetra(endCol, tabSize)));

                    AddSelection(i, start, p, sel);
                }
            }
        }

        private void AddSelection(int i, Pos start, Pos p, Selection sel)
        {
            if (i == start.Line)
                Buffer.Selections.Set(sel);
            else
            {
                var osel = Buffer.Selections.GetSelection(p);

                if (osel != null)
                    Buffer.Selections.Remove(osel);

                Buffer.Selections.AddFirst(sel);
            }
        }
    }
}
