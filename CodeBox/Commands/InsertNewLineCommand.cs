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

        public override void Execute(CommandArgument arg, Selection selection)
        {
            undoPos = selection.Start;
            redoSel = selection.Clone();

            if (!selection.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(Context, selection);

            var pos = InsertNewLine(Document, undoPos);
            selection.Clear(pos);
        }

        public override Pos Redo()
        {
            @string = null;
            var sel = redoSel;
            Execute(default(CommandArgument), sel);
            return sel.Caret;
        }

        public override Pos Undo()
        {
            var pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);

            var line = Document.Lines[pos.Line];
            var nextLine = Document.Lines[pos.Line + 1];
            Document.Lines.Remove(nextLine);
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

            var newLn = doc.NewLine(str);
            doc.Lines.Insert(pos.Line + 1, newLn);
            pos = new Pos(pos.Line + 1, 0);
            return pos;
        }
    }
}
