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
    internal class EndCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos pos)
        {
            return MoveEnd(context.Document, pos);
        }

        internal static Pos MoveEnd(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            return new Pos(pos.Line, ln.Length);
        }
    }
}
