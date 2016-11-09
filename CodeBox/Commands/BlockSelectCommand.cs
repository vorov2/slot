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
    [CommandBehavior(/*Scroll |*/ SingleRun)]
    public sealed class BlockSelectCommand : Command
    {
        public override ActionResults Execute(CommandArgument arg, Selection sel)
        {
            DoSelection(arg.Pos, arg.Location);
            return ActionResults.Clean;
        }

        private void DoSelection(Pos p, Point loc)
        {
            var start = Buffer.Selections.Main.Start;
            var pline = p.Line;
            var tetra = (loc.X - Context.Info.TextLeft) / Context.Info.CharWidth;

            var lines = Document.Lines;

            if (lines[pline].Length == 0)
                return;

            var maxLen = p.Col;

            Buffer.Selections.Clear();

            if (start > p)
            {
                for (var i = start.Line; i > p.Line - 1; i--)
                {
                    var ln = lines[i];

                    if (ln.Length == 0)
                        continue;

                    var sel = new Selection(
                        new Pos(i, start.Col),
                        new Pos(i, start.Col - maxLen < 0 ? 0 : start.Col - maxLen));

                    if (i == start.Line)
                        Buffer.Selections.Set(sel);
                    else
                    {
                        var osel = Buffer.Selections.GetSelection(p);
                        if (osel != null)
                            Buffer.Selections.Remove(osel);

                        Buffer.Selections.Add(sel);
                    }
                }
            }
            else
            {
                for (var i = start.Line; i < pline + 1; i++)
                {
                    var ln = lines[i];
                    var lnt = ln.GetTetras(Context.Settings.TabSize);

                    if (lnt < start.Col)
                        continue;

                    var endCol = tetra > lnt ? lnt : tetra;

                    //var te = endCol;

                    //for (var j = 0; j < te; j++)
                    //    if (ln[i].Char == '\t')
                    //        endCol -= (Context.Settings.TabSize - 1);

                    var sel = new Selection(new Pos(i, start.Col), new Pos(i, endCol));

                    if (i == start.Line)
                        Buffer.Selections.Set(sel);
                    else
                    {
                        var osel = Buffer.Selections.GetSelection(p);

                        if (osel != null)
                            Buffer.Selections.Remove(osel);

                        Buffer.Selections.Add(sel);
                    }
                }
            }
        }
    }
}
