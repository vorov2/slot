using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal class LeftCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos pos)
        {
            return MoveLeft(context.Document, pos);
        }

        internal static Pos MoveLeft(Document doc, Pos pos)
        {
            pos = new Pos(pos.Line, pos.Col - 1);

            if (pos.Col < 0 && pos.Line > 0)
            {
                var line = doc.Lines[pos.Line - 1];
                pos = new Pos(line.Index, line.Length);
            }

            return pos;
        }
    }
}
