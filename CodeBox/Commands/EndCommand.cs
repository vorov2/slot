using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public class EndCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = MoveEnd(Document, sel.Caret);
            sel.SetToRestore(pos);
            return pos;
        }

        internal static Pos MoveEnd(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            return new Pos(pos.Line, ln.Length);
        }
    }
}
