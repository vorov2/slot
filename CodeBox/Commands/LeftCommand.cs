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
        protected override Pos GetPosition(Selection sel) => MoveLeft(Document, sel);

        internal static Pos MoveLeft(Document doc, Selection sel)
        {
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col - 1);

            if (pos.Col < 0 && pos.Line > 0)
            {
                var line = doc.Lines[pos.Line - 1];
                pos = new Pos(pos.Line - 1, line.Length);
            }
            else if (pos.Col < 0)
                pos = new Pos(pos.Line, 0);

            sel.SetToRestore(pos);
            return pos;
        }
    }
}
