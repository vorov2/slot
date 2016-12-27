using System;
using System.Drawing;
using System.Windows.Forms;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.selectblock")]
    public sealed class BlockSelectCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            DoSelection(Ed.Caret, Ed.PointToClient(Cursor.Position));
            return Scroll | Clean;
        }

        private void DoSelection(Pos p, Point loc)
        {
            var start = Buffer.Selections[Buffer.Selections.Count - 1].Start;
            var pline = p.Line;
            var tetra = (loc.X - Ed.Info.TextLeft) / Ed.Info.CharWidth;
            tetra = tetra < 0 ? 0 : tetra;
            var lines = Document.Lines;

            if (lines[pline].Length == 0)
                return;

            var indentSize = Ed.IndentSize;
            var startTetra = lines[start.Line].GetTetras(start.Col, indentSize);
            var maxLen = p.Col;
            Buffer.Selections.Clear();

            if (start > p)
            {
                for (var i = start.Line; i > pline - 1; i--)
                {
                    var ln = lines[i];
                    var lnt = ln.GetTetras(indentSize);
                    var min = tetra > startTetra ? startTetra : tetra;

                    if (lnt < min)
                        continue;

                    var startCol = lnt < startTetra ? lnt : startTetra;
                    var endCol = tetra > startCol && tetra > lnt ? lnt : tetra;

                    var sel = new Selection(
                        new Pos(i, ln.GetColumnForTetra(startCol, indentSize)),
                        new Pos(i, ln.GetColumnForTetra(endCol, indentSize)));

                    AddSelection(i, start, p, sel);
                }
            }
            else
            {
                for (var i = start.Line; i < pline + 1; i++)
                {
                    var ln = lines[i];
                    var lnt = ln.GetTetras(indentSize);

                    if (lnt < startTetra)
                        continue;

                    var endCol = tetra > lnt ? lnt : tetra;

                    var sel = new Selection(
                        new Pos(i, ln.GetColumnForTetra(startTetra, indentSize)),
                        new Pos(i, ln.GetColumnForTetra(endCol, indentSize)));

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
                //var osel = Buffer.Selections.GetSelection(p);

                //if (osel != null)
                //    Buffer.Selections.Remove(osel);

                Buffer.Selections.AddFirst(sel);
            }
        }

        internal override bool SingleRun => true;
    }
}
