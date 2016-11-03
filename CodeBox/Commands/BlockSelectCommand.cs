using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
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
            DoSelection(arg.Pos);
            return ActionResults.Clean;
        }

        private void DoSelection(Pos p)
        {
            var lines = Document.Lines;

            if (lines[p.Line].Length == 0)
                return;

            var start = Buffer.Selections[0].Start;
            var maxLen = 0;

            foreach (var sel in Buffer.Selections)
            {
                var len = Math.Abs(sel.End.Col - sel.Start.Col);

                if (len > maxLen)
                    maxLen = len;
            }

            var lastLen = Math.Abs(start.Col - p.Col);

            if (p.Col < lines[p.Line].Length - 1 || lastLen > maxLen)
                maxLen = lastLen;

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
                for (var i = start.Line; i < p.Line + 1; i++)
                {
                    var ln = lines[i];

                    if (ln.Length == 0)
                        continue;

                    var endCol = p.Col < start.Col
                        ? (start.Col - maxLen < 0 ? 0 : start.Col - maxLen)
                        : (start.Col + maxLen > ln.Length ? ln.Length : start.Col + maxLen);
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
