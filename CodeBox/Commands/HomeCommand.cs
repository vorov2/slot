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
    public class HomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveHome(Document, sel.Caret);

        internal static Pos MoveHome(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            var ch = '\0';

            if (pos.Col > 0 && ((ch = ln.CharAt(pos.Col - 1)) == ' ' || ch == '\t'))
                return new Pos(pos.Line, 0);

            for (var i = 0; i < ln.Length; i++)
            {
                var c = ln.CharAt(i);

                if (c != ' ' && c != '\t')
                    return new Pos(pos.Line, i);
            }

            return new Pos(pos.Line, 0);
        }
    }
}
