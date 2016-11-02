using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Scroll | Undoable)]
    public class InsertCharCommand : Command
    {
        private Character @char;
        private Character @redoChar;
        private IEnumerable<Character> @string;
        private Pos undoPos;
        private Selection redoSel;

        public override ActionChange Execute(CommandArgument arg, Selection sel)
        {
            var line = Document.Lines[sel.Caret.Line];
            undoPos = sel.Start;
            redoSel = sel.Clone();
            var res = ActionChange.Atomic;

            if (!sel.IsEmpty)
            {
                res = ActionChange.Mixed;
                @string = DeleteRangeCommand.DeleteRange(Context, sel);
            }
            else if (Buffer.Overtype)
            {
                @char = line[sel.Caret.Col];
                line.RemoveAt(sel.Caret.Col);
            }

            @redoChar = new Character(arg.Char);
            Document.Lines[sel.Caret.Line].Insert(sel.Caret.Col, @redoChar);
            sel.Clear(new Pos(sel.Caret.Line, sel.Caret.Col + 1));
            return res;
        }

        public override Pos Redo()
        {
            @string = null;
            @char = Character.Empty;
            var arg = new CommandArgument(redoChar.Char);
            redoChar = Character.Empty;
            Execute(arg, redoSel);
            return new Pos(redoSel.Caret.Line, redoSel.Caret.Col + 1);
        }

        public override Pos Undo()
        {
            var lines = Document.Lines;
            lines[undoPos.Line].RemoveAt(undoPos.Col);
            var pos = Pos.Empty;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);

            if (!@char.IsEmpty)
                lines[undoPos.Line].Insert(undoPos.Col, @char);

            if (pos.IsEmpty)
                pos = undoPos;

            Buffer.Selections.Set(pos);
            return pos;
        }
    }
}
