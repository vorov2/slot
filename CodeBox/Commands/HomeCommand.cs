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
    internal class HomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Pos pos)
        {
            return MoveHome(Document, pos);
        }

        internal static Pos MoveHome(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];

            if (pos.Col > 0 && ln.CharAt(pos.Col - 1) == ' ')
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
