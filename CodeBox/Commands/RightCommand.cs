using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    [CommandBehavior(ActionExponent.Scroll | ActionExponent.ClearSelections)]
    internal class RightCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return MoveRight(context, caret);
        }

        internal static Pos MoveRight(EditorContext context, Pos pos)
        {
            var doc = context.Document;
            pos = new Pos(pos.Line, pos.Col + 1);
            var line = doc.Lines[pos.Line];

            if (pos.Col > line.Length && pos.Line < doc.Lines.Count)
            {
                line = doc.Lines[pos.Line + 1];
                pos = new Pos(line.Index, 0);
            }

            return pos;
        }
    }
}
