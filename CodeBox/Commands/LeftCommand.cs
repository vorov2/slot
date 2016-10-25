using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    public class LeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos pos)
        {
            return MoveLeft(Document, pos);
        }

        internal static Pos MoveLeft(Document doc, Pos pos)
        {
            pos = new Pos(pos.Line, pos.Col - 1);

            if (pos.Col < 0 && pos.Line > 0)
            {
                var line = doc.Lines[pos.Line - 1];
                pos = new Pos(pos.Line - 1, line.Length);
            }

            return pos;
        }
    }
}
