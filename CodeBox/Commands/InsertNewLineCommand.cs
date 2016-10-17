using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    internal sealed class InsertNewLineCommand : Command //Tested
    {
        private IEnumerable<Character> @string;
        private Pos undoPos;
        private Selection redoSel;

        public override void Execute(EditorContext context, Selection selection)
        {
            undoPos = selection.Start;
            redoSel = selection.Clone();

            if (!selection.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(context, selection);

            var pos = InsertNewLine(context.Document, undoPos);
            selection.Clear(pos);
        }

        public override Pos Redo(EditorContext context)
        {
            @string = null;
            var sel = redoSel;
            Execute(context, sel);
            return sel.Caret;
        }

        public override Pos Undo(EditorContext context)
        {
            var pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(context.Document, undoPos, @string);

            var line = context.Document.Lines[pos.Line];
            var nextLine = context.Document.Lines[pos.Line + 1];
            context.Document.Lines.Remove(nextLine);
            line.Append(nextLine);
            return pos;
        }

        internal static Pos InsertNewLine(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            var str = default(IEnumerable<Character>);

            if (pos.Col != ln.Length)
            {
                str = ln.GetRange(pos.Col, ln.Length - pos.Col);
                ln.RemoveRange(pos.Col, ln.Length - pos.Col);
            }

            var newLn = new Line(str, ++doc.LineSequence);
            doc.Lines.Insert(pos.Line + 1, newLn);
            pos = new Pos(pos.Line + 1, 0);
            return pos;
        }
    }
}
